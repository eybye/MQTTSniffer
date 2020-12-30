using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTSniffer.Model
{
    public class MQTTMessage
    {
        public enum QualityOfServiceLevel
        {
            AtMostOnce = 0,
            AtLeastOnce = 1,
            ExactlyOnce = 2
        }

        public DateTime MessageReceived { get; set; } = DateTime.Now;
        public string Topic { get; set; } = string.Empty;
        public byte[]? Payload { get; set; }
        public QualityOfServiceLevel QoS { get; set; }
        public bool Retain { get; set; }
    }
}
