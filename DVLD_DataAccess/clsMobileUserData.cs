using System.Data;
using Microsoft.Data.SqlClient;

namespace DVLD_DataAccess
{
    public class clsMobileUserData
    {
        public static bool GetMobileUserInfoByUsername(
            string Username,
            ref int MobileUserID,
            ref int PersonID,
            ref string PasswordHash,
            ref bool IsActive)
        {
            using (SqlConnection connection =
                new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                const string query =
                    @"SELECT MobileUserID,
                             PersonID,
                             PasswordHash,
                             IsActive
                      FROM MobileUsers
                      WHERE Username = @Username;";

                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters
                        .Add("@Username", SqlDbType.NVarChar, 50)
                        .Value = Username;

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        MobileUserID = (int)reader["MobileUserID"];
                        PersonID = (int)reader["PersonID"];
                        PasswordHash = (string)reader["PasswordHash"];
                        IsActive = (bool)reader["IsActive"];

                        return true;
                    }
                }
            }

        }
        public static bool GetMobileUserInfoByMobileUserID(
    int MobileUserID,
    ref int PersonID,
    ref string Username,
    ref string PasswordHash,
    ref bool IsActive)
        {
            using (SqlConnection connection =
                new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                const string query =
                    @"SELECT PersonID,
                     Username,
                     PasswordHash,
                     IsActive
              FROM MobileUsers
              WHERE MobileUserID = @MobileUserID;";

                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters
                        .Add("@MobileUserID", SqlDbType.Int)
                        .Value = MobileUserID;

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        PersonID = (int)reader["PersonID"];
                        Username = (string)reader["Username"];
                        PasswordHash = (string)reader["PasswordHash"];
                        IsActive = (bool)reader["IsActive"];

                        return true;
                    }
                }
            }
        }
    }
}