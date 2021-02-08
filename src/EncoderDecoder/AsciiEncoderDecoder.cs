using MQTTSniffer.Common;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;

namespace MQTTSniffer.EncoderDecoder
{
    [Export(typeof(IEncoderDecoder))]
    class AsciiEncoderDecoder : IEncoderDecoder
    {
        public string Name => "Ascii";

        public string Decode(string topic, byte[] payload)
        {
            return ASCIIEncoding.ASCII.GetString(payload);
        }

        public byte[] Encode(string topic, string payload)
        {
            if (!string.IsNullOrEmpty(payload))
                return ASCIIEncoding.ASCII.GetBytes(payload);
            return new byte[0];
        }
    }
}
