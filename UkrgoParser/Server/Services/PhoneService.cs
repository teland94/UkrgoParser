using System.Linq;
using System.Threading.Tasks;
using UkrgoParser.Server.Helpers;

namespace UkrgoParser.Server.Services
{
    public interface IPhoneService
    {
        Task<bool> CheckNumberAsync(string phoneNumber);

        Task AddNumberAsync(string phoneNumber);

        Task<string> GetPhoneTitleAsync(string phoneNumber);
    }

    public class PhoneService : IPhoneService
    {
        public async Task<bool> CheckNumberAsync(string phoneNumber)
        {
            var lines = await FileHelper.ReadAllLinesAsync("black-list.txt");
            return !lines.Contains(phoneNumber);
        }

        public async Task AddNumberAsync(string phoneNumber)
        {
            var lines = (await FileHelper.ReadAllLinesAsync("black-list.txt")).ToList();

            if (lines.Contains(phoneNumber)) { return; }

            lines.Add(phoneNumber);

            await FileHelper.WriteAllLinesAsync("black-list.txt", lines);
        }

        public async Task<string> GetPhoneTitleAsync(string phoneNumber)
        {
            var lines = await FileHelper.ReadAllLinesAsync("named-list.txt");
            var title = lines.FirstOrDefault(line => line.Contains(phoneNumber));
            return title ?? phoneNumber;
        }
    }
}
