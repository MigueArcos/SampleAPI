using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ArchitectureTest.TestUtils;

public static class ObjectComparer {
    public static bool JsonCompare<T>(T first, T second, string[]? propertiesToIgnore = null) {
        string firstJson = JsonSerializer.Serialize(first);
        string secondJson = JsonSerializer.Serialize(second);

        if (propertiesToIgnore != null && propertiesToIgnore.Length > 0)
            return JsonCompareIgnoringProperties(firstJson, secondJson, propertiesToIgnore);

        return firstJson == secondJson;
    }

    public static bool JsonTransformAndCompare<T, R>(T first, T second) {
        string firstJson = JsonSerializer.Serialize(first);
        string secondJson = JsonSerializer.Serialize(second);

        var firstAsValueObject = JsonSerializer.Deserialize<R>(firstJson);
        var secondAsValueObject = JsonSerializer.Deserialize<R>(secondJson);

        firstJson = JsonSerializer.Serialize(firstAsValueObject);
        secondJson = JsonSerializer.Serialize(secondAsValueObject);

        return firstJson == secondJson;
    }

    public static bool JsonCompareIgnoringProperties(string firstJson, string secondJson, string[] propertiesToIgnore) {
        var firstAsDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(firstJson);
        var secondAsDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(secondJson);

        Array.ForEach(propertiesToIgnore, prop => {
            firstAsDictionary!.Remove(prop);
            secondAsDictionary!.Remove(prop);
        });

        string newFirstJson = JsonSerializer.Serialize(firstAsDictionary);
        string newSecondJson = JsonSerializer.Serialize(secondAsDictionary);
        return newFirstJson == newSecondJson;
    }
}
