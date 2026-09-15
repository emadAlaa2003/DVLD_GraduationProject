using System.Data;
using DVLD_Buisness;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers
{
    [ApiController]
    [Route("api/people")]
    public class PeopleController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllPeople()
        {
            DataTable people = clsPerson.GetAllPeople();

            var result = new List<object>();

            foreach (DataRow row in people.Rows)
            {
                result.Add(new
                {
                    PersonID = Convert.ToInt32(row["PersonID"]),
                    NationalNo = Convert.ToString(row["NationalNo"]),
                    FirstName = Convert.ToString(row["FirstName"]),
                    SecondName = Convert.ToString(row["SecondName"]),
                    ThirdName = Convert.ToString(row["ThirdName"]),
                    LastName = Convert.ToString(row["LastName"]),
                    DateOfBirth = Convert.ToDateTime(row["DateOfBirth"]),
                    Gendor = Convert.ToInt16(row["Gendor"]),
                    GendorCaption = Convert.ToString(row["GendorCaption"]),
                    Address = Convert.ToString(row["Address"]),
                    Phone = Convert.ToString(row["Phone"]),
                    Email = Convert.ToString(row["Email"]),
                    NationalityCountryID = Convert.ToInt32(row["NationalityCountryID"]),
                    CountryName = Convert.ToString(row["CountryName"]),
                    ImagePath = Convert.ToString(row["ImagePath"])
                });
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetPersonById(int id)
        {
            clsPerson person = clsPerson.Find(id);

            if (person == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                PersonID = person.PersonID,
                NationalNo = person.NationalNo,
                FirstName = person.FirstName,
                SecondName = person.SecondName,
                ThirdName = person.ThirdName,
                LastName = person.LastName,
                FullName = person.FullName,
                DateOfBirth = person.DateOfBirth,
                Gendor = person.Gendor,
                Address = person.Address,
                Phone = person.Phone,
                Email = person.Email,
                NationalityCountryID = person.NationalityCountryID,
                CountryName = person.CountryInfo?.CountryName,
                ImagePath = person.ImagePath
            });
        }
        [HttpGet("national/{nationalNo}")]
        public IActionResult GetPersonByNationalNo(string nationalNo)
        {
            clsPerson person = clsPerson.Find(nationalNo);

            if (person == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                PersonID = person.PersonID,
                NationalNo = person.NationalNo,
                FirstName = person.FirstName,
                SecondName = person.SecondName,
                ThirdName = person.ThirdName,
                LastName = person.LastName,
                FullName = person.FullName,
                DateOfBirth = person.DateOfBirth,
                Gendor = person.Gendor,
                Address = person.Address,
                Phone = person.Phone,
                Email = person.Email,
                NationalityCountryID = person.NationalityCountryID,
                CountryName = person.CountryInfo?.CountryName,
                ImagePath = person.ImagePath
            });
        }
    }
}