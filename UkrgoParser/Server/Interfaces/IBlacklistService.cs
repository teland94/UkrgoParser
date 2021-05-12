using System.Collections.Generic;
using System.Threading.Tasks;

namespace UkrgoParser.Server.Interfaces
{
    public interface IBlacklistService
    {
        Task<IEnumerable<string>> GetPhoneNumbersAsync();

        Task<bool> CheckNumberAsync(string phoneNumber);

        Task AddPhoneNumberAsync(string phoneNumber);

        Task DeletePhoneNumberAsync(string phoneNumber);
    }
}