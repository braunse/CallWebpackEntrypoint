using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.WebUtilities;

namespace SBraun.CallWebpackEntrypoint
{
    public class CallWebpackEntrypoint : ViewComponent
    {
        private readonly WebpackEntrypoints _webpackEntrypoints;
        private readonly string _encodedUrlPrefix;

        public CallWebpackEntrypoint(WebpackEntrypoints webpackEntrypoints)
        {
            _webpackEntrypoints = webpackEntrypoints;
            _encodedUrlPrefix = HtmlEncoder.Default.Encode(_webpackEntrypoints.UrlPrefix + (_webpackEntrypoints.UrlPrefix.EndsWith('/') ? "" : "/"));
        }

        public async Task<IViewComponentResult> InvokeAsync(string names)
        {
            var namesEnum = Regex.Split(names, @"\s*,\s*")
                .Where(name => !string.IsNullOrWhiteSpace(name));
            var entrypoint = await _webpackEntrypoints.GetEntrypointAsync(namesEnum);

            var b = new StringBuilder();
            foreach (var script in entrypoint.Scripts)
            {
                b.AppendFormat("<script src=\"{0}{1}\" integrity=\"{2}\" crossorigin=\"anonymous\" defer></script>\n",
                    _encodedUrlPrefix,
                    HtmlEncoder.Default.Encode(script.File),
                    HtmlEncoder.Default.Encode(script.SriHash));
            }

            foreach (var style in entrypoint.Styles)
            {
                b.AppendFormat("<link rel=\"stylesheet\" href=\"{0}{1}\" integrity=\"{2}\" crossorigin=\"anonymous\">\n",
                    _encodedUrlPrefix,
                    HtmlEncoder.Default.Encode(style.File),
                    HtmlEncoder.Default.Encode(style.SriHash));
            }

            return new HtmlContentViewComponentResult(new HtmlString(b.ToString()));
        }
    }
}