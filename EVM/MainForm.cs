using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EVM
{
    public class MainForm : Form
    {
        private TextBox txtInputFile = null!;
        private TextBox txtOutputFolder = null!;
        private TextBox txtOutputFileName = null!;
        private NumericUpDown numFl = null!;
        private NumericUpDown numFh = null!;
        private NumericUpDown numNLevels = null!;
        private NumericUpDown numAttenuation = null!;
        private NumericUpDown numAlpha = null!;
        private ComboBox cmbPyramidType = null!;
        private Button btnBrowseInput = null!;
        private Button btnBrowseOutput = null!;
        private Button btnStart = null!;
        private Button btnHeartbeat = null!;
        private Button btnBreathing = null!;
        private ProgressBar progressBar = null!;
        private Label lblStatus = null!;
        private TextBox txtLog = null!;
        private double detectedFps = 30;

        public MainForm()
        {
            Text = "EVM Video Processing Configuration";
            Size = new Size(720, 620);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            int y = 20;
            int labelWidth = 130;
            int controlWidth = 420;
            int controlHeight = 25;
            int spacing = 35;

            // Input File
            Label lblInput = new Label
            {
                Text = "Input Video File:",
                Location = new Point(20, y),
                Size = new Size(labelWidth, controlHeight),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblInput);

            txtInputFile = new TextBox
            {
                Location = new Point(150, y),
                Size = new Size(controlWidth, controlHeight),
                Text = ""
            };
            txtInputFile.TextChanged += TxtInputFile_TextChanged;
            Controls.Add(txtInputFile);

            btnBrowseInput = new Button
            {
                Text = "Browse...",
                Location = new Point(580, y),
                Size = new Size(80, controlHeight)
            };
            btnBrowseInput.Click += BtnBrowseInput_Click;
            Controls.Add(btnBrowseInput);

            y += spacing;

            // Output Folder
            Label lblOutput = new Label
            {
                Text = "Output Folder:",
                Location = new Point(20, y),
                Size = new Size(labelWidth, controlHeight),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblOutput);

            txtOutputFolder = new TextBox
            {
                Location = new Point(150, y),
                Size = new Size(controlWidth, controlHeight),
                Text = ""
            };
            Controls.Add(txtOutputFolder);

            btnBrowseOutput = new Button
            {
                Text = "Browse...",
                Location = new Point(580, y),
                Size = new Size(80, controlHeight)
            };
            btnBrowseOutput.Click += BtnBrowseOutput_Click;
            Controls.Add(btnBrowseOutput);

            y += spacing;

            // Output File Name
            Label lblOutputFile = new Label
            {
                Text = "Output File Name:",
                Location = new Point(20, y),
                Size = new Size(labelWidth, controlHeight),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblOutputFile);

            txtOutputFileName = new TextBox
            {
                Location = new Point(150, y),
                Size = new Size(controlWidth, controlHeight),
                Text = ""
            };
            Controls.Add(txtOutputFileName);

            y += spacing + 10;

            // Parameters Group
            GroupBox groupParams = new GroupBox
            {
                Text = "Processing Parameters",
                Location = new Point(20, y),
                Size = new Size(640, 150)
            };
            Controls.Add(groupParams);

            int paramY = 25;
            int paramLabelWidth = 100;
            int paramControlWidth = 100;

            // Pyramid Type
            Label lblPyramid = new Label
            {
                Text = "Pyramid Type:",
                Location = new Point(20, paramY),
                Size = new Size(paramLabelWidth, controlHeight),
                TextAlign = ContentAlignment.MiddleLeft
            };
            groupParams.Controls.Add(lblPyramid);

            cmbPyramidType = new ComboBox
            {
                Location = new Point(120, paramY),
                Size = new Size(paramControlWidth, controlHeight),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPyramidType.Items.Add("Gaussian");
            cmbPyramidType.Items.Add("Laplacian");
            cmbPyramidType.SelectedIndex = 0;
            groupParams.Controls.Add(cmbPyramidType);

            // Alpha (Amplification)
            Label lblAlpha = new Label
            {
                Text = "Alpha:",
                Location = new Point(420, paramY),
                Size = new Size(50, controlHeight),
                TextAlign = ContentAlignment.MiddleLeft
            };
            groupParams.Controls.Add(lblAlpha);

            numAlpha = new NumericUpDown
            {
                Location = new Point(470, paramY),
                Size = new Size(90, controlHeight),
                DecimalPlaces = 0,
                Minimum = 1,
                Maximum = 200,
                Value = 50,
                Increment = 1
            };
            groupParams.Controls.Add(numAlpha);

            // attenuation
            Label lblAttenuation = new Label
            {
                Text = "Attenuation:",
                Location = new Point(240, paramY),
                Size = new Size(80, controlHeight),
                TextAlign = ContentAlignment.MiddleLeft
            };
            groupParams.Controls.Add(lblAttenuation);

            numAttenuation = new NumericUpDown
            {
                Location = new Point(320, paramY),
                Size = new Size(90, controlHeight),
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 100,
                Value = 1,
                Increment = 0.01m
            };
            groupParams.Controls.Add(numAttenuation);

            paramY += spacing;

            // nLevels
            Label lblNLevels = new Label
            {
                Text = "nLevels:",
                Location = new Point(20, paramY),
                Size = new Size(paramLabelWidth, controlHeight),
                TextAlign = ContentAlignment.MiddleLeft
            };
            groupParams.Controls.Add(lblNLevels);

            numNLevels = new NumericUpDown
            {
                Location = new Point(120, paramY),
                Size = new Size(paramControlWidth, controlHeight),
                Minimum = 1,
                Maximum = 10,
                Value = 8
            };
            groupParams.Controls.Add(numNLevels);

            // fl
            Label lblFl = new Label
            {
                Text = "fl (Hz):",
                Location = new Point(240, paramY),
                Size = new Size(60, controlHeight),
                TextAlign = ContentAlignment.MiddleLeft
            };
            groupParams.Controls.Add(lblFl);

            numFl = new NumericUpDown
            {
                Location = new Point(300, paramY),
                Size = new Size(90, controlHeight),
                DecimalPlaces = 4,
                Minimum = 0,
                Maximum = 10,
                Value = 0.3333m,
                Increment = 0.01m
            };
            groupParams.Controls.Add(numFl);

            // fh
            Label lblFh = new Label
            {
                Text = "fh (Hz):",
                Location = new Point(410, paramY),
                Size = new Size(60, controlHeight),
                TextAlign = ContentAlignment.MiddleLeft
            };
            groupParams.Controls.Add(lblFh);

            numFh = new NumericUpDown
            {
                Location = new Point(470, paramY),
                Size = new Size(90, controlHeight),
                DecimalPlaces = 4,
                Minimum = 0,
                Maximum = 10,
                Value = 1.6667m,
                Increment = 0.01m
            };
            groupParams.Controls.Add(numFh);

            paramY += spacing;

            // Preset Buttons
            Label lblPreset = new Label
            {
                Text = "Presets:",
                Location = new Point(20, paramY),
                Size = new Size(60, controlHeight),
                TextAlign = ContentAlignment.MiddleLeft
            };
            groupParams.Controls.Add(lblPreset);

            btnHeartbeat = new Button
            {
                Text = "Heartbeat (1-3 Hz)",
                Location = new Point(80, paramY - 2),
                Size = new Size(130, 28),
                BackColor = Color.LightPink
            };
            btnHeartbeat.Click += BtnHeartbeat_Click;
            groupParams.Controls.Add(btnHeartbeat);

            btnBreathing = new Button
            {
                Text = "Breathing (0.2-0.67 Hz)",
                Location = new Point(220, paramY - 2),
                Size = new Size(150, 28),
                BackColor = Color.LightCyan
            };
            btnBreathing.Click += BtnBreathing_Click;
            groupParams.Controls.Add(btnBreathing);

            y += 190;

            // Start Button
            btnStart = new Button
            {
                Text = "Start Processing",
                Location = new Point(20, y),
                Size = new Size(140, 35),
                BackColor = Color.LightBlue,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnStart.Click += BtnStart_Click;
            Controls.Add(btnStart);

            // Progress Bar
            progressBar = new ProgressBar
            {
                Location = new Point(170, y + 5),
                Size = new Size(350, 25),
                Minimum = 0,
                Maximum = 100
            };
            Controls.Add(progressBar);

            // Status Label
            lblStatus = new Label
            {
                Text = "Ready",
                Location = new Point(530, y + 5),
                Size = new Size(130, controlHeight),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblStatus);

            y += 50;

            // Log Area
            Label lblLog = new Label
            {
                Text = "Processing Log:",
                Location = new Point(20, y),
                Size = new Size(labelWidth, controlHeight),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblLog);

            y += 25;

            txtLog = new TextBox
            {
                Location = new Point(20, y),
                Size = new Size(640, 180),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Consolas", 9)
            };
            Controls.Add(txtLog);
        }

        private void TxtInputFile_TextChanged(object? sender, EventArgs e)
        {
            UpdateDefaultOutputPath();
            DetectVideoFps();
        }

        private void DetectVideoFps()
        {
            string inputFile = txtInputFile.Text;
            if (string.IsNullOrWhiteSpace(inputFile) || !File.Exists(inputFile))
            {
                detectedFps = 30;
                return;
            }

            try
            {
                using (var capture = new Emgu.CV.VideoCapture(inputFile))
                {
                    if (capture.IsOpened)
                    {
                        detectedFps = capture.Get(Emgu.CV.CvEnum.CapProp.Fps);
                        if (detectedFps <= 0 || double.IsNaN(detectedFps))
                        {
                            detectedFps = 30;
                        }
                        Log($"Detected video FPS: {detectedFps:F2} (important for Butterworth filter)");
                    }
                }
            }
            catch (Exception ex)
            {
                detectedFps = 30;
                Log($"Failed to detect FPS: {ex.Message}, using default: 30");
            }
        }

        private void BtnHeartbeat_Click(object? sender, EventArgs e)
        {
            numFl.Value = 1.0m;
            numFh.Value = 3.0m;
            Log("Preset: Heartbeat - fl=1 Hz, fh=3 Hz (equals to 60-180 BPM)");
        }

        private void BtnBreathing_Click(object? sender, EventArgs e)
        {
            numFl.Value = 0.2m;
            numFh.Value = 0.67m;
            Log("Preset: Breathing - fl=0.2 Hz, fh=0.67 Hz (equals to 12-40 BPM)");
        }

        private void Log(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(Log), message);
                return;
            }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }

        private void UpdateDefaultOutputPath()
        {
            string inputFile = txtInputFile.Text;
            if (string.IsNullOrWhiteSpace(inputFile) || !File.Exists(inputFile))
            {
                return;
            }

            string? inputDir = Path.GetDirectoryName(inputFile);
            if (string.IsNullOrEmpty(inputDir))
            {
                return;
            }

            // Only set default output folder if it's empty (first time)
            string defaultOutputFolder = Path.Combine(inputDir, "results");
            if (string.IsNullOrWhiteSpace(txtOutputFolder.Text))
            {
                txtOutputFolder.Text = defaultOutputFolder;
            }

            // Always update output filename when input file changes
            string inputFileName = Path.GetFileNameWithoutExtension(inputFile);
            string inputExtension = Path.GetExtension(inputFile);
            txtOutputFileName.Text = $"amplified_{inputFileName}{inputExtension}";
        }

        private void BtnBrowseInput_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Input Video File";
                dialog.Filter = "Video Files (*.mp4;*.avi;*.mov;*.mkv)|*.mp4;*.avi;*.mov;*.mkv|All Files (*.*)|*.*";
                if (!string.IsNullOrWhiteSpace(txtInputFile.Text))
                {
                    string? dir = Path.GetDirectoryName(txtInputFile.Text);
                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    {
                        dialog.InitialDirectory = dir;
                    }
                }
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtInputFile.Text = dialog.FileName;
                }
            }
        }

        private void BtnBrowseOutput_Click(object? sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select Output Folder";
                if (Directory.Exists(txtOutputFolder.Text))
                {
                    dialog.SelectedPath = txtOutputFolder.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtInputFile.Text))
                {
                    string? inputDir = Path.GetDirectoryName(txtInputFile.Text);
                    if (!string.IsNullOrEmpty(inputDir) && Directory.Exists(inputDir))
                    {
                        dialog.SelectedPath = inputDir;
                    }
                }
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutputFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            btnStart.Enabled = false;
            lblStatus.Text = "Processing...";
            txtLog.Clear();

            PyramidType pyramidType = cmbPyramidType.SelectedIndex == 0 ? PyramidType.Gaussian : PyramidType.Laplacian;

            VideoProcessorConfig config = new VideoProcessorConfig
            {
                InputFile = txtInputFile.Text,
                OutputFolder = txtOutputFolder.Text,
                OutputFileName = txtOutputFileName.Text,
                Fl = (double)numFl.Value,
                Fh = (double)numFh.Value,
                NLevels = (int)numNLevels.Value,
                Attenuation = (double)numAttenuation.Value,
                PyramidType = pyramidType,
                Fps = detectedFps
            };

            VideoProcessor processor = new VideoProcessor(config);
            processor.OnProgress += Processor_OnProgress;
            processor.OnLog += Processor_OnLog;
            processor.OnComplete += Processor_OnComplete;

            processor.ProcessAsync();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtInputFile.Text))
            {
                MessageBox.Show("Please select an input video file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!File.Exists(txtInputFile.Text))
            {
                MessageBox.Show($"Input file does not exist: {txtInputFile.Text}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtOutputFolder.Text))
            {
                MessageBox.Show("Please specify an output folder!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtOutputFileName.Text))
            {
                MessageBox.Show("Please specify an output file name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Create output folder if it doesn't exist
            if (!Directory.Exists(txtOutputFolder.Text))
            {
                try
                {
                    Directory.CreateDirectory(txtOutputFolder.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to create output folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            return true;
        }

        private void Processor_OnProgress(int percentage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(Processor_OnProgress), percentage);
                return;
            }
            progressBar.Value = percentage;
        }

        private void Processor_OnLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(Processor_OnLog), message);
                return;
            }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }

        private void Processor_OnComplete(bool success, string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool, string>(Processor_OnComplete), success, message);
                return;
            }

            btnStart.Enabled = true;
            lblStatus.Text = success ? "Completed" : "Failed";
            
            if (success)
            {
                MessageBox.Show(message, "Processing Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(message, "Processing Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
