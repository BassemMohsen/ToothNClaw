using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Tooth.Backend
{
    internal class Communication
    {
        private readonly NamedPipeServerStream _server;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public EventHandler ConnectedEvent {  get; set; }
        public EventHandler DisconnectedEvent {  get; set; }
        public EventHandler<string> ReceivedEvent { get; set; }

        public Communication(string packageSid)
        {
            Console.WriteLine($"[Connection] Package SID: {packageSid}");
            var pipeName = $"Sessions\\{Process.GetCurrentProcess().SessionId}\\AppContainerNamedObjects\\{packageSid}\\ToothPipe";
            Console.WriteLine($"[Connection] Pipe name: {pipeName}");

            _server = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut, 1,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous, 128, 128, GetPipeSecurity(packageSid));
            _reader = new StreamReader(_server);
            _writer = new StreamWriter(_server);
        }

        private static PipeSecurity GetPipeSecurity(string packageSid)
        {
            var ps = new PipeSecurity();
            var clientRule = new PipeAccessRule(
                new SecurityIdentifier(packageSid),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow);
            var ownerRule = new PipeAccessRule(
                WindowsIdentity.GetCurrent().User,
                PipeAccessRights.FullControl,
                AccessControlType.Allow);
            ps.AddAccessRule(clientRule);
            ps.AddAccessRule(ownerRule);
            return ps;
        }

        public async Task Run(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        Console.WriteLine("[Connection] Waiting for connection...");

                        // WaitForConnection is blocking — move to Task.Run so we can cancel
                        await Task.Run(() => _server.WaitForConnection(), token);
                        if (token.IsCancellationRequested) break;

                        Console.WriteLine("[Connection] Connection established");
                        ConnectedEvent?.Invoke(this, EventArgs.Empty);

                        using var reader = _reader;
                        while (_server.IsConnected && !token.IsCancellationRequested)
                        {
                            // Check for cancellation in between blocking reads
                            if (token.IsCancellationRequested)
                                break;

                            string? message = null;

                            // ReadLine() can block, so wrap in Task.Run to allow cancellation
                            try
                            {
                                message = await Task.Run(() => reader.ReadLine(), token);
                            }
                            catch (OperationCanceledException)
                            {
                                break;
                            }

                            if (message == null)
                            {
                                // Null => disconnected or EOF
                                Console.WriteLine("[Connection] Stream closed");
                                break;
                            }

                            Console.WriteLine($"[Server Connection] Received: {message}");
                            ReceivedEvent?.Invoke(this, message);
                        }
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("[Connection] IO Exception — likely disconnected");
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("[Connection] Cancelled");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Connection] Exception: {ex}");
                    }
                    finally
                    {
                        if (_server.IsConnected)
                        {
                            try { _server.Disconnect(); } catch { }
                        }

                        DisconnectedEvent?.Invoke(this, EventArgs.Empty);
                    }

                    // Small delay before retrying, avoids busy loop on rapid reconnects
                    await Task.Delay(500, token);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Connection] Run loop cancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Connection] Unhandled exception in Run: {ex}");
            }
            finally
            {
                try
                {
                    if (_server.IsConnected)
                        _server.Disconnect();
                }
                catch { }

                Console.WriteLine("[Connection] Run loop exited");
            }
        }

        public void Send(string message)
        {
            if (!_server.IsConnected || message == null)
                return;

            Console.WriteLine($"[Connection] Sent: {message}");
            _writer.WriteLine(message);
            _writer.Flush();
        }
    }
}
