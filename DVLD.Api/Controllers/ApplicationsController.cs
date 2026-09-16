using System;
using System.Collections.Generic;
using System.Data;
using DVLD_Buisness;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers
{
    [ApiController]
    [Route("api/people/{personId}/applications")]
    public class ApplicationsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetPersonApplications(int personId)
        {
            clsPerson person = clsPerson.Find(personId);

            if (person == null)
                return NotFound();

            DataTable applications =
                clsApplication.GetPersonApplications(personId);

            var result = new List<object>();

            foreach (DataRow row in applications.Rows)
            {
                int applicationTypeID =
                    Convert.ToInt32(row["ApplicationTypeID"]);

                int applicationStatus =
                    Convert.ToInt32(row["ApplicationStatus"]);

                result.Add(new
                {
                    ApplicationID =
                        Convert.ToInt32(row["ApplicationID"]),

                    ApplicantPersonID =
                        Convert.ToInt32(row["ApplicantPersonID"]),

                    ApplicationDate =
                        Convert.ToDateTime(row["ApplicationDate"]),

                    ApplicationTypeID =
                        applicationTypeID,

                    ApplicationTypeName =
                        Enum.IsDefined(
                            typeof(clsApplication.enApplicationType),
                            applicationTypeID)
                            ? ((clsApplication.enApplicationType)applicationTypeID).ToString()
                            : "Unknown",

                    ApplicationStatus =
                        applicationStatus,

                    StatusText =
                        GetStatusText(applicationStatus),

                    LastStatusDate =
                        Convert.ToDateTime(row["LastStatusDate"]),

                    PaidFees =
                        Convert.ToSingle(row["PaidFees"]),

                    LocalDrivingLicenseApplicationID =
                        row["LocalDrivingLicenseApplicationID"] == DBNull.Value
                            ? (int?)null
                            : Convert.ToInt32(
                                row["LocalDrivingLicenseApplicationID"]),

                    LicenseClassID =
                        row["LicenseClassID"] == DBNull.Value
                            ? (int?)null
                            : Convert.ToInt32(row["LicenseClassID"]),

                    ClassName =
                        row["ClassName"] == DBNull.Value
                            ? null
                            : Convert.ToString(row["ClassName"])
                });
            }

            return Ok(result);
        }

        [HttpGet("/api/applications/{applicationId}")]
        public IActionResult GetApplicationById(int applicationId)
        {
            clsApplication application =
                clsApplication.FindBaseApplication(applicationId);

            if (application == null)
                return NotFound();

            string applicationTypeName =
                Enum.IsDefined(
                    typeof(clsApplication.enApplicationType),
                    application.ApplicationTypeID)
                    ? ((clsApplication.enApplicationType)
                        application.ApplicationTypeID).ToString()
                    : "Unknown";

            // New Local Driving License
            if (application.ApplicationTypeID ==
                (int)clsApplication.enApplicationType.NewDrivingLicense)
            {
                clsLocalDrivingLicenseApplication localApplication =
                    clsLocalDrivingLicenseApplication.FindByApplicationID(applicationId);

                return Ok(new
                {
                    ApplicationID = application.ApplicationID,
                    ApplicantPersonID = application.ApplicantPersonID,
                    ApplicationDate = application.ApplicationDate,
                    ApplicationTypeID = application.ApplicationTypeID,
                    ApplicationTypeName = applicationTypeName,
                    ApplicationStatus = (int)application.ApplicationStatus,
                    StatusText = application.StatusText,
                    LastStatusDate = application.LastStatusDate,
                    PaidFees = application.PaidFees,

                    Details = localApplication == null
                        ? null
                        : new
                        {
                            LocalDrivingLicenseApplicationID =
                                localApplication.LocalDrivingLicenseApplicationID,

                            LicenseClassID =
                                localApplication.LicenseClassID,

                            ClassName =
                                localApplication.LicenseClassInfo?.ClassName
                        }
                });
            }

            // Renew, Lost Replacement, or Damaged Replacement
            if (application.ApplicationTypeID ==
                    (int)clsApplication.enApplicationType.RenewDrivingLicense
                ||
                application.ApplicationTypeID ==
                    (int)clsApplication.enApplicationType.ReplaceLostDrivingLicense
                ||
                application.ApplicationTypeID ==
                    (int)clsApplication.enApplicationType.ReplaceDamagedDrivingLicense)
            {
                int licenseID =
                    clsLicense.GetLicenseIDByApplicationID(applicationId);

                clsLicense license =
                    licenseID == -1
                        ? null
                        : clsLicense.Find(licenseID);

                return Ok(new
                {
                    ApplicationID = application.ApplicationID,
                    ApplicantPersonID = application.ApplicantPersonID,
                    ApplicationDate = application.ApplicationDate,
                    ApplicationTypeID = application.ApplicationTypeID,
                    ApplicationTypeName = applicationTypeName,
                    ApplicationStatus = (int)application.ApplicationStatus,
                    StatusText = application.StatusText,
                    LastStatusDate = application.LastStatusDate,
                    PaidFees = application.PaidFees,

                    Details = license == null
                        ? null
                        : new
                        {
                            LicenseID = license.LicenseID,
                            LicenseClassID = license.LicenseClass,
                            ClassName = license.LicenseClassIfo?.ClassName,
                            IssueDate = license.IssueDate,
                            ExpirationDate = license.ExpirationDate,
                            IsActive = license.IsActive,
                            IssueReason = (int)license.IssueReason,
                            IssueReasonText = license.IssueReasonText
                        }
                });
            }

            // New International License
            if (application.ApplicationTypeID ==
                (int)clsApplication.enApplicationType.NewInternationalLicense)
            {
                int internationalLicenseID =
                    clsInternationalLicense.GetInternationalLicenseIDByApplicationID(
                        applicationId);

                clsInternationalLicense internationalLicense =
                    internationalLicenseID == -1
                        ? null
                        : clsInternationalLicense.Find(internationalLicenseID);

                return Ok(new
                {
                    ApplicationID = application.ApplicationID,
                    ApplicantPersonID = application.ApplicantPersonID,
                    ApplicationDate = application.ApplicationDate,
                    ApplicationTypeID = application.ApplicationTypeID,
                    ApplicationTypeName = applicationTypeName,
                    ApplicationStatus = (int)application.ApplicationStatus,
                    StatusText = application.StatusText,
                    LastStatusDate = application.LastStatusDate,
                    PaidFees = application.PaidFees,

                    Details = internationalLicense == null
                        ? null
                        : new
                        {
                            InternationalLicenseID =
                                internationalLicense.InternationalLicenseID,

                            DriverID =
                                internationalLicense.DriverID,

                            IssuedUsingLocalLicenseID =
                                internationalLicense.IssuedUsingLocalLicenseID,

                            IssueDate =
                                internationalLicense.IssueDate,

                            ExpirationDate =
                                internationalLicense.ExpirationDate,

                            IsActive =
                                internationalLicense.IsActive,

                            IsExpired =
                                internationalLicense.ExpirationDate < DateTime.Now,

                            IsCurrentlyValid =
                                internationalLicense.IsActive &&
                                internationalLicense.ExpirationDate >= DateTime.Now
                        }
                });
            }

            // Release Detained Driving License
            if (application.ApplicationTypeID ==
                (int)clsApplication.enApplicationType.ReleaseDetainedDrivingLicsense)
            {
                int detainID =
                    clsDetainedLicense.GetDetainIDByReleaseApplicationID(
                        applicationId);

                clsDetainedLicense detainedLicense =
                    detainID == -1
                        ? null
                        : clsDetainedLicense.Find(detainID);

                return Ok(new
                {
                    ApplicationID = application.ApplicationID,
                    ApplicantPersonID = application.ApplicantPersonID,
                    ApplicationDate = application.ApplicationDate,
                    ApplicationTypeID = application.ApplicationTypeID,
                    ApplicationTypeName = applicationTypeName,
                    ApplicationStatus = (int)application.ApplicationStatus,
                    StatusText = application.StatusText,
                    LastStatusDate = application.LastStatusDate,
                    PaidFees = application.PaidFees,

                    Details = detainedLicense == null
                        ? null
                        : new
                        {
                            DetainID = detainedLicense.DetainID,
                            LicenseID = detainedLicense.LicenseID,
                            DetainDate = detainedLicense.DetainDate,
                            FineFees = detainedLicense.FineFees,
                            IsReleased = detainedLicense.IsReleased,

                            ReleaseDate =
                                detainedLicense.IsReleased
                                    ? detainedLicense.ReleaseDate
                                    : (DateTime?)null,

                            ReleaseApplicationID =
                                detainedLicense.ReleaseApplicationID
                        }
                });
            }

            // Retake Test
            if (application.ApplicationTypeID ==
                (int)clsApplication.enApplicationType.RetakeTest)
            {
                int testAppointmentID =
                    clsTestAppointment.GetTestAppointmentIDByRetakeApplicationID(
                        applicationId);

                clsTestAppointment testAppointment =
                    testAppointmentID == -1
                        ? null
                        : clsTestAppointment.Find(testAppointmentID);

                return Ok(new
                {
                    ApplicationID = application.ApplicationID,
                    ApplicantPersonID = application.ApplicantPersonID,
                    ApplicationDate = application.ApplicationDate,
                    ApplicationTypeID = application.ApplicationTypeID,
                    ApplicationTypeName = applicationTypeName,
                    ApplicationStatus = (int)application.ApplicationStatus,
                    StatusText = application.StatusText,
                    LastStatusDate = application.LastStatusDate,
                    PaidFees = application.PaidFees,

                    Details = testAppointment == null
                        ? null
                        : new
                        {
                            TestAppointmentID =
                                testAppointment.TestAppointmentID,

                            TestTypeID =
                                (int)testAppointment.TestTypeID,

                            TestTypeName =
                                testAppointment.TestTypeID.ToString(),

                            LocalDrivingLicenseApplicationID =
                                testAppointment.LocalDrivingLicenseApplicationID,

                            AppointmentDate =
                                testAppointment.AppointmentDate,

                            AppointmentPaidFees =
                                testAppointment.PaidFees,

                            IsLocked =
                                testAppointment.IsLocked,

                            TestID =
                                testAppointment.TestID
                        }
                });
            }

            // Other application types for now
            return Ok(new
            {
                ApplicationID = application.ApplicationID,
                ApplicantPersonID = application.ApplicantPersonID,
                ApplicationDate = application.ApplicationDate,
                ApplicationTypeID = application.ApplicationTypeID,
                ApplicationTypeName = applicationTypeName,
                ApplicationStatus = (int)application.ApplicationStatus,
                StatusText = application.StatusText,
                LastStatusDate = application.LastStatusDate,
                PaidFees = application.PaidFees
            });
        }

        private static string GetStatusText(int status)
        {
            return status switch
            {
                1 => "New",
                2 => "Cancelled",
                3 => "Completed",
                _ => "Unknown"
            };
        }
    }
}
