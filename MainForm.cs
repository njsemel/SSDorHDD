using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SSDorHDD
{
    public partial class MainForm : Form
    {
        private bool dragging;
        private Point startPoint;
        private Label infoLabel;
        private Button okButton;

        public MainForm()
        {
            InitializeComponent();
            LoadDiskInfo();
        }

        private void LoadDiskInfo()
        {
            string resultString = GetDiskInfo();

            Text = "Drive Information";
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Black;
            FormBorderStyle = FormBorderStyle.None;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            infoLabel = new Label
            {
                Text = resultString,
                AutoSize = true,
                Font = new Font("Courier New", 14, FontStyle.Bold),
                ForeColor = Color.Lime,
                BackColor = Color.Black,
                Location = new Point(10, 10)
            };
            Controls.Add(infoLabel);

            okButton = new Button
            {
                Text = "OK",
                Font = new Font("Courier New", 14, FontStyle.Bold),
                ForeColor = Color.Lime,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Size = new Size(100, 40)
            };
            okButton.FlatAppearance.BorderColor = Color.Lime;
            okButton.FlatAppearance.BorderSize = 2;
            okButton.Location = new Point((ClientSize.Width - okButton.Width) / 2, infoLabel.Bottom + 10);
            okButton.Click += (sender, e) => Close();
            Controls.Add(okButton);

            MouseDown += Form_MouseDown;
            MouseMove += Form_MouseMove;
            MouseUp += Form_MouseUp;
        }

        private string GetDiskInfo()
        {
            string resultString = "";
            string powerShellCommand = "Get-Disk | Sort-Object -Property Number | Get-PhysicalDisk | Select-Object DeviceId, Manufacturer, Model, SerialNumber, MediaType | ConvertTo-Json";

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{powerShellCommand}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                process.Start();
                resultString = ParsePowerShellOutput(process.StandardOutput.ReadToEnd());
            }

            if (!resultString.Contains("SSD"))
            {
                resultString += "Warning: No SSD detected. Installing OS on an SSD can significantly boost performance.";
            }

            return resultString;
        }

        private string ParsePowerShellOutput(string output)
        {
            var resultString = "";
            var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int diskIndex = 0;
            string manufacturer = "";

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim().TrimEnd(',');
                if (trimmedLine.Contains("DeviceId"))
                {
                    resultString += $"Disk {diskIndex}\n";
                    manufacturer = "";
                }
                else if (trimmedLine.Contains("Manufacturer"))
                {
                    manufacturer = ExtractValue(trimmedLine);
                    manufacturer = string.IsNullOrEmpty(manufacturer) || manufacturer == "null" ? "(Standard disk drives)" : manufacturer;
                }
                else if (trimmedLine.Contains("Model"))
                {
                    resultString += $"Manufacturer: {manufacturer}\nModel: {ExtractValue(trimmedLine)}\n";
                }
                else if (trimmedLine.Contains("SerialNumber"))
                {
                    resultString += $"Serial Number: {ExtractValue(trimmedLine)}\n";
                }
                else if (trimmedLine.Contains("MediaType"))
                {
                    resultString += $"Media Type: {ExtractValue(trimmedLine)}\n\n";
                    diskIndex++;
                }
            }

            return resultString;
        }

        private string ExtractValue(string line)
        {
            int startIndex = line.IndexOf(':') + 1;
            return startIndex > 0 ? line.Substring(startIndex).Trim(' ', '"') : "Unknown";
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                startPoint = new Point(e.X, e.Y);
            }
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Location = new Point(Left + (e.X - startPoint.X), Top + (e.Y - startPoint.Y));
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
    }
}
