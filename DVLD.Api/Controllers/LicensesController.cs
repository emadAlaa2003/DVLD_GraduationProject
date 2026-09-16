using System;
using System.Collections.Generic;
using System.Data;
using DVLD_Buisness;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers
{
    [ApiController]
    [Route("api/people/{personId}/licenses")]
    public class LicensesController : ControllerBase
    {
        // GET: /api/people/{personId}/licenses
        // يرجع الرخص المحلية الفعالة وغير المنتهية فقط
        [HttpGet]
        public IActionResult GetPersonLicenses(int personId)
        {
            clsPerson person = clsPerson.Find(personId);

            if (person == null)
            {
                return NotFound();
            }

            clsDriver driver = clsDriver.FindByPersonID(personId);

            if (driver == null)
            {
                return Ok(new List<object>());
            }

            DataTable licenses = clsDriver.GetLicenses(driver.DriverID);

            var result = new List<object>();

            foreach (DataRow row in licenses.Rows)
            {
                int licenseID =
                    Convert.ToInt32(row["LicenseID"]);

                bool isActive =
                    Convert.ToBoolean(row["IsActive"]);

                DateTime expirationDate =
                    Convert.ToDateTime(row["ExpirationDate"]);

                // نعرض فقط الرخص الفعالة وغير المنتهية
                if (!isActive || expirationDate < DateTime.Now)
                {
                    continue;
                }

                bool isDetained =
                    clsDetainedLicense.IsLicenseDetained(licenseID);

                result.Add(new
                {
                    LicenseID = licenseID,

                    ApplicationID =
                        Convert.ToInt32(row["ApplicationID"]),

                    ClassName =
                        Convert.ToString(row["ClassName"]),

                    IssueDate =
                        Convert.ToDateTime(row["IssueDate"]),

                    ExpirationDate =
                        expirationDate,

                    IsActive =
                        isActive,

                    IsDetained =
                        isDetained
                });
            }

            return Ok(result);
        }

        // GET: /api/licenses/{licenseId}
        // يرجع التفاصيل الكاملة لرخصة محددة
        [HttpGet("/api/licenses/{licenseId}")]
        public IActionResult GetLicenseById(int licenseId)
        {
            clsLicense license = clsLicense.Find(licenseId);

            if (license == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                LicenseID = license.LicenseID,
                ApplicationID = license.ApplicationID,

                PersonID = license.DriverInfo?.PersonID,
                FullName = license.DriverInfo?.PersonInfo?.FullName,
                NationalNo = license.DriverInfo?.PersonInfo?.NationalNo,

                LicenseClassID = license.LicenseClass,
                ClassName = license.LicenseClassIfo?.ClassName,
                ClassDescription = license.LicenseClassIfo?.ClassDescription,

                IssueDate = license.IssueDate,
                ExpirationDate = license.ExpirationDate,

                Notes = license.Notes,
                PaidFees = license.PaidFees,

                IsActive = license.IsActive,
                IsExpired = license.IsLicenseExpired(),

                IssueReason = (int)license.IssueReason,
                IssueReasonText = license.IssueReasonText,

                IsDetained = license.IsDetained
            });
        }
    }
}