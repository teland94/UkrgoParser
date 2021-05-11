using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using UkrgoParser.Shared.Models.Entities;

namespace UkrgoParser.Server.Services
{
    public interface IContactService
    {
        Task<IEnumerable<Contact>> GetContactsAsync();

        Task EditContactAsync(Contact contact);
    }

    public class ContactService : IContactService
    {
        private string FileName => "contacts.csv";

        public async Task<IEnumerable<Contact>> GetContactsAsync()
        {
            return await ReadContactsAsync();
        }

        public async Task EditContactAsync(Contact contact)
        {
            var contacts = await ReadContactsAsync();
            var contactsList = new List<Contact>(contacts);
            var dbContact = contactsList.FirstOrDefault(c => c.PhoneNumber == contact.PhoneNumber);
            if (dbContact != null)
            {
                if (!string.IsNullOrEmpty(dbContact.Name) && string.IsNullOrEmpty(contact.Name))
                {
                    contactsList = contactsList.Where(c => c.PhoneNumber != contact.PhoneNumber).ToList();
                }
                else
                {
                    dbContact.Name = contact.Name;
                }
            }
            else
            {
                contactsList.Add(contact);
            }
            await WriteContacts(contactsList);
        }

        private async Task<IEnumerable<Contact>> ReadContactsAsync()
        {
            if (!File.Exists(FileName)) return new List<Contact>();
            using var reader = new StreamReader(FileName);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            return await csv.GetRecordsAsync<Contact>().ToListAsync();
        }

        private async Task WriteContacts(IEnumerable<Contact> contacts)
        {
            await using var writer = new StreamWriter(FileName);
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            await csv.WriteRecordsAsync(contacts);
        }
    }
}
