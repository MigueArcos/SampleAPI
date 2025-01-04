using System;
using System.Linq;

namespace ArchitectureTest.TestUtils;

public static class StubData {
    public const string Email = "system@undertest.com";
    public const string Password = "P455w0rd";
    public const string HashedPassword = "10000.key.salt";
    public const string UserName = "Test User";
    public const string JwtToken = "eyzhdhhdhd.fhfhhf.fggg";
    public const string RefreshToken = "4nyR3fr35hT0k3n";
    public const string UserId = "1";
    public const string NoteId = "10";
    public const string ChecklistId = "10";
    public const string NoteTitle = "My Fancy Note";
    public const string ChecklistTitle = "My Fancy Checklist";
    public const string ChecklistTaskName = "My Fancy TaskName";
    public const string NoteContent = "My beautiful content";
    public static DateTime Today { get => DateTime.Now.Date; }
    public static DateTime NextWeek { get => DateTime.Now.AddDays(7).Date; }

    public static string CreateRandomString(int? length = null)
    {
        var random = new Random();
        length ??= random.Next(6, 16);

        const string dictionary = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        return new string(Enumerable.Repeat(dictionary, length.Value)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
