using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace IntelAPUSettings.Backend
{
    internal class Handler
    {
        //private IntelAdj _adj;
        private CpuBoostController cpuBoostController;

        public Handler()
        {
            cpuBoostController = new CpuBoostController();
            //_adj = IntelAdj.Open();
        }

        public void Register(Communication comm)
        {
            comm.ConnectedEvent += OnConnected;
            comm.ReceivedEvent += OnReceived;
        }

        void OnConnected(object sender, EventArgs e)
        {
            (sender as Communication).Send("connected");
        }

        void OnReceived(object sender, string message)
        {
            var comm = sender as Communication;
            string[] args = message.Split(' ');
            if (args.Length == 0)
                return;
            switch (args[0])
            {
                case "set-boost":
                    {
                        if (cpuBoostController == null)
                        {
                            cpuBoostController = new CpuBoostController();
                        }
                        Console.WriteLine($"[Handler] Setting CPU Boost to {args[1]}");
                        if (Enum.TryParse(args[1], out CpuBoostController.BoostMode mode))
                        {
                            cpuBoostController.SetBoostMode(mode);
                        }
                        else
                        {
                            Console.WriteLine($"[Handler] Invalid Boost Mode: {args[1]}");
                        }
                    }
                    break;

                case "set-Fps":
                    {
                        Console.WriteLine($"[Handler] Todo: Setting Fps to {args[1]}");
                    }
                    break;

                case "set-EnduranceGaming":
                    {
                        Console.WriteLine($"[Handler] Todo: Setting Endurance Gaming mode to {args[1]}");
                    }
                    break;

                case "set-LowLatencyMode":
                    {
                        Console.WriteLine($"[Handler] Todo: Setting Low Latency mode to {args[1]}");
                    }
                    break;
            }
        }
    }
}
