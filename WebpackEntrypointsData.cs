using System.Collections.Generic;
using Newtonsoft.Json;

namespace SBraun.CallWebpackEntrypoint
{
    [JsonObject]
    internal sealed class WebpackEntrypointsData
    {
        [JsonProperty("entrypoints", Required = Required.Always)] public IDictionary<string, WebpackEntrypointData>? Entrypoints { get; set; }

        [JsonProperty("files", Required = Required.Always)] public IDictionary<string, WebpackFileData>? Files { get; set; }
    }

    [JsonObject]
    internal sealed class WebpackFileData
    {
        [JsonProperty("contentType", Required = Required.Always)] public string? ContentType { get; set; }
        
        [JsonProperty("sriHash", Required = Required.Always)] public string? SriHash { get; set; }

        [JsonProperty("variants", Required = Required.Always)] public IDictionary<string, WebpackVariantData>? Variants { get; set; }
    }

    [JsonObject]
    internal sealed class WebpackVariantData
    {
        [JsonProperty("file", Required = Required.Always)] public string? FileName { get; set; }

        [JsonProperty("hash", Required = Required.Always)] public string? Hash { get; set; }

        [JsonProperty("size", Required = Required.Always)] public long Size { get; set; }
    }

    [JsonObject]
    internal sealed class WebpackEntrypointData
    {
        [JsonProperty("stylesheets", Required = Required.Always)] public string[]? Stylesheets { get; set; }

        [JsonProperty("scripts", Required = Required.Always)] public string[]? Scripts { get; set; }
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