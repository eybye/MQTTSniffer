using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Dock.Model;
using MQTTSniffer.Dialogs;
using MQTTSniffer.Dialogs.Documents;
using MQTTSniffer.Model;
using MQTTSniffer.ViewModels.Documents;
using MQTTSniffer.Views.Layouts;
using ReactiveUI;

namespace MQTTSniffer.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IDropTarget, IMainContext//, IActivatableViewModel
    {
        public const string DocumentsDockId = "Topics";

        //private IDockSerializer? _serializer;
        private IFactory? _factory;
        private IDock? _layout;
        private MQTTClient? _client = null;
        private bool _brokerSelectionEnabled = true;
        private CanConnectTracker _connectTracker;
        private CanDisconnectTracker _disconnectTracker;
        private MQTTMessageTracker _messageTracker = new MQTTMessageTracker();
        private string _addEditName = "Add";

        public ObservableCollection<BrokerEntity> BrokerEntities { get; set; } = new ObservableCollection<BrokerEntity>();

        public IReactiveCommand OnConnectCommand { get; }
        public IReactiveCommand OnDisconnectCommand { get; }

        public string SubscribeTopic { get; set; } = string.Empty;

        public BrokerEntity SelectedBrokerEntity { get; set; }
        public bool BrokerSelectionEnabled
        {
            get => _brokerSelectionEnabled;
            set => this.RaiseAndSetIfChanged(ref _brokerSelectionEnabled, value);
        }

        //public IDockSerializer? Serializer
        //{
        //    get => _serializer;
        //    set => this.RaiseAndSetIfChanged(ref _serializer, value);
        //}

        public IFactory? Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        public IDock? Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        public string AddEditName
        {
            get => _addEditName;
            set => this.RaiseAndSetIfChanged(ref _addEditName, value);
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<Boolean>> _observers;
            private IObserver<Boolean> _observer;

            public Unsubscriber(List<IObserver<Boolean>> observers, IObserver<Boolean> observer)
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
        class BaseTracker : IObservable<Boolean>
        {
            protected List<IObserver<Boolean>> _observers = new List<IObserver<bool>>();

            public IDisposable Subscribe(IObserver<bool> observer)
            {
                if (!_observers.Contains(observer))
                    _observers.Add(observer);
                return new Unsubscriber(_observers, observer);
            }
        }
        class CanConnectTracker : BaseTracker
        {
            public void CanConnect(bool state)
            {
                // notify all observers of state change
                _observers.ForEach(o => o.OnNext(state));
            }
        }
        class CanDisconnectTracker : BaseTracker
        {
            public void CanDisconnect(bool state)
            {
                // notify all observers of state change
                _observers.ForEach(o => o.OnNext(state));
            }
        }
        public MainWindowViewModel()
        {
            BrokerEntities.Add(new BrokerEntity { ProfileName = "Add new...", IsDummy = true });
            SelectedBrokerEntity = BrokerEntities[0];

            _connectTracker = new CanConnectTracker();
            _disconnectTracker = new CanDisconnectTracker();
            OnConnectCommand = ReactiveCommand.Create(() => OnConnectAction(), _connectTracker);
            OnDisconnectCommand = ReactiveCommand.Create(() => OnDisconnectAction(), _disconnectTracker);
        }
        private Encoding GetEncoding(string path)
        {
            using (var reader = new StreamReader(path, Encoding.Default, true))
            {
                if (reader.Peek() >= 0)
                {
                    reader.Read();
                }
                return reader.CurrentEncoding;
            }
        }

        // triggered when selection changes on combobox
        public void BrokerSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as BrokerEntity;
                if (BrokerEntities.IndexOf(item) > 0)
                {
                    AddEditName = "Edit";
                    _connectTracker.CanConnect(true);
                }
                else
                {
                    AddEditName = "Add";
                    _connectTracker.CanConnect(false);
                }
            }
        }
    //private SubscribeViewModel OpenFileViewModel(string path)
    //{
    //    Encoding encoding = GetEncoding(path);
    //    string text = File.ReadAllText(path, encoding);
    //    string title = Path.GetFileName(path);
    //    return new SubscribeViewModel()
    //    {
    //        Title = title,
    //        Text = text,
    //        Encoding = encoding.WebName
    //    };
    //}

    //private void SaveFileViewModel(SubscribeViewModel fileViewModel)
    //{
    //    File.WriteAllText(fileViewModel.Path, fileViewModel.Text, Encoding.GetEncoding(fileViewModel.Encoding));
    //}

    //private void UpdateFileViewModel(SubscribeViewModel fileViewModel, string path)
    //{
    //    fileViewModel.Title = Path.GetFileName(path);
    //}

        private void AddSubscribeViewModel(TopicViewModel topicViewModel)
        {
            if (Layout?.ActiveDockable is IDock active)
            {
                if (active.Factory?.FindDockable(active, (d) => d.Id == DocumentsDockId) is IDock dock)
                {
                    Factory?.AddDockable(dock, topicViewModel);
                    Factory?.SetActiveDockable(topicViewModel);
                    Factory?.SetFocusedDockable(Layout, topicViewModel);
                }
            }
        }

        private TopicViewModel? GetActiveSubscribeViewModel()
        {
            if (Layout?.ActiveDockable is IDock active)
            {
                if (active.Factory?.FindDockable(active, (d) => d.Id == DocumentsDockId) is IDock dock)
                {
                    return dock.ActiveDockable as TopicViewModel;
                }
            }
            return null;
        }

        public void NewSubscriptionCommand()
        {
            if (!string.IsNullOrEmpty(SubscribeTopic))
            {
                // if document window already exist with that name, switch to it
                if (Layout?.ActiveDockable is IDock active)
                {
                    if (active.Factory?.FindDockable(active, (d) => d.Title == SubscribeTopic) is IDockable dock)
                    {
                        Factory?.SetActiveDockable(dock);
                        Factory?.SetFocusedDockable(Layout, dock);
                        return;
                    }
                }
                var topicViewModel = new TopicViewModel(_messageTracker)
                {
                    Title = SubscribeTopic,
                };

                AddSubscribeViewModel(topicViewModel);
                if (_client != null)
                    _client.Subscribe(SubscribeTopic);
            }
        }

        public async void OnEditBrokerUrlCommand()
        {
            var edit = new EditBrokerDialog();

            BrokerEntity? changingEntity = SelectedBrokerEntity.IsDummy ? null : SelectedBrokerEntity;

            EditBrokerDialogViewModel editBrokerViewModel = new EditBrokerDialogViewModel(changingEntity);
            edit.DataContext = editBrokerViewModel;
            var result = await edit.ShowDialog<string>(GetWindow());
            if (Constants.SAVE.Equals(result))
            {
                // save
                if (changingEntity == null)
                {
                    BrokerEntities.Add(new BrokerEntity
                    {
                        ProfileName = editBrokerViewModel.ProfileName,
                        URL = editBrokerViewModel.BrokerURL,
                        Port = editBrokerViewModel.BrokerPort,
                        ClientId = editBrokerViewModel.ClientId,
                        UserName = editBrokerViewModel.UserName,
                        Password = editBrokerViewModel.Password
                    });
                }
                else
                {
                    changingEntity.ProfileName = editBrokerViewModel.ProfileName;
                    changingEntity.URL = editBrokerViewModel.BrokerURL;
                    changingEntity.Port = editBrokerViewModel.BrokerPort;
                    changingEntity.ClientId = editBrokerViewModel.ClientId;
                    changingEntity.UserName = editBrokerViewModel.UserName;
                    changingEntity.Password = editBrokerViewModel.Password;
                }
            }
            else if (Constants.DELETE.Equals(result))
            {
                // delete
                if (SelectedBrokerEntity != null)
                    BrokerEntities.Remove(SelectedBrokerEntity);
            }
        }

        private void OnConnectAction()
        {
            if (_client == null && SelectedBrokerEntity != null)
            {
                _client = new MQTTClient(SelectedBrokerEntity);
                _client.MessageReceived = MQTTMessageReceived;
                _client.Start();

                _connectTracker.CanConnect(false);
                _disconnectTracker.CanDisconnect(true);
                BrokerSelectionEnabled = false;

                // subscribe on all topics
            }
        }

        private void OnDisconnectAction()
        {
            if (_client != null)
            {
                _client.Stop();
                _client.Dispose();
                _client = null;

                _connectTracker.CanConnect(true);
                _disconnectTracker.CanDisconnect(false);
                BrokerSelectionEnabled = true;
            }
        }
        private void MQTTMessageReceived(MQTTMessage message)
        {
            _messageTracker.NewMessage(message);
        }

        //public async void FileOpen()
        //{
        //    var dlg = new OpenFileDialog();
        //    dlg.Filters.Add(new FileDialogFilter() { Name = "Text document", Extensions = { "txt" } });
        //    dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
        //    dlg.AllowMultiple = true;
        //    var result = await dlg.ShowAsync(GetWindow());
        //    if (result != null && result.Length > 0)
        //    {
        //        foreach (var path in result)
        //        {
        //            if (!string.IsNullOrEmpty(path))
        //            {
        //                var untitledFileViewModel = OpenFileViewModel(path);
        //                if (untitledFileViewModel != null)
        //                {
        //                    AddFileViewModel(untitledFileViewModel);
        //                }
        //            }
        //        }
        //    }
        //}

        //public async void FileSave()
        //{
        //    if (GetFileViewModel() is SubscribeViewModel fileViewModel)
        //    {
        //        if (string.IsNullOrEmpty(fileViewModel.Path))
        //        {
        //            await FileSaveAsImpl(fileViewModel);
        //        }
        //        else
        //        {
        //            SaveFileViewModel(fileViewModel);
        //        }
        //    }
        //}

        //public async void FileSaveAs()
        //{
        //    if (GetFileViewModel() is SubscribeViewModel fileViewModel)
        //    {
        //        await FileSaveAsImpl(fileViewModel);
        //    }
        //}

        //public async Task FileSaveAsImpl(SubscribeViewModel fileViewModel)
        //{
        //    var dlg = new SaveFileDialog();
        //    dlg.Filters.Add(new FileDialogFilter() { Name = "Text document", Extensions = { "txt" } });
        //    dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
        //    dlg.InitialFileName = fileViewModel.Title;
        //    dlg.DefaultExtension = "txt";
        //    var result = await dlg.ShowAsync(GetWindow());
        //    if (result != null)
        //    {
        //        if (!string.IsNullOrEmpty(result))
        //        {
        //            UpdateFileViewModel(fileViewModel, result);
        //            SaveFileViewModel(fileViewModel);
        //        }
        //    }
        //}

        public void FileExit()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Shutdown();
            }
        }

        public void DragOver(object? sender, DragEventArgs e)
        {
            if (!e.Data.Contains(DataFormats.FileNames))
            {
                e.DragEffects = DragDropEffects.None; 
                e.Handled = true;
            }
        }

        public void Drop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames))
            {
                var result = e.Data.GetFileNames();

                foreach (var path in result)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        //var untitledFileViewModel = OpenFileViewModel(path);
                        //if (untitledFileViewModel != null)
                        //{
                        //    AddSubscribeViewModel(untitledFileViewModel);
                        //}
                    }
                }

                e.Handled = true;
            }
        }

        private void CopyDocuments(IDock source, IDock target, string id)
        {
            if (source.Factory?.FindDockable(source, (d) => d.Id == id) is IDock sourceFiles
                && target.Factory?.FindDockable(target, (d) => d.Id == id) is IDock targetFiles
                && sourceFiles.VisibleDockables != null
                && targetFiles.VisibleDockables != null)
            {
                targetFiles.VisibleDockables.Clear();
                targetFiles.ActiveDockable = null;

                foreach (var visible in sourceFiles.VisibleDockables)
                {
                    targetFiles.VisibleDockables.Add(visible);
                }

                targetFiles.ActiveDockable = sourceFiles.ActiveDockable;
            }
        }

        public async void WindowSaveWindowLayout()
        {
            if (GetWindow() is Window onwer)
            {
                var window = new SaveWindowLayoutWindow();

                // TODO:

                await window.ShowDialog(onwer);
            }

            // TODO:

            if (Layout?.ActiveDockable is IDock active)
            {
                var clone = (IDock?)active.Clone();
                if (clone != null)
                {
                    clone.Title = clone.Title + "-copy";
                    active.Close();
                    Layout.Factory?.AddDockable(Layout, clone);
                    Layout.Navigate(clone);
                    Layout.Factory?.SetFocusedDockable(Layout, clone);
                    Layout.DefaultDockable = clone;
                }
            }
        }

        public void WindowApplyWindowLayout(IDock dock)
        {
            if (Layout?.ActiveDockable is IDock active && dock != active)
            {
                active.Close();
                CopyDocuments(active, dock, DocumentsDockId);
                Layout.Navigate(dock);
                Layout.Factory?.SetFocusedDockable(Layout, dock);
                Layout.DefaultDockable = dock;
            }
        }

        public async void WindowManageWindowLayouts()
        {
            if (GetWindow() is Window onwer)
            {
                var window = new ManageWindowLayoutsWindow();

                // TODO:

                await window.ShowDialog(onwer);
            }
        }

        public async void WindowResetWindowLayout()
        {
            if (GetWindow() is Window onwer)
            {
                var window = new ResetWindowLayoutWindow();

                // TODO:

                await window.ShowDialog(onwer);
            }

            // TODO:

            if (Layout?.ActiveDockable is IDock active)
            {
                var layout = Factory?.CreateLayout();
                if (layout != null)
                {
                    Factory?.InitLayout(layout);
                    CopyDocuments(active, layout, DocumentsDockId);
                    Layout?.Close();
                    Layout = layout;
                }
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
    }
}
