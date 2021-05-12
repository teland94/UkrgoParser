using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UkrgoParser.Server.Filters;
using UkrgoParser.Server.Interfaces;
using UkrgoParser.Server.Services;

namespace UkrgoParser.Server.Controllers
{
    [ServiceFilter(typeof(DelayFilter))]
    [Route("api/[controller]")]
    [ApiController]
    public class BrowserController : ControllerBase
    {
        private const int CacheAgeSeconds = 60 * 60 * 24 * 30; // 30 days

        private IBrowserService BrowserService { get; }

        public BrowserController(IBrowserService browserService)
        {
            BrowserService = browserService;
        }

        [HttpGet(nameof(GetPostLinks))]
        public async Task<IActionResult> GetPostLinks([FromQuery] Uri uri)
        {
            try
            {
                return Ok(await BrowserService.GetPostLinksAsync(uri));
            }
            catch (HttpRequestException e)
            {
                return e.StatusCode != null ? StatusCode((int)e.StatusCode) : BadRequest();
            }
        }

        [HttpGet(nameof(GetPhoneNumber))]
        public async Task<IActionResult> GetPhoneNumber([FromQuery] Uri postLinkUri)
        {
            try
            {
                return Ok(await BrowserService.GetPhoneNumberAsync(postLinkUri));
            }
            catch (HttpRequestException e)
            {
                return e.StatusCode != null ? StatusCode((int)e.StatusCode) : BadRequest();
            }
        }

        [HttpGet(nameof(GetPostDetails))]
        public async Task<IActionResult> GetPostDetails([FromQuery] Uri postLinkUri)
        {
            try
            {
                return Ok(await BrowserService.GetPostDetails(postLinkUri));
            }
            catch (HttpRequestException e)
            {
                return e.StatusCode != null ? StatusCode((int)e.StatusCode) : BadRequest();
            }
        }

        [HttpGet(nameof(GetImage))]
        public async Task<IActionResult> GetImage([FromQuery] Uri imageUri, [FromQuery] bool cropUnwantedBackground = false)
        {
            try
            {
                Response.Headers["Cache-Control"] = $"public,max-age={CacheAgeSeconds}";

                return File(await BrowserService.GetImage(imageUri, cropUnwantedBackground), "image/jpeg");
            }
            catch (HttpRequestException e)
            {
                return e.StatusCode != null ? StatusCode((int)e.StatusCode) : BadRequest();
            }
        }
    }
}