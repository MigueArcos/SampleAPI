# Use the official .NET Core SDK as a parent image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ArchitectureTest.sln ./

COPY ./src/*/*.csproj ./
COPY ./tests/*/*.csproj ./

RUN TESTPROJS=$(find . -name \*.Tests.csproj -printf '%f\n') && for item in $TESTPROJS; do \
    dotnet sln remove ./tests/"${item%.*}"/$item; \
    rm $item; \
    done;

RUN PROJS=$(find . -name \*.csproj -printf '%f\n') && for item in $PROJS; do \
    mkdir -p ./src/"${item%.*}"; \
    mv $item ./src/"${item%.*}"/$item; \
    done;

# RUN echo "MaxProtocol = TLSv1.2" >> /etc/ssl/openssl.cnf && dotnet restore
RUN dotnet restore

COPY ./src ./src
COPY ./.git ./.git

# Publish the application
WORKDIR /app/src/ArchitectureTest.Web

RUN BRANCH=$(git branch --show-current) && HASH=$(git rev-parse --short HEAD) && \
    dotnet build /p:InformationalVersion="$BRANCH-$HASH" -c Release

RUN dotnet publish -c Release -o out --no-build

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/src/ArchitectureTest.Web/out ./

# Expose the port your application will run on (default 8080 on .NET 8 and higher)
# https://learn.microsoft.com/en-us/dotnet/core/compatibility/containers/8.0/aspnet-port
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "ArchitectureTest.Web.dll"]
