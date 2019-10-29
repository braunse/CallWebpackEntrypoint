using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SBraun.CallWebpackEntrypoint;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CallWebpackEntrypointExtensions
    {
        public static IServiceCollection AddWebpackEntrypoints(this IServiceCollection serviceCollection,
            Assembly assembly,
            string pathToJson = "webpack-entrypoints.json",
            string urlPrefix = "/assets")
        {
            serviceCollection.AddSingleton(_ => new WebpackEntrypoints(assembly, pathToJson, urlPrefix));
            return serviceCollection;
        }
    }
}
