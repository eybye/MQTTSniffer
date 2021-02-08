using MQTTSniffer.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MQTTSniffer
{
    public interface IMainContext
    {
        void Publish(MQTTMessage message);
    }
}
