using Microsoft.AspNetCore.Builder;
using SBraun.CallWebpackEntrypoints;

namespace Microsoft.AspNetCore.Builder
{
    public static class WebpackEntrypointsDependencyInjectionExtensions
    {
        public static IApplicationBuilder UseWebpackAssets(this IApplicationBuilder builder, AssetServerOptions options)
        {
            builder.UseMiddleware<AssetServerMiddleware>(options);
            return builder;
        }
    }
}