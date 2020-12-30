using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MQTTSniffer.Model
{
    public class BrokerEntity : IEquatable<BrokerEntity>
    {
        public enum eProtocolVersion
        {
            V310,
            V311,
            V500
        }
        public bool IsDummy;
        public string? ProfileName { get; set; }
        public string? URL { get; set; }
        public uint? Port { get; set; }
        public eProtocolVersion? ProtocolVersion { get; set; }
        public string? ClientId { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }

        public bool Equals([AllowNull] BrokerEntity other)
        {
            return ProfileName?.Equals(other?.ProfileName) ?? false;
        }
    }
}
