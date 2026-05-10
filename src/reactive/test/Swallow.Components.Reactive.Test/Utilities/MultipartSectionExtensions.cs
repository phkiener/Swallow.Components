using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Swallow.Components.Reactive.Test.Utilities;

public static class MultipartSectionExtensions
{
    extension(MultipartSection section)
    {
        public async Task<IFormCollection> ReadAsFormAsync()
        {
            var textContent = await section.ReadAsStringAsync();

            var splitData = textContent.Split('&')
                .ToLookup(static t => t.Split('=')[0], static t => t.Split('=')[1])
                .ToDictionary(static g => g.Key, static g => new StringValues(g.ToArray()));

            return new FormCollection(splitData);
        }
    }
}
