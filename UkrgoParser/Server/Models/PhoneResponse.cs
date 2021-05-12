using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UkrgoParser.Server.Models
{
    public class PhoneResponse
    {
        [JsonPropertyName("phones")]
        public IEnumerable<string> Phones { get; set; }
    }
}
