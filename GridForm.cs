using System.Windows.Forms;

namespace WindowOrganizer;

public partial class GridForm : Form
{
    public event EventHandler<Keys>? KeyPressed;
    private int screenIndex;
    private int columns;
    private int rows;

    public GridForm(int screenIndex, int columns, int rows)
    {
        this.screenIndex = screenIndex;
        this.columns = columns;
        this.rows = rows;
        InitializeComponent();
        this.KeyPreview = true;
        this.KeyDown += GridForm_KeyDown;
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        var screens = Screen.AllScreens;
        var screen = screens.Length > screenIndex ? screens[screenIndex] : Screen.PrimaryScreen;
        this.Bounds = screen.Bounds;
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.Manual;
        this.TopMost = true;
        this.BackColor = Color.FromArgb(240, 240, 240);
        this.Opacity = 0.9;
        this.Paint += GridForm_Paint;
        this.ResumeLayout(false);
    }

    private void GridForm_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int width = this.ClientSize.Width / columns;
        int height = this.ClientSize.Height / rows;

        // Draw grid lines
        using var pen = new Pen(Color.Gray, 2);
        for (int i = 1; i < columns; i++)
        {
            g.DrawLine(pen, i * width, 0, i * width, this.ClientSize.Height);
        }
        for (int i = 1; i < rows; i++)
        {
            g.DrawLine(pen, 0, i * height, this.ClientSize.Width, i * height);
        }

        // Draw key labels (only for 4x2, fallback for others)
        using var font = new Font("Arial", 48, FontStyle.Bold);
        using var brush = new SolidBrush(Color.Black);
        string[] keys = { "Q", "W", "E", "R", "A", "S", "D", "F" };
        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                string label = (columns == 4 && rows == 2 && index < keys.Length) ? keys[index++] : $"{col},{row}";
                var x = col * width + width / 2 - 30;
                var y = row * height + height / 2 - 30;
                g.DrawString(label, font, brush, x, y);
            }
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            this.Hide();
            return true;
        }

        if (keyData is Keys.Q or Keys.W or Keys.E or Keys.R or
            Keys.A or Keys.S or Keys.D or Keys.F)
        {
            KeyPressed?.Invoke(this, keyData);
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void GridForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            this.Hide();
            return;
        }

        if (e.KeyCode is Keys.Q or Keys.W or Keys.E or Keys.R or
            Keys.A or Keys.S or Keys.D or Keys.F)
        {
            KeyPressed?.Invoke(this, e.KeyCode);
        }
    }

    public void SetScreen(int screenIndex, int columns, int rows)
    {
        var screens = Screen.AllScreens;
        var screen = screens.Length > screenIndex ? screens[screenIndex] : Screen.PrimaryScreen;
        this.Bounds = screen.Bounds;
        this.columns = columns;
        this.rows = rows;
        this.Invalidate();
    }
} 