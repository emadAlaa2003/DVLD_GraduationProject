using System.Security.Claims;

namespace DVLD.Api.Authentication
{
    public static class MobileAuthClaims
    {
        public static bool TryGetPersonId(
            ClaimsPrincipal user,
            out int personId)
        {
            personId = -1;

            string? personIdValue =
                user.FindFirstValue(
                    MobileAuthDefaults.PersonIdClaim);

            return int.TryParse(
                personIdValue,
                out personId);
        }
    }
}