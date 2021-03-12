using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UkrgoParser.Server.Services;
using UkrgoParser.Shared.Models.Request;

namespace UkrgoParser.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlacklistController : ControllerBase
    {
        private IBlacklistService BlacklistService { get; }

        public BlacklistController(IBlacklistService phoneService)
        {
            BlacklistService = phoneService;
        }

        [HttpGet(nameof(CheckNumber))]
        public async Task<IActionResult> CheckNumber(string phoneNumber)
        {
            return Ok(await BlacklistService.CheckNumberAsync(phoneNumber));
        }

        [HttpPost(nameof(AddPhoneNumber))]
        public async Task<IActionResult> AddPhoneNumber([FromBody] BlockNumberRequestModel model)
        {
            await BlacklistService.AddPhoneNumberAsync(model.PhoneNumber);
            return NoContent();
        }
    }
}