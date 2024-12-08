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

# Publish the application
WORKDIR /app/src/ArchitectureTest.Web
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/src/ArchitectureTest.Web/out ./

# Expose the port your application will run on
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "ArchitectureTest.Web.dll"]
