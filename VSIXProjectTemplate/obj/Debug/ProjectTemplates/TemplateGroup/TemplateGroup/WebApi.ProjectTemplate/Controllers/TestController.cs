using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using $safeprojectname$.BL;

namespace $safeprojectname$.Controllers
{
    /// <summary>
    /// Тестовый контроллер
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ITestLogic _testLogic;
        /// <summary>
        /// Констурктор тестового контроллера.
        /// </summary>
        /// <param name="testRepository"></param>
        public TestController(ITestLogic testLogic)
        {
            _testLogic = testLogic;
        }
        /// <summary>
        /// Тестовый метод
        /// </summary>
        /// <returns>Response 200</returns>
        [HttpGet]
        public ActionResult Test()
        {
            var result = _testLogic.DataFromTest();
            return Ok(result);
        }
    }
}
