using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SSDorHDD
{
    public partial class MainForm : Form
    {
        private bool dragging = false;
        private Point startPoint = new Point(0, 0);
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

            this.Text = "Drive Information";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // Initialize and configure label
            infoLabel = new Label
            {
                Text = resultString,
                AutoSize = true,
                Font = new Font("Courier New", 14, FontStyle.Bold),
                ForeColor = Color.Lime,
                BackColor = Color.Black,
                Location = new Point(10, 10)
            };
            this.Controls.Add(infoLabel);

            // Initialize and configure OK button
            okButton = new Button
            {
                Text = "OK",
                Font = new Font("Courier New", 14, FontStyle.Bold),
                ForeColor = Color.Lime,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Size = new Size(100, 40) // Set the size to fit the text
            };
            okButton.FlatAppearance.BorderColor = Color.Lime; // Set border color
            okButton.FlatAppearance.BorderSize = 2; // Set border size

            // Calculate the X coordinate to center the button
            int xPosition = (this.ClientSize.Width - okButton.Width) / 1;

            // Set the button's location
            okButton.Location = new Point(xPosition, infoLabel.Bottom + 10);

            // Close the form on button click
            okButton.Click += (sender, e) => this.Close();

            this.Controls.Add(okButton);

            // Event handlers for dragging
            this.MouseDown += new MouseEventHandler(Form_MouseDown);
            this.MouseMove += new MouseEventHandler(Form_MouseMove);
            this.MouseUp += new MouseEventHandler(Form_MouseUp);
        }

        private string GetDiskInfo()
        {
            string resultString = "";
            bool hasSSD = true;

            string powerShellCommand = "Get-Disk | Sort-Object -Property Number | Get-PhysicalDisk | Select-Object DeviceId, Manufacturer, Model, SerialNumber, MediaType | ConvertTo-Json";

            // Execute PowerShell command
            using (var process = new Process())
            {
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"-Command \"{powerShellCommand}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                resultString = ParsePowerShellOutput(output);

                // Check if any disk is SSD
                if (resultString.Contains("SSD"))
                {
                    hasSSD = true;
                }
            }

            if (!hasSSD)
            {
                resultString += "Warning: No SSD detected. Installing OS on an SSD can significantly boost performance.";
            }

            return resultString;
        }

        private string ParsePowerShellOutput(string output)
        {
            string resultString = "";
            string[] lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int diskIndex = 0; // To label disks as Disk 0, Disk 1, etc.
            string manufacturer = "";

            foreach (string line in lines)
            {
                // Remove unwanted characters and trim spaces
                string trimmedLine = line.Trim().TrimEnd(',');

                // Check for JSON-like key-value pairs
                if (trimmedLine.Contains("DeviceId"))
                {
                    resultString += $"Disk {diskIndex}\n";
                    manufacturer = ""; // Reset manufacturer for the next disk
                }
                else if (trimmedLine.Contains("Manufacturer"))
                {
                    manufacturer = ExtractValue(trimmedLine);
                    if (string.IsNullOrEmpty(manufacturer) || manufacturer == "null")
                    {
                        manufacturer = "(Standard disk drives)"; // Default value
                    }
                }
                else if (trimmedLine.Contains("Model"))
                {
                    resultString += $"Manufacturer: {manufacturer}\n"; // Add manufacturer info
                    resultString += $"Model: {ExtractValue(trimmedLine)}\n";
                }
                else if (trimmedLine.Contains("SerialNumber"))
                {
                    resultString += $"Serial Number: {ExtractValue(trimmedLine)}\n";
                }
                else if (trimmedLine.Contains("MediaType"))
                {
                    resultString += $"Media Type: {ExtractValue(trimmedLine)}\n\n";
                    diskIndex++; // Move to the next disk
                }
            }

            return resultString;
        }

        private string ExtractValue(string line)
        {
            // Extract the value from a key-value pair (e.g., "Key": "Value")
            int startIndex = line.IndexOf(':') + 1;
            if (startIndex < 0) return "Unknown";

            string value = line.Substring(startIndex).Trim(' ', '"');
            return string.IsNullOrEmpty(value) ? "Unknown" : value;
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
                Point newPoint = new Point(this.Left + (e.X - startPoint.X), this.Top + (e.Y - startPoint.Y));
                this.Location = newPoint;
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
    }
}
