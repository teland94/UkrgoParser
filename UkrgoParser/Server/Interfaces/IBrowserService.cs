using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UkrgoParser.Shared.Models.Entities;

namespace UkrgoParser.Server.Interfaces
{
    public interface IBrowserService
    {
        Task<IEnumerable<PostLink>> GetPostLinksAsync(Uri uri);

        Task<string> GetPhoneNumberAsync(Uri postLinkUri);

        Task<Post> GetPostDetails(Uri postLinkUri);

        Task<byte[]> GetImage(Uri imageUri, bool cropUnwantedBackground = false);
    }
}