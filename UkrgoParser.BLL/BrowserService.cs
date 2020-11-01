using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace UkrgoParser.BLL
{
    public interface IBrowserService
    {
        Task<IEnumerable<string>> GetPostLinksAsync(string url);

        Task<string> GetPhoneNumberAsync(string postLink);
    }

    public class BrowserService : IBrowserService
    {
        private readonly HtmlDocument _doc;

        public BrowserService()
        {
            _doc = new HtmlDocument { OptionReadEncoding = false };
        }

        public async Task<IEnumerable<string>> GetPostLinksAsync(string url)
        {
            await LoadPageAsync(url);

            return _doc.DocumentNode
                .SelectNodes("//div[contains(@class, 'post_top')]/div[contains(@class, 'post')]//a[contains(@class, 'link_post')]|//div[contains(@class, 'main-content')]//table//td/h3/a[contains(@class, 'link_post')]")
                .Where(elem => elem.Attributes["href"].Value.StartsWith("http"))
                .Select(elem => elem.Attributes["href"].Value)
                .ToList();
        }

        public async Task<string> GetPhoneNumberAsync(string postLink)
        {
            await LoadPageAsync(postLink);

            var postPhonesShowDiv = _doc.DocumentNode.SelectSingleNode("//div[@id='post-phones-show-div']");

            if (postPhonesShowDiv == null) return null;

            var input = postPhonesShowDiv.SelectSingleNode(".//input");
            var onCLickStr = input?.GetAttributeValue("onclick", null);

            var funcArgs = GetFuncArgs(onCLickStr).ToArray();
            var formData = new Dictionary<string, string> { ["i"] = funcArgs[0], ["s"] = funcArgs[1] };
            var postContactsDiv = await SendPostRequestAsync("http://ukrgo.com/moduls/showphonesnumbers.php", formData);

            if (!string.IsNullOrEmpty(postContactsDiv))
            {
                _doc.LoadHtml(postContactsDiv);
                var postContactsSpan = _doc.DocumentNode.SelectSingleNode("//span");

                return postContactsSpan?.InnerText;
            }

            return null;
        }

        private async Task<string> SendPostRequestAsync(string url, IDictionary<string, string> data)
        {
            using (var http = new HttpClient())
            {
                var content = new FormUrlEncodedContent(data);

                var res = await http.PostAsync(url, content);
                return await res.Content.ReadAsStringAsync();
            }
        }

        private IEnumerable<string> GetFuncArgs(string func)
        {
            var extractFuncRegex = @"\b[^()]+\((.*)\)";
            var extractArgsRegex = @"([^,]+\(.+?\))|([^,]+)";

            var match = Regex.Match(func, extractFuncRegex);
            var innerArgs = match.Groups[1].Value;
            var matches = Regex.Matches(innerArgs, extractArgsRegex);

            return matches.OfType<Match>().Select(m => m.Value.Replace("'", "").Replace(" ", ""));
        }

        private async Task LoadPageAsync(string url)
        {
            using (var http = new HttpClient())
            {
                var source = await http.GetStringAsync(url);
                source = WebUtility.HtmlDecode(source);

                _doc.LoadHtml(source);
            }
        }
    }
}
