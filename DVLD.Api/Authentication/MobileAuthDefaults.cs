namespace DVLD.Api.Authentication
{
    public static class MobileAuthDefaults
    {
        public const string Scheme = "DVLD.MobileCitizen";
        public const string CookieName = "DVLD.MobileAuth";

        public const string PersonIdClaim = "person_id";
        public const string AccountTypeClaim = "account_type";
        public const string MobileCitizenAccountType = "mobile_citizen";

        public const string EmployeeApiPolicy = "EmployeeApiOnly";
    }
}