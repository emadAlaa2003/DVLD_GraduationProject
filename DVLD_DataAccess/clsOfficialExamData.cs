using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DVLD_DataAccess
{
    // Read-only appointment facts. The Business layer will decide whether
    // the candidate may start an exam after an authorized employee confirms identity.
    public sealed class OfficialExamAppointmentInfo
    {
        public int TestAppointmentID { get; set; }
        public int PersonID { get; set; }
        public int LocalDrivingLicenseApplicationID { get; set; }
        public int TestTypeID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int ApplicationStatus { get; set; }
        public bool IsLocked { get; set; }
        public bool HasRecordedTest { get; set; }
        public bool HasOfficialAttempt { get; set; }
        public bool HasPassedVisionTest { get; set; }
    }

    public static class clsOfficialExamData
    {
        // The national number must belong to the person who owns this booking.
        // This method does NOT authenticate the candidate by itself.
        // An authorized employee must verify identity before starting an exam.
        // Employee-facing lookup by national number. Return written-test
        // appointments (TestTypeID = 2); eligibility is decided in Business.
        // Do not publish this lookup as an anonymous public API endpoint:
        // knowing a national number alone is not proof of identity.
        public static List<OfficialExamAppointmentInfo>
            GetWrittenAppointmentsByNationalNo(string nationalNo)
        {
            List<OfficialExamAppointmentInfo> appointments =
                new List<OfficialExamAppointmentInfo>();

            if (string.IsNullOrWhiteSpace(nationalNo))
                return appointments;

            const string query = @"
                SELECT
                    TA.TestAppointmentID,
                    A.ApplicantPersonID AS PersonID,
                    TA.LocalDrivingLicenseApplicationID,
                    TA.TestTypeID,
                    TA.AppointmentDate,
                    A.ApplicationStatus,
                    TA.IsLocked,
                    CAST(CASE WHEN EXISTS (
                        SELECT 1 FROM dbo.Tests AS T
                        WHERE T.TestAppointmentID = TA.TestAppointmentID
                    ) THEN 1 ELSE 0 END AS bit) AS HasRecordedTest,
                    CAST(CASE WHEN EXISTS (
                        SELECT 1 FROM dbo.OfficialExamAttempts AS EA
                        WHERE EA.TestAppointmentID = TA.TestAppointmentID
                    ) THEN 1 ELSE 0 END AS bit) AS HasOfficialAttempt,
                    CAST(CASE WHEN EXISTS (
                        SELECT 1
                        FROM dbo.TestAppointments AS VA
                        INNER JOIN dbo.Tests AS VT
                            ON VT.TestAppointmentID = VA.TestAppointmentID
                        WHERE VA.LocalDrivingLicenseApplicationID =
                              TA.LocalDrivingLicenseApplicationID
                          AND VA.TestTypeID = 1
                          AND VT.TestResult = 1
                    ) THEN 1 ELSE 0 END AS bit) AS HasPassedVisionTest
                FROM dbo.TestAppointments AS TA
                INNER JOIN dbo.LocalDrivingLicenseApplications AS LDLA
                    ON LDLA.LocalDrivingLicenseApplicationID =
                       TA.LocalDrivingLicenseApplicationID
                INNER JOIN dbo.Applications AS A
                    ON A.ApplicationID = LDLA.ApplicationID
                INNER JOIN dbo.People AS P
                    ON P.PersonID = A.ApplicantPersonID
                WHERE P.NationalNo = @NationalNo
                  AND TA.TestTypeID = 2
                ORDER BY TA.AppointmentDate DESC,
                         TA.TestAppointmentID DESC;";

            using (SqlConnection connection = new SqlConnection(
                clsDataAccessSettings.ConnectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@NationalNo", SqlDbType.NVarChar, 30)
                    .Value = nationalNo.Trim();

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        appointments.Add(new OfficialExamAppointmentInfo
                        {
                            TestAppointmentID = Convert.ToInt32(reader["TestAppointmentID"]),
                            PersonID = Convert.ToInt32(reader["PersonID"]),
                            LocalDrivingLicenseApplicationID = Convert.ToInt32(
                                reader["LocalDrivingLicenseApplicationID"]),
                            TestTypeID = Convert.ToInt32(reader["TestTypeID"]),
                            AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                            ApplicationStatus = Convert.ToInt32(reader["ApplicationStatus"]),
                            IsLocked = Convert.ToBoolean(reader["IsLocked"]),
                            HasRecordedTest = Convert.ToBoolean(reader["HasRecordedTest"]),
                            HasOfficialAttempt = Convert.ToBoolean(reader["HasOfficialAttempt"]),
                            HasPassedVisionTest = Convert.ToBoolean(reader["HasPassedVisionTest"])
                        });
                    }
                }
            }

            return appointments;
        }

        public static OfficialExamAppointmentInfo GetCandidateAppointment(
            int testAppointmentID,
            string nationalNo)
        {
            if (testAppointmentID <= 0 || string.IsNullOrWhiteSpace(nationalNo))
                return null;

            const string query = @"
                SELECT TOP (1)
                    TA.TestAppointmentID,
                    A.ApplicantPersonID AS PersonID,
                    TA.LocalDrivingLicenseApplicationID,
                    TA.TestTypeID,
                    TA.AppointmentDate,
                    A.ApplicationStatus,
                    TA.IsLocked,
                    CAST(CASE WHEN EXISTS (
                        SELECT 1 FROM dbo.Tests AS T
                        WHERE T.TestAppointmentID = TA.TestAppointmentID
                    ) THEN 1 ELSE 0 END AS bit) AS HasRecordedTest,
                    CAST(CASE WHEN EXISTS (
                        SELECT 1 FROM dbo.OfficialExamAttempts AS EA
                        WHERE EA.TestAppointmentID = TA.TestAppointmentID
                    ) THEN 1 ELSE 0 END AS bit) AS HasOfficialAttempt,
                    CAST(CASE WHEN EXISTS (
                        SELECT 1
                        FROM dbo.TestAppointments AS VA
                        INNER JOIN dbo.Tests AS VT
                            ON VT.TestAppointmentID = VA.TestAppointmentID
                        WHERE VA.LocalDrivingLicenseApplicationID =
                              TA.LocalDrivingLicenseApplicationID
                          AND VA.TestTypeID = 1
                          AND VT.TestResult = 1
                    ) THEN 1 ELSE 0 END AS bit) AS HasPassedVisionTest
                FROM dbo.TestAppointments AS TA
                INNER JOIN dbo.LocalDrivingLicenseApplications AS LDLA
                    ON LDLA.LocalDrivingLicenseApplicationID =
                       TA.LocalDrivingLicenseApplicationID
                INNER JOIN dbo.Applications AS A
                    ON A.ApplicationID = LDLA.ApplicationID
                INNER JOIN dbo.People AS P
                    ON P.PersonID = A.ApplicantPersonID
                WHERE TA.TestAppointmentID = @TestAppointmentID
                  AND P.NationalNo = @NationalNo;";

            using (SqlConnection connection = new SqlConnection(
                clsDataAccessSettings.ConnectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@TestAppointmentID", SqlDbType.Int)
                    .Value = testAppointmentID;
                command.Parameters.Add("@NationalNo", SqlDbType.NVarChar, 30)
                    .Value = nationalNo.Trim();

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(
                    CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                        return null;

                    return new OfficialExamAppointmentInfo
                    {
                        TestAppointmentID = Convert.ToInt32(reader["TestAppointmentID"]),
                        PersonID = Convert.ToInt32(reader["PersonID"]),
                        LocalDrivingLicenseApplicationID = Convert.ToInt32(
                            reader["LocalDrivingLicenseApplicationID"]),
                        TestTypeID = Convert.ToInt32(reader["TestTypeID"]),
                        AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]),
                        ApplicationStatus = Convert.ToInt32(reader["ApplicationStatus"]),
                        IsLocked = Convert.ToBoolean(reader["IsLocked"]),
                        HasRecordedTest = Convert.ToBoolean(reader["HasRecordedTest"]),
                        HasOfficialAttempt = Convert.ToBoolean(reader["HasOfficialAttempt"]),
                        HasPassedVisionTest = Convert.ToBoolean(reader["HasPassedVisionTest"])
                    };
                }
            }
        }
    }
}
