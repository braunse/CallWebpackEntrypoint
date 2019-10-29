using System.Collections.Generic;
using Newtonsoft.Json;

namespace SBraun.CallWebpackEntrypoint
{
    [JsonObject]
    internal sealed class WebpackEntrypointsData
    {
        [JsonProperty("entrypoints")] public IDictionary<string, WebpackEntrypointData> Entrypoints { get; set; }

        [JsonProperty("hashes")] public IDictionary<string, string> Hashes { get; set; }
    }

    [JsonObject]
    internal sealed class WebpackEntrypointData
    {
        [JsonProperty("stylesheets")] public string[] Stylesheets { get; set; }

        [JsonProperty("scripts")] public string[] Scripts { get; set; }
    }

    public struct WebpackEntrypoint
    {
        public WebpackElement[] Styles { get; internal set; }
        public WebpackElement[] Scripts { get; internal set; }
    }
    
    public struct WebpackElement
    {
        public string File { get; internal set; }
        public string SriHash { get; internal set; }
    }
}