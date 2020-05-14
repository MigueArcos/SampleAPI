using Microsoft.AspNetCore.Http;
using System;

namespace ArchitectureTest.Infrastructure.Extensions {
	public static class CookieExtensions {
		public static void SetCookie(this HttpContext context, string cookieName, string value) {
			if (!context.CookieExists(cookieName)) {
				context.Response.Cookies.Append(cookieName, value, new CookieOptions { Expires = DateTime.Now.AddDays(1), HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Strict, MaxAge = TimeSpan.FromDays(1) });
			}
			else {
				//Replace cookie
				context.Response.Cookies.Delete(cookieName);
				context.Response.Cookies.Append(cookieName, value, new CookieOptions { Expires = DateTime.Now.AddDays(1), HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Strict, MaxAge = TimeSpan.FromDays(1) });
			}
		}
		public static void RemoveCookie(this HttpContext context, string cookieName) {
			if (context.CookieExists(cookieName)) {
				context.Response.Cookies.Delete(cookieName);
			}
		}
		public static string GetCookieValue(this HttpContext context, string cookieName) {
			if (context.CookieExists(cookieName)) {
				return context.Request.Cookies[cookieName];
			}
			return string.Empty;
		}
		public static bool CookieExists(this HttpContext context, string cookieName) {
			return context.Request.Cookies[cookieName] != null;
		}
	}
}
