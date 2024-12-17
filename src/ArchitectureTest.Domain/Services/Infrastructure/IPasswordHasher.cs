namespace ArchitectureTest.Domain.Services.Infrastructure;

public interface IPasswordHasher {
    string Hash(string password);

    (bool Verified, bool NeedsUpgrade) Check(string hash, string password);
}
