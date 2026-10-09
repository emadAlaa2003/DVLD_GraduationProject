using System;

namespace DVLD_DataAccess
{
    static class clsDataAccessSettings
    {
        public static string ConnectionString
        {
            get
            {
                string connectionString =
                    Environment.GetEnvironmentVariable("DVLD_CONNECTION_STRING");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    connectionString =
                        Environment.GetEnvironmentVariable(
                            "DVLD_CONNECTION_STRING",
                            EnvironmentVariableTarget.User);
                }

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "DVLD_CONNECTION_STRING environment variable is not configured.");
                }

                return connectionString;
            }
        }
    }
}