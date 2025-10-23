# <img src="https://github.com/user-attachments/assets/118f29e9-f890-4fa2-a8e2-7fb228d7f65e" width="50" height="50" alt="LockScreenLogo" style="vertical-align: middle; margin-right: 8px;" /> Tooth N Claw

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/BassemMohsen/ToothNClaw?style=for-the-badge&color=blue)](https://github.com/BassemMohsen/ToothNClaw/releases/latest)
[![GitHub all releases](https://img.shields.io/github/downloads/BassemMohsen/ToothNClaw/total?style=for-the-badge&color=green)](https://github.com/BassemMohsen/ToothNClaw/releases)
![Build Status](https://img.shields.io/github/actions/workflow/status/BassemMohsen/ToothNClaw/dotnet-desktop.yml?branch=main&style=for-the-badge&label=Build)
![GitHub Repo stars](https://img.shields.io/github/stars/BassemMohsen/ToothNClaw?style=for-the-badge&color=yellow&label=Stars)

<h1 align="center">
  <img width="788" height="364" alt="Tooth N Claw Banner" src="https://github.com/user-attachments/assets/2405791a-4a9e-4a29-8c4c-8528ac5782e1">
</h1>


**Tooth N Claw Xbox Game Bar Widget** brings native, controller-friendly performance tuning directly into your gameplay experience.
- On-the-fly tuning: Adjust APU power and graphics settings instantly from gamebar, mid-game, without interrupting your session.
- Controller-first experience: Designed for handheld and couch gaming — no need to navigate desktop apps with tiny UI elements made for mouse and keyboard.
- Filling Windows XBox Fullscreen Experience gaps letting gamers fine-tune system performance directly from the Xbox Game Bar — no more dropping to Desktop mode for simple APU changes.
- Zero compute footprint: Uses no CPU or GPU compute and has no power impact.

<img width="1920" height="1200" alt="Screenshot 2025-10-21 190036" src="https://github.com/user-attachments/assets/3504fcf3-bb55-4cb2-82ef-72573bf96b13" />

**Tooth N Claw Color Remaster** 

- Bring your games to life with Color Remaster, a hardware-accelerated color enhancement tool inspired by VibrantDeck.
- Remaster your visuals: Transform your game’s color and mood — go full Matrix with deep green hues or recreate the golden tones of Lisan al Gaib from Dune.
- OLED-like experience with punchy colors: Boost your display’s color intensity with Saturation and Contrast controls for rich, vivid output. (Contrast 55, Saturation: 58)
- Hardware-accelerated performance: Powered by LUT (Look-Up Tables) blocks running directly on the display pipeline — 0% GPU compute usage and exceptional power efficiency from Intel.
- Fine-tune your visuals: Adjust Brightness, Contrast, and Gamma to find your perfect balance between light and darkness.

<img width="1920" height="1200" alt="Heroshot_ColorRemaster" src="https://github.com/user-attachments/assets/cedf8565-39e8-44ee-b553-26fff7752bcd" />

- Next-gen LunarLake/PantherLaken sharpness: Unlock Intel Adaptive Sharpening — a novel feature by Intel that enhances blurred or upscaled content for a crisper, more detailed image while preserving natural textures.
- Adaptive Sharpening is Efficient and smart: Runs entirely on Intel’s display engine block — no GPU load, no performance hit, and minimal power draw.

<img width="1920" height="1200" alt="Intel_Adaptive_sharpening" src="https://github.com/user-attachments/assets/4093ed32-2707-4a0d-9420-096902a8c04b" />

# ☕ Support Me
If you like my work, you can buy me a coffee:   [![Donate via PayPal](https://img.shields.io/badge/Donate-PayPal-blue.svg)](https://paypal.me/bassemnomany)

# [:floppy_disk: Download](https://github.com/BassemMohsen/ToothNClaw/releases/tag/v1.0.60.0)

# Features list
- **CPU Boost Toggle** – Enable or disable CPU Boost to reduce power consumption by up to 50%, freeing power headroom for the GPU.
- **Frame Limiter** – Lock your frame rate for consistent performance and thermals. With VRR displays, optimal values are typically 48–116 FPS.
- **Frame Sync Options** – Choose your preferred frame synchronization mode to balance latency and visual smoothness.
- **Display Resolution Control** – Some Windows games and apps behave unpredictably when changing resolutions dynamically; this tool helps manage those scenarios cleanly.
- **Endurance Gaming (Auto-TDP)** – Intel’s adaptive power management feature that automatically adjusts CPU/GPU power allocation to maintain a target frame rate while extending battery life.
- **Xe Low Latency (XeLL)** – Reduces input latency by intelligently delaying frame presentation for improved responsiveness.
- **Intel Graphics Software Launcher** – Provides quick access to the Intel Graphics Control Center, even when running in Xbox Game Bar full-screen mode.
- **Hot Key function** using View button and X button marked in blue in the image [Only in Desktop mode, not XBox Fullscreen experience]
- **Color Remaster** Contrast, Saturation, Brightness, Hue and Gamma.
- **Intel Adaptive Sharpening** Enhance details in blurred or upscaled content to make it crisper. 

Note: This is designed to be lightweight and complement MSI Quick Settings options. This is not intended to replace MSI Quick Settings.

# Supported Devices
- MSI Claw 8 AI+ A2VM with Intel Lunar Lake.
- Requires Intel Graphics Software and Intel Drivers for Lunar Lake APU.

# Bugs & Features
Found a bug and want it fixed? Have an idea for a new feature?
Please [open an issue](https://github.com/BassemMohsen/ToothNClaw/issues) in the tracker.  

# Credits & Libraries
- [Valkirie/HandheldCompanion](https://github.com/Valkirie/HandheldCompanion)
- [Intel IGCL Library](https://github.com/intel/drivers.gpu.control-library)
- [chenx-dust/RyzenAdjUWP](https://github.com/chenx-dust/RyzenAdjUWP)

> Big shout-out to the Intel Graphics Team for the IGCL library, with its well-designed APIs and useful samples that Tooth N Claw leverages to offer Game Bar widget controls.

# Frequently Asked Questions
## Anti-virus warnings
Some anti-virus programs may flag this software because it is signed with a developer certificate rather than a widely recognized certificate.
To run it without warnings, you will need to install the included developer certificate first.
Acquiring a widely recognized certificate can be expensive. With enough community support or donations, I hope to obtain one in the future.

## Installation issues: "This app package’s publisher certificate could not be verified."
Windows need to trust my developer signature on the .msixbundle
1. Double-click the .cer file → Install Certificate.
2. Choose **Local Machine** → Place all certificates in the **Trusted Root Certification Authorities** store.
3. After this, Windows will trust the certificate, and you should be able to install the MSIX bundle.
   **Note:** Make sure to install it for **Local Machine**, If you only install it to Current User, some apps may still fail depending on execution context.

## How to uninstall my Tooth N Claw?
Open Windows Settings -> Apps -> Installed Apps
Uninstall Tooth N Claw using the ... -> Uninstall

## How to uninstall my developer certificate?
1. Press Win + R, type: 'certlm.msc'
2. Navigate to: ' Trusted Root Certification Authorities → Certificates'
   Find my certificate under name "Bassem Mohsen" or "0E013939-CEFB-4F80-B4B4-B857260CB91A"
3. Right Click then Delete
   
