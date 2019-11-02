using System.Linq;
using Microsoft.AspNetCore.Http.Headers;

public static class ContentNegotiationHelpers
{
    public static bool IsAcceptableEncoding(RequestHeaders requestHeaders, string variantName)
    {
        // RFC7234, 5.3.4:
        //  #1: If no Accept-Encoding field is in the request, any content-encoding is considered acceptable
        if (requestHeaders.AcceptEncoding.Count == 0) return true;

        //  #2: if the representation has no content-coding...
        if (variantName == "identity")
        {
            // ... then it is acceptable by default unless specifically exluded by the Accept-Encoding field stating
            // either identity;q=0 ...
            var identityElement = requestHeaders.AcceptEncoding.FirstOrDefault(x => x.Value == "identity");
            if (identityElement != null && identityElement.Quality == 0) return false;

            // ... or "*;q=0" without a more specific entry for "identity".
            var starElement = requestHeaders.AcceptEncoding.FirstOrDefault(x => x.Value == "*");
            if (starElement != null && identityElement == null && starElement.Quality == 0) return false;

            // recall: ... then it is acceptable by default ...
            return true;
        }

        //  #3: If the representation's content-coding is one of the content-codings listed in the Accept-Encoding field, ...
        var element = requestHeaders.AcceptEncoding.FirstOrDefault(x => x.Value == variantName);

        // ... then it is acceptable ...
        if (element != null)
        {
            // ... unless it is accompanied by a qvalue of 0.
            if (element.Quality == 0) return false;

            return true;
        }

        // if no rule gave us an answer, the variant is not acceptable
        return false;
    }
}