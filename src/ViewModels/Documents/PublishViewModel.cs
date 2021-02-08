using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Dock.Model.Controls;
using MQTTSniffer.Model;
using ReactiveUI;
using MQTTSniffer.Common;
using MQTTSniffer.Services;

namespace MQTTSniffer.ViewModels.Documents
{
    public class PublishViewModel : Document
    {
        private readonly IIEncoderDecoderService _encoderDecoderService;
        private readonly IMainContext _mainContext;
        // is fired when this doc is closed
        public event EventHandler OnClosedEvent;

        public string PublishContent { get; set; }

        public string PublishTopic { get; set; }

        public ObservableCollection<string> Encoders { get; set; } = new ObservableCollection<string>();

        private IEncoderDecoder _selectedEncodeDeocder;
        private string? _selectedDecoderName;
        public string? SelectedEncoder
        {
            get => _selectedDecoderName;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedDecoderName, value);
                _selectedEncodeDeocder = _encoderDecoderService.GetDecoder(value);
            }
        }

        public PublishViewModel(IMainContext mainContext, IIEncoderDecoderService encodeDecodeService)
        {
            Title = "Publish";
            _mainContext = mainContext;
            _encoderDecoderService = encodeDecodeService;
            foreach (var encoder in _encoderDecoderService.GetDecoderNames())
            {
                Encoders.Add(encoder);
            }
            SelectedEncoder = Encoders.First();
        }

        public void PublishCommand()
        {
            MQTTMessage msg = new MQTTMessage
            {
                Topic = PublishTopic
            };
            msg.Payload = _selectedEncodeDeocder.Encode(msg.Topic, PublishContent);
            _mainContext.Publish(msg);
        }
        public override bool OnClose()
        {
            OnClosedEvent?.Invoke(this, new EventArgs());
            return base.OnClose();
        }
    }
}
