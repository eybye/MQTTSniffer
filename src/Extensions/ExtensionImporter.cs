using MQTTSniffer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Runtime.Loader;
using System.Composition.Hosting;
using MQTTSniffer.EncoderDecoder;
using Splat;

namespace MQTTSniffer.Extensions
{
    class ExtensionImporter : IExtensionImporter
    {
        private IEnumerable<IEncoderDecoder> _encoderDecoders = null;
        private readonly ILogger _logger;

        public ExtensionImporter(ILogger logger)
        {
            _logger = logger;
        }

        public void ResolvePlugins(string path)
        {
            var configuration = new ContainerConfiguration()
                .WithPart(typeof(AsciiEncoderDecoder));

            try
            {
                if (Directory.Exists(path))
                {
                    var assemblies = Directory
                                .GetFiles(path, "*.dll", SearchOption.AllDirectories)
                                .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                                .ToList();
                    configuration = configuration.WithAssemblies(assemblies);
                }
                using (var container = configuration.CreateContainer())
                {
                    _encoderDecoders = container.GetExports<IEncoderDecoder>();
                }
            }
            catch (Exception e)
            {
                _logger.Write(e.Message, LogLevel.Error);
                GetDefaultEncoderDecoder();
            }
        }
        private void GetDefaultEncoderDecoder()
        {
            var configuration = new ContainerConfiguration()
                .WithPart(typeof(AsciiEncoderDecoder));
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
