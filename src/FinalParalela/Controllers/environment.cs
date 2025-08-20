using Microsoft.AspNetCore.Mvc;

namespace FinalParalela.Controllers
{
    [ApiController]
    [Route("api/cores")]
    public class environmentController : Controller
    {
        [HttpGet()]
        public IActionResult ComputerCores()
        {
            //retornaremos simplemente los cores disponibles
            int cores = Environment.ProcessorCount;
            return Ok( new {systemCores = cores});
        }
    }
}
