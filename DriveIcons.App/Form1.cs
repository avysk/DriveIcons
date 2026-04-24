using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace DriveIconsApp;

public partial class Form1 : Form
{
    private const int DetailsLeft = 496;
    private const int DetailsTop = 12;
    private const int DetailsWidth = 292;
    private const int DetailsSpacing = 6;
    private const int DriveGridRowHeight = 24;
    private const int DriveGridIconPadding = 4;
    private const string NoCustomIconText = "(none)";
    private const string NoSystemIconText = "(unavailable)";
    private const string MissingDriveSystemIconText = "(drive not present)";
    private const uint InvalidFileAttributes = 0xFFFFFFFF;

    private readonly DataGridView dgvDrives = new();
    private readonly StatusStrip statusStrip = new();
    private readonly ToolStripStatusLabel lblStatus = new();
    private readonly PictureBox pbPreview = new();
    private readonly Label lblSystemIcon = new();
    private readonly Label lblCustomIcon = new();
    private readonly Button btnBrowse = new();
    private readonly Button btnSet = new();
    private readonly Button btnReset = new();
    private readonly Button btnRestartExplorer = new();
    private readonly OpenFileDialog openFileDialog = new();
    private string _lastBrowsePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    private Fi.Pentode.Registry.Lib.DriveIcons? _driveIcons;
    private Dictionary<char, Bitmap> _iconCache = new();
    private char? _selectedDrive;

    public Form1()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        ((ISupportInitialize)dgvDrives).BeginInit();
        statusStrip.SuspendLayout();
        ((ISupportInitialize)pbPreview).BeginInit();
        SuspendLayout();

        // dgvDrives
        dgvDrives.AllowUserToAddRows = false;
        dgvDrives.AllowUserToDeleteRows = false;
        dgvDrives.AllowUserToResizeRows = false;
        dgvDrives.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvDrives.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Letter", Width = 50 });
        dgvDrives.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Type", Width = 90 });
        dgvDrives.Columns.Add(new DataGridViewImageColumn
        {
            HeaderText = string.Empty,
            Width = 32,
            ImageLayout = DataGridViewImageCellLayout.Normal,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });
        dgvDrives.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Custom Icon Path", Width = 300 });
        dgvDrives.Location = new Point(12, 12);
        dgvDrives.MultiSelect = false;
        dgvDrives.Name = "dgvDrives";
        dgvDrives.ReadOnly = true;
        dgvDrives.RowTemplate.Height = DriveGridRowHeight;
        dgvDrives.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvDrives.Size = new Size(472, 460);
        dgvDrives.TabIndex = 0;
        dgvDrives.SelectionChanged += dgvDrives_SelectionChanged;

        // statusStrip
        statusStrip.Items.Add(lblStatus);
        statusStrip.Location = new Point(0, 525);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(800, 22);
        statusStrip.TabIndex = 1;
        statusStrip.Text = "statusStrip1";

        // lblStatus
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(39, 17);
        lblStatus.Text = "Ready";

        // pbPreview - fixed size, not AutoSize
        pbPreview.BorderStyle = BorderStyle.FixedSingle;
        pbPreview.Location = new Point(DetailsLeft, 60);
        pbPreview.Name = "pbPreview";
        pbPreview.Size = new Size(128, 128);
        pbPreview.SizeMode = PictureBoxSizeMode.Zoom;
        pbPreview.TabIndex = 2;
        pbPreview.TabStop = false;
        pbPreview.BackColor = Color.White;

        // lblSystemIcon
        lblSystemIcon.AutoEllipsis = true;
        lblSystemIcon.AutoSize = false;
        lblSystemIcon.Location = new Point(DetailsLeft, DetailsTop);
        lblSystemIcon.Name = "lblSystemIcon";
        lblSystemIcon.Size = new Size(DetailsWidth, 15);
        lblSystemIcon.TabIndex = 3;
        lblSystemIcon.Text = $"System: {NoSystemIconText}";
        lblSystemIcon.TextAlign = ContentAlignment.MiddleLeft;

        // lblCustomIcon
        lblCustomIcon.AutoEllipsis = true;
        lblCustomIcon.AutoSize = false;
        lblCustomIcon.Location = new Point(DetailsLeft, 30);
        lblCustomIcon.Name = "lblCustomIcon";
        lblCustomIcon.Size = new Size(DetailsWidth, 15);
        lblCustomIcon.TabIndex = 4;
        lblCustomIcon.Text = $"Custom: {NoCustomIconText}";
        lblCustomIcon.TextAlign = ContentAlignment.MiddleLeft;

        // btnBrowse
        btnBrowse.Location = new Point(DetailsLeft, 190);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(120, 30);
        btnBrowse.TabIndex = 5;
        btnBrowse.Text = "Browse...";
        btnBrowse.UseVisualStyleBackColor = true;
        btnBrowse.Click += btnBrowse_Click;

        // btnSet
        btnSet.Location = new Point(DetailsLeft, 226);
        btnSet.Name = "btnSet";
        btnSet.Size = new Size(120, 30);
        btnSet.TabIndex = 6;
        btnSet.Text = "Set Icon";
        btnSet.UseVisualStyleBackColor = true;
        btnSet.Click += btnSet_Click;

        // btnReset
        btnReset.Location = new Point(DetailsLeft, 262);
        btnReset.Name = "btnReset";
        btnReset.Size = new Size(120, 30);
        btnReset.TabIndex = 7;
        btnReset.Text = "Reset Icon";
        btnReset.UseVisualStyleBackColor = true;
        btnReset.Click += btnReset_Click;

        // btnRestartExplorer
        btnRestartExplorer.Location = new Point(DetailsLeft, 298);
        btnRestartExplorer.Name = "btnRestartExplorer";
        btnRestartExplorer.Size = new Size(180, 30);
        btnRestartExplorer.TabIndex = 8;
        btnRestartExplorer.Text = "Restart Explorer";
        btnRestartExplorer.UseVisualStyleBackColor = true;
        btnRestartExplorer.Click += btnRestartExplorer_Click;

        // openFileDialog
        openFileDialog.Filter = "Icon Files (*.ico)|*.ico|All Files (*.*)|*.*";
        openFileDialog.Title = "Select Icon File";

        // Form1
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 547);
        Controls.Add(btnRestartExplorer);
        Controls.Add(btnReset);
        Controls.Add(btnSet);
        Controls.Add(btnBrowse);
        Controls.Add(lblCustomIcon);
        Controls.Add(lblSystemIcon);
        Controls.Add(pbPreview);
        Controls.Add(statusStrip);
        Controls.Add(dgvDrives);
        lblCustomIcon.BringToFront();
        lblSystemIcon.BringToFront();
        pbPreview.SendToBack();
        LayoutDetailsPane();
        Name = "Form1";
        Text = "DriveIcons — Custom Drive Icon Editor";
        FormClosed += Form1_FormClosed;
        Load += Form1_Load;
        ((ISupportInitialize)dgvDrives).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ((ISupportInitialize)pbPreview).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private void dgvDrives_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvDrives.SelectedRows.Count > 0)
        {
            var row = dgvDrives.SelectedRows[0];
            string letterStr = row.Cells[0].Value?.ToString() ?? "";
            if (char.TryParse(letterStr, out char drive))
            {
                _selectedDrive = drive;
                UpdatePreview(drive);
            }
        }
    }

    private void UpdatePreview(char drive)
    {
        string drivePath = $"{drive}:\\";
        lblStatus.Text = "Ready.";
        SetPreviewImage(null);
        lblSystemIcon.Text = $"System: {NoSystemIconText}";
        lblCustomIcon.Text = $"Custom: {NoCustomIconText}";

        // Check if drive exists using Win32 API
        uint fileAttrs = GetFileAttributes(drivePath);
        if (fileAttrs == InvalidFileAttributes)
        {
            lblSystemIcon.Text = $"System: {MissingDriveSystemIconText}";
            lblStatus.Text = $"Drive {drive}: does not exist.";
        }
        else
        {
            // Extract system icon
            try
            {
                Bitmap? systemBitmap = TryLoadDriveShellBitmap(drivePath, fileAttrs, largeIcon: true);
                if (systemBitmap != null)
                {
                    SetPreviewImage(systemBitmap);
                    lblSystemIcon.Text = $"System: {drivePath}";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error loading system icon: {ex.Message}";
            }
        }

        // Check for custom icon
        if (_driveIcons != null)
        {
            try
            {
                string? customPath = _driveIcons[drive];
                if (string.IsNullOrWhiteSpace(customPath))
                {
                    return;
                }

                string displayedCustomPath =
                    TryResolveIconFilePath(customPath, out string resolvedCustomPath)
                    ? resolvedCustomPath
                    : customPath;
                lblCustomIcon.Text = $"Custom: {displayedCustomPath}";
                Bitmap? customBitmap = TryLoadCustomIconBitmap(customPath, largeIcon: true);
                if (customBitmap != null)
                {
                    SetPreviewImage(customBitmap);
                }
                else
                {
                    lblCustomIcon.Text = $"Custom: {displayedCustomPath} (unreadable)";
                }
            }
            catch (Exception ex)
            {
                lblCustomIcon.Text = "Custom: (error)";
                lblStatus.Text = $"Error reading registry: {ex.Message}";
            }
        }
    }

    private void btnBrowse_Click(object? sender, EventArgs e)
    {
        if (_selectedDrive == null) { lblStatus.Text = "Please select a drive first."; return; }
        openFileDialog.InitialDirectory = _lastBrowsePath;
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            string filePath = openFileDialog.FileName;
            _lastBrowsePath = Path.GetDirectoryName(filePath) ?? _lastBrowsePath;
            lblCustomIcon.Text = $"Custom: {filePath}";
            lblStatus.Text = $"Selected: {filePath}. Click 'Set Icon' to apply.";
        }
    }

    private void btnSet_Click(object? sender, EventArgs e)
    {
        if (_selectedDrive == null) { lblStatus.Text = "Please select a drive first."; return; }
        string customText = lblCustomIcon.Text.Replace("Custom: ", "");
        if (string.IsNullOrEmpty(customText) || customText == NoCustomIconText)
        { lblStatus.Text = "No icon file selected."; return; }
        if (!TryResolveIconFilePath(customText, out string iconFilePath) || !File.Exists(iconFilePath))
        { lblStatus.Text = $"File not found: {customText}"; return; }
        try
        {
            if (_driveIcons == null) { lblStatus.Text = "Application not initialized."; return; }
            _driveIcons[_selectedDrive.Value] = customText;
            RefreshRow(_selectedDrive.Value);
            lblStatus.Text = $"Icon set for drive {_selectedDrive.Value}.";
        }
        catch (Fi.Pentode.Registry.Lib.RegistryException ex)
        { lblStatus.Text = $"Registry error: {ex.Message}"; }
        catch (Exception ex) { lblStatus.Text = $"Error: {ex.Message}"; }
    }

    private void btnReset_Click(object? sender, EventArgs e)
    {
        if (_selectedDrive == null) { lblStatus.Text = "Please select a drive first."; return; }
        try
        {
            if (_driveIcons == null) { lblStatus.Text = "Application not initialized."; return; }
            _driveIcons[_selectedDrive.Value] = null;
            RefreshRow(_selectedDrive.Value);
            lblStatus.Text = $"Icon reset for drive {_selectedDrive.Value}.";
        }
        catch (Fi.Pentode.Registry.Lib.RegistryException ex)
        { lblStatus.Text = $"Registry error: {ex.Message}"; }
        catch (Exception ex) { lblStatus.Text = $"Error: {ex.Message}"; }
    }

    private void btnRestartExplorer_Click(object? sender, EventArgs e)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c taskkill /f /im explorer.exe & start explorer.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(psi);
            lblStatus.Text = "Restarting Explorer...";
        }
        catch (Exception ex) { lblStatus.Text = $"Error restarting Explorer: {ex.Message}"; }
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        if (!IsRunningAsAdmin())
        {
            MessageBox.Show(
                "This application requires administrator privileges.\n" +
                "Please run it as admin or use 'Run as administrator'.",
                "Admin Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            Application.Exit();
            return;
        }

        _driveIcons = new Fi.Pentode.Registry.Lib.DriveIcons(
            new Fi.Pentode.Registry.Lib.WindowsRegistryKey(Microsoft.Win32.Registry.LocalMachine));
        PopulateDrives();
        lblStatus.Text = "Ready.";
    }

    private static bool IsRunningAsAdmin()
    {
        var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }

    private void PopulateDrives()
    {
        DisposeDriveGridIcons();
        dgvDrives.Rows.Clear();
        DisposeIconCache();
        for (char drive = 'A'; drive <= 'Z'; drive++)
        {
            string drivePath = $"{drive}:\\";
            string type = "(none)";
            string customPath = "(none)";
            try
            {
                var di = new DriveInfo(drivePath);
                if (di.IsReady)
                {
                    type = di.DriveType switch
                    {
                        DriveType.Fixed => "Fixed",
                        DriveType.Removable => "Removable",
                        DriveType.CDRom => "CDROM",
                        DriveType.Network => "Network",
                        DriveType.Ram => "RAM",
                        _ => di.DriveType.ToString()
                    };
                }
            }
            catch { type = "(none)"; }

            // Read custom icon path separately
            try { customPath = _driveIcons?[drive] ?? "(none)"; }
            catch { customPath = "(none)"; }

            int rowIndex = dgvDrives.Rows.Add(drive.ToString(), type, null, customPath);

            // Extract system icon for thumbnail
            uint fileAttrs = GetFileAttributes(drivePath);
            if (fileAttrs != InvalidFileAttributes)
            {
                try
                {
                    Bitmap? bitmap = TryLoadDriveShellBitmap(drivePath, fileAttrs, largeIcon: true);
                    if (bitmap != null)
                    {
                        _iconCache[drive] = bitmap;
                    }
                }
                catch { }
            }

            ApplyDriveRowIcon(rowIndex, drive, customPath);
        }
        if (dgvDrives.Rows.Count > 0) dgvDrives.Rows[0].Selected = true;
    }

    private void RefreshRow(char drive)
    {
        for (int i = 0; i < dgvDrives.Rows.Count; i++)
        {
            string letter = dgvDrives.Rows[i].Cells[0].Value?.ToString() ?? "";
            if (letter == drive.ToString())
            {
                string customPath = "(none)";
                try { customPath = _driveIcons?[drive] ?? "(none)"; } catch { }
                dgvDrives.Rows[i].Cells[3].Value = customPath;

                ApplyDriveRowIcon(i, drive, customPath);
                break;
            }
        }
        if (_selectedDrive == drive) UpdatePreview(drive);
    }

    private void ApplyDriveRowIcon(int rowIndex, char drive, string? customPath)
    {
        Bitmap? customBitmap = TryLoadCustomIconBitmap(customPath, largeIcon: true);
        try
        {
            Bitmap? displayBitmap = customBitmap != null
                ? ResizeDriveGridIcon(customBitmap)
                : _iconCache.TryGetValue(drive, out var systemBitmap)
                    ? ResizeDriveGridIcon(systemBitmap)
                    : null;
            SetDriveRowIcon(rowIndex, displayBitmap);
        }
        finally
        {
            customBitmap?.Dispose();
        }
    }

    private void LayoutDetailsPane()
    {
        int labelHeight = TextRenderer.MeasureText("Ag", lblSystemIcon.Font).Height + 2;
        lblSystemIcon.SetBounds(DetailsLeft, DetailsTop, DetailsWidth, labelHeight);
        lblCustomIcon.SetBounds(
            DetailsLeft,
            lblSystemIcon.Bottom + DetailsSpacing,
            DetailsWidth,
            labelHeight
        );
        pbPreview.Location = new Point(DetailsLeft, lblCustomIcon.Bottom + DetailsSpacing);
        btnBrowse.Location = new Point(DetailsLeft, pbPreview.Bottom + 12);
        btnSet.Location = new Point(DetailsLeft, btnBrowse.Bottom + DetailsSpacing);
        btnReset.Location = new Point(DetailsLeft, btnSet.Bottom + DetailsSpacing);
        btnRestartExplorer.Location = new Point(DetailsLeft, btnReset.Bottom + DetailsSpacing);
    }

    private static Bitmap? TryLoadDriveShellBitmap(string drivePath, uint fileAttrs, bool largeIcon)
    {
        var shfi = new SHFILEINFO();
        IntPtr result = SHGetFileInfo(
            drivePath,
            fileAttrs,
            ref shfi,
            (uint)Marshal.SizeOf(shfi),
            SHGFI.Icon
            | (largeIcon ? SHGFI.LargeIcon : SHGFI.SmallIcon)
            | SHGFI.UseFileAttributes
        );

        if (result == IntPtr.Zero || shfi.hIcon == IntPtr.Zero)
        {
            return null;
        }

        IntPtr hIcon = shfi.hIcon;
        try
        {
            using var clonedIcon = (Icon)Icon.FromHandle(hIcon).Clone();
            return clonedIcon.ToBitmap();
        }
        finally
        {
            DestroyIcon(hIcon);
        }
    }

    private static Bitmap? TryLoadCustomIconBitmap(string? iconLocation, bool largeIcon)
    {
        if (!TryResolveIconFilePath(iconLocation, out string iconFilePath))
        {
            return null;
        }

        if (!File.Exists(iconFilePath))
        {
            return null;
        }

        try
        {
            if (string.Equals(Path.GetExtension(iconFilePath), ".ico", StringComparison.OrdinalIgnoreCase))
            {
                using var fileIcon = new Icon(
                    iconFilePath,
                    largeIcon ? SystemInformation.IconSize : SystemInformation.SmallIconSize
                );
                return fileIcon.ToBitmap();
            }

            using var associatedIcon = Icon.ExtractAssociatedIcon(iconFilePath);
            return associatedIcon?.ToBitmap();
        }
        catch
        {
            return null;
        }
    }

    private static bool TryResolveIconFilePath(string? iconLocation, out string iconFilePath)
    {
        iconFilePath = string.Empty;
        if (string.IsNullOrWhiteSpace(iconLocation))
        {
            return false;
        }

        string expandedLocation = Environment.ExpandEnvironmentVariables(iconLocation.Trim());
        if (expandedLocation.Length == 0)
        {
            return false;
        }

        if (expandedLocation[0] == '"')
        {
            int closingQuote = expandedLocation.IndexOf('"', 1);
            if (closingQuote > 0)
            {
                iconFilePath = expandedLocation[1..closingQuote];
                return !string.IsNullOrWhiteSpace(iconFilePath);
            }
        }

        int commaIndex = expandedLocation.LastIndexOf(',');
        if (commaIndex > 0 && int.TryParse(expandedLocation[(commaIndex + 1)..].Trim(), out _))
        {
            expandedLocation = expandedLocation[..commaIndex];
        }

        iconFilePath = expandedLocation.Trim().Trim('"');
        return !string.IsNullOrWhiteSpace(iconFilePath);
    }

    private Bitmap? ResizeDriveGridIcon(Image? source)
    {
        if (source == null)
        {
            return null;
        }

        int maxDimension = Math.Max(1, dgvDrives.RowTemplate.Height - DriveGridIconPadding);
        float scale = Math.Min((float)maxDimension / source.Width, (float)maxDimension / source.Height);
        int resizedWidth = Math.Max(1, (int)Math.Round(source.Width * scale));
        int resizedHeight = Math.Max(1, (int)Math.Round(source.Height * scale));

        var resizedBitmap = new Bitmap(resizedWidth, resizedHeight);
        using var graphics = Graphics.FromImage(resizedBitmap);
        graphics.Clear(Color.Transparent);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.DrawImage(source, 0, 0, resizedWidth, resizedHeight);
        return resizedBitmap;
    }

    private void SetDriveRowIcon(int rowIndex, Bitmap? bitmap)
    {
        var cell = dgvDrives.Rows[rowIndex].Cells[2];
        if (cell.Value is Image previousImage)
        {
            previousImage.Dispose();
        }

        cell.Value = bitmap;
    }

    private void DisposeDriveGridIcons()
    {
        foreach (DataGridViewRow row in dgvDrives.Rows)
        {
            if (row.Cells[2].Value is Image image)
            {
                image.Dispose();
                row.Cells[2].Value = null;
            }
        }
    }

    private void DisposeIconCache()
    {
        foreach (Bitmap bitmap in _iconCache.Values)
        {
            bitmap.Dispose();
        }

        _iconCache.Clear();
    }

    private void SetPreviewImage(Image? image)
    {
        var previousImage = pbPreview.Image;
        pbPreview.Image = image;
        previousImage?.Dispose();
    }

    private void Form1_FormClosed(object? sender, FormClosedEventArgs e)
    {
        SetPreviewImage(null);
        DisposeDriveGridIcons();
        DisposeIconCache();
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern uint GetFileAttributes(string path);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi,
        uint cbSizeFileInfo, SHGFI uFlags);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [Flags]
    private enum SHGFI : uint
    {
        Icon = 0x000000100, SmallIcon = 0x000000001, LargeIcon = 0x000000000, UseFileAttributes = 0x000000010
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        public string szTypeName;
    }
}
