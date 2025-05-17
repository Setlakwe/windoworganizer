using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowOrganizer;

public partial class MainForm : Form
{
    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_ID = 1;
    private GridForm? gridForm;
    private bool isWaitingForSecondKey = false;
    private Keys firstKey = Keys.None;
    private NotifyIcon? trayIcon;
    private IntPtr? memorizedWindow = null;
    private int? firstGridX = null;
    private int? firstGridY = null;
    private int currentScreenIndex = 0;
    private bool waitingForScreenSelection = false;
    private GridConfig config;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public MainForm()
    {
        config = GridConfig.Load();
        InitializeComponent();
        InitializeTrayIcon();
        RegisterHotKey();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.ClientSize = new System.Drawing.Size(0, 0);
        this.Name = "MainForm";
        this.ShowInTaskbar = false;
        this.WindowState = FormWindowState.Minimized;
        this.FormBorderStyle = FormBorderStyle.None;
        this.ResumeLayout(false);
    }

    private void InitializeTrayIcon()
    {
        trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Window Organizer",
            Visible = true
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Config...", null, (s, e) => ShowConfigForm());
        contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());
        trayIcon.ContextMenuStrip = contextMenu;
    }

    private void RegisterHotKey()
    {
        // Register Win+Shift+O hotkey (0x0008 is MOD_WIN, 0x0004 is MOD_SHIFT)
        int modifiers = 0x0008 | 0x0004;
        if (!RegisterHotKey(this.Handle, HOTKEY_ID, modifiers, (int)Keys.O))
        {
            MessageBox.Show("Failed to register hotkey Win+Shift+O", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
        {
            memorizedWindow = GetForegroundWindow();
            // Get screen under mouse
            var mousePos = Cursor.Position;
            var screens = Screen.AllScreens;
            currentScreenIndex = Array.FindIndex(screens, s => s.Bounds.Contains(mousePos));
            if (currentScreenIndex < 0) currentScreenIndex = 0;
            ShowGridForm(currentScreenIndex);
            waitingForScreenSelection = screens.Length > 1;
            firstGridX = null;
            firstGridY = null;
        }
        base.WndProc(ref m);
    }

    private void ShowGridForm(int screenIndex)
    {
        var (columns, rows) = config.GetGridSize(screenIndex);
        if (gridForm == null || gridForm.IsDisposed)
        {
            gridForm = new GridForm(screenIndex, columns, rows);
            gridForm.KeyPressed += OnGridKeyPressed;
        }
        else
        {
            gridForm.SetScreen(screenIndex, columns, rows);
        }

        if (!gridForm.Visible)
        {
            gridForm.Show();
            gridForm.Activate();
            gridForm.Focus();
        }
    }

    private void OnGridKeyPressed(object? sender, Keys key)
    {
        var screens = Screen.AllScreens;
        if (waitingForScreenSelection)
        {
            if (key is Keys.D1 or Keys.NumPad1) { currentScreenIndex = 0; ShowGridForm(0); return; }
            if (key is Keys.D2 or Keys.NumPad2 && screens.Length > 1) { currentScreenIndex = 1; ShowGridForm(1); return; }
            if (key is Keys.D3 or Keys.NumPad3 && screens.Length > 2) { currentScreenIndex = 2; ShowGridForm(2); return; }
            if (key is Keys.D4 or Keys.NumPad4 && screens.Length > 3) { currentScreenIndex = 3; ShowGridForm(3); return; }
            waitingForScreenSelection = false;
        }
        var (x, y) = GetGridPosition(key);
        if (firstGridX == null || firstGridY == null)
        {
            firstGridX = x;
            firstGridY = y;
        }
        else
        {
            PositionWindow((firstGridX.Value, firstGridY.Value), (x, y));
            firstGridX = null;
            firstGridY = null;
            gridForm?.Hide();
        }
    }

    private void PositionWindow((int X, int Y) first, (int X, int Y) second)
    {
        if (memorizedWindow == null || memorizedWindow == IntPtr.Zero)
            return;
        var screen = Screen.AllScreens[currentScreenIndex];
        var workArea = screen.WorkingArea;
        var (columns, rows) = config.GetGridSize(currentScreenIndex);

        int width = workArea.Width / columns;
        int height = workArea.Height / rows;

        int startX = Math.Min(first.X, second.X);
        int startY = Math.Min(first.Y, second.Y);
        int endX = Math.Max(first.X, second.X);
        int endY = Math.Max(first.Y, second.Y);

        int finalX = workArea.Left + (startX * width);
        int finalY = workArea.Top + (startY * height);
        int finalWidth = ((endX - startX + 1) * width);
        int finalHeight = ((endY - startY + 1) * height);

        MoveWindow(memorizedWindow.Value, finalX, finalY, finalWidth, finalHeight, true);
        memorizedWindow = null;
    }

    private (int X, int Y) GetGridPosition(Keys key)
    {
        return key switch
        {
            Keys.Q => (0, 0),
            Keys.W => (1, 0),
            Keys.E => (2, 0),
            Keys.R => (3, 0),
            Keys.A => (0, 1),
            Keys.S => (1, 1),
            Keys.D => (2, 1),
            Keys.F => (3, 1),
            _ => (0, 0)
        };
    }

    private void ShowConfigForm()
    {
        using var form = new ConfigForm(config);
        if (form.ShowDialog() == DialogResult.OK)
        {
            config = GridConfig.Load();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        UnregisterHotKey(this.Handle, HOTKEY_ID);
        trayIcon?.Dispose();
        base.OnFormClosing(e);
    }
} 