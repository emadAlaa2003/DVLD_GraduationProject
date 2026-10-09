using System;
using System.Collections.Generic;
using DVLD_DataAccess;

namespace DVLD_Buisness
{
    // Preliminary eligibility ONLY. This class does not authenticate candidates,
    // authorize an employee, or start an exam.
    public static class clsOfficialExam
    {
        public enum enPrecheckStatus
        {
            EligibleForEmployeeVerification = 0,
            AppointmentNotFound = 1,
            NotWrittenTest = 2,
            ApplicationNotActive = 3,
            AppointmentLocked = 4,
            TestAlreadyRecorded = 5,
            OfficialAttemptAlreadyExists = 6,
            VisionTestNotPassed = 7
        }

        public sealed class PrecheckResult
        {
            public int TestAppointmentID { get; internal set; }
            public int LocalDrivingLicenseApplicationID { get; internal set; }
            public DateTime? AppointmentDate { get; internal set; }
            public enPrecheckStatus Status { get; internal set; }

            public bool IsEligibleForEmployeeVerification
            {
                get
                {
                    return Status ==
                        enPrecheckStatus.EligibleForEmployeeVerification;
                }
            }
        }

        // For the employee's appointment lookup screen. Never expose this
        // national-number lookup through an anonymous/public API endpoint.
        public static List<PrecheckResult> GetWrittenAppointments(
            string nationalNo)
        {
            List<PrecheckResult> results = new List<PrecheckResult>();

            if (string.IsNullOrWhiteSpace(nationalNo))
                return results;

            List<OfficialExamAppointmentInfo> appointments =
                clsOfficialExamData.GetWrittenAppointmentsByNationalNo(
                    nationalNo.Trim());

            foreach (OfficialExamAppointmentInfo appointment in appointments)
                results.Add(Evaluate(appointment));

            return results;
        }

        // Precheck a specific appointment belonging to the supplied person.
        // This MUST be followed by employee identity verification, an
        // appointment-time check, and an atomic eligibility recheck at Start.
        public static PrecheckResult CheckAppointment(
            int testAppointmentID,
            string nationalNo)
        {
            if (testAppointmentID <= 0 ||
                string.IsNullOrWhiteSpace(nationalNo))
            {
                return new PrecheckResult
                {
                    TestAppointmentID = testAppointmentID,
                    Status = enPrecheckStatus.AppointmentNotFound
                };
            }

            OfficialExamAppointmentInfo appointment =
                clsOfficialExamData.GetCandidateAppointment(
                    testAppointmentID,
                    nationalNo.Trim());

            return Evaluate(appointment);
        }

        private static PrecheckResult Evaluate(
            OfficialExamAppointmentInfo appointment)
        {
            if (appointment == null)
            {
                return new PrecheckResult
                {
                    Status = enPrecheckStatus.AppointmentNotFound
                };
            }

            PrecheckResult result = new PrecheckResult
            {
                TestAppointmentID = appointment.TestAppointmentID,
                LocalDrivingLicenseApplicationID =
                    appointment.LocalDrivingLicenseApplicationID,
                AppointmentDate = appointment.AppointmentDate,
                Status = enPrecheckStatus.EligibleForEmployeeVerification
            };

            // TestTypeID = 2 (WrittenTest) in the existing DVLD system.
            if (appointment.TestTypeID != 2)
                result.Status = enPrecheckStatus.NotWrittenTest;
            // ApplicationStatus = 1 (New) in clsApplication.
            else if (appointment.ApplicationStatus != 1)
                result.Status = enPrecheckStatus.ApplicationNotActive;
            else if (appointment.IsLocked)
                result.Status = enPrecheckStatus.AppointmentLocked;
            else if (appointment.HasRecordedTest)
                result.Status = enPrecheckStatus.TestAlreadyRecorded;
            else if (appointment.HasOfficialAttempt)
                result.Status = enPrecheckStatus.OfficialAttemptAlreadyExists;
            else if (!appointment.HasPassedVisionTest)
                result.Status = enPrecheckStatus.VisionTestNotPassed;

            return result;
        }
    }
}
