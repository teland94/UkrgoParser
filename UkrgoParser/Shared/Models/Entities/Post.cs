using System;
using System.Collections.Generic;

namespace UkrgoParser.Shared.Models.Entities
{
    public class Post
    {
        public string Title { get; set; }

        public string Attributes { get; set; }

        public string Description { get; set; }

        public IList<Uri> ImageUris { get; set; }
    }
}
