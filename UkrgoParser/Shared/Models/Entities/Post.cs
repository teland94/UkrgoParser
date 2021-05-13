using System;
using System.Collections.Generic;

namespace UkrgoParser.Shared.Models.Entities
{
    public class Post
    {
        public string Title { get; set; }

        public IEnumerable<string> Attributes { get; set; }

        public string Description { get; set; }

        public ICollection<Uri> ImageUris { get; set; }

        public string Price { get; set; }
    }
}
