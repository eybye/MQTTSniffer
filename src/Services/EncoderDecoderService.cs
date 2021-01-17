using MQTTSniffer.Common;
using MQTTSniffer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MQTTSniffer.Services
{
    class EncoderDecoderService : IIEncoderDecoderService
    {
        private IEnumerable<IEncoderDecoder> _encoderDecoders;
        public EncoderDecoderService(IExtensionImporter extensionImporter)
        {
            _encoderDecoders = extensionImporter.GetEncoderDecoders();
        }

        public IEnumerable<string> GetDecoderNames()
        {
            return _encoderDecoders.Select(d => d.Name);
        }

        public IEnumerable<string> GetEncoderNames()
        {
            return _encoderDecoders.Select(d => d.Name);
        }

        public IEncoderDecoder GetDecoder(string decoderName)
        {
            return _encoderDecoders.Where(d => d.Name == decoderName).FirstOrDefault();
        }
    }
}
