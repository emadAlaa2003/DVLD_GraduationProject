using System.Security.Claims;
using DVLD_Buisness;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DVLD.Api.Authentication
{
    public sealed class MobileCookieAuthenticationEvents
        : CookieAuthenticationEvents
    {
        public override async Task ValidatePrincipal(
            CookieValidatePrincipalContext context)
        {
            string? mobileUserIdValue =
                context.Principal?
                    .FindFirstValue(ClaimTypes.NameIdentifier);

            string? personIdValue =
                context.Principal?
                    .FindFirstValue(
                        MobileAuthDefaults.PersonIdClaim);

            string? accountType =
                context.Principal?
                    .FindFirstValue(
                        MobileAuthDefaults.AccountTypeClaim);

            if (!int.TryParse(
                    mobileUserIdValue,
                    out int mobileUserId)
                ||
                !int.TryParse(
                    personIdValue,
                    out int personId)
                ||
                accountType !=
                    MobileAuthDefaults.MobileCitizenAccountType)
            {
                await RejectPrincipalAsync(context);
                return;
            }

            clsMobileUser mobileUser =
                clsMobileUser.FindByMobileUserID(
                    mobileUserId);

            if (mobileUser == null
                ||
                !mobileUser.IsActive
                ||
                mobileUser.PersonID != personId)
            {
                await RejectPrincipalAsync(context);
            }
        }

        private static async Task RejectPrincipalAsync(
            CookieValidatePrincipalContext context)
        {
            context.RejectPrincipal();

            await context.HttpContext.SignOutAsync(
                MobileAuthDefaults.Scheme);
        }
    }
}