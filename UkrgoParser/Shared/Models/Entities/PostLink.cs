using System;

namespace UkrgoParser.Shared.Models.Entities
{
    public class PostLink
    {
        public Uri ImageUri { get; set; }

        public string Caption { get; set; }

        public Uri Uri { get; set; }

        public string Location { get; set; }

        public string Date { get; set; }

        public string Price { get; set; }
    }
}
