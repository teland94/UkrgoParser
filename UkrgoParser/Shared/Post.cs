using System.Collections.Generic;

namespace UkrgoParser.Shared
{
    public class Post
    {
        public string Attributes { get; set; }

        public string Description { get; set; }

        public IList<byte[]> Images { get; set; }
    }
}
