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
        public static bool GetMobileUserInfoByPersonID(
    int PersonID,
    ref int MobileUserID,
    ref string Username,
    ref string PasswordHash,
    ref bool IsActive)
        {
            using (SqlConnection connection =
                new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                const string query =
                    @"SELECT MobileUserID,
                     Username,
                     PasswordHash,
                     IsActive
              FROM dbo.MobileUsers
              WHERE PersonID = @PersonID;";

                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters
                        .Add("@PersonID", SqlDbType.Int)
                        .Value = PersonID;

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        MobileUserID = (int)reader["MobileUserID"];
                        Username = (string)reader["Username"];
                        PasswordHash = (string)reader["PasswordHash"];
                        IsActive = (bool)reader["IsActive"];

                        return true;
                    }
                }
            }
        }
        public static int AddNewMobileUser(
    int PersonID,
    string Username,
    string PasswordHash,
    bool IsActive)
        {
            using (SqlConnection connection =
                new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                const string query =
                    @"INSERT INTO dbo.MobileUsers
                (PersonID, Username, PasswordHash, IsActive)
              VALUES
                (@PersonID, @Username, @PasswordHash, @IsActive);

              SELECT CAST(SCOPE_IDENTITY() AS int);";

                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters
                        .Add("@PersonID", SqlDbType.Int)
                        .Value = PersonID;

                    command.Parameters
                        .Add("@Username", SqlDbType.NVarChar, 50)
                        .Value = Username;

                    command.Parameters
                        .Add("@PasswordHash", SqlDbType.NVarChar, 512)
                        .Value = PasswordHash;

                    command.Parameters
                        .Add("@IsActive", SqlDbType.Bit)
                        .Value = IsActive;

                    connection.Open();

                    object result = command.ExecuteScalar();

                    if (result == null)
                        return -1;

                    return (int)result;
                }
            }
        }
        public static bool UpdatePasswordHash(
    int MobileUserID,
    string PasswordHash)
        {
            using (SqlConnection connection =
                new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                const string query =
                    @"UPDATE dbo.MobileUsers
              SET PasswordHash = @PasswordHash
              WHERE MobileUserID = @MobileUserID;";

                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters
                        .Add("@PasswordHash", SqlDbType.NVarChar, 512)
                        .Value = PasswordHash;

                    command.Parameters
                        .Add("@MobileUserID", SqlDbType.Int)
                        .Value = MobileUserID;

                    connection.Open();

                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
        }
        public static bool UpdateIsActive(
    int MobileUserID,
    bool IsActive)
        {
            using (SqlConnection connection =
                new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                const string query =
                    @"UPDATE dbo.MobileUsers
              SET IsActive = @IsActive
              WHERE MobileUserID = @MobileUserID;";

                using (SqlCommand command =
                    new SqlCommand(query, connection))
                {
                    command.Parameters
                        .Add("@IsActive", SqlDbType.Bit)
                        .Value = IsActive;

                    command.Parameters
                        .Add("@MobileUserID", SqlDbType.Int)
                        .Value = MobileUserID;

                    connection.Open();

                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
        }
    }
}