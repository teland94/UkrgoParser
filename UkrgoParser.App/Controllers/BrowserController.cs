using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UkrgoParser.BLL;

namespace UkrgoParser.App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrowserController : ControllerBase
    {
        private IBrowserService BrowserService { get; }

        public BrowserController(IBrowserService browserService)
        {
            BrowserService = browserService;
        }

        [HttpGet(nameof(GetPostLinks))]
        public async Task<IActionResult> GetPostLinks([FromQuery] string url)
        {
            return Ok(await BrowserService.GetPostLinksAsync(url));
        }

        [HttpGet(nameof(GetPhoneNumber))]
        public async Task<IActionResult> GetPhoneNumber([FromQuery] string postLink)
        {
            return Ok(await BrowserService.GetPhoneNumberAsync(postLink));
        }
    }
}