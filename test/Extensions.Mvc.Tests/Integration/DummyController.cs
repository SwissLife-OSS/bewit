using Bewit.Mvc.Filter;
using Microsoft.AspNetCore.Mvc;

namespace Bewit.Extensions.Mvc.Tests.Integration
{
    [Route("api/[controller]")]
    [ApiController]
    public class DummyController : Controller
    {
        [HttpGet("NoBewitProtection")]
        public ActionResult NoBewitProtection()
        {
            return Content("bar");
        }

        [HttpGet("WithBewitProtection")]
        [BewitUrlAuthorization]
        public ActionResult WithBewitProtection()
        {
            return Content("bar");
        }

        [HttpGet("WithBewitParameters/{id}")]
        [Bewit]
        public ActionResult WithBewitParameters(
            string id,
            [FromBewit] string firstName,
            [FromBewit] string lastName)
        {
            return Content($"{id}: {firstName} {lastName}");
        }
    }
}
