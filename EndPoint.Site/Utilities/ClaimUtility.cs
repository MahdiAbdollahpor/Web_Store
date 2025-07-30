using System.Security.Claims;

namespace EndPoint.Site.Utilities
{
    public static class ClaimUtility
    {
        public static long? GetUserId(ClaimsPrincipal User)
        {
            try
            {
                if (User?.Identity is ClaimsIdentity claimsIdentity)
                {
                    var nameIdentifier = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    if (nameIdentifier != null && long.TryParse(nameIdentifier.Value, out long userId))
                    {
                        return userId;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static string GetUserEmail(ClaimsPrincipal User)
        {
            try
            {
                if (User?.Identity is ClaimsIdentity claimsIdentity)
                {
                    var emailClaim = claimsIdentity.FindFirst(ClaimTypes.Email);
                    return emailClaim?.Value;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static List<string> GetRolse(ClaimsPrincipal User)
        {
            try
            {
                if (User?.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.Claims != null)
                {
                    return claimsIdentity.Claims
                        .Where(p => p.Type.EndsWith("role", StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.Value)
                        .ToList();
                }
                return new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
