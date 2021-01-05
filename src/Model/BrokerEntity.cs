using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MQTTSniffer.Model
{
    public class BrokerEntity : ICloneable
    {
        public enum eProtocolVersion
        {
            V310,
            V311,
            V500
        }
        public string? ProfileName { get; set; }
        public string? URL { get; set; }
        public uint? Port { get; set; }
        public eProtocolVersion? ProtocolVersion { get; set; }
        public string? ClientId { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }

        public List<string> Topics { get; set; } = new List<string>();

        [Newtonsoft.Json.JsonIgnore]
        public string FilePath = string.Empty;
        [Newtonsoft.Json.JsonIgnore]
        public string FileName = string.Empty;

        public object Clone()
        {
            return new BrokerEntity
            {
                ProfileName = this.ProfileName,
                URL = this.URL,
                Port = this.Port,
                ProtocolVersion = this.ProtocolVersion,
                ClientId = this.ClientId,
                UserName = this.UserName,
                Password = this.Password
            };
        }
    }
}
