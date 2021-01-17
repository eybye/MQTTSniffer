using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTSniffer.Common
{
    public interface IEncoderDecoder
    {
        /// <summary>
        /// Friendly name of encode decoder
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Method for decoding the payload
        /// </summary>
        /// <param name="topic">topic of the message</param>
        /// <param name="payload">payload of the message</param>
        /// <returns>Decoded payload into a string.</returns>
        string Decode(string topic, byte[] payload);
        /// <summary>
        /// Method for encoding a payload
        /// </summary>
        /// <param name="topic">topic of the message</param>
        /// <param name="payload">the payload to encode</param>
        /// <returns>Encoded payload</returns>
        byte[] Encode(string topic, string payload);
    }
}
