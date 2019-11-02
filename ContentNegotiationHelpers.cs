using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace SBraun.CallWebpackEntrypoints
{
    public static class ContentNegotiationHelpers
    {
        public static bool IsAcceptableEncoding(IList<StringWithQualityHeaderValue> acceptEncoding, string variantName, ILogger? logger)
        {
            // RFC7234, 5.3.4:
            //  #1: If no Accept-Encoding field is in the request, any content-encoding is considered acceptable
            if (acceptEncoding.Count == 0)
            {
                logger?.LogVariantAcceptableBecauseNoHeaderGiven(variantName);
                return true;
            }

            //  #2: if the representation has no content-coding...
            if (variantName == "identity")
            {
                // ... then it is acceptable by default unless specifically exluded by the Accept-Encoding field stating
                // either identity;q=0 ...
                var identityElement = acceptEncoding.FirstOrDefault(x => x.Value == "identity");
                if (identityElement != null && identityElement.Quality == 0) 
                {
                    logger?.LogIdentityExplicitlyUnacceptable();
                    return false;
                }

                // ... or "*;q=0" without a more specific entry for "identity".
                var starElement = acceptEncoding.FirstOrDefault(x => x.Value == "*");
                if (starElement != null && identityElement == null && starElement.Quality == 0)
                {
                    logger?.LogIdentityUnacceptableBecauseAnyUnacceptable();
                    return false;
                }

                // recall: ... then it is acceptable by default ...
                logger?.LogIdentityAcceptableByDefault();
                return true;
            }

            //  #3: If the representation's content-coding is one of the content-codings listed in the Accept-Encoding field, ...
            var element = acceptEncoding.FirstOrDefault(x => x.Value == variantName || x.Value == "*");

            // ... then it is acceptable ...
            if (element != null)
            {
                // ... unless it is accompanied by a qvalue of 0.
                if (element.Quality == 0)
                {
                    logger?.LogVariantExplicitlyUnacceptable(variantName);
                    return false;
                }

                logger?.LogVariantExplicitlyAcceptable(variantName);
                return true;
            }

            // if no rule gave us an answer, the variant is not acceptable
            logger?.LogVariantUnacceptableBecauseNotListed(variantName);
            return false;
        }
    }
}