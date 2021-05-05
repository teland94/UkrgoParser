using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using UkrgoParser.Client.Dialogs;
using UkrgoParser.Client.HttpClients;
using UkrgoParser.Client.ViewModels;
using UkrgoParser.Shared.Models.Entities;

namespace UkrgoParser.Client.Pages
{
    public partial class Index : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; }
        [Inject] private Blazored.LocalStorage.ILocalStorageService LocalStorage { get; set; }
        [Inject] private CurrieTechnologies.Razor.Clipboard.ClipboardService Clipboard { get; set; }
        [Inject] private IMatToaster Toaster { get; set; }
        [Inject] private IMatDialogService MatDialogService { get; set; }

        [Inject] private BrowserHttpClient BrowserHttpClient { get; set; }
        [Inject] private BlacklistHttpClient BlacklistHttpClient { get; set; }
        [Inject] private ContactHttpClient ContactHttpClient { get; set; }

        private string Url { get; set; }
        private IList<PostLinkViewModel> PostLinks { get; set; }
        private double Progress { get; set; }
        private bool? IsSmall { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (await LocalStorage.ContainKeyAsync("url") && await LocalStorage.ContainKeyAsync("postLinks"))
            {
                try
                {
                    Url = await LocalStorage.GetItemAsStringAsync("url");
                    PostLinks = await LocalStorage.GetItemAsync<IList<PostLinkViewModel>>("postLinks");
                }
                catch (Exception)
                {
                    await LocalStorage.ClearAsync();
                }
            }
        }

        private async Task Process(MouseEventArgs e)
        {
            PostLinks = new List<PostLinkViewModel>();
            Progress = 0.0;

            var contacts = await ContactHttpClient.GetContactsAsync();

            IList<PostLink> postLinks = new List<PostLink>();
            try
            {
                postLinks = await BrowserHttpClient.GetPostLinksAsync(new Uri(Url));
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
                Toaster.Add("Ошибка загрузки ссылок объявлений", MatToastType.Danger);
            }

            var step = (double)1 / postLinks.Count;
            foreach (var postLink in postLinks)
            {
                try
                {
                    var phoneNumber = await BrowserHttpClient.GetPhoneNumberAsync(postLink.Uri);
                    if (string.IsNullOrEmpty(phoneNumber))
                    {
                        Progress += step;
                        StateHasChanged();
                        continue;
                    }
                    var existsInBlacklist = await BlacklistHttpClient.CheckPhoneNumberAsync(phoneNumber);
                    if (existsInBlacklist && PostLinks.All(p => p.Contact.PhoneNumber != phoneNumber))
                    {
                        PostLinks.Add(new PostLinkViewModel
                        {
                            PostLink = new PostLink
                            {
                                ImageUri = postLink.ImageUri,
                                Uri = postLink.Uri,
                                Caption = postLink.Caption
                            },
                            Contact = new Contact
                            {
                                Name = contacts?.FirstOrDefault(c => c.PhoneNumber == phoneNumber)?.Name,
                                PhoneNumber = phoneNumber
                            }
                        });
                    }
                    Progress += step;
                    StateHasChanged();
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine(ex);
                    if (ex.StatusCode != HttpStatusCode.NotFound)
                    {
                        Progress = 0.0;
                        StateHasChanged();
                        Toaster.Add("Ошибка обработки объявлений", MatToastType.Danger);
                        break;
                    }
                    Progress += step;
                    StateHasChanged();
                }
            }
        }


        private async Task SaveData(MouseEventArgs e)
        {
            await LocalStorage.SetItemAsync("url", Url);
            await LocalStorage.SetItemAsync("postLinks", PostLinks);
            Toaster.Add("Данные успешно сохранены", MatToastType.Success);
        }

        private async Task CopyPhoneNumber(string phoneNumber, MouseEventArgs e)
        {
            await Clipboard.WriteTextAsync(phoneNumber);
            Toaster.Add($"Телефон {phoneNumber} успешно скопирован", MatToastType.Success);
        }

        private async Task BlockPhoneNumber(string phoneNumber, MouseEventArgs e)
        {
            if (await MatDialogService.ConfirmAsync($"Вы действительно хотите заблокировать телефон - {phoneNumber}?"))
            {
                var response = await BlacklistHttpClient.AddPhoneNumberAsync(phoneNumber);
                if (response.IsSuccessStatusCode)
                {
                    PostLinks.RemoveAt(PostLinks.FindIndex(p => p.Contact.PhoneNumber == phoneNumber));
                    Toaster.Add($"Телефон {phoneNumber} успешно заблокирован", MatToastType.Success);
                }
                else
                {
                    Toaster.Add("Ошибка блокировки номера", MatToastType.Danger);
                }
            }
        }

        private async Task ShowPostDetails(Uri postLinkUri, MouseEventArgs e)
        {
            try
            {
                var post = await BrowserHttpClient.GetPostDetailsAsync(postLinkUri);
                await MatDialogService.OpenAsync(typeof(PostDetailsDialog), new MatDialogOptions
                {
                    Attributes = new Dictionary<string, object>
                    {
                        { "Post", post }
                    }
                });
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Toaster.Add("Страница не найдена", MatToastType.Danger);
                }
            }
        }

        private async Task EditContact(string phoneNumber, MouseEventArgs e)
        {
            var postLinkContact = PostLinks
                .Where(pl => pl.Contact.PhoneNumber == phoneNumber)
                .Select(pl => pl.Contact)
                .FirstOrDefault();
            var resultContact = (Contact)await MatDialogService.OpenAsync(typeof(ContactDialog), new MatDialogOptions
            {
                Attributes = new Dictionary<string, object>
                {
                    { "Contact", postLinkContact }
                }
            });
            if (resultContact == null) return;
            await ContactHttpClient.AddContactAsync(resultContact);
            if (postLinkContact != null)
            {
                postLinkContact.Name = resultContact.Name;
                await SaveContact(postLinkContact);
                Toaster.Add("Контакт успешно сохранен", MatToastType.Success);
            }
        }

        private async Task SaveContact(Contact contact)
        {
            var storagePostLinks = await LocalStorage.GetItemAsync<IList<PostLinkViewModel>>("postLinks");
            var storageContact = storagePostLinks?.Where(pl => pl.Contact.PhoneNumber == contact.PhoneNumber)
                .Select(pl => pl.Contact)
                .FirstOrDefault();
            if (storageContact != null)
            {
                storageContact.Name = contact.Name;
                await LocalStorage.SetItemAsync("postLinks", PostLinks);
            }
        }

        private string GetViberUrl(string phoneNumber)
        {
            var trimmedPhoneNumber = phoneNumber.Trim();
            var fullPhoneNumber = trimmedPhoneNumber;
            const string countryCode = "380";
            const string internalCountryCode = "0";
            if (trimmedPhoneNumber.StartsWith("+"))
            {
                fullPhoneNumber = $"{trimmedPhoneNumber[1..]}";
            }
            if (trimmedPhoneNumber.StartsWith(internalCountryCode))
            {
                fullPhoneNumber = $"{countryCode}{trimmedPhoneNumber[1..]}";
            }
            return $"viber://chat?number={fullPhoneNumber}";
        }
    }
}
