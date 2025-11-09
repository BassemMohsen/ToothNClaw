using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Tooth.Backend;

class ModernStandbyMonitor
{
    [DllImport("user32.dll")]
    private static extern int SendMessage(int hWnd, int Msg, int wParam, int lParam);

    private const int HWND_BROADCAST = 0xffff;
    private const int WM_SYSCOMMAND = 0x0112;
    private const int SC_SUSPEND = 0xF170;

    private readonly XNamespace ns = "http://schemas.microsoft.com/win/2004/08/events/event";

    public ModernStandbyMonitor()
    {
        string xpath = "*[System[(EventID=506 or EventID=507) and Provider[@Name='Microsoft-Windows-Kernel-Power']]]";

        var query = new EventLogQuery("System", PathType.LogName, xpath);
        var watcher = new EventLogWatcher(query);

        watcher.EventRecordWritten += OnEventRecordWritten;
        watcher.Enabled = true;

        Console.WriteLine("[ModernStandbyMonitor] Watching for sleep (506) / wake (507) events...");
    }

    private void SuspendSystem()
    {
        SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, SC_SUSPEND, 2);
    }

    private void OnEventRecordWritten(object sender, EventRecordWrittenEventArgs e)
    {
        if (e.EventRecord == null)
            return;

        int eventId = e.EventRecord.Id;
        DateTime eventTime = e.EventRecord.TimeCreated?.ToLocalTime() ?? DateTime.Now;

        if (eventId == 506)
        {
            Console.WriteLine($"[ModernStandbyMonitor] System entered Modern Standby at {eventTime:yyyy-MM-dd HH:mm:ss}");
            GameSuspendController.SuspendForegroundApp();
        }
        else if (eventId == 507)
        {
            string reason = ParseWakeReason(e.EventRecord);
            Console.WriteLine($"[ModernStandbyMonitor] System woke up from Modern Standby at {eventTime:yyyy-MM-dd HH:mm:ss}. Reason: {reason}");
            if (!reason.Equals("Power Button", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[ModernStandbyMonitor] System woke up for other reason but power button, go back go to sleep!");
                SuspendSystem();
            }
            else
            {
                // It's power button, resume foreground app
                GameSuspendController.ResumeForegroundApp();
            }
        }
    }

    private string ParseWakeReason(EventRecord evt)
    {
        try
        {
            var xml = evt.ToXml();
            var doc = XDocument.Parse(xml);

            var reasonVal = doc
                .Descendants(ns + "Data")
                .FirstOrDefault(x => x.Attribute("Name")?.Value == "Reason")?.Value;

            if (!int.TryParse(reasonVal, out int reasonCode))
                return "Unknown";

            return reasonCode switch
            {
                0 => "Unknown",
                1 => "Power Button",
                7 => "Joystick",
                28 => "ChargerConnected",
                _ => $"Unknown (Reason code {reasonCode})"
            };
        }
        catch
        {
            return "Unknown (failed to parse)";
        }
    }
}
