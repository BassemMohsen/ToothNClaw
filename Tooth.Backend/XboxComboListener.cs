using SharpDX.XInput;
using System;
using System.Threading;
using static System.Windows.Forms.AxHost;

namespace Tooth.Backend
{
    public class XboxComboListener
    {
        private readonly Controller controller = new Controller(UserIndex.One);
        private SharpDX.XInput.State prevState;
        private bool running;
        private Thread thread;

        // Event fired when the combo is pressed
        public event Action ComboPressed;
        // Optional: event fired when combo released
        public event Action ComboReleased;

        public bool IsComboActive { get; private set; }

        public void Start()
        {
            if (!controller.IsConnected)
            {
                Console.WriteLine("Xbox controller not connected.");
                return;
            }

            running = true;
            thread = new Thread(ListenLoop) { IsBackground = true };
            thread.Start();
        }

        public void Stop() => running = false;

        private void ListenLoop()
        {
            while (running)
            {
                if (!controller.IsConnected)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                var state = controller.GetState();
                var buttons = state.Gamepad.Buttons;

                bool viewPressed = (buttons & GamepadButtonFlags.Back) != 0;
                bool aPressed = (buttons & GamepadButtonFlags.A) != 0;

                bool comboNow = viewPressed && aPressed;
                bool comboBefore = IsComboActive;

                if (comboNow && !comboBefore)
                {
                    IsComboActive = true;
                    ComboPressed?.Invoke();
                }
                else if (!comboNow && comboBefore)
                {
                    IsComboActive = false;
                    ComboReleased?.Invoke();
                }

                prevState = state;
                Thread.Sleep(25); // fast but lightweight
            }
        }
    }
}