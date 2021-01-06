using MQTTSniffer.Model;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System.IO;

namespace MQTTSniffer.Dialogs.Documents
{
    public class EditBrokerDialogViewModel : ReactiveObject
    {
        private string? _brokerURL;
        private uint? _brokerPort;
        private string? _clientId;
        private string? _protocolVersion;
        private string? _userName;
        private string? _password;
        private string _tlsEnabled = "Disabled";
        private bool _isOnTlsEnabled;
        private string? _clientCertificateText;

        private BrokerEntity? _entityCopy;
        public ObservableCollection<string> ProtocolVersions { get; set; } = new ObservableCollection<string>();
        public EditBrokerDialogViewModel(BrokerEntity? brokerEntity=null)
        {
            if (brokerEntity != null)
            {
                _entityCopy = brokerEntity.Clone() as BrokerEntity;
                _brokerURL = brokerEntity.URL;
                _brokerPort = brokerEntity.Port;
                _clientId = brokerEntity.ClientId;
                _protocolVersion = brokerEntity.ProtocolVersion.ToString();
                _userName = brokerEntity.UserName;
                _password = brokerEntity.Password;
                _isOnTlsEnabled = brokerEntity.TlsEnabled;
                _clientCertificateText = brokerEntity.ClientCertificate;
            }
            foreach (var item in Enum.GetNames(typeof(BrokerEntity.eProtocolVersion)))
            {
                ProtocolVersions.Add(item);
            }
        }
        public BrokerEntity? GetEntity()
        {
            if (_entityCopy != null)
            {
                _entityCopy.URL = _brokerURL;
                _entityCopy.Port = _brokerPort;
                _entityCopy.ClientId = _clientId;
                _entityCopy.UserName = _userName;
                _entityCopy.Password = _password;
                _entityCopy.ProtocolVersion = (BrokerEntity.eProtocolVersion)Enum.Parse(typeof(BrokerEntity.eProtocolVersion), _protocolVersion ?? BrokerEntity.eProtocolVersion.V310.ToString());
                _entityCopy.TlsEnabled = _isOnTlsEnabled;
                _entityCopy.ClientCertificate = _clientCertificateText;
            }
            return _entityCopy;
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
        #region Security
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
        public string TlsEnabledText
        {
            get => _tlsEnabled;
            set => this.RaiseAndSetIfChanged(ref _tlsEnabled, value);
        }
        public bool IsOnTlsEnabled
        {
            get => _isOnTlsEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _isOnTlsEnabled, value);
                TlsEnabledText = _isOnTlsEnabled ? "Enabled" : "Disabled";
            }
        }
        public string? ClientCertificateText
        {
            get => _clientCertificateText;
            set => this.RaiseAndSetIfChanged(ref _clientCertificateText, value);
        }

        public async void OnLoadClientCertCommand()
        {
            var dlg = new OpenFileDialog();
            dlg.Filters.Add(new FileDialogFilter() { Name = "Broker certificate", Extensions = { "cert","crt","pem" } });
            dlg.Filters.Add(new FileDialogFilter() { Name = "All files", Extensions = { "*" } });
            dlg.AllowMultiple = false;
            var result = await dlg.ShowAsync(GetWindow());
            if (result != null && result.Length > 0)
            {
                ClientCertificateText = File.ReadAllText(result[0]);
            }
        }
        private Window? GetWindow()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return desktopLifetime.MainWindow;
            }
            return null;
        }

        #endregion
        public void OnMQTTClientIdGenerateCommand()
        {
            ClientId = Guid.NewGuid().ToString();
        }
    }
}
