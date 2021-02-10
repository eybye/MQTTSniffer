using MQTTSniffer.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTSniffer.Extensions
{
    public interface IExtensionImporter
    {
        void ResolvePlugins(string path);
        IEnumerable<IEncoderDecoder> GetEncoderDecoders();
    }
}
