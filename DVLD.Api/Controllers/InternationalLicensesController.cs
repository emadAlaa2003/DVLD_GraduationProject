using System;
using System.Collections.Generic;
using System.Data;
using DVLD_Buisness;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers
{
    [ApiController]
    [Route("api/people/{personId}/international-licenses")]
    public class InternationalLicensesController : ControllerBase
    {
        // GET: /api/people/{personId}/international-licenses
        // يرجع الرخص الدولية الفعالة وغير المنتهية فقط
        [HttpGet]
        public IActionResult GetPersonInternationalLicenses(int personId)
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

            DataTable licenses =
                clsDriver.GetInternationalLicenses(driver.DriverID);

            var result = new List<object>();

            foreach (DataRow row in licenses.Rows)
            {
                bool isActive = Convert.ToBoolean(row["IsActive"]);
                DateTime expirationDate =
                    Convert.ToDateTime(row["ExpirationDate"]);

                // نعرض للموبايل فقط الرخص الدولية
                // الفعالة وغير المنتهية
                if (!isActive || expirationDate < DateTime.Now)
                {
                    continue;
                }

                result.Add(new
                {
                    InternationalLicenseID =
                        Convert.ToInt32(row["InternationalLicenseID"]),

                    ApplicationID =
                        Convert.ToInt32(row["ApplicationID"]),

                    IssuedUsingLocalLicenseID =
                        Convert.ToInt32(row["IssuedUsingLocalLicenseID"]),

                    IssueDate =
                        Convert.ToDateTime(row["IssueDate"]),

                    ExpirationDate =
                        expirationDate,

                    IsActive =
                        isActive
                });
            }

            return Ok(result);
        }

        // GET: /api/international-licenses/{internationalLicenseId}
        // يرجع تفاصيل رخصة دولية محددة
        [HttpGet("/api/international-licenses/{internationalLicenseId}")]
        public IActionResult GetInternationalLicenseById(int internationalLicenseId)
        {
            clsInternationalLicense license =
                clsInternationalLicense.Find(internationalLicenseId);

            if (license == null)
            {
                return NotFound();
            }

            bool isExpired = license.ExpirationDate < DateTime.Now;
            bool isCurrentlyValid = license.IsActive && !isExpired;

            return Ok(new
            {
                InternationalLicenseID =
                    license.InternationalLicenseID,

                ApplicationID =
                    license.ApplicationID,

                PersonID =
                    license.DriverInfo?.PersonID,

                FullName =
                    license.DriverInfo?.PersonInfo?.FullName,

                NationalNo =
                    license.DriverInfo?.PersonInfo?.NationalNo,

                DriverID =
                    license.DriverID,

                IssuedUsingLocalLicenseID =
                    license.IssuedUsingLocalLicenseID,

                IssueDate =
                    license.IssueDate,

                ExpirationDate =
                    license.ExpirationDate,

                IsActive =
                    license.IsActive,

                IsExpired =
                    isExpired,

                IsCurrentlyValid =
                    isCurrentlyValid,

                ApplicationDate =
                    license.ApplicationDate,

                ApplicationStatus =
                    (int)license.ApplicationStatus,

                PaidFees =
                    license.PaidFees
            });
        }
    }
}