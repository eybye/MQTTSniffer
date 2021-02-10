using MQTTSniffer.Common;
using MQTTSniffer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MQTTSniffer.Services
{
    class EncoderDecoderService : IIEncoderDecoderService
    {
        private readonly IExtensionImporter _extensionImporter;
        public EncoderDecoderService(IExtensionImporter extensionImporter)
        {
            _extensionImporter = extensionImporter;
        }

        public IEnumerable<string> GetDecoderNames()
        {
            return _extensionImporter.GetEncoderDecoders().Select(d => d.Name);
        }

        public IEnumerable<string> GetEncoderNames()
        {
            return _extensionImporter.GetEncoderDecoders().Select(d => d.Name);
        }

        public IEncoderDecoder GetDecoder(string decoderName)
        {
            return _extensionImporter.GetEncoderDecoders().Where(d => d.Name == decoderName).FirstOrDefault();
        }
    }
}
