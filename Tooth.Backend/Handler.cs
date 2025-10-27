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
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Controls;
using System.Text.Json;
using static Tooth.Backend.DisplayController;
using static Tooth.GraphicsProcessingUnit.IntelGPU;
using static Tooth.IGCL.IGCLBackend;
using System.Diagnostics.Eventing.Reader;


namespace Tooth.Backend
{
    internal class Handler
    {
        private CpuBoostController cpuBoostController;
        private IntelGPU intelGPUController;
        private Communication _communication;
        private List<Resolution> resolutions;

        private bool _gpuScalingEnabled = false;
        private bool _retroScalingEnabled = false;


        public enum ScalingModeMethod : int
        {
            DISPLAY_SCALING_MAINTAIN_ASPECT_RATIO = 0,
            GPU_SCALING_MAINTAIN_ASPECT_RATIO = 1,
            GPU_SCALING_STRETCH = 2,
            GPU_SCALING_CENTER = 3,
            RETRO_SCALING_INTEGER = 4,
            RETRO_SCALING_NEAREST_NEIGHBOUR = 5,
            UNKNOWN = 6
        }

        public Handler()
        {
            cpuBoostController = new CpuBoostController();
            intelGPUController = new IntelGPU();
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
                        ctl_fps_limiter_t fpsLimiter = intelGPUController.GetFPSLimiter();
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
                        ctl_fps_limiter_t fpsLimiter = intelGPUController.GetFPSLimiter();
                        Console.WriteLine($"[Server Handler] Responding with FPS Limiter value {fpsLimiter.fpsLimitValue}");
                        (sender as Communication).Send("fps-limiter-value" + ' ' + fpsLimiter.fpsLimitValue);
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
                            // Check that value is between min VRR and max VRR supported by Claw display, min should be 48
                            if (int.TryParse(args[2], out int fps) && fps >= 30 && fps <= 120)
                            {
                                bool result = intelGPUController.SetFPSLimiter(false, fps);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetFPSLimiter {result}");
                            }
                        }
                        else if (args[1] == "1") // Limiter on
                        {
                            // Check that value is between min VRR and max VRR supported by Claw display, min should be 48
                            if (int.TryParse(args[2], out int fps) && fps >= 30 && fps <= 120)
                            {
                                bool result = intelGPUController.SetFPSLimiter(true, fps);
                                Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetFPSLimiter {result}");
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
                case "set-resolution":
                    {
                        Console.WriteLine($"[Server Handler] Setting Resolution to {args[1]}");
                        bool result = false;
                        if (int.TryParse(args[1], out int id) && resolutions != null)
                        {
                            var res = resolutions.FirstOrDefault(r => r.Id == id);
                            Console.WriteLine($"[Server Handler] Set Display Resolution {result}");
                            if (!res.Equals(default(Resolution)))
                            {
                                result = DisplayController.SetPrimaryResolution(res.Width, res.Height);
                                if (id == 0)
                                {
                                    result = intelGPUController.SetGPUScalingTypeMode(true, IntelGPU.ScalingModeAndMethod.DISPLAY_SCALING);
                                    Console.WriteLine($"[Server Handler] Set SetGPUScalingTypeMode to {result}");
                                }
                                else
                                {
                                    result = intelGPUController.SetGPUScalingTypeMode(true, IntelGPU.ScalingModeAndMethod.GPU_SCALING_STRETCH_FIT);
                                    Console.WriteLine($"[Server Handler] Set SetGPUScalingTypeMode to {result}");
                                }

                            }
                            else
                            {
                                Console.WriteLine($"[Server Handler] Wrong Arg value: Display Resolution won't be set {args[1]}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[Server Handler] Wrong Arg value: Display Resolution won't be set {args[1]}");
                        }

                    }
                    break;
                case "get-supported-resolutions":
                    {
                        resolutions = DisplayController.GetPrimaryDisplaySupportedResolutions();
                        // Serialize to JSON string
                        string json = System.Text.Json.JsonSerializer.Serialize(resolutions);
                        (sender as Communication).Send("supported-resolutions " + json);

                        foreach (var res in resolutions)
                        {
                            Console.WriteLine($"Supported Resolution Is Native: {res.IsNative} - Id: {res.Id}  - {res.Width}x{res.Height} @{res.Frequency}Hz");
                        }
                    }
                    break;
                case "get-resolution":
                    {
                        DisplayController.Resolution currentResolution = DisplayController.GetPrimaryDisplayResolution();

                        // Try to find matching Id
                        var match = resolutions.FirstOrDefault(r =>
                            r.Width == currentResolution.Width && r.Height == currentResolution.Height);

                        int currentId = match.Equals(default(Resolution)) ? 0 : match.Id;
                        Console.WriteLine($"[Server Handler] Responding with Resolution {currentId}");
                        (sender as Communication).Send("resolution" + ' ' + currentId);
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

                case "get-Sharpness-Value":
                    {
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }
                        int sharpnessValue = intelGPUController.GetImageSharpeningSharpness();
                        Console.WriteLine($"[Server Handler] Responding with Sharpness value {sharpnessValue}");
                        (sender as Communication).Send("Sharpness-Value" + ' ' + sharpnessValue);
                    }
                    break;

                case "set-Sharpness-Value":
                    {
                        Console.WriteLine($"[Server Handler] Setting Sharpness to {args[1]}");
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }
                        if (int.TryParse(args[1], out int sharpness) && sharpness >= 0 && sharpness <= 100)
                        {
                            bool result = intelGPUController.SetImageSharpeningSharpness(sharpness);
                            Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetImageSharpeningSharpness {result}");
                        }
                    }
                    break;

                case "get-Hue-Value":
                    {
                        double hue = SettingsManager.Get<double>("Hue");
                        Console.WriteLine($"[Server Handler] Responding with Hue Value {hue}");
                        (sender as Communication).Send("Hue-Value" + ' ' + $"{hue}");
                    }
                    break;
                case "set-Hue-Value":
                    {
                        Console.WriteLine($"[Server Handler] Setting Hue to {args[1]}");
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }
                        if (double.TryParse(args[1], out double hue) && hue >= -180 && hue <= 180)
                        {
                            SettingsManager.Set("Hue", hue);
                            bool result = intelGPUController.SetHueSaturation(hue, SettingsManager.Get<double>("Saturation"));
                            Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetHueSaturation {result}");
                        }
                    }
                    break;
                case "get-Saturation-Value":
                    {
                        double saturation = SettingsManager.Get<double>("Saturation");
                        Console.WriteLine($"[Server Handler] Responding with Saturation Value {saturation}");
                        (sender as Communication).Send("Saturation-Value" + ' ' + $"{saturation}");
                    }
                    break;
                case "set-Saturation-Value":
                    {
                        Console.WriteLine($"[Server Handler] Setting Saturation to {args[1]}");
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }
                        if (double.TryParse(args[1], out double saturation) && saturation >= 0 && saturation <= 100)
                        {
                            SettingsManager.Set("Saturation", saturation);
                            bool result = intelGPUController.SetHueSaturation(SettingsManager.Get<double>("Hue"), saturation);
                            Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetHueSaturation {result}");
                        }
                    }
                    break;
                case "get-Contrast-Value":
                    {
                        double contrast = SettingsManager.Get<double>("Contrast");
                        Console.WriteLine($"[Server Handler] Responding with Contrast Value {contrast}");
                        (sender as Communication).Send($"Contrast-Value" + ' ' + $"{contrast}");
                    }
                    break;
                case "set-Contrast-Value":
                    {
                        Console.WriteLine($"[Server Handler] Setting Contrast to {args[1]}");
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }
                        if (double.TryParse(args[1], out double contrast) && contrast >= 0 && contrast <= 100)
                        {
                            SettingsManager.Set("Contrast", contrast);
                            bool result = intelGPUController.SetBrightnessContrastGamma(contrast, SettingsManager.Get<double>("Gamma"), SettingsManager.Get<double>("Brightness"));
                            Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetBrightnessContrastGamma {result}");
                        }
                    }
                    break;
                case "get-Brightness-Value":
                    {
                        double brightness = SettingsManager.Get<double>("Brightness");
                        Console.WriteLine($"[Server Handler] Responding with Brightness Value {brightness}");
                        (sender as Communication).Send($"Brightness-Value" + ' ' + $"{brightness}");
                    }
                    break;
                case "set-Brightness-Value":
                    {
                        Console.WriteLine($"[Server Handler] Setting Brightness to {args[1]}");
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }
                        if (double.TryParse(args[1], out double brightness) && brightness >= 0 && brightness <= 100)
                        {
                            SettingsManager.Set("Brightness", brightness);
                            bool result = intelGPUController.SetBrightnessContrastGamma(SettingsManager.Get<double>("Contrast"), SettingsManager.Get<double>("Gamma"), brightness);
                            Console.WriteLine($"[Server Handler] IGCL Result of execution inte GPUController.SetBrightnessContrastGamma {result}");
                        }
                    }
                    break;
                case "get-Gamma-Value":
                    {
                        double gamma = SettingsManager.Get<double>("Gamma");
                        Console.WriteLine($"[Server Handler] Responding with Gamma Value {gamma}");
                        (sender as Communication).Send($"Gamma-Value" + ' ' + $"{gamma}");
                    }
                    break;
                case "set-Gamma-Value":
                    {
                        Console.WriteLine($"[Server Handler] Setting Panel Gamma to {args[1]}");
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }
                        if (double.TryParse(args[1], out double gamma) && gamma >= 0.3 && gamma <= 2.8)
                        {
                            SettingsManager.Set("Gamma", gamma);
                            bool result = intelGPUController.SetBrightnessContrastGamma(SettingsManager.Get<double>("Contrast"), gamma, SettingsManager.Get<double>("Brightness"));
                            Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetBrightnessContrastGamma {result}");
                        }
                    }
                    break;
                case "get-Scaling":
                    {
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }

                        _retroScalingEnabled = intelGPUController.GetRetroScalingEnabled();
                        if (_retroScalingEnabled)
                        {
                            ctl_retro_scaling_type_flags_t scalingType = intelGPUController.GetRetroScalingType();
                            switch (scalingType)
                            {
                                case ctl_retro_scaling_type_flags_t.CTL_RETRO_SCALING_TYPE_FLAG_INTEGER:
                                    Console.WriteLine($"[Server Handler] Responding with Retro Scaling Type Integer 0");
                                    (sender as Communication).Send("Scaling" + ' ' + $"{(int)ScalingModeMethod.RETRO_SCALING_INTEGER}");
                                    break;
                                case ctl_retro_scaling_type_flags_t.CTL_RETRO_SCALING_TYPE_FLAG_NEAREST_NEIGHBOUR:
                                    Console.WriteLine($"[Server Handler] Responding with Retro Scaling Type Nearest Neighbour 1");
                                    (sender as Communication).Send("Scaling" + ' ' + $"{(int)ScalingModeMethod.RETRO_SCALING_NEAREST_NEIGHBOUR}");
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            ScalingModeAndMethod GPUScalingMode = intelGPUController.GetGPUScaling();
                            switch (GPUScalingMode)
                            {
                                case ScalingModeAndMethod.DISPLAY_SCALING:
                                    Console.WriteLine($"[Server Handler] Responding with GPU Scaling Mode Display Scaling 0");
                                    (sender as Communication).Send("Scaling" + ' ' + $"{(int)ScalingModeMethod.DISPLAY_SCALING_MAINTAIN_ASPECT_RATIO}");
                                    break;
                                case ScalingModeAndMethod.GPU_SCALING_MAINTAIN_ASPECT_RATIO:
                                    Console.WriteLine($"[Server Handler] Responding with GPU Scaling Mode Maintain Aspect Ratio 1");
                                    (sender as Communication).Send("Scaling" + ' ' + $"{(int)ScalingModeMethod.GPU_SCALING_MAINTAIN_ASPECT_RATIO}");
                                    break;
                                case ScalingModeAndMethod.GPU_SCALING_STRETCH_FIT:
                                    Console.WriteLine($"[Server Handler] Responding with GPU Scaling Mode Stretch to Fit 2");
                                    (sender as Communication).Send("Scaling" + ' ' + $"{(int)ScalingModeMethod.GPU_SCALING_STRETCH}");
                                    break;
                                case ScalingModeAndMethod.GPU_SCALING_CENTER_IN_SCREEN:
                                    Console.WriteLine($"[Server Handler] Responding with GPU Scaling Mode Centered 3");
                                    (sender as Communication).Send("Scaling" + ' ' + $"{(int)ScalingModeMethod.GPU_SCALING_CENTER}");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;

                case "set-Scaling":
                    {
                        if (intelGPUController == null)
                        {
                            intelGPUController = new IntelGPU();
                        }

                        bool result = false;
                        // Set GPU Scaling
                        int scaling = int.Parse(args[1]);
                        Console.WriteLine($"[Server Handler] Setting Scaling Value Received is {args[1]}");
                        switch (scaling)
                        {
                            case (int)ScalingModeMethod.DISPLAY_SCALING_MAINTAIN_ASPECT_RATIO:
                                {
                                    if (_retroScalingEnabled)
                                    {
                                        // Disable retro gaming first:
                                        _retroScalingEnabled = false;
                                        result = intelGPUController.SetRetroScaling(_retroScalingEnabled, ctl_retro_scaling_type_flags_t.CTL_RETRO_SCALING_TYPE_FLAG_INTEGER);
                                        Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetRetroScaling {result}");
                                    }


                                    result = intelGPUController.SetGPUScalingTypeMode(true, ScalingModeAndMethod.DISPLAY_SCALING);
                                    Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetGPUScalingTypeMode {result}");
                                }
                                break;
                            case (int)ScalingModeMethod.GPU_SCALING_MAINTAIN_ASPECT_RATIO:
                                {
                                    if (_retroScalingEnabled)
                                    {
                                        // Disable retro gaming first:
                                        _retroScalingEnabled = false;
                                        result = intelGPUController.SetRetroScaling(_retroScalingEnabled, ctl_retro_scaling_type_flags_t.CTL_RETRO_SCALING_TYPE_FLAG_INTEGER);
                                        Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetRetroScaling {result}");
                                    }

                                    result = intelGPUController.SetGPUScalingTypeMode(true, ScalingModeAndMethod.GPU_SCALING_MAINTAIN_ASPECT_RATIO);
                                    Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetGPUScalingTypeMode {result}");
                                }
                                break;
                            case (int)ScalingModeMethod.GPU_SCALING_STRETCH:
                                {
                                    if (_retroScalingEnabled)
                                    {
                                        // Disable retro gaming first:
                                        _retroScalingEnabled = false;
                                        result = intelGPUController.SetRetroScaling(_retroScalingEnabled, ctl_retro_scaling_type_flags_t.CTL_RETRO_SCALING_TYPE_FLAG_INTEGER);
                                        Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetRetroScaling {result}");
                                    }

                                    result = intelGPUController.SetGPUScalingTypeMode(true, ScalingModeAndMethod.GPU_SCALING_STRETCH_FIT);
                                    Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetGPUScalingTypeMode {result}");
                                }
                                break;
                            case (int)ScalingModeMethod.GPU_SCALING_CENTER:
                                {
                                    if (_retroScalingEnabled)
                                    {
                                        // Disable retro gaming first:
                                        _retroScalingEnabled = false;
                                        result = intelGPUController.SetRetroScaling(_retroScalingEnabled, ctl_retro_scaling_type_flags_t.CTL_RETRO_SCALING_TYPE_FLAG_INTEGER);
                                        Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetRetroScaling {result}");
                                    }

                                    result = intelGPUController.SetGPUScalingTypeMode(true, ScalingModeAndMethod.GPU_SCALING_CENTER_IN_SCREEN);
                                    Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetGPUScalingTypeMode {result}");
                                }
                                break;
                            case (int)ScalingModeMethod.RETRO_SCALING_INTEGER:
                                {
                                    _retroScalingEnabled = true;
                                    result = intelGPUController.SetRetroScaling(_retroScalingEnabled, ctl_retro_scaling_type_flags_t.CTL_RETRO_SCALING_TYPE_FLAG_INTEGER);
                                    Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetRetroScaling  CTL_RETRO_SCALING_TYPE_FLAG_INTEGER {result}");
                                }
                                break;
                            case (int)ScalingModeMethod.RETRO_SCALING_NEAREST_NEIGHBOUR:
                                {
                                    _retroScalingEnabled = true;
                                    result = intelGPUController.SetRetroScaling(_retroScalingEnabled, ctl_retro_scaling_type_flags_t.CTL_RETRO_SCALING_TYPE_FLAG_NEAREST_NEIGHBOUR);
                                    Console.WriteLine($"[Server Handler] IGCL Result of execution intelGPUController.SetRetroScaling  CTL_RETRO_SCALING_TYPE_FLAG_NEAREST_NEIGHBOUR {result}");
                                }
                                break;
                            default:
                                Console.WriteLine($"[Server Handler] Wrong Arg value: Setting GPU Scaling to {args[1]}");
                                break;
                        }
                    }
                    break;
                default:
                    break;
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
