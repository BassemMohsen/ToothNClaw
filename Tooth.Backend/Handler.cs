using MS.WindowsAPICodePack.Internal;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tooth.GraphicsProcessingUnit;
using Tooth.IGCL;
using Windows.System;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using static Tooth.GraphicsProcessingUnit.IntelGPU;
using static Tooth.IGCL.IGCLBackend;
using Task = System.Threading.Tasks.Task;

namespace Tooth.Backend
{
    internal class Handler
    {
        private CpuBoostController cpuBoostController;
        private IntelGPU intelGPUController;
        private Communication _communication;


        public Handler()
        {
            cpuBoostController = new CpuBoostController();
            intelGPUController = new IntelGPU();
            if (!RTSSManager.EnsureRunning())
            {
                Console.WriteLine("RTSS not available. Exiting.");
                return;
            }
        }

        public void Register(Communication comm)
        {
            _communication = comm;
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

                case "get-fps-limiter-enabled":
                    {
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }
                        //ctl_fps_limiter_t fpsLimiter = intelGPUController.GetFPSLimiter();

                        ctl_fps_limiter_t fpsLimiter;
                        fpsLimiter.isLimiterEnabled = RTSSManager.GetGlobalFramerateLimit() > 0;
                        if (fpsLimiter.isLimiterEnabled)
                        {
                            Console.WriteLine($"[Server Handler] Responding with FPS Limiter Enabled 1");
                            (sender as Communication).Send("fps-limiter-enabled" + ' ' + "1");
                        }
                        else
                        {
                            Console.WriteLine($"[Server Handler] Responding with FPS Limiter Enabled 0");
                            (sender as Communication).Send("fps-limiter-enabled" + ' ' + "0");
                        }
                    }
                    break;
                case "get-fps-limiter-value":
                    {
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }
                        //ctl_fps_limiter_t fpsLimiter = intelGPUController.GetFPSLimiter();
                        //Console.WriteLine($"[Server Handler] Responding with FPS Limiter value {fpsLimiter.fpsLimitValue}");
                        //(sender as Communication).Send("fps-limiter-value" + ' ' + fpsLimiter.fpsLimitValue);
                        (sender as Communication).Send("fps-limiter-value" + ' ' + RTSSManager.GetGlobalFramerateLimit());

                    }
                    break;

                case "set-Fps-limiter":
                    {
                        Console.WriteLine($"[Server Handler] Setting Fps Enabled to {args[1]} with FPS Cap at: {args[2]} ");
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }

                        if (args[1] == "0") // Limiter off
                        {
                            bool result = RTSSManager.SetGlobalFramerateLimit(0);
                            Console.WriteLine($"[Server Handler] RTTS Result of execution RTTS.SetTargetFPS {result}"); 
                            Console.WriteLine($"[Server Handler] RTTS Reading Framerate Limit: {RTSSManager.GetGlobalFramerateLimit()}");
                            // Check that value is between min VRR and max VRR supported by Claw display, min should be 48
                            /*if (int.TryParse(args[2], out int fps) && fps >= 30 && fps <= 120)
                            {
                                bool result = intelGPUController.SetFPSLimiter(false, fps);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetFPSLimiter {result}");
                            }*/
                        }
                        else if (args[1] == "1") // Limiter on
                        {
                            // Check that value is between min VRR and max VRR supported by Claw display, min should be 48
                            if (int.TryParse(args[2], out int fps) && fps >= 30 && fps <= 120)
                            {
                                bool result = RTSSManager.SetGlobalFramerateLimit(fps);
                                Console.WriteLine($"[Server Handler] RTTS Result of execution RTTS.SetTargetFPS {result}");
                                Console.WriteLine($"[Server Handler] RTTS Reading Framerate Limit: {RTSSManager.GetGlobalFramerateLimit()}");
                                //bool result = intelGPUController.SetFPSLimiter(true, fps);
                                //Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetFPSLimiter {result}");
                            }
                            else
                            {
                            }
                        }
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

                case "set-Low-Latency-Mode":
                    {
						Console.WriteLine($"[Server Handler] Setting Low Latency mode to {args[1]}");
                        bool result = false;
                        switch (args[1])
                        {
                            case "0":
                                result = intelGPUController.SetLowLatency(IGCL.IGCLBackend.ctl_3d_low_latency_types_t.CTL_3D_LOW_LATENCY_TYPES_TURN_OFF);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetLowLatency {result}");
                                break;
                            case "1":
                                result = intelGPUController.SetLowLatency(IGCL.IGCLBackend.ctl_3d_low_latency_types_t.CTL_3D_LOW_LATENCY_TYPES_TURN_ON);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetLowLatency {result}");
                                break;
                            case "2":
                                result = intelGPUController.SetLowLatency(IGCL.IGCLBackend.ctl_3d_low_latency_types_t.CTL_3D_LOW_LATENCY_TYPES_TURN_ON_BOOST_MODE_ON);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetLowLatency {result}");
                                break;
                            default:
                                Console.WriteLine($"[Server Handler] Wrong Arg value: Setting Low Latency to {args[1]}");
                                break;
                        }
                    }
                    break;

                case "get-Low-Latency-Mode":
                    {
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }
                        ctl_3d_low_latency_types_t lowlatencymode = intelGPUController.GetLowLatency();

                        switch (lowlatencymode)
                        {
                            case ctl_3d_low_latency_types_t.CTL_3D_LOW_LATENCY_TYPES_TURN_OFF:
                                Console.WriteLine($"[Server Handler] Responding with GPU Low Latency 0");
                                (sender as Communication).Send("Low-Latency-Mode" + ' ' + "0");
                                break;
                            case ctl_3d_low_latency_types_t.CTL_3D_LOW_LATENCY_TYPES_TURN_ON:
                                Console.WriteLine($"[Server Handler] Responding with GPU Low Latency 1");
                                (sender as Communication).Send("Low-Latency-Mode" + ' ' + "1");
                                break;
                            case ctl_3d_low_latency_types_t.CTL_3D_LOW_LATENCY_TYPES_TURN_ON_BOOST_MODE_ON:
                                Console.WriteLine($"[Server Handler] Responding with GPU Low Latency 2");
                                (sender as Communication).Send("Low-Latency-Mode" + ' ' + "2");
                                break;
                            default:
                                break;
                        }
                    }
                    break;

                case "set-Frame-Sync-Mode":
                    {
                        Console.WriteLine($"[Server Handler] Setting Frame Sync mode to {args[1]}");
                        bool result = false;
                        switch (args[1])
                        {
                            case "0":
                                result = intelGPUController.SetFrameSyncMode(IntelGPU.Vsync_Mode.APPLICATION_CHOICE);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetFrameSyncMode {result}");
                                break;
                            case "1":
                                result = intelGPUController.SetFrameSyncMode(IntelGPU.Vsync_Mode.VSYNC_OFF);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetFrameSyncMode {result}");
                                break;
                            case "2":
                                result = intelGPUController.SetFrameSyncMode(IntelGPU.Vsync_Mode.VSYNC_ON);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetFrameSyncMode {result}");
                                break;
                            case "3":
                                result = intelGPUController.SetFrameSyncMode(IntelGPU.Vsync_Mode.SMOOTH_SYNC);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetFrameSyncMode {result}");
                                break;
                            case "4":
                                result = intelGPUController.SetFrameSyncMode(IntelGPU.Vsync_Mode.SPEED_SYNC);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetFrameSyncMode {result}");
                                break;
                            case "5":
                                result = intelGPUController.SetFrameSyncMode(IntelGPU.Vsync_Mode.CAPPED_FPS);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetFrameSyncMode {result}");
                                break;
                            default:
                                Console.WriteLine($"[Server Handler] Wrong Arg value: Setting Frame Sync to {args[1]}");
                                break;
                        }
                    }
                    break;

                case "get-Frame-Sync-Mode":
                    {
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }
                        Vsync_Mode vsyncmode = intelGPUController.GetFrameSyncMode();

                        switch (vsyncmode)
                        {
                            case Vsync_Mode.APPLICATION_CHOICE:
                                Console.WriteLine($"[Server Handler] Responding with GPU Frame Sync Mode 0");
                                (sender as Communication).Send("Frame-Sync-Mode" + ' ' + "0");
                                break;
                            case Vsync_Mode.VSYNC_OFF:
                                Console.WriteLine($"[Server Handler] Responding with GPU Frame Sync Mode 0");
                                (sender as Communication).Send("Frame-Sync-Mode" + ' ' + "1");
                                break;
                            case Vsync_Mode.VSYNC_ON:
                                Console.WriteLine($"[Server Handler] Responding with GPU Frame Sync Mode 0");
                                (sender as Communication).Send("Frame-Sync-Mode" + ' ' + "2");
                                break;
                            case Vsync_Mode.SMOOTH_SYNC:
                                Console.WriteLine($"[Server Handler] Responding with GPU Frame Sync Mode 0");
                                (sender as Communication).Send("Frame-Sync-Mode" + ' ' + "3");
                                break;
                            case Vsync_Mode.SPEED_SYNC:
                                Console.WriteLine($"[Server Handler] Responding with GPU Frame Sync Mode 0");
                                (sender as Communication).Send("Frame-Sync-Mode" + ' ' + "4");
                                break;
                            case Vsync_Mode.CAPPED_FPS:
                                Console.WriteLine($"[Server Handler] Responding with GPU Frame Sync Mode 0");
                                (sender as Communication).Send("Frame-Sync-Mode" + ' ' + "5");
                                break;
                        }
                    }
                    break;
                case "IntelGraphicsSofware":
                    {
						Console.WriteLine($"[Server Handler] Launch Intel Graphics Software");
                        launchIntelGraphicsSofware();
                    }
                    break;
                case "set-resolution":
                    {
                        Console.WriteLine($"[Server Handler] Setting Resolution to {args[1]}");
                        bool result = false;
                        switch (args[1])
                        {
                            case "0":
                                result = intelGPUController.SetGPUScaling(false);
                                Console.WriteLine($"[Server Handler] Set SetGPUScaling to {result}");
                                result = intelGPUController.SetScalingMode(0);
                                Console.WriteLine($"[Server Handler] Set SetScalingMode to {result}");

                                result = DisplayController.SetResolution(1920, 1200);
                                Console.WriteLine($"[Server Handler] Set Display Resolution {result}");
                                break;
                            case "1":

                                result = intelGPUController.SetGPUScaling(true);
                                Console.WriteLine($"[Server Handler] Set SetGPUScaling to {result}");
                                result = intelGPUController.SetScalingMode(1);
                                Console.WriteLine($"[Server Handler] Set SetScalingMode to {result}");

                                result = DisplayController.SetResolution(1680, 1050);
                                Console.WriteLine($"[Server Handler] Set Display Resolution {result}");
                                break;
                            case "2":
                                result = intelGPUController.SetGPUScaling(true);
                                Console.WriteLine($"[Server Handler] Set SetGPUScaling to {result}");
                                result = intelGPUController.SetScalingMode(1);
                                Console.WriteLine($"[Server Handler] Set SetScalingMode to {result}");

                                result = DisplayController.SetResolution(1440, 900);
                                Console.WriteLine($"[Server Handler] Set Display Resolution {result}");
                                break;
                            case "3":
                                result = intelGPUController.SetGPUScaling(true);
                                Console.WriteLine($"[Server Handler] Set SetGPUScaling to {result}");
                                result = intelGPUController.SetScalingMode(1);
                                Console.WriteLine($"[Server Handler] Set SetScalingMode to {result}");

                                result = DisplayController.SetResolution(1280, 800);
                                Console.WriteLine($"[Server Handler] Set Display Resolution {result}");
                                break;
                            default:
                                Console.WriteLine($"[Server Handler] Wrong Arg value: Display Resolution won't be set {args[1]}");
                                break;
                        }
                    }
                    break;
                case "get-resolution":
                    {
                        DisplayController.Resolution currentResolution = DisplayController.GetResolution();

                        if (currentResolution.Width == 1920 && currentResolution.Height == 1200)
                        {
                            Console.WriteLine($"[Server Handler] Responding with Resolution 0");
                            (sender as Communication).Send("resolution" + ' ' + "0");
                        }
                        else if (currentResolution.Width == 1680 && currentResolution.Height == 1050)
                        {
                            Console.WriteLine($"[Server Handler] Responding with Resolution 1");
                            (sender as Communication).Send("resolution" + ' ' + "1");
                        }
                        else if (currentResolution.Width == 1440 && currentResolution.Height == 900)
                        {
                            Console.WriteLine($"[Server Handler] Responding with Resolution 2");
                            (sender as Communication).Send("resolution" + ' ' + "2");
                        }
                        else if (currentResolution.Width == 1280 && currentResolution.Height == 800)
                        {
                            Console.WriteLine($"[Server Handler] Responding with Resolution 3");
                            (sender as Communication).Send("resolution" + ' ' + "3");
                        }
                        else
                        {
                            Console.WriteLine($"[Server Handler] Responding with Resolution Unknown");
                            (sender as Communication).Send("resolution" + ' ' + "-1");
                        }
                    }
                    break;
                case "init":
                    {
                        bool enabled = false;
                        Console.WriteLine($"[Handler] Get AutoStart Status: {enabled}");
                        comm.Send($"autostart {enabled}");
                    }
                    break;
				case "autostart":
					{
						Console.WriteLine($"[Handler] Set Auto Start: {args[1]}");
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

        public void sendLaunchGameBarWidget()
        {
            if (_communication != null) { 
                Console.WriteLine($"[Server Handler] Send launch-gamebar-widget");
                _communication.Send("launch-gamebar-widget");
            }
        }
    }
}
