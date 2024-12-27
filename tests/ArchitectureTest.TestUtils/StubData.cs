using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ArchitectureTest.TestUtils;

public static class StubData {
    public const string Email = "system@undertest.com";
    public const string Password = "P455w0rd";
    public const string HashedPassword = "10000.key.salt";
    public const string UserName = "Test User";
    public const string JwtToken = "eyzhdhhdhd.fhfhhf.fggg";
    public const string RefreshToken = "4nyR3fr35hT0k3n";
    public const long UserId = 1;
    public const long NoteId = 10;
    public const string NoteTitle = "My Fancy Title";
    public const string NoteContent = "My beautiful content";
    public static DateTime Today { get => DateTime.Now.Date; }
    public static DateTime NextWeek { get => DateTime.Now.AddDays(7).Date; }

    public static bool JsonCompare<T>(T first, T second, string[]? propertiesToIgnore = null) {
        string firstJson = JsonSerializer.Serialize(first);
        string secondJson = JsonSerializer.Serialize(second);

        if (propertiesToIgnore != null && propertiesToIgnore.Length > 0)
            return JsonCompareIgnoringProperties(firstJson, secondJson, propertiesToIgnore);

        return firstJson == secondJson;
    }

    public static bool JsonCompareIgnoringProperties(string firstJson, string secondJson, string[] propertiesToIgnore) {
        var firstAsDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(firstJson);
        var secondAsDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(secondJson);
        foreach (var prop in propertiesToIgnore){
            firstAsDictionary!.Remove(prop);
            secondAsDictionary!.Remove(prop);
        }
        string newFirstJson = JsonSerializer.Serialize(firstAsDictionary);
        string newSecondJson = JsonSerializer.Serialize(secondAsDictionary);
        return newFirstJson == newSecondJson;
    }
}
