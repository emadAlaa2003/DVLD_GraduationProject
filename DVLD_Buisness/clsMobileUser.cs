using System;
using DVLD_DataAccess;
using System.Security.Cryptography;
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

            Username = Username.Trim();
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
        public static clsMobileUser FindByPersonID(int PersonID)
        {
            if (PersonID <= 0)
                return null;

            int MobileUserID = -1;
            string Username = "";
            string PasswordHash = "";
            bool IsActive = false;

            bool IsFound =
                clsMobileUserData.GetMobileUserInfoByPersonID(
                    PersonID,
                    ref MobileUserID,
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
        public static clsMobileUser Create(
    int PersonID,
    string Username,
    string PasswordHash,
    bool IsActive = true)
        {
            if (PersonID <= 0)
                return null;

            if (string.IsNullOrWhiteSpace(Username))
                return null;

            if (string.IsNullOrWhiteSpace(PasswordHash))
                return null;

            // Make sure the person really exists.
            if (clsPerson.Find(PersonID) == null)
                return null;

            Username = Username.Trim().ToLowerInvariant();

            // One mobile account per person.
            if (FindByPersonID(PersonID) != null)
                return null;

            // Username must also be unique.
            if (FindByUsername(Username) != null)
                return null;

            int MobileUserID =
                clsMobileUserData.AddNewMobileUser(
                    PersonID,
                    Username,
                    PasswordHash,
                    IsActive);

            if (MobileUserID <= 0)
                return null;

            return new clsMobileUser(
                MobileUserID,
                PersonID,
                Username,
                PasswordHash,
                IsActive);
        }
        public bool ResetPasswordHash(string PasswordHash)
        {
            if (string.IsNullOrWhiteSpace(PasswordHash))
                return false;

            if (this.MobileUserID <= 0)
                return false;

            bool IsUpdated =
                clsMobileUserData.UpdatePasswordHash(
                    this.MobileUserID,
                    PasswordHash);

            if (!IsUpdated)
                return false;

            this.PasswordHash = PasswordHash;

            return true;
        }
        public bool SetIsActive(bool IsActive)
        {
            if (this.MobileUserID <= 0)
                return false;

            bool IsUpdated =
                clsMobileUserData.UpdateIsActive(
                    this.MobileUserID,
                    IsActive);

            if (!IsUpdated)
                return false;

            this.IsActive = IsActive;

            return true;
        }
        public static string HashPassword(string Password)
        {
            if (string.IsNullOrEmpty(Password))
                return null;

            const int IterationCount = 100000;
            const int SaltSize = 16;
            const int SubkeyLength = 32;
            const uint HmacSha512Prf = 2;

            byte[] Salt = new byte[SaltSize];

            using (RandomNumberGenerator rng =
                RandomNumberGenerator.Create())
            {
                rng.GetBytes(Salt);
            }

            byte[] Subkey;

            using (Rfc2898DeriveBytes pbkdf2 =
                new Rfc2898DeriveBytes(
                    Password,
                    Salt,
                    IterationCount,
                    HashAlgorithmName.SHA512))
            {
                Subkey = pbkdf2.GetBytes(SubkeyLength);
            }

            byte[] OutputBytes =
                new byte[13 + SaltSize + SubkeyLength];

            OutputBytes[0] = 0x01;

            WriteNetworkByteOrder(
                OutputBytes,
                1,
                HmacSha512Prf);

            WriteNetworkByteOrder(
                OutputBytes,
                5,
                (uint)IterationCount);

            WriteNetworkByteOrder(
                OutputBytes,
                9,
                (uint)SaltSize);

            Buffer.BlockCopy(
                Salt,
                0,
                OutputBytes,
                13,
                SaltSize);

            Buffer.BlockCopy(
                Subkey,
                0,
                OutputBytes,
                13 + SaltSize,
                SubkeyLength);

            return Convert.ToBase64String(OutputBytes);
        }
        private static void WriteNetworkByteOrder(
    byte[] Buffer,
    int Offset,
    uint Value)
        {
            Buffer[Offset] = (byte)(Value >> 24);
            Buffer[Offset + 1] = (byte)(Value >> 16);
            Buffer[Offset + 2] = (byte)(Value >> 8);
            Buffer[Offset + 3] = (byte)Value;
        }
        public static string GenerateTemporaryPassword()
        {
            const string Characters =
                "ABCDEFGHJKLMNPQRSTUVWXYZ" +
                "abcdefghijkmnopqrstuvwxyz" +
                "23456789" +
                "!@#$%";

            const int PasswordLength = 12;

            char[] Password = new char[PasswordLength];
            byte[] RandomBytes = new byte[PasswordLength];

            using (RandomNumberGenerator rng =
                RandomNumberGenerator.Create())
            {
                rng.GetBytes(RandomBytes);
            }

            for (int i = 0; i < PasswordLength; i++)
            {
                Password[i] =
                    Characters[
                        RandomBytes[i] % Characters.Length];
            }

            return new string(Password);
        }
    }
}