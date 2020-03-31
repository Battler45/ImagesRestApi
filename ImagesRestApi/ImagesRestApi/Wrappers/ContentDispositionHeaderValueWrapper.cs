using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace ImagesRestApi.Wrappers
{
    public class ContentDispositionHeaderValueFabric
    {

    }
    public interface IContentDispositionHeaderValueWrapper
    {
        bool TryParse(StringSegment input, out ContentDispositionHeaderValue parsedValue);
    }

    public class ContentDispositionHeaderValueWrapper : IContentDispositionHeaderValueWrapper
    {
        public bool TryParse(StringSegment input, out ContentDispositionHeaderValue parsedValue) =>
            ContentDispositionHeaderValue.TryParse(input, out parsedValue);
    }
}
