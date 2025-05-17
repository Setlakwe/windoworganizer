using System.Windows.Forms;
using Microsoft.Win32;
using System.Drawing;

namespace WindowOrganizer;

public class ConfigForm : Form
{
    private GridConfig config;
    private DataGridView gridView;
    private Button saveButton;
    private Button cancelButton;

    public ConfigForm(GridConfig config)
    {
        this.config = config;
        this.Text = "Grid Configuration";
        this.Width = 400;
        this.Height = 300;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Opacity = 0.95;

        gridView = new DataGridView
        {
            Dock = DockStyle.Top,
            Height = 200,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false
        };
        gridView.Columns.Add("Screen", "Screen");
        gridView.Columns.Add("Columns", "Columns");
        gridView.Columns.Add("Rows", "Rows");

        var screens = Screen.AllScreens;
        for (int i = 0; i < screens.Length; i++)
        {
            var (cols, rows) = config.GetGridSize(i);
            gridView.Rows.Add($"Screen {i + 1}", cols, rows);
        }

        saveButton = new Button { Text = "Save", Dock = DockStyle.Bottom };
        cancelButton = new Button { Text = "Cancel", Dock = DockStyle.Bottom };
        saveButton.Click += SaveButton_Click;
        cancelButton.Click += (s, e) => this.Close();

        this.Controls.Add(gridView);
        this.Controls.Add(saveButton);
        this.Controls.Add(cancelButton);

        ApplyTheme();
    }

    private void ApplyTheme()
    {
        bool dark = IsDarkMode();
        if (dark)
        {
            this.BackColor = Color.FromArgb(32, 32, 32);
            this.ForeColor = Color.White;
            gridView.BackgroundColor = Color.FromArgb(32, 32, 32);
            gridView.DefaultCellStyle.BackColor = Color.FromArgb(32, 32, 32);
            gridView.DefaultCellStyle.ForeColor = Color.White;
            gridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(48, 48, 48);
            gridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            saveButton.BackColor = Color.FromArgb(48, 48, 48);
            saveButton.ForeColor = Color.White;
            cancelButton.BackColor = Color.FromArgb(48, 48, 48);
            cancelButton.ForeColor = Color.White;
        }
        else
        {
            this.BackColor = SystemColors.Control;
            this.ForeColor = SystemColors.ControlText;
            gridView.BackgroundColor = Color.White;
            gridView.DefaultCellStyle.BackColor = Color.White;
            gridView.DefaultCellStyle.ForeColor = Color.Black;
            gridView.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
            gridView.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
            saveButton.BackColor = SystemColors.Control;
            saveButton.ForeColor = SystemColors.ControlText;
            cancelButton.BackColor = SystemColors.Control;
            cancelButton.ForeColor = SystemColors.ControlText;
        }
    }

    private bool IsDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            if (key != null && key.GetValue("AppsUseLightTheme") is int v)
                return v == 0;
        }
        catch { }
        return false;
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        var screens = new List<ScreenConfig>();
        for (int i = 0; i < gridView.Rows.Count; i++)
        {
            if (int.TryParse(gridView.Rows[i].Cells[1].Value?.ToString(), out int cols) &&
                int.TryParse(gridView.Rows[i].Cells[2].Value?.ToString(), out int rows))
            {
                screens.Add(new ScreenConfig { Columns = cols, Rows = rows });
            }
        }
        config.Screens = screens;
        File.WriteAllText("config.json", System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
} 