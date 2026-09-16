using DVLD_Buisness;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace DVLD.Api.Controllers
{
    [ApiController]
    [Route("api/people/{personId}/test-appointments")]
    public class TestAppointmentsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetPersonTestAppointments(int personId)
        {
            clsPerson person = clsPerson.Find(personId);

            if (person == null)
                return NotFound();

            DataTable appointments =
                clsTestAppointment.GetPersonTestAppointments(personId);

            var result = new List<object>();

            foreach (DataRow row in appointments.Rows)
            {
                result.Add(new
                {
                    TestAppointmentID =
                        Convert.ToInt32(row["TestAppointmentID"]),

                    TestTypeID =
                        Convert.ToInt32(row["TestTypeID"]),

                    TestTypeTitle =
                        Convert.ToString(row["TestTypeTitle"]),

                    LocalDrivingLicenseApplicationID =
                        Convert.ToInt32(
                            row["LocalDrivingLicenseApplicationID"]),

                    ApplicationID =
                        Convert.ToInt32(row["ApplicationID"]),

                    LicenseClassID =
                        Convert.ToInt32(row["LicenseClassID"]),

                    ClassName =
                        Convert.ToString(row["ClassName"]),

                    AppointmentDate =
                        Convert.ToDateTime(row["AppointmentDate"]),

                    PaidFees =
                        Convert.ToSingle(row["PaidFees"]),

                    IsLocked =
                        Convert.ToBoolean(row["IsLocked"]),

                    RetakeTestApplicationID =
                        row["RetakeTestApplicationID"] == DBNull.Value
                            ? (int?)null
                            : Convert.ToInt32(
                                row["RetakeTestApplicationID"]),

                    TestID =
                        row["TestID"] == DBNull.Value
                            ? (int?)null
                            : Convert.ToInt32(row["TestID"]),

                    TestResult =
                        row["TestResult"] == DBNull.Value
                            ? (bool?)null
                            : Convert.ToBoolean(row["TestResult"]),

                    Notes =
                        row["Notes"] == DBNull.Value
                            ? null
                            : Convert.ToString(row["Notes"])
                });
            }

            return Ok(result);
        }
        [HttpGet("/api/test-appointments/{testAppointmentId}")]
        public IActionResult GetTestAppointmentById(int testAppointmentId)
        {
            clsTestAppointment appointment =
                clsTestAppointment.Find(testAppointmentId);

            if (appointment == null)
                return NotFound();

            int testID = appointment.TestID;

            clsTest test =
                testID == -1
                    ? null
                    : clsTest.Find(testID);

            return Ok(new
            {
                TestAppointmentID =
                    appointment.TestAppointmentID,

                TestTypeID =
                    (int)appointment.TestTypeID,

                TestTypeName =
                    appointment.TestTypeID.ToString(),

                LocalDrivingLicenseApplicationID =
                    appointment.LocalDrivingLicenseApplicationID,

                AppointmentDate =
                    appointment.AppointmentDate,

                PaidFees =
                    appointment.PaidFees,

                IsLocked =
                    appointment.IsLocked,

                RetakeTestApplicationID =
                    appointment.RetakeTestApplicationID == -1
                        ? (int?)null
                        : appointment.RetakeTestApplicationID,

                Test = test == null
                    ? null
                    : new
                    {
                        TestID =
                            test.TestID,

                        TestResult =
                            test.TestResult,

                        ResultText =
                            test.TestResult
                                ? "Passed"
                                : "Failed",

                        Notes =
                            string.IsNullOrWhiteSpace(test.Notes)
                                ? null
                                : test.Notes
                    }
            });
        }
    }
}