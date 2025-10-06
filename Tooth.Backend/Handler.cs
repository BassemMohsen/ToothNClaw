using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tooth.GraphicsProcessingUnit;
using Windows.System;
using static Tooth.IGCL.IGCLBackend;
namespace Tooth.Backend
{
    internal class Handler
    {
        private CpuBoostController cpuBoostController;
        private IntelGPU intelGPUController;
		private readonly AutoStart _autoStart;

		public Handler(AutoStart autoStart)
        {
            cpuBoostController = new CpuBoostController();
            intelGPUController = new IntelGPU();
            _autoStart = autoStart;
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
                case "get-boost":
                    {
                        if (cpuBoostController == null)
                        {
                            cpuBoostController = new CpuBoostController();
                        }
                        Console.WriteLine($"[Server Handler] Responding with CPU Boost {cpuBoostController.GetBoostMode().ToString()}");

                        (sender as Communication).Send("boost" + ' ' + (int)cpuBoostController.GetBoostMode());
                    }
                    break;
                case "set-boost":
                    {
                        if (cpuBoostController == null)
                        {
                            cpuBoostController = new CpuBoostController();
                        }
						Console.WriteLine($"[Server Handler] Setting CPU Boost to {args[1]}");
                        if (Enum.TryParse(args[1], out CpuBoostController.BoostMode mode))
                        {
                            cpuBoostController.SetBoostMode(mode);
                        }
                        else
                        {
							Console.WriteLine($"[Server Handler] Invalid Boost Mode: {args[1]}");
                        }
                    }
                    break;

                case "set-Fps":
                    {
						Console.WriteLine($"[Server Handler] Todo: Setting Fps to {args[1]}");
                    }
                    break;
                case "get-EnduranceGaming":
                    {
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }
                        ctl_endurance_gaming_t enduranceGaming = intelGPUController.GetEnduranceGaming();

                        if (enduranceGaming.EGControl == ctl_3d_endurance_gaming_control_t.OFF)
                        {
                            Console.WriteLine($"[Server Handler] Responding with GPU Endurance Gaming 0");
                            (sender as Communication).Send("EnduranceGaming" + ' ' + 0);
                        }
                        else if (enduranceGaming.EGControl == ctl_3d_endurance_gaming_control_t.AUTO)
                        {
                            switch (enduranceGaming.EGMode)
                            {
                                case ctl_3d_endurance_gaming_mode_t.PERFORMANCE:
                                    Console.WriteLine($"[Server Handler] Responding with GPU Endurance Gaming 1");
                                    (sender as Communication).Send("EnduranceGaming" + ' ' + 1);
                                    break;
                                case ctl_3d_endurance_gaming_mode_t.BALANCED:
                                    Console.WriteLine($"[Server Handler] Responding with GPU Endurance Gaming 2");
                                    (sender as Communication).Send("EnduranceGaming" + ' ' + 2);
                                    break;
                                case ctl_3d_endurance_gaming_mode_t.BATTERY:
                                    Console.WriteLine($"[Server Handler] Responding with GPU Endurance Gaming 3");
                                    (sender as Communication).Send("EnduranceGaming" + ' ' + 3);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;
                case "set-EnduranceGaming":
                    {
                        Console.WriteLine($"[Server Handler] Setting Endurance Gaming mode to {args[1]}");
                        bool result = false;
                        switch (args[1])
                        {
                            case "0":
                                result = intelGPUController.SetEnduranceGaming(IGCL.IGCLBackend.ctl_3d_endurance_gaming_control_t.OFF, IGCL.IGCLBackend.ctl_3d_endurance_gaming_mode_t.MAX);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetEnduranceGaming {result}");
                                break;
                            case "1":
                                result = intelGPUController.SetEnduranceGaming(IGCL.IGCLBackend.ctl_3d_endurance_gaming_control_t.AUTO, IGCL.IGCLBackend.ctl_3d_endurance_gaming_mode_t.PERFORMANCE);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetEnduranceGaming {result}");
                                break;
                            case "2":
                                result = intelGPUController.SetEnduranceGaming(IGCL.IGCLBackend.ctl_3d_endurance_gaming_control_t.AUTO, IGCL.IGCLBackend.ctl_3d_endurance_gaming_mode_t.BALANCED);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetEnduranceGaming {result}");
                                break;
                            case "3":
                                result = intelGPUController.SetEnduranceGaming(IGCL.IGCLBackend.ctl_3d_endurance_gaming_control_t.AUTO, IGCL.IGCLBackend.ctl_3d_endurance_gaming_mode_t.BATTERY);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetEnduranceGaming {result}");
                                break;
                            default:
                                Console.WriteLine($"[Server Handler] Wrong Arg value: Setting Endurance Gaming mode to {args[1]}");
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetEnduranceGaming {result}");
                                break;
                        }
                    }
                    break;

                case "set-LowLatencyMode":
                    {
						Console.WriteLine($"[Server Handler] Todo: Setting Low Latency mode to {args[1]}");
                    }
                    break;
                case "IntelGraphicsSofware":
                    {
						Console.WriteLine($"[Server Handler] Launch Intel Graphics Software");
                        launchIntelGraphicsSofware();
                    }
                    break;
				case "init":
                    {
                        bool enabled = AutoStart.IsEnabled(_autoStart.name);
                        Console.WriteLine($"[Handler] Get AutoStart Status: {enabled}");
                        comm.Send($"autostart {enabled}");
                    }
                    break;
				case "autostart":
					{
						Console.WriteLine($"[Handler] Set Auto Start: {args[1]}");
						bool enabled = bool.Parse(args[1]);
						_autoStart.SetEnabled(enabled);
						comm.Send($"autostart {AutoStart.IsEnabled(_autoStart.name)}");
					}
					break;
			}
        }

        async void launchIntelGraphicsSofware()
        {

            string path = @"C:\Program Files\Intel\Intel Graphics Software\IntelGraphicsSoftware.exe";

            if (!File.Exists(path))
            {
				// File doesn’t exist, log or notify gracefully
				Console.WriteLine($"[Warning] Intel Graphics Software not found at '{path}'");
                return;
            }

            try
            {
                await Task.Run(() =>
                {
                    Process.Start("C:\\Program Files\\Intel\\Intel Graphics Software\\IntelGraphicsSoftware.exe");
                });
				Console.WriteLine("[Info] Launched Intel Graphics Software successfully.");
            }
            catch (Exception ex)
            {
				// Catch any other exceptions and log them
				Console.WriteLine($"[Error] Failed to launch Intel Graphics Software: {ex.Message}");
            }
        }
    }
}
