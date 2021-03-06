﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Linq;
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
using Newtonsoft.Json.Linq;
using ReactiveUI;
using MQTTSniffer.Extensions;
using MQTTSniffer.Common;
using Splat;
using MQTTSniffer.Services;

namespace MQTTSniffer.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IDropTarget, IMainContext//, IActivatableViewModel
    {
        public const string DocumentsDockId = "Topics";
        private string _titleText = "MQTT Sniffer";
        private IFactory? _factory;
        private IDock? _layout;
        private MQTTClient? _client = null;
        private CanConnectTracker _connectTracker;
        private CanDisconnectTracker _disconnectTracker;
        private MQTTMessageTracker _messageTracker = new MQTTMessageTracker();
        private BrokerEntity? SelectedBrokerEntity;
        private IExtensionImporter _extensionImporter;
        public IReactiveCommand OnConnectCommand { get; }
        public IReactiveCommand OnDisconnectCommand { get; }

        public string SubscribeTopic { get; set; } = string.Empty;

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

        public string TitleText
        {
            get => _titleText;
            set => this.RaiseAndSetIfChanged(ref _titleText, value);
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
        public MainWindowViewModel(IExtensionImporter extensionImporter)
        {
            _extensionImporter = extensionImporter;
            _connectTracker = new CanConnectTracker();
            _disconnectTracker = new CanDisconnectTracker();
            OnConnectCommand = ReactiveCommand.Create(() => OnConnectAction(), _connectTracker);
            OnDisconnectCommand = ReactiveCommand.Create(() => OnDisconnectAction(), _disconnectTracker);
        }
        private IEncoderDecoder? _encodeDecode;
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
        private void UpdateTitle()
        {
            if (SelectedBrokerEntity != null)
            {
                TitleText = $"MQTT Sniffer - {SelectedBrokerEntity.FileName}";
                if (SelectedBrokerEntity.IsDirty)
                    TitleText += "*";
            }
            else
            {
                TitleText = "MQTT Sniffer";
            }
        }
        private BrokerEntity? OpenFileViewModel(string path)
        {
            Encoding encoding = GetEncoding(path);
            string text = File.ReadAllText(path, encoding);
            string title = Path.GetFileName(path);

            try
            {
                JObject jObject = JObject.Parse(text);

                var nn = jObject.ToObject<BrokerEntity>();
                nn.FileName = title;
                nn.FilePath = path;
                return nn;
            }
            catch (Exception e)
            {
            }
            return null;
        }

        private void SaveFileViewModel(BrokerEntity fileViewModel)
        {
            JObject j = JObject.FromObject(fileViewModel);

            File.WriteAllText(fileViewModel.FilePath, j.ToString(Newtonsoft.Json.Formatting.Indented), Encoding.UTF8);
            if (SelectedBrokerEntity != null)
                SelectedBrokerEntity.IsDirty = false;
            UpdateTitle();
        }

        private void UpdateFileViewModel(BrokerEntity fileViewModel, string path)
        {
            fileViewModel.FilePath = path;
            fileViewModel.FileName = Path.GetFileName(path);

            UpdateTitle();
        }

        private List<SubscribeViewModel> _subscribeViewModels = new List<SubscribeViewModel>();

        private void AddSubscribeViewModel(SubscribeViewModel subscribeViewModel)
        {
            if (Layout?.ActiveDockable is IDock active)
            {
                if (active.Factory?.FindDockable(active, (d) => d.Id == DocumentsDockId) is IDock dock)
                {
                    Factory?.AddDockable(dock, subscribeViewModel);
                    Factory?.SetActiveDockable(subscribeViewModel);
                    Factory?.SetFocusedDockable(Layout, subscribeViewModel);
                    
                    subscribeViewModel.OnClosedEvent += SubscribeViewModel_OnClosedEvent;

                    _subscribeViewModels.Add(subscribeViewModel);
                }
            }
        }
        private void AddPublishViewModel(PublishViewModel publishViewModel)
        {
            if (Layout?.ActiveDockable is IDock active)
            {
                if (active.Factory?.FindDockable(active, (d) => d.Id == DocumentsDockId) is IDock dock)
                {
                    Factory?.AddDockable(dock, publishViewModel);
                    Factory?.SetActiveDockable(publishViewModel);
                    Factory?.SetFocusedDockable(Layout, publishViewModel);
                //    _subscribeViewModels.Add(subscribeViewModel);
                }
            }
        }
        private void CloseAllSubscribeViews()
        {
            foreach (var item in _subscribeViewModels)
            {
                Factory?.RemoveDockable(item, true);
            }
            _subscribeViewModels.Clear();
        }

        private SubscribeViewModel? GetActiveSubscribeViewModel()
        {
            if (Layout?.ActiveDockable is IDock active)
            {
                if (active.Factory?.FindDockable(active, (d) => d.Id == DocumentsDockId) is IDock dock)
                {
                    return dock.ActiveDockable as SubscribeViewModel;
                }
            }
            return null;
        }

        public async void NewSubscriptionCommand()
        {
            if (!string.IsNullOrEmpty(SubscribeTopic))
            {
                // if document window already exist with that name, switch to it
                if (Layout?.ActiveDockable is IDock active)
                {
                    if (active.Factory?.FindDockable(active, (d) => d.Title == SubscribeTopic) is IDock dock)
                    {
                        Factory?.SetActiveDockable(dock);
                        Factory?.SetFocusedDockable(Layout, dock);
                        return;
                    }
                }
                var service = Bootstrapper.ServiceProvider.GetService<IIEncoderDecoderService>();
                var subscribeViewModel = new SubscribeViewModel(_messageTracker, service)
                {
                    Title = SubscribeTopic,
                };
                AddSubscribeViewModel(subscribeViewModel);
                if (SelectedBrokerEntity != null && !SelectedBrokerEntity.Topics.Contains(SubscribeTopic))
                {
                    SelectedBrokerEntity.Topics.Add(SubscribeTopic);
                    SelectedBrokerEntity.IsDirty = true;
                    UpdateTitle();
                }

                if (_client != null)
                    await _client.SubscribeAsync(SubscribeTopic);
            }
        }

        public async void NewPublishCommand()
        {
            // if document window already exist with that name, switch to it
            //if (Layout?.ActiveDockable is IDock active)
            //{
            //    if (active.Factory?.FindDockable(active, (d) => d.Title == SubscribeTopic) is IDock dock)
            //    {
            //        Factory?.SetActiveDockable(dock);
            //        Factory?.SetFocusedDockable(Layout, dock);
            //        return;
            //    }
            //}
            var service = Bootstrapper.ServiceProvider.GetService<IIEncoderDecoderService>();
            var publishViewModel = new PublishViewModel(this, service);
            AddPublishViewModel(publishViewModel);
        }
        private async void SubscribeViewModel_OnClosedEvent(object? sender, EventArgs e)
        {
            if (sender is SubscribeViewModel tvm)
            {
                tvm.OnClosedEvent -= SubscribeViewModel_OnClosedEvent;

                if (SelectedBrokerEntity != null && SelectedBrokerEntity.Topics.Contains(tvm.Title))
                {
                    SelectedBrokerEntity.Topics.Remove(tvm.Title);
                    SelectedBrokerEntity.IsDirty = true;
                    UpdateTitle();
                }

                _subscribeViewModels.Remove(tvm);
                // unsubscribe
                if (_client != null)
                    await _client.UnsubscribeAsync(tvm.Title);
            }
        }

        private async void OnConnectAction()
        {
            if (_client == null && SelectedBrokerEntity != null)
            {
                _client = new MQTTClient(SelectedBrokerEntity);
                _client.MessageReceived = MQTTMessageReceived;
                _client.Start();

                _connectTracker.CanConnect(false);
                _disconnectTracker.CanDisconnect(true);

                // subscribe on all topics
                foreach (var topic in SelectedBrokerEntity.Topics)
                {
                    await _client.SubscribeAsync(topic);
                }
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
            }
        }

        private void MQTTMessageReceived(MQTTMessage message)
        {
            _messageTracker.NewMessage(message);
        }

        #region FileCommands

        public void FileNewCommand()
        {
            if (_client != null)
            {
                OnDisconnectAction();
            }
            _connectTracker.CanConnect(false);

            SelectedBrokerEntity = null;
            UpdateTitle();
            CloseAllSubscribeViews();
        }
        public async void FileOpenCommand()
        {
            var dlg = new OpenFileDialog();
            dlg.Filters.Add(new FileDialogFilter() { Name = "MQTT Sniffer config", Extensions = { "json" } });
            dlg.AllowMultiple = false;
            var result = await dlg.ShowAsync(GetWindow());
            if (result != null && result.Length > 0)
            {
                foreach (var path in result)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        SelectedBrokerEntity = OpenFileViewModel(path);
                        if (SelectedBrokerEntity != null)
                        {
                            if (_client != null)
                            {
                                OnDisconnectAction();
                            }
                            UpdateTitle();
                            _extensionImporter.ResolvePlugins(SelectedBrokerEntity.PluginPath);
                            _connectTracker.CanConnect(true);
                            CloseAllSubscribeViews();
                            var service = Bootstrapper.ServiceProvider.GetService<IIEncoderDecoderService>();
                            foreach (var topic in SelectedBrokerEntity.Topics)
                            {
                                AddSubscribeViewModel(new SubscribeViewModel(_messageTracker, service) { Title = topic });
                            }
                        }
                    }
                }
            }
        }

        public async void FileSaveCommand()
        {
            if (SelectedBrokerEntity != null)
            {
                if (string.IsNullOrEmpty(SelectedBrokerEntity.FilePath))
                {
                    await FileSaveAsImpl(SelectedBrokerEntity);
                }
                else
                {
                    SaveFileViewModel(SelectedBrokerEntity);
                }
            }
        }

        public async void FileSaveAsCommand()
        {
            if (SelectedBrokerEntity != null)
            {
                await FileSaveAsImpl(SelectedBrokerEntity);
            }
        }

        public async Task FileSaveAsImpl(BrokerEntity fileViewModel)
        {
            var dlg = new SaveFileDialog();
            dlg.Filters.Add(new FileDialogFilter() { Name = "MQTT Sniffer config", Extensions = { "json" } });
            //dlg.InitialFileName = fileViewModel.ProfileName;
            dlg.DefaultExtension = "json";
            var result = await dlg.ShowAsync(GetWindow());
            if (result != null)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    UpdateFileViewModel(fileViewModel, result);
                    SaveFileViewModel(fileViewModel);
                }
            }
        }

        public void FileExitCommand()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Shutdown();
            }
        }
        #endregion

        #region EditCommands
        public async void EditSettingsCommand()
        {
            var edit = new EditBrokerDialog();

            EditBrokerDialogViewModel editBrokerViewModel = new EditBrokerDialogViewModel(SelectedBrokerEntity);
            edit.DataContext = editBrokerViewModel;
            var result = await edit.ShowDialog<string>(GetWindow());
            if (Constants.SAVE.Equals(result))
            {
                // save
                SelectedBrokerEntity = editBrokerViewModel.GetEntity();
                _extensionImporter.ResolvePlugins(SelectedBrokerEntity.PluginPath);
                _connectTracker.CanConnect(true);
            }
        }

        #endregion
        #region DragDrop
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
        #endregion

        #region WindowLayout
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
        #endregion
        private Window? GetWindow()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return desktopLifetime.MainWindow;
            }
            return null;
        }

        #region IMainContext
        public void Publish(MQTTMessage message)
        {
            _client.SendRawMessage(message.Topic, message.Payload);
        }

        #endregion
    }
}
