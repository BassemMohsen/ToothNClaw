# Tooth N Claw

Tooth N Claw XBox widget allows on the fly adjust the APU power and graphics settings mid game.
Windows Fullscreen Experience lacks launchers for Intel Graphics Software, and easy controller based settings toggles.
This improves XBox FSX and gamers don't have to drop off to Desktop mode often to adjust simple APU settings.

<img width="1920" height="857" alt="Tooth-Widget" src="https://github.com/user-attachments/assets/8d1f7695-8fda-4d01-b793-eec0eb274c66" />


# Features
- **CPU Boost Toggle** – Enable or disable CPU Boost to reduce power consumption by up to 50%, freeing power headroom for the GPU.
- **Frame Limiter** – Lock your frame rate for consistent performance and thermals. With VRR displays, optimal values are typically 48–116 FPS.
- **Frame Sync Options** – Choose your preferred frame synchronization mode to balance latency and visual smoothness.
- **Display Resolution Control** – Some Windows games and apps behave unpredictably when changing resolutions dynamically; this tool helps manage those scenarios cleanly.
- **Endurance Gaming (Auto-TDP)** – Intel’s adaptive power management feature that automatically adjusts CPU/GPU power allocation to maintain a target frame rate while extending battery life.
- **Xe Low Latency (XeLL)** – Reduces input latency by intelligently delaying frame presentation for improved responsiveness.
- **Intel Graphics Software Launcher** – Provides quick access to the Intel Graphics Control Center, even when running in Xbox Game Bar full-screen mode.
- Hot Key function using View button and X button [Only in Desktop mode, not XBox Fullscreen experience]

# ☕ Support Me
If you like my work, you can buy me a coffee:   [![Donate via PayPal](https://img.shields.io/badge/Donate-PayPal-blue.svg)](https://paypal.me/bassemnomany)

# [:floppy_disk: Download](https://github.com/BassemMohsen/ToothNClaw/releases/download/v1.0.45.0/Tooth.Package_1.0.45.0_x64.zip)


Note: This is designed to be lightweight and complement MSI Quick Settings options. This is not intended to replace MSI Quick Settings.

# Supported Devices
MSI Claw 8 AI+ A2VM with Intel Lunar Lake.

# Bugs & Features
Found a bug and want it fixed? Have an idea for a new feature?
Please [open an issue](https://github.com/BassemMohsen/ToothNClaw/issues) in the tracker.  

# Credits & Libraries
- [Valkirie/HandheldCompanion](https://github.com/Valkirie/HandheldCompanion)
- [Intel IGCL Library](https://github.com/intel/drivers.gpu.control-library)
- [chenx-dust/RyzenAdjUWP](https://github.com/chenx-dust/RyzenAdjUWP)

# Frequently Asked Questions
## How to unhide or enable CPU Boost policy?
Open command as administrator and run following commands:
<pre> ```powershell
  reg add "HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\be337238-0d82-4146-a960-4f3749d470c7" /v Attributes /t REG_DWORD /d 2 /f
  powercfg -setactive SCHEME_CURRENT
``` </pre>
Then close down ToothNClaw widget from Gamebar, and restart the widget. It should work now!

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
   
