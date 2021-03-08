using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UkrgoParser.Server.Models.Request;
using UkrgoParser.Server.Services;

namespace UkrgoParser.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhoneController : ControllerBase
    {
        private IPhoneService PhoneService { get; set; }

        public PhoneController(IPhoneService phoneService)
        {
            PhoneService = phoneService;
        }

        [HttpGet(nameof(CheckNumber))]
        public async Task<IActionResult> CheckNumber(string phoneNumber)
        {
            return Ok(await PhoneService.CheckNumberAsync(phoneNumber));
        }

        [HttpPost(nameof(AddNumber))]
        public async Task<IActionResult> AddNumber([FromBody] AddNumberRequestModel model)
        {
            await PhoneService.AddNumberAsync(model.PhoneNumber);
            return NoContent();
        }
    }
}