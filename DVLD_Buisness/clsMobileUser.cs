using System;
using DVLD_DataAccess;

namespace DVLD_Buisness
{
    public class clsMobileUser
    {
        public int MobileUserID { get; private set; }
        public int PersonID { get; private set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }
        public bool IsActive { get; private set; }

        public clsPerson PersonInfo { get; private set; }

        private clsMobileUser(
            int MobileUserID,
            int PersonID,
            string Username,
            string PasswordHash,
            bool IsActive)
        {
            this.MobileUserID = MobileUserID;
            this.PersonID = PersonID;
            this.Username = Username;
            this.PasswordHash = PasswordHash;
            this.IsActive = IsActive;

            this.PersonInfo = clsPerson.Find(PersonID);
        }

        public static clsMobileUser FindByUsername(string Username)
        {
            if (string.IsNullOrWhiteSpace(Username))
                return null;

            Username = Username.Trim().ToLowerInvariant();

            int MobileUserID = -1;
            int PersonID = -1;
            string PasswordHash = "";
            bool IsActive = false;

            bool IsFound =
                clsMobileUserData.GetMobileUserInfoByUsername(
                    Username,
                    ref MobileUserID,
                    ref PersonID,
                    ref PasswordHash,
                    ref IsActive);

            if (!IsFound)
                return null;

            return new clsMobileUser(
                MobileUserID,
                PersonID,
                Username,
                PasswordHash,
                IsActive);
        }

        public static clsMobileUser FindByMobileUserID(int MobileUserID)
        {
            if (MobileUserID <= 0)
                return null;

            int PersonID = -1;
            string Username = "";
            string PasswordHash = "";
            bool IsActive = false;

            bool IsFound =
                clsMobileUserData.GetMobileUserInfoByMobileUserID(
                    MobileUserID,
                    ref PersonID,
                    ref Username,
                    ref PasswordHash,
                    ref IsActive);

            if (!IsFound)
                return null;

            return new clsMobileUser(
                MobileUserID,
                PersonID,
                Username,
                PasswordHash,
                IsActive);
        }
    }
}