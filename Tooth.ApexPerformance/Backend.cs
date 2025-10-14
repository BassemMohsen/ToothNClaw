using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Media.Protection.PlayReady;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Tooth
{
    internal class Backend
    {
        public event EventHandler<string> MessageReceivedEvent;
        public event EventHandler ClosedOrFailedEvent;

        private static Backend _instance;
        public static Backend Instance => _instance ?? (_instance = new Backend());

        private NamedPipeClientStream _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        private Task _loop;

        public bool IsConnected => _client.IsConnected;

        private Backend()
        {
            _client = new NamedPipeClientStream(".", @"LOCAL\ToothPipe",
                PipeDirection.InOut, PipeOptions.Asynchronous);

            _reader = new StreamReader(_client);
            _writer = new StreamWriter(_client);

            _loop = Task.Run(Loop);
        }

        public static async Task LaunchBackend()
        {
            ApplicationData.Current.LocalSettings.Values["PackageSid"] = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper();
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
        }

        private void Loop()
        {
            while (true)
            {
                if (!_client.IsConnected)
                {
                    ClosedOrFailedEvent?.Invoke(this, null);
                    _client.Connect();
                }
                try
                {
                    string message = _reader.ReadLine();

                    if (message != null)
                        MessageReceivedEvent?.Invoke(this, message);
                }
                catch { }
            }
        }

        public void Send(string message)
        {
            try
            {
                lock (_writer)
                {
                    _writer.WriteLine(message);
                    _writer.Flush();
                }
            }
            catch
            {
                ClosedOrFailedEvent?.Invoke(this, null);
            }
        }
    }
}
