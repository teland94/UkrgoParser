using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UkrgoParser.Server.Services
{
    public interface IBlacklistService
    {
        Task<bool> CheckNumberAsync(string phoneNumber);

        Task AddPhoneNumberAsync(string phoneNumber);
    }

    public class BlacklistService : IBlacklistService
    {
        private string FileName { get; } = "black-list.txt";

        public async Task<bool> CheckNumberAsync(string phoneNumber)
        {
            var lines = await File.ReadAllLinesAsync(FileName);
            return !lines.Contains(phoneNumber);
        }

        public async Task AddPhoneNumberAsync(string phoneNumber)
        {
            var lines = (await File.ReadAllLinesAsync(FileName)).ToList();

            if (lines.Contains(phoneNumber)) { return; }

            lines.Add(phoneNumber);

            await File.WriteAllLinesAsync(FileName, lines);
        }
    }
}
