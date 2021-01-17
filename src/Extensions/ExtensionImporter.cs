using MQTTSniffer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Runtime.Loader;
using System.Composition.Hosting;
using MQTTSniffer.EncoderDecoder;

namespace MQTTSniffer.Extensions
{
    class ExtensionImporter : IExtensionImporter
    {
        private IEnumerable<IEncoderDecoder> _encoderDecoders = null;

        public ExtensionImporter()
        {
            var executableLocation = Assembly.GetEntryAssembly().Location;
            var path = Path.Combine(Path.GetDirectoryName(executableLocation), "Plugins");
            var assemblies = Directory
                        .GetFiles(path, "*.dll", SearchOption.AllDirectories)
                        .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                        .ToList();

            var configuration = new ContainerConfiguration()
                .WithPart(typeof(AsciiEncoderDecoder))
                .WithAssemblies(assemblies);
            using (var container = configuration.CreateContainer())
            {
                _encoderDecoders = container.GetExports<IEncoderDecoder>();
            }
        }

        public IEnumerable<IEncoderDecoder> GetEncoderDecoders()
        {
            return _encoderDecoders;
        }
    }
}
