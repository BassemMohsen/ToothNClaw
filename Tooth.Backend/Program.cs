using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Tooth.Backend
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var mutex = new Mutex(true, "Tooth.Backend");
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("[Mutex] Only one instance at a time");
                return;
            }

            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Console.WriteLine("[Permission] Should run as Administrator");
                return;
            }

            var comm = new Communication();
            var handler = new Handler();
            handler.Register(comm);
            comm.Run();
        }
    }
}
