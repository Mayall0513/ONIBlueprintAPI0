using Microsoft.AspNetCore.Mvc;

namespace BlueprintRepository.Controllers.AWS {
    [ApiController]
    [Route("aws/[controller]")]
    public class HealthCheckController : Controller {
        public HealthCheckController() {  }

        [HttpGet]
        public ActionResult Get() {
            return Ok();
        }
    }
}
