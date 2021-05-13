using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using UkrgoParser.Server.Helpers;
using UkrgoParser.Server.Interfaces;
using UkrgoParser.Shared.Models.Entities;

namespace UkrgoParser.Server.Services
{
    public class BrowserService : BrowserBaseService, IBrowserService
    {
        public BrowserService(HttpClient client) : base(client)
        {
            Console.WriteLine(client);
        }

        public async Task<IEnumerable<PostLink>> GetPostLinksAsync(Uri uri)
        {
            await LoadPageAsync(uri);

            var postElements = Doc.DocumentNode
                .SelectNodes("//div[contains(@class, 'post_top')]/div[contains(@class, 'post')]|//div[contains(@class, 'main-content')]//table");

            return (from postElement in postElements 
                let postLinkElem = postElement.SelectSingleNode(".//a[contains(@class, 'link_post') and not(img)]") 
                where postLinkElem.Attributes["href"].Value.StartsWith("http") 
                let postImgElem = postElement.SelectSingleNode(".//img")
                let postLinkUri = new Uri(postLinkElem.Attributes["href"].Value)
                select new PostLink
                {
                    ImageUri = new Uri(postLinkUri.GetLeftPart(UriPartial.Authority)
                                       + postImgElem.Attributes["src"].Value[postImgElem.Attributes["src"].Value.IndexOf("/", StringComparison.OrdinalIgnoreCase)..]),
                    Caption = postLinkElem.InnerText.Trim(), 
                    Uri = postLinkUri
                }).ToList();
        }

        public async Task<string> GetPhoneNumberAsync(Uri postLinkUri)
        {
            await LoadPageAsync(postLinkUri);

            var postPhonesShowDiv = Doc.DocumentNode.SelectSingleNode("//div[@id='post-phones-show-div']");

            if (postPhonesShowDiv == null) return null;

            var input = postPhonesShowDiv.SelectSingleNode(".//input");
            var onCLickStr = input?.GetAttributeValue("onclick", null);

            var funcArgs = GetFuncArgs(onCLickStr).ToArray();
            var formData = new Dictionary<string, string> { ["i"] = funcArgs[0], ["s"] = funcArgs[1] };
            var postContactsDiv = await SendPostFormUrlEncodedRequestAsync(new Uri("http://ukrgo.com/moduls/showphonesnumbers.php"), formData);

            if (!string.IsNullOrEmpty(postContactsDiv))
            {
                Doc.LoadHtml(postContactsDiv);
                var postContactsSpan = Doc.DocumentNode.SelectSingleNode("//span");

                return postContactsSpan?.InnerText;
            }

            return null;
        }

        public async Task<Post> GetPostDetails(Uri postLinkUri)
        {
            await LoadPageAsync(postLinkUri);

            var detailsTable = Doc.DocumentNode.SelectSingleNode("//table[4]//table");
            var header = detailsTable.SelectSingleNode(".//tr[1]/td/h1");
            var attributesDiv = detailsTable.SelectSingleNode(".//tr[2]/td/div");
            var descriptionDiv = detailsTable.SelectSingleNode(".//tr[3]/td/div");
            var imageUris = detailsTable
                .SelectNodes(".//tr[5]/td/table//tr//img")
                ?.Select(elem => new Uri(postLinkUri.GetLeftPart(UriPartial.Authority) 
                                         + elem.Attributes["src"].Value[elem.Attributes["src"].Value.IndexOf("/", StringComparison.OrdinalIgnoreCase)..]))
                .Distinct()
                .ToList();

            return new Post
            {
                Title = header.InnerText.Trim(),
                //Attributes = attributesDiv.InnerHtml.Trim(),
                Description = descriptionDiv.ChildNodes[0].InnerText.Trim(),
                ImageUris = imageUris
            };
        }
    }
}
