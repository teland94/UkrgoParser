using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UkrgoParser.Server.Services
{
    public interface IBlacklistService
    {
        Task<IEnumerable<string>> GetPhoneNumbersAsync();

        Task<bool> CheckNumberAsync(string phoneNumber);

        Task AddPhoneNumberAsync(string phoneNumber);

        Task DeletePhoneNumberAsync(string phoneNumber);
    }

    public class BlacklistService : IBlacklistService
    {
        private string FileName { get; } = "black-list.txt";

        public async Task<IEnumerable<string>> GetPhoneNumbersAsync()
        {
            return await ReadPhoneNumbersAsync();
        }

        public async Task<bool> CheckNumberAsync(string phoneNumber)
        {
            var lines = await ReadPhoneNumbersAsync();
            return !lines.Contains(phoneNumber);
        }

        public async Task AddPhoneNumberAsync(string phoneNumber)
        {
            var lines = await ReadPhoneNumbersAsync();

            if (lines.Contains(phoneNumber)) { return; }

            lines.Add(phoneNumber);

            await File.WriteAllLinesAsync(FileName, lines);
        }

        public async Task DeletePhoneNumberAsync(string phoneNumber)
        {
            var lines = await ReadPhoneNumbersAsync();

            lines.Remove(phoneNumber);

            await File.WriteAllLinesAsync(FileName, lines);
        }

        private async Task<IList<string>> ReadPhoneNumbersAsync()
        {
            return !File.Exists(FileName) ? new List<string>() : (await File.ReadAllLinesAsync(FileName)).ToList();
        }
    }
}
