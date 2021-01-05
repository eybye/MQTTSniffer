using MQTTSniffer.Model;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MQTTSniffer.Dialogs.Documents
{
    public class EditBrokerDialogViewModel : ReactiveObject
    {
        private string? _profileName;
        private string? _brokerURL;
        private uint? _brokerPort;
        private string? _clientId;
        private string? _protocolVersion;
        private string? _userName;
        private string? _password;

        public ObservableCollection<string> ProtocolVersions { get; set; } = new ObservableCollection<string>();
        public EditBrokerDialogViewModel(BrokerEntity? brokerEntity=null)
        {
            _profileName = brokerEntity?.ProfileName;
            _brokerURL = brokerEntity?.URL;
            _brokerPort = brokerEntity?.Port;
            _clientId = brokerEntity?.ClientId;
            _protocolVersion = brokerEntity?.ProtocolVersion.ToString();
            _userName = brokerEntity?.UserName;
            _password = brokerEntity?.Password;

            foreach (var item in Enum.GetNames(typeof(BrokerEntity.eProtocolVersion)))
            {
                ProtocolVersions.Add(item);
            }
        }
        public BrokerEntity GetEntity()
        {
            return new BrokerEntity
            {
                ProfileName = _profileName,
                URL = _brokerURL,
                Port = _brokerPort,
                ClientId = _clientId,
                UserName = _userName,
                Password = _password,
                ProtocolVersion = (BrokerEntity.eProtocolVersion)Enum.Parse(typeof(BrokerEntity.eProtocolVersion), _protocolVersion ?? BrokerEntity.eProtocolVersion.V310.ToString())
            };
        }
        public string? ProfileName
        {
            get => _profileName;
            set => this.RaiseAndSetIfChanged(ref _profileName, value);
        }
        public string? BrokerURL
        {
            get => _brokerURL;
            set => this.RaiseAndSetIfChanged(ref _brokerURL, value);
        }

        public uint? BrokerPort
        {
            get => _brokerPort;
            set => this.RaiseAndSetIfChanged(ref _brokerPort, value);
        }

        public string? ClientId
        {
            get => _clientId;
            set => this.RaiseAndSetIfChanged(ref _clientId, value);
        }

        public string? SelectedProtocolVersion
        {
            get => _protocolVersion;
            set => this.RaiseAndSetIfChanged(ref _protocolVersion, value);
        }
        public string? UserName
        {
            get => _userName;
            set => this.RaiseAndSetIfChanged(ref _userName, value);
        }
        public string? Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }
        public void OnMQTTClientIdGenerateCommand()
        {
            ClientId = Guid.NewGuid().ToString();
        }
    }
}
