using System.Collections.Generic;
using System.Threading.Tasks;
using UkrgoParser.Shared.Models.Entities;

namespace UkrgoParser.Server.Interfaces
{
    public interface IContactService
    {
        Task<IEnumerable<Contact>> GetContactsAsync();

        Task EditContactAsync(Contact contact);
    }
}