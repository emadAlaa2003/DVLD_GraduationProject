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
        // يرجع الرخص المحلية الفعالة فقط الخاصة بالمواطن
        [HttpGet]
        public IActionResult GetPersonLicenses(int personId)
        {
            clsPerson person = clsPerson.Find(personId);

            // الشخص غير موجود أصلاً
            if (person == null)
            {
                return NotFound();
            }

            clsDriver driver = clsDriver.FindByPersonID(personId);

            // الشخص موجود لكنه ليس Driver بعد
            if (driver == null)
            {
                return Ok(new List<object>());
            }

            DataTable licenses = clsDriver.GetLicenses(driver.DriverID);

            var result = new List<object>();

            foreach (DataRow row in licenses.Rows)
            {
                // الموبايل يعرض الرخص الفعالة فقط
                if (!Convert.ToBoolean(row["IsActive"]))
                {
                    continue;
                }

                result.Add(new
                {
                    LicenseID = Convert.ToInt32(row["LicenseID"]),
                    ApplicationID = Convert.ToInt32(row["ApplicationID"]),
                    ClassName = Convert.ToString(row["ClassName"]),
                    IssueDate = Convert.ToDateTime(row["IssueDate"]),
                    ExpirationDate = Convert.ToDateTime(row["ExpirationDate"]),
                    IsActive = Convert.ToBoolean(row["IsActive"])
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