using MQTTSniffer.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTSniffer.Extensions
{
    interface IExtensionImporter
    {
        IEnumerable<IEncoderDecoder> GetEncoderDecoders();
    }
}
