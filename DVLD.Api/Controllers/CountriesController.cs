using System.Data;
using DVLD_Buisness;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Controllers
{
    [ApiController]
    [Route("api/countries")]
    public class CountriesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllCountries()
        {
            DataTable countries = clsCountry.GetAllCountries();

            var result = new List<object>();

            foreach (DataRow row in countries.Rows)
            {
                result.Add(new
                {
                    CountryID = Convert.ToInt32(row["CountryID"]),
                    CountryName = Convert.ToString(row["CountryName"])
                });
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetCountryById(int id)
        {
            clsCountry country = clsCountry.Find(id);

            if (country == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                CountryID = country.ID,
                CountryName = country.CountryName
            });
        }
    }
}