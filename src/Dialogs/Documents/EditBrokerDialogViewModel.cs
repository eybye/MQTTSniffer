using MQTTSniffer.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTSniffer.Dialogs.Documents
{
    public class EditBrokerDialogViewModel : ReactiveObject
    {
        public EditBrokerDialogViewModel(BrokerEntity? brokerEntity=null)
        {
            _profileName = brokerEntity?.ProfileName;
            _brokerURL = brokerEntity?.URL;
            _brokerPort = brokerEntity?.Port;
            _clientId = brokerEntity?.ClientId;
            _userName = brokerEntity?.UserName;
            _password = brokerEntity?.Password;
        }

        private string? _profileName;
        private string? _brokerURL;
        private uint? _brokerPort;
        private string? _clientId;
        private string? _userName;
        private string? _password;
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
