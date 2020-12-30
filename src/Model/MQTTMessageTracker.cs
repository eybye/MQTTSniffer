using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTSniffer.Model
{
    public class MQTTObserverUnsubscribe : IDisposable
    {
        private List<IObserver<MQTTMessage>> _observers;
        private IObserver<MQTTMessage> _observer;

        public MQTTObserverUnsubscribe(List<IObserver<MQTTMessage>> observers, IObserver<MQTTMessage> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
    public class MQTTMessageTracker : IObservable<MQTTMessage>
    {
        protected List<IObserver<MQTTMessage>> _observers = new List<IObserver<MQTTMessage>>();

        public IDisposable Subscribe(IObserver<MQTTMessage> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new MQTTObserverUnsubscribe(_observers, observer);
        }

        public void NewMessage(MQTTMessage message)
        {
            _observers.ForEach(o => o.OnNext(message));
        }
    }
}
