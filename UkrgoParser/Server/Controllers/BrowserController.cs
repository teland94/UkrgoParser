using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UkrgoParser.Server.Services;

namespace UkrgoParser.Server.Controllers
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
            try
            {
                return Ok(await BrowserService.GetPostLinksAsync(url));
            }
            catch (HttpRequestException e)
            {
                return e.StatusCode != null ? StatusCode((int)e.StatusCode) : BadRequest();
            }
        }

        [HttpGet(nameof(GetPhoneNumber))]
        public async Task<IActionResult> GetPhoneNumber([FromQuery] string postLink)
        {
            try
            {
                return Ok(await BrowserService.GetPhoneNumberAsync(postLink));
            }
            catch (HttpRequestException e)
            {
                return e.StatusCode != null ? StatusCode((int)e.StatusCode) : BadRequest();
            }
        }

        [HttpGet(nameof(GetPostDetails))]
        public async Task<IActionResult> GetPostDetails([FromQuery] string postLink)
        {
            try
            {
                return Ok(await BrowserService.GetPostDetails(postLink));
            }
            catch (HttpRequestException e)
            {
                return e.StatusCode != null ? StatusCode((int)e.StatusCode) : BadRequest();
            }
        }
    }
}