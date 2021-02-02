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
    public class PublishViewModel : Document, IObserver<MQTTMessage>
    {
        public ObservableCollection<MQTTMessage> TopicItems { get; set; } = new ObservableCollection<MQTTMessage>();

        private IDisposable _messageTrackerDisposable;
        private Regex? _regex;
        private string _selectedContent;
        private readonly IIEncoderDecoderService _encoderDecoderService;
        // is fired when this doc is closed
        public event EventHandler OnClosedEvent;

        private MQTTMessage _selectedItem;
        public MQTTMessage SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                if (_selectedEncodeDeocder != null)
                    SelectedContent = _selectedEncodeDeocder.Decode(_selectedItem.Topic, _selectedItem.Payload);// Encoding.UTF8.GetString(_selectedItem.Payload);
            }
        }

        public string SelectedContent
        {
            get => _selectedContent;
            set => this.RaiseAndSetIfChanged(ref _selectedContent, value);
        }

        public ObservableCollection<string> Decoders { get; set; } = new ObservableCollection<string>();

        private IEncoderDecoder _selectedEncodeDeocder;
        private string? _selectedDecoderName;
        public string? SelectedDecoder
        {
            get => _selectedDecoderName;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedDecoderName, value);
                _selectedEncodeDeocder = _encoderDecoderService.GetDecoder(value);
            }
        }

        public PublishViewModel(MQTTMessageTracker tracker, IIEncoderDecoderService encodeDecodeService)
        {
            _messageTrackerDisposable = tracker.Subscribe(this);
            _encoderDecoderService = encodeDecodeService;
            foreach (var decoder in _encoderDecoderService.GetDecoderNames())
            {
                Decoders.Add(decoder);
            }
            SelectedDecoder = Decoders.First();
        }

        public override bool OnClose()
        {
            OnClosedEvent?.Invoke(this, new EventArgs());
            _messageTrackerDisposable?.Dispose();
            return base.OnClose();
        }

        #region IObserver
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(MQTTMessage value)
        {
            if (_regex == null)
            {
                var topicSplit = Title.Split('/');
                StringBuilder b = new StringBuilder();
                foreach (var item in topicSplit)
                {
                    if (b.Length > 0)
                        b.Append(@"/");
                    if (item == "#")
                    {
                        // match everything
                        b.Append(@"[\w\d/]+");
                        break;
                    }
                    else if (item == "+")
                    {
                        // match any char or digit
                        b.Append(@"[\w\d]+");
                    }
                    else
                        b.Append(item);
                }
                _regex = new Regex(b.ToString());
            }
            if (_regex.IsMatch(value.Topic))
            {
                TopicItems.Add(value);
            }
        }
        #endregion
    }
}
