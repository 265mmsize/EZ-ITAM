using System.Windows.Forms;

namespace EzItam.AgentTray;

public sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly HistoryWindow _window;
    private readonly System.Windows.Forms.Timer _refreshTimer;

    public TrayApplicationContext()
    {
        var reader = new AgentStatusReader();
        _window = new HistoryWindow(reader);

        var menu = new ContextMenuStrip();
        menu.Items.Add("열기", null, (_, _) => ShowWindow());
        menu.Items.Add("새로고침", null, (_, _) => _window.Refresh_Data());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("종료", null, (_, _) => ExitApp());

        _trayIcon = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Information,
            Text = "EzItam 에이전트",
            Visible = true,
            ContextMenuStrip = menu
        };
        _trayIcon.DoubleClick += (_, _) => ShowWindow();

        _refreshTimer = new System.Windows.Forms.Timer { Interval = 15000 };
        _refreshTimer.Tick += (_, _) => { if (_window.Visible) _window.Refresh_Data(); };
        _refreshTimer.Start();
    }

    private void ShowWindow()
    {
        _window.Refresh_Data();
        _window.Show();
        _window.WindowState = FormWindowState.Normal;
        _window.Activate();
    }

    private void ExitApp()
    {
        _trayIcon.Visible = false;
        _refreshTimer.Stop();
        _window.Dispose();
        ExitThread();
    }
}
