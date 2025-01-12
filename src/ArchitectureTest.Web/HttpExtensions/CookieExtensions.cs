namespace ArchitectureTest.Web.Extensions;

public static class CookieExtensions {
    public static void SetResponseCookie(this HttpContext context, string cookieName, string value) {
        if (!context.CheckIfCookieExistsInRequest(cookieName)) {
            context.Response.Cookies.Append(
                cookieName,
                value,
                new CookieOptions {
                    Expires = DateTime.Now.AddDays(1),
                    HttpOnly = true, IsEssential = true,
                    SameSite = SameSiteMode.Strict,
                    MaxAge = TimeSpan.FromDays(1) 
                }
            );
        }
        else {
            //Replace cookie
            context.Response.Cookies.Delete(cookieName);
            context.Response.Cookies.Append(
                cookieName,
                value,
                new CookieOptions {
                    Expires = DateTime.Now.AddDays(1),
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict,
                    MaxAge = TimeSpan.FromDays(1)
                }
            );
        }
    }

    public static void RemoveCookieFromReponse(this HttpContext context, string cookieName) {
        if (context.CheckIfCookieExistsInRequest(cookieName)) {
            context.Response.Cookies.Delete(cookieName);
        }
    }

    public static string? GetCookieValueFromRequest(this HttpContext context, string cookieName) {
        if (context.CheckIfCookieExistsInRequest(cookieName)) {
            return context.Request?.Cookies[cookieName];
        }
        return string.Empty;
    }

    public static bool CheckIfCookieExistsInRequest(this HttpContext context, string cookieName) {
        return context.Request.Cookies[cookieName] != null;
    }
}
