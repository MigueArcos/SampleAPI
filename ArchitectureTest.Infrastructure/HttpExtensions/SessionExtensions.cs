using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ArchitectureTest.Infrastructure.Extensions {
	public static class SessionExtensions {
		public static void SetObject<T>(this ISession session, string key, T value) {
			session.SetString(key, JsonConvert.SerializeObject(value));
		}

		public static T GetObject<T>(this ISession session, string key) {
			var value = session.GetString(key);
			return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
		}

		public static void SetTypedObject<T>(this ISession session, string key, T value) {
			session.SetString(key, JsonConvert.SerializeObject(value, new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.All
			}));
		}

		public static T GetTypedObject<T>(this ISession session, string key) {
			var value = session.GetString(key);
			return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.Auto
			});
		}
	}
}
