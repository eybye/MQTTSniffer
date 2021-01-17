using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Client.Receiving;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTSniffer.Model;
using System.Text;
using Splat;

namespace MQTTSniffer
{
    public class MQTTClient : IDisposable, IEnableLogger, IMqttApplicationMessageReceivedHandler, IMqttClientConnectedHandler, IMqttClientDisconnectedHandler
    {
        enum ConnectResult
        {
            ConnectionAccepted = 0,
            ConnectionRefused_1 = 1, //, unacceptable protocol version
            ConnectionRefused_2 = 2, //, identifier rejected
            ConnectionRefused_3 = 3, //, Server unavailable
            ConnectionRefused_4 = 4, //, bad user name or password
            ConnectionRefused_5 = 5 //, not authorized
        }

        enum SubscribeResult
        {
            Success_Maximum_QoS_0 = 0,
            Success_Maximum_QoS_1 = 1,
            Success_Maximum_QoS_2 = 2,
            Failure = 128
        }

        private IMqttClient? _client;
        private IMqttClientOptions _options;
        private bool _isStopping = false;
        private Task _waitTask = null;
        private uint _responseWaitTime = 10;

        public bool Online { get; set; }

        public MQTTClient(BrokerEntity brokerEntity)
        {
            var host = brokerEntity.URL;
            int port = 1883;
            if (brokerEntity.Port.HasValue)
                port = (int)brokerEntity.Port.Value;

            var username = brokerEntity.UserName;
            var password = brokerEntity.Password;

            MQTTnet.Formatter.MqttProtocolVersion protocolVersion = MQTTnet.Formatter.MqttProtocolVersion.V311;
            switch (brokerEntity.ProtocolVersion)
            {
                case BrokerEntity.eProtocolVersion.V310:
                    protocolVersion = MQTTnet.Formatter.MqttProtocolVersion.V310;
                    break;

                case BrokerEntity.eProtocolVersion.V311:
                    protocolVersion = MQTTnet.Formatter.MqttProtocolVersion.V311;
                    break;

                case BrokerEntity.eProtocolVersion.V500:
                    protocolVersion = MQTTnet.Formatter.MqttProtocolVersion.V500;
                    break;
            }

            var keepAlive = 1000;
            var clientid = brokerEntity.ClientId;
            _responseWaitTime = 1000;
            // create client instance 
            if (_client != null) Stop();
            _isStopping = false;

            var logger = new MqttNetLogger("MQTTSniffer");
            logger.LogMessagePublished += (sender, args) => { this.Log().Debug(args.LogMessage.Message); };

            _client = new MqttFactory().CreateMqttClient(logger);

            _client.ApplicationMessageReceivedHandler = this;
            _client.ConnectedHandler = this;
            _client.DisconnectedHandler = this;

            //var will = new MqttApplicationMessageBuilder()
            //    .WithPayload("offline")
            //    .WithTopic($"gw/{macAddress}/status")
            //    .WithRetainFlag(true)
            //    .WithAtLeastOnceQoS()
            //    .Build();

            var options = new MqttClientOptionsBuilder()
                .WithCleanSession(true)
                .WithClientId(clientid)
                .WithCredentials(username, password)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(keepAlive))
                .WithCommunicationTimeout(TimeSpan.FromSeconds(30))
                .WithProtocolVersion(protocolVersion)
                //.WithWillMessage(will)
                .WithTcpServer(server =>
                {
                    server.Server = host;
                    server.Port = port;
                });

            if (brokerEntity.TlsEnabled && !string.IsNullOrEmpty(brokerEntity.ClientCertificate))
            {
                X509Certificate x509ServerCert = new X509Certificate2(Encoding.ASCII.GetBytes(brokerEntity.ClientCertificate));

                //string clientCert = string.Empty;
                //X509Certificate x509ClientCert = new X509Certificate2(Path.Combine(loc, clientCert), "test");

                options = options.WithTls(opt =>
                {
                    opt.UseTls = true;
                    opt.SslProtocol = System.Security.Authentication.SslProtocols.Tls12;
                    opt.Certificates = new List<X509Certificate>() { x509ServerCert };
                    opt.CertificateValidationHandler = (context) => true;
                });
            }
            _options = options.Build();
        }

        public void Start()
        {
            if (!Online)
            {
                var task = _client.ConnectAsync(_options);
                task.Wait(TimeSpan.FromSeconds(10));

                if (task.Status == TaskStatus.RanToCompletion)
                {
                    if (this.Log().IsDebugEnabled)
                        this.Log().Debug($"Connect returned: {task.Result.ReasonString}");

                    if (this.Log().IsInfoEnabled) this.Log().Info("Started");

                    Online = true;
                }
            }
        }
        public Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            return Task.CompletedTask;
            //if (!_isStopping)
            //{
            //    Online = false;
            //    if (_log.IsEnabled(LogLevel.Debug)) _log.LogDebug("Connection closed - opening in 1 seconds");
            //    _waitTask = Task.Run(() => Thread.Sleep(TimeSpan.FromSeconds(1)))
            //        .ContinueWith(a => Start(_clientId));
            //}
        }

        public Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            return Task.CompletedTask;
        }

        public void Stop()
        {
            if (_client != null)
            {
                _isStopping = true;
                if (_waitTask != null && !_waitTask.IsCompleted)
                    _waitTask.Wait(TimeSpan.FromSeconds(30));

                if (_client.IsConnected)
                    _client.DisconnectAsync().Wait();
                _client = null;

                Online = false;
            }
        }

        public Action<MQTTMessage> MessageReceived { get; set; }

        public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            return Task.Run(() =>
            {
                MQTTMessage msg = new MQTTMessage
                {
                    Topic = eventArgs.ApplicationMessage.Topic,
                    Payload = eventArgs.ApplicationMessage.Payload,
                    Retain = eventArgs.ApplicationMessage.Retain
                };
                if (eventArgs.ApplicationMessage.QualityOfServiceLevel == MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                    msg.QoS = MQTTMessage.QualityOfServiceLevel.AtMostOnce;
                else if (eventArgs.ApplicationMessage.QualityOfServiceLevel == MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    msg.QoS = MQTTMessage.QualityOfServiceLevel.AtLeastOnce;
                else
                    msg.QoS = MQTTMessage.QualityOfServiceLevel.ExactlyOnce;

                MessageReceived?.Invoke(msg);
            });
        }

        internal Task SendRawMessage(string topic, byte[] data)
        {
            var mqttMsg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithAtLeastOnceQoS()
                .WithPayload(data)
                .Build();

            return _client.PublishAsync(mqttMsg);
        }

        internal Task SubscribeAsync(string topic)
        {
            var topicT = new MqttTopicFilterBuilder()
                                .WithTopic(topic)
                                .WithAtLeastOnceQoS()
                                .Build();

            return _client.SubscribeAsync(topicT);
        }
        internal Task UnsubscribeAsync(string topic)
        {
            return _client.UnsubscribeAsync(topic);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }


        #endregion
    }
}
