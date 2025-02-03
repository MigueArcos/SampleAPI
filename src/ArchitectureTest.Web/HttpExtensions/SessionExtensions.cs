using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace ArchitectureTest.Infrastructure.Extensions;

[ExcludeFromCodeCoverage]
public static class SessionExtensions {
    public static void SetObject<T>(this ISession session, string key, T value) {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetObject<T>(this ISession session, string key) {
        var value = session.GetString(key);
        return string.IsNullOrWhiteSpace(value) ? default : JsonSerializer.Deserialize<T>(value);
    }

    // public static void SetTypedObject<T>(this ISession session, string key, T value) {
    //     session.SetString(key, JsonSerializer.Serialize(value, new JsonSerializerOptions {
    //         // TypeNameHandling = TypeNameHandling.All
    //     }));
    // }

    // public static T? GetTypedObject<T>(this ISession session, string key) {
    //     var value = session.GetString(key);
    //     return value == null ? default(T) : JsonSerializer.Deserialize<T>(value, new JsonSerializerOptions {
    //         // TypeNameHandling = TypeNameHandling.Auto
    //     });
    // }
}
