using MQTTSniffer.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTSniffer.Services
{
    public interface IIEncoderDecoderService
    {
        IEnumerable<string> GetDecoderNames();
        IEnumerable<string> GetEncoderNames();
        IEncoderDecoder GetDecoder(string decoderName);
    }
}
