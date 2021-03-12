using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using UkrgoParser.Server.Services;
using UkrgoParser.Shared.Models.Entities;

namespace UkrgoParser.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private IContactService ContactService { get; }

        public ContactController(IContactService contactService)
        {
            ContactService = contactService;
        }

        [HttpGet]
        public async Task<IEnumerable<Contact>> Get()
        {
            return await ContactService.GetContactsAsync();
        }

        [HttpPost]
        public async Task Post([FromBody] Contact contact)
        {
            await ContactService.AddContactAsync(contact);
        }
    }
}
