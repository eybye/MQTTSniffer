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

namespace MQTTSniffer.ViewModels.Documents
{
    public class TopicViewModel : Document, IObserver<MQTTMessage>
    {
        public ObservableCollection<MQTTMessage> TopicItems { get; set; } = new ObservableCollection<MQTTMessage>();

        private IDisposable _messageTrackerDisposable;
        private Regex? _regex;
        private string _selectedContent;

        private MQTTMessage _selectedItem;
        public MQTTMessage SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                SelectedContent = Encoding.UTF8.GetString(_selectedItem.Payload);
            }
        }

        public string SelectedContent
        {
            get => _selectedContent;
            set => this.RaiseAndSetIfChanged(ref _selectedContent, value);
        }

        public TopicViewModel(MQTTMessageTracker tracker)
        {
            _messageTrackerDisposable = tracker.Subscribe(this);
        }

        public override bool OnClose()
        {
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
