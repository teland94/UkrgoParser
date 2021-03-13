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

        [HttpGet]
        public async Task<IActionResult> GetPhoneNumbers()
        {
            return Ok(await BlacklistService.GetPhoneNumbersAsync());
        }

        [HttpGet(nameof(CheckNumber))]
        public async Task<IActionResult> CheckNumber(string phoneNumber)
        {
            return Ok(await BlacklistService.CheckNumberAsync(phoneNumber));
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoneNumber([FromBody] BlockNumberRequestModel model)
        {
            await BlacklistService.AddPhoneNumberAsync(model.PhoneNumber);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePhoneNumber(string phoneNumber)
        {
            await BlacklistService.DeletePhoneNumberAsync(phoneNumber);
            return NoContent();
        }
    }
}