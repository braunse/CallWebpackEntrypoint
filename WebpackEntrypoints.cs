using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SBraun.CallWebpackEntrypoint
{
    public class WebpackEntrypoints
    {
        private readonly Lazy<Task<WebpackEntrypointsData>> _entrypointsDataIniter;
        
        public string UrlPrefix { get; private set; }

        public WebpackEntrypoints(Assembly assembly, string filename, string urlPrefix)
        {
            UrlPrefix = urlPrefix;
            _entrypointsDataIniter = new Lazy<Task<WebpackEntrypointsData>>(() => ReadEntrypoints(assembly, filename));
        }

        public async Task<WebpackEntrypoint> GetEntrypointAsync(IEnumerable<string> names)
        {
            var entrypoints = await _entrypointsDataIniter.Value;
            var scripts = new List<string>();
            var styles = new List<string>();

            foreach (var name in names)
            {
                var entrypoint = entrypoints.Entrypoints![name];
                if (entrypoint == null)
                {
                    throw new KeyNotFoundException($"No entrypoint available named {name}");
                }
                
                foreach (var epscript in entrypoint.Scripts!)
                {
                    if (!scripts.Contains(epscript))
                    {
                        scripts.Add(epscript);
                    }
                }

                foreach (var epstyle in entrypoint.Stylesheets!)
                {
                    if (!styles.Contains(epstyle))
                    {
                        styles.Add(epstyle);
                    }
                }
            }

            WebpackElement[] ToElements(List<string> list)
            {
                return list
                    .Select(elem => new WebpackElement()
                    {
                        File = elem,
                        SriHash = entrypoints.Files![elem].SriHash!,
                    })
                    .ToArray();
            }

            return new WebpackEntrypoint()
            {
                Scripts = ToElements(scripts),
                Styles = ToElements(styles),
            };
        }

        private static async Task<WebpackEntrypointsData> ReadEntrypoints(Assembly assembly, string resourceName)
        {
            await using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException($"Manifest stream {resourceName} not found in assembly {assembly}");

                var reader = new StreamReader(stream, Encoding.UTF8);
                var contents = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<WebpackEntrypointsData>(contents);
            }
        }
    }
}