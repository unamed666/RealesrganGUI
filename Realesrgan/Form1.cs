using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaInfo;
using SkiaSharp;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace Realesrgan
{
    public partial class gui1 : Form
    {

        //data isian
        string imgoutputfolder = string.Empty;
        string vidoutputfolder = string.Empty;
        string imgfilePath = string.Empty;
        string imgfilePath2 = string.Empty;
        string imgfileName = string.Empty;
        string imgfileExt = string.Empty;
        string imgfileoutExt = string.Empty;
        string imgdirPath = string.Empty;
        string vidfileName = string.Empty;
        string vidfileExt = string.Empty;
        string vidfilePath = string.Empty;
        string viddirPath = string.Empty;
        string dataimg = string.Empty;
        string dataimg2 = string.Empty;
        string datavid = string.Empty;
        string datavid2 = string.Empty;
        string imgscale = string.Empty;
        string vidscale = string.Empty;
        string imgfileOut = string.Empty;
        string vidfileOut = string.Empty;
        string imgoutname = string.Empty;
        string vidoutname = string.Empty;

        string vidFPS = string.Empty;
        string vidFPS2 = string.Empty;
        string vidLength = string.Empty;
        string vidlength2 = string.Empty;
        string customcmd = string.Empty;
        string twoLevelsUp = Directory.GetCurrentDirectory();

        string folderPath = string.Empty;
        float frameDelay;
        float frameDelay2 = 50;
        string thumbpath = string.Empty;
        string thumbext = string.Empty;
        string textBoxContent = string.Empty;
        string destinationPath = string.Empty;
        string palettePath = string.Empty;

        int framedetector = 0;
        int framedetector2 = 0;
        int framedetector3 = 0;
        string modelz = string.Empty;
        string largestimg = string.Empty;
        int durasivideo;

        string exeFilePath = string.Empty;
        string exeFilePath2 = string.Empty;

        int closing = 1;

        private readonly Stopwatch sw = new Stopwatch();
        private CancellationTokenSource _cts;
        public gui1()
        {
            InitializeComponent();
            InitializeApplication();
            LoadCheckboxState();
            this.TopMost = checkTop.Checked;
        }

        private async Task UpdateLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    txtTimer.TextAlign = HorizontalAlignment.Left;
                    txtTimer.Text = sw.Elapsed.ToString(@"d\.hh\:mm\:ss\.fff");
                    await Task.Delay(10, token);
                }
            }
            catch (OperationCanceledException) { /* normal saat dibatalkan */ }
        }

        private void TimerMulai()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            sw.Restart();
            _ = UpdateLoopAsync(_cts.Token);   // mulai loop async (tidak blocking)
            txtTimer.Text = sw.Elapsed.ToString(@"d\.hh\:mm\:ss\.fff");

        }

        private void TimerStop()
        {
            _cts?.Cancel();
            sw.Stop();
            txtTimer.Text = sw.Elapsed.ToString(@"d\.hh\:mm\:ss\.fff");
        }
        private void LoadCheckboxState()
        {
            // Set the checkbox state based on the stored setting
            checkSound.Checked = Properties.Settings.Default.SOUND;
            checkimg.Checked = Properties.Settings.Default.PREVIEW;
            checkTop.Checked = Properties.Settings.Default.TOP;
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveCheckboxState();
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }
            closing = 1;
            await KillEXE();

            if (closing == 0)
            {
                e.Cancel = true;
                return;
            }
        }
        private async Task KillEXE()
        {
            // deteksi siapa yang sedang berjalan
            var running = GetRunningTargets(exeFilePath, exeFilePath2);
            if (running.Count == 0)
            {
                return;
            }
            else
            {
                // tampilkan daftar lalu konfirmasi
                var sb = new StringBuilder();
                sb.Append("Program still running, Kill it?" + Environment.NewLine);
                sb.AppendLine("Running Program:" + Environment.NewLine);
                foreach (var name in running) sb.AppendLine(" - " + name + ".exe" + Environment.NewLine);
                sb.AppendLine();


                var res = MessageBox.Show(sb.ToString(), "Sia Babi",
                                          MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                                          MessageBoxDefaultButton.Button2);

                if (res == DialogResult.Yes)
                {
                    // kill hanya yang terdeteksi berjalan
                    foreach (var name in running)
                        KillProcessTreeByName(name);

                    MessageBox.Show("KILLED", "Ngentot", MessageBoxButtons.OK);
                }
                else
                {
                    closing = 0;
                }

            }

        }

        // --- helper: cari nama proses dari path lalu cek apakah ada instance yang aktif ---
        private static List<string> GetRunningTargets(params string[] exePaths)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in exePaths)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;

                var name = Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrEmpty(name)) continue;
                if (seen.Contains(name)) continue; // hindari duplikat
                seen.Add(name);

                try
                {
                    if (Process.GetProcessesByName(name).Length > 0)
                        result.Add(name);
                }
                catch { /* abaikan error akses */ }
            }
            return result;
        }

        // --- kill tree proses berbasis nama (taskkill dulu, lalu fallback) ---
        private static void KillProcessTreeByName(string processName)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = "/IM " + processName + ".exe /F /T",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit(5000);
                    if (p.ExitCode == 0) return; // sukses
                }
            }
            catch { /* lanjut fallback */ }

            foreach (var proc in Process.GetProcessesByName(processName))
            {
                try { proc.Kill(); proc.WaitForExit(3000); }
                catch { }
                finally { proc.Dispose(); }
            }
        }

        private void SaveCheckboxState()
        {
            // Save the current state of the checkbox
            Properties.Settings.Default.SOUND = checkSound.Checked;
            Properties.Settings.Default.PREVIEW = checkimg.Checked;
            Properties.Settings.Default.Save();
        }

        private void InitializeApplication()
        {
            radimgA.Checked = true;
            radimgX4.Checked = true;
            radimgOutloc1.Checked = true;

            string imgoutputFolderPath = Path.Combine(twoLevelsUp, "Output", "Images");
            if (!Directory.Exists(imgoutputFolderPath))
            {
                // Create the folder if it doesn't exist
                Directory.CreateDirectory(imgoutputFolderPath);
            }
            imgoutputfolder = imgoutputFolderPath;

            radvidA.Checked = true;
            radvidX4.Checked = true;
            radvidOutloc1.Checked = true;

            string vidoutputFolderPath = Path.Combine(twoLevelsUp, "Output", "Videos");
            if (!Directory.Exists(vidoutputFolderPath))
            {
                // Create the folder if it doesn't exist
                Directory.CreateDirectory(vidoutputFolderPath);
            }
            // Define the folder path and ensure it exists
            folderPath = Path.Combine(twoLevelsUp, "Log");

            // Ensure the log folder exists; if not, create it
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            vidoutputfolder = vidoutputFolderPath;

            txtFPS.KeyPress += txtFps_KeyPress;
            txtvidLength.KeyPress += txtFps_KeyPress;


        }

        private void txtFps_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the key pressed is a digit, control key (backspace), or decimal point
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != '-' && e.KeyChar != ':' && e.KeyChar != ';' && e.KeyChar != ',')
            {
                e.Handled = true;  // Invalid input
            }

            // Only allow one decimal point
            if (e.KeyChar == '.' && (sender as TextBox).Text.Contains("."))
            {
                e.Handled = true;  // Prevent multiple decimal points
            }

            // Only allow a minus sign at the start of the number
            if (e.KeyChar == '-' && ((sender as TextBox).SelectionStart != 0 || (sender as TextBox).Text.Contains("-")))
            {
                e.Handled = true;  // Prevent additional minus signs
            }
        }

        private void PlayCompletionSound()
        {
            // Set the path to your sound file (.wav)
            string soundFilePath = Path.Combine(twoLevelsUp, "Realesrgan", "completion_sound.wav");

            // Use SoundPlayer to play the sound
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(soundFilePath);
            if (checkSound.Checked == true)
            {
                player.Play();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }


        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }



        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkimgOutname.CheckState == CheckState.Checked)
            {
                txtimgOutname.Enabled = true;
            }
            else if (checkimgOutname.CheckState == CheckState.Unchecked)
            {
                txtimgOutname.Enabled = false;
                imgoutname = Path.GetFileNameWithoutExtension(imgfileName) + "-X" + imgscale + dataimg2;
                txtimgOutname.Text = imgoutname;
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
        }

        private void button9_Click(object sender, EventArgs e)
        {


            if (radimgOutloc1.Checked)
            {
                Process.Start("explorer.exe", imgdirPath);
            }
            if (radimgOutloc2.Checked)
            {
                Process.Start("explorer.exe", imgoutputfolder);
            }

            btnimgOutdir.BackColor = Color.DimGray;
            btnimgOutdir.ForeColor = Color.Transparent;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radimgOutloc1.Checked)
            {
                imgfileOut = imgdirPath;
            }
            if (radimgOutloc2.Checked)
            {
                imgfileOut = imgoutputfolder;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (radimgOutloc1.Checked)
            {
                imgfileOut = imgdirPath;
            }
            if (radimgOutloc2.Checked)
            {
                imgfileOut = imgoutputfolder;
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "All files (*.*)|*.*|Image Files|*.jpg;*.jpeg;*.png;*.webp";
                ofd.FilterIndex = 2;
                ofd.RestoreDirectory = true;

                if (radimgOutloc1.Checked)
                {
                    imgfileOut = imgdirPath;
                }
                if (radimgOutloc2.Checked)
                {
                    imgfileOut = imgoutputfolder;
                }

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtvidEND.Text = null;
                    imgfilePath = '\u0022' + ofd.FileName + '\u0022';
                    txtimgPath.Text = ofd.FileName;
                    imgdirPath = Path.GetDirectoryName(txtimgPath.Text);
                    imgfileName = ofd.SafeFileName;
                    imgoutname = Path.GetFileNameWithoutExtension(imgfileName) + "-X" + imgscale + dataimg2;
                    txtimgOutname.Text = imgoutname;
                    imgfileExt = Path.GetExtension(txtimgPath.Text);
                }
            }
            //IMAGE PREVIEW
            if (imgfileExt == ".webp")
            {
                // Handle .webp images using SkiaSharp
                LoadWebpImage(txtimgPath.Text);
            }
            else if (imgfileExt == ".gif" || imgfileExt == ".jpg" || imgfileExt == ".png")
            {
                // Handle .gif, .jpg, .png images using the built-in .NET Image class
                LoadStandardImage(txtimgPath.Text);
            }
            else
            {
                MessageBox.Show($"Invalid file format: {imgfileExt}.", "Error");
            }
        }

        private void button1_Click_12(object sender, EventArgs e)
        {
            if (radimgOutloc1.Checked)
            {
                imgfileOut = imgdirPath;
            }
            if (radimgOutloc2.Checked)
            {
                imgfileOut = imgoutputfolder;
            }

            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.Title = "Select Image Folder";
                dialog.EnsurePathExists = true;
                dialog.AllowNonFileSystemItems = false;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    txtvidEND.Text = null;
                    imgdirPath = dialog.FileName;
                    txtimgPath2.Text = imgdirPath;
                    imgfilePath2 = imgdirPath;
                    HitungGambar();

                    // Ambil nama folder sebagai nama output
                    imgfileName = new DirectoryInfo(imgdirPath).Name;

                    imgfileOut = imgdirPath; // Atur sebagai output folder juga
                }
            }

            // Tidak ada preview gambar karena folder tidak bisa dipreview seperti file.
        }

        private void LoadWebpImage(string filePath)
        {
            try
            {
                // Load the WEBP image from file using SkiaSharp
                using (var stream = File.OpenRead(filePath))
                {
                    var webpImage = SKBitmap.Decode(stream);

                    if (webpImage == null)
                    {
                        MessageBox.Show("Failed to decode the WebP image.");
                        return;
                    }

                    // Convert the SkiaSharp bitmap to a .NET System.Drawing Bitmap
                    using (var image = new Bitmap(webpImage.Width, webpImage.Height))
                    {
                        using (var graphics = Graphics.FromImage(image))
                        {
                            // Draw the SkiaSharp image onto the System.Drawing image
                            graphics.DrawImage(SKBitmapToBitmap(webpImage), 0, 0);
                        }
                        // Set the PictureBox image to the System.Drawing bitmap
                        pictureBox1.Image = new Bitmap(image);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading WebP image: {ex.Message}");
            }
        }

        private Bitmap SKBitmapToBitmap(SKBitmap skBitmap)
        {
            // Convert SkiaSharp SKBitmap to System.Drawing Bitmap
            Bitmap bitmap = new Bitmap(skBitmap.Width, skBitmap.Height);

            for (int x = 0; x < skBitmap.Width; x++)
            {
                for (int y = 0; y < skBitmap.Height; y++)
                {
                    SKColor color = skBitmap.GetPixel(x, y);
                    bitmap.SetPixel(x, y, Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue));
                }
            }
            return bitmap;
        }

        private void LoadStandardImage(string thumbpath)
        {
            try
            {
                string fileExtension = Path.GetExtension(thumbpath).ToLower();

                if (fileExtension == ".gif")
                {
                    // Directly load GIF without copying to a MemoryStream
                    pictureBox1.Image = Image.FromFile(thumbpath);  // This handles animated GIFs as well
                }
                else
                {
                    using (FileStream fs = new FileStream(thumbpath, FileMode.Open, FileAccess.Read))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);  // Copy the file stream to the memory stream
                            ms.Position = 0; // Reset the position of the memory stream to the beginning

                            // Create the image from the memory stream
                            pictureBox1.Image = Image.FromStream(ms);
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Error loading image (invalid format): {ex.Message}");
            }
            catch (ExternalException ex)
            {
                MessageBox.Show($"GDI+ error: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}");
            }
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private async void submit_Click(object sender, EventArgs e)
        {
            if (imgfilePath != string.Empty)
            {
                panelimg.Enabled = false;
                panelimgBatch.Enabled = false;
                panelvid.Enabled = false;
                // RealRS 
                if (radimgS.Checked == true)
                {
                    exeFilePath = Path.Combine(twoLevelsUp, "Realsr", "realsr-ncnn-vulkan.exe");
                    modelz = " -m ";
                }
                // Real-Esrgan
                else
                {
                    exeFilePath = Path.Combine(twoLevelsUp, "Realesrgan", "realesrgan-ncnn-vulkan.exe");
                    modelz = " -n ";
                }
                string datasubmit = " -i " + imgfilePath + modelz + dataimg
                   + " -s " + imgscale + " -o " + '\u0022' + imgfileOut + "\\" + imgoutname + imgfileExt + '\u0022';
                ProcessStartInfo ps = new ProcessStartInfo
                {
                    FileName = exeFilePath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = datasubmit
                };
                TimerMulai();
                using (Process process = new Process())
                {
                    process.StartInfo = ps;                    

                    try
                    {
                        process.Start();

                        // Begin reading output and error asynchronously
                        process.OutputDataReceived += (outputSender, outputEvent) =>
                        {
                            if (!string.IsNullOrEmpty(outputEvent.Data))
                            {
                                // Use Invoke to update the TextBox safely from another thread
                                txtvidEND.Invoke((MethodInvoker)delegate
                                {
                                    txtvidEND.AppendText(outputEvent.Data + Environment.NewLine);
                                    txtvidEND.SelectionStart = txtvidEND.Text.Length;
                                    txtvidEND.ScrollToCaret();
                                });
                            }
                        };

                        process.ErrorDataReceived += (errorSender, errorEvent) =>
                        {
                            if (!string.IsNullOrEmpty(errorEvent.Data))
                            {
                                // Use Invoke to update the TextBox safely from another thread
                                txtvidEND.Invoke((MethodInvoker)delegate
                                {
                                    txtvidEND.AppendText(errorEvent.Data + Environment.NewLine);
                                    txtvidEND.SelectionStart = txtvidEND.Text.Length;
                                    txtvidEND.ScrollToCaret();
                                });
                            }
                        };

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        await Task.Run(() => process.WaitForExit());  // Run WaitForExit on a background thread

                        txtvidEND.Invoke((MethodInvoker)delegate
                        {
                            if (File.Exists($"{imgfileOut}\\{imgoutname}{imgfileExt}"))
                            {
                                txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Process completed successfully." + Environment.NewLine);
                                btnimgOutdir.BackColor = Color.Lime;
                                btnimgOutdir.ForeColor = Color.Black;
                            }
                            else
                            {
                                txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Process failed, check Log file." + Environment.NewLine);
                                button1.BackColor = Color.Red;
                            }
                            txtvidEND.SelectionStart = txtvidEND.Text.Length;
                            txtvidEND.ScrollToCaret();
                        });
                    }
                    catch (Exception ex)
                    {
                        txtvidEND.Invoke((MethodInvoker)delegate
                        {
                            txtvidEND.Text = $"Error: {ex.Message}";
                        });
                    }
                }
                TimerStop();
                imgfileoutExt = Path.GetExtension(imgfileOut + "\\" + imgoutname + imgfileExt);
                //Preview Output ver
                //IMAGE PREVIEW
                if (imgfileoutExt == ".webp")
                {
                    // Handle .webp images using SkiaSharp
                    LoadWebpImage(imgfileOut + "\\" + imgoutname + imgfileExt);
                }
                else if (imgfileoutExt == ".gif" || imgfileoutExt == ".jpg" || imgfileoutExt == ".png")
                {
                    // Handle .gif, .jpg, .png images using the built-in .NET Image class
                    LoadStandardImage(imgfileOut + "\\" + imgoutname + imgfileExt);
                }
                panelimg.Enabled = true;
                panelimgBatch.Enabled = true;
                panelvid.Enabled = true;
                PlayCompletionSound();
                textBoxContent = txtvidEND.Text;
                ManageLogFiles();

            }
            else
            {
                MessageBox.Show("Please select a file first.", "No File Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private async void submit_Click12(object sender, EventArgs e)
        {
            if (imgdirPath != string.Empty)
            {

                panelimgBatch.Enabled = false;
                panelvid.Enabled = false;


                string modelz = string.Empty;
                // Tentukan path executable
                if (radimgS.Checked)
                {
                    exeFilePath = Path.Combine(twoLevelsUp, "Realsr", "realsr-ncnn-vulkan.exe");
                    modelz = " -m ";
                }
                else
                {
                    exeFilePath = Path.Combine(twoLevelsUp, "Realesrgan", "realesrgan-ncnn-vulkan.exe");
                    modelz = " -n ";
                }

                // Buat folder UPSCALED jika belum ada
                string upscaledFolder = Path.Combine(imgfileOut, "UPSCALED");
                if (!Directory.Exists(upscaledFolder))
                {
                    Directory.CreateDirectory(upscaledFolder);
                }

                // Ambil semua gambar, skip yang berada di dalam folder UPSCALED
                string[] validExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                var imageFiles = Directory.GetFiles(imgfilePath2, "*.*", SearchOption.AllDirectories)
                    .Where(f => validExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .Where(f => !f.StartsWith(upscaledFolder)) // Lewati file dalam UPSCALED
                    .ToArray();
                TimerMulai();
                foreach (string inputFile in imageFiles)
                {
                    string filename = Path.GetFileName(inputFile);
                    string outputFile = Path.Combine(upscaledFolder, filename);

                    string datasubmit = $"-i \"{inputFile}\" -o \"{outputFile}\" {modelz} {dataimg} -s {imgscale}";
                    txtvidEND.AppendText(datasubmit + Environment.NewLine);

                    ProcessStartInfo ps = new ProcessStartInfo
                    {
                        FileName = exeFilePath,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Arguments = datasubmit
                    };

                    using (Process process = new Process())
                    {
                        process.StartInfo = ps;

                        try
                        {
                            process.Start();
                            process.OutputDataReceived += (outputSender, outputEvent) =>
                            {
                                if (!string.IsNullOrEmpty(outputEvent.Data))
                                {
                                    txtvidEND.Invoke((MethodInvoker)delegate
                                    {
                                        txtvidEND.AppendText(outputEvent.Data + Environment.NewLine);
                                        txtvidEND.ScrollToCaret();
                                    });
                                }
                            };

                            process.ErrorDataReceived += (errorSender, errorEvent) =>
                            {
                                if (!string.IsNullOrEmpty(errorEvent.Data))
                                {
                                    txtvidEND.Invoke((MethodInvoker)delegate
                                    {
                                        txtvidEND.AppendText(errorEvent.Data + Environment.NewLine);
                                        txtvidEND.ScrollToCaret();
                                    });
                                }
                            };

                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            await Task.Run(() => process.WaitForExit());


                            txtvidEND.Invoke((MethodInvoker)delegate
                            {
                                if (File.Exists(outputFile))
                                {
                                    txtvidEND.AppendText($"✔ Completed\n\n");
                                }
                                else
                                {
                                    txtvidEND.AppendText($"✖ Failed: {filename}\n\n");
                                }
                                txtvidEND.ScrollToCaret();
                            });
                        }
                        catch (Exception ex)
                        {
                            txtvidEND.Invoke((MethodInvoker)delegate
                            {
                                txtvidEND.AppendText($"Error: {ex.Message}\n");
                            });
                        }
                    }
                }
                TimerStop();
                panelimg.Enabled = true;
                panelimgBatch.Enabled = true;
                panelvid.Enabled = true;
                PlayCompletionSound();
                textBoxContent = txtvidEND.Text;
                ManageLogFiles();
            }
            else
            {
                MessageBox.Show("Please select a folder first.", "No Folder Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkvidFPS.Checked == true)
            {
                txtFPS.Enabled = true;
            }
            if (checkvidFPS.Checked == false)
            {
                txtFPS.Enabled = false;
                if (vidfileExt == ".gif")
                {
                    txtFPS.Text = frameDelay2.ToString();
                    frameDelay = frameDelay2;
                }
                else
                {
                    txtFPS.Text = vidFPS2;
                    vidFPS = vidFPS2;
                }


            }
            if (checkvidLength.Checked == true)
            {
                txtvidLength.Enabled = true;
            }
            if (checkvidLength.Checked == false)
            {
                txtvidLength.Enabled = false;
                txtvidLength.Text = vidlength2;
                vidLength = vidlength2;
            }
            if (checkvidCus.Checked == true)
            {
                txtvidCus.Enabled = true;
                customcmd = txtvidCus.Text;
            }
            if (checkvidCus.Checked == false)
            {
                txtvidCus.Enabled = false;
                customcmd = string.Empty;
            }
        }

        private void txtoutputname_TextChanged(object sender, EventArgs e)
        {
            if (txtimgOutname.Enabled == true)
            {
                imgoutname = txtimgOutname.Text;
            }
            if (radimgOutloc1.Checked)
            {
                imgfileOut = imgdirPath;
            }
            if (radimgOutloc2.Checked)
            {
                imgfileOut = imgoutputfolder;
            }
        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

        private void radimgA_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidA.Checked == true)
            {
                radvidX2.Enabled = true;
                radvidX3.Enabled = true;
                datavid = "realesr-animevideov3";
                datavid2 = "-A";
                vidoutname = Path.GetFileNameWithoutExtension(vidfileName) + "-X" + vidscale + datavid2;
                txtvidOutname.Text = vidoutname;

            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidOutloc1.Checked)
            {
                vidfileOut = viddirPath;
            }
            if (radvidOutloc2.Checked)
            {
                vidfileOut = vidoutputfolder;
            }
        }

        private void txtvidCus_TextChanged(object sender, EventArgs e)
        {
            customcmd = txtvidCus.Text;
        }

        private void txtvidLengthX_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkvidOutname_CheckedChanged(object sender, EventArgs e)
        {
            if (checkvidOutname.CheckState == CheckState.Checked)
            {
                txtvidOutname.Enabled = true;
            }
            else if (checkvidOutname.CheckState == CheckState.Unchecked)
            {
                txtvidOutname.Enabled = false;
                vidoutname = Path.GetFileNameWithoutExtension(vidfileName) + "-X" + vidscale + datavid2;
                txtvidOutname.Text = vidoutname;
            }
        }

        private void radimgA_CheckedChanged_1(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.Checked)
            {
                if (rb == radimgA)
                    radimgA2.Checked = true;
                else if (rb == radimgA2)
                    radimgA.Checked = true;
            }
            if (radimgA.Checked == true)
            {
                radimgX2.Enabled = true;
                radimgX3.Enabled = true;
                dataimg = "realesr-animevideov3";
                dataimg2 = "-A";
                imgoutname = Path.GetFileNameWithoutExtension(imgfileName) + "-X" + imgscale + dataimg2;
                txtimgOutname.Text = imgoutname;
            }
        }

        private void radimgN_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.Checked)
            {
                if (rb == radimgN)
                    radimgN2.Checked = true;
                else if (rb == radimgN2)
                    radimgN.Checked = true;
            }

            if (radimgN.Checked == true)
            {
                radimgX2.Enabled = false;
                radimgX3.Enabled = false;
                radimgX4.Checked = true;
                dataimg = "realesrgan-x4plus";
                dataimg2 = "-N";
                imgoutname = Path.GetFileNameWithoutExtension(imgfileName) + "-X" + imgscale + dataimg2;
                txtimgOutname.Text = imgoutname;
            }
        }

        private void radvidN_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidN.Checked == true)
            {
                radvidX2.Enabled = false;
                radvidX3.Enabled = false;
                radvidX4.Checked = true;
                datavid = "realesrgan-x4plusv3";
                datavid2 = "-N";
                vidoutname = Path.GetFileNameWithoutExtension(vidfileName) + "-X" + vidscale + datavid2;
                txtvidOutname.Text = vidoutname;
            }
        }

        private void btnvidFind_Click(object sender, EventArgs e)
        {
            btnframeCounter.Text = "Click";
            btnframeCounter.BackColor = Color.DimGray;
            btnframeCounter.FlatStyle = FlatStyle.Standard;
            txtTimer.TextAlign = HorizontalAlignment.Center;
            txtTimer.Text = "Counter";
            txtPred.TextAlign = HorizontalAlignment.Center;
            txtPred.Text = "Estimate";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                txtvidEND.Text = null;
                ofd.Filter = "All files (*.*)|*.*|Video Files|*.mp4;*.mkv;*.3gp;*.gif";
                ofd.FilterIndex = 2;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    vidfilePath = "\"" + ofd.FileName + "\"";
                    txtvidPath.Text = ofd.FileName;
                    viddirPath = Path.GetDirectoryName(txtvidPath.Text);
                    vidfileOut = viddirPath;
                    vidfileName = ofd.SafeFileName;
                    vidoutname = Path.GetFileNameWithoutExtension(vidfileName) + "-X" + vidscale + datavid2;
                    txtvidOutname.Text = vidoutname;
                    vidfileExt = Path.GetExtension(txtvidPath.Text);

                    if (radvidOutloc1.Checked)
                    {
                        vidfileOut = viddirPath;
                    }
                    if (radvidOutloc2.Checked)
                    {
                        vidfileOut = vidoutputfolder;
                    }

                    string[] allowedExtensions = { ".mp4", ".mkv", ".3gp", ".gif" };
                    // Check if the extension is not in the allowed list
                    if (!allowedExtensions.Contains(vidfileExt))
                    {
                        MessageBox.Show($"Invalid file format: {vidfileExt}.", "Error");
                    }

                    //Thumbnail menu
                    if (vidfileExt == ".mp4" || vidfileExt == ".mkv")
                    {
                        label11.Visible = true;
                        checkThumb.Visible = true;
                        btnvidThumb.Visible = true;
                        if (File.Exists(thumbpath))
                        {
                            //IMAGE PREVIEW
                            if (thumbext == ".webp")
                            {
                                // Handle .webp images using SkiaSharp
                                LoadWebpImage(thumbpath);
                            }
                            else if (thumbext == ".jpeg" || thumbext == ".jpg" || thumbext == ".png")
                            {
                                // Handle .gif, .jpg, .png images using the built-in .NET Image class
                                LoadStandardImage(thumbpath);
                            }
                        }
                    }
                    else
                    {

                        if (vidfileExt == ".gif")
                        {
                            LoadStandardImage(txtvidPath.Text);
                        }
                        else
                        {
                            if (pictureBox1.Image != null)
                            {
                                pictureBox1.Image.Dispose();
                                pictureBox1.Image = null;
                            }
                        }
                        btnvidThumb.Visible = false;
                        checkThumb.Checked = false;
                        label11.Visible = false;
                        checkThumb.Visible = false;
                    }

                    //FPS
                    MediaInfo.MediaInfo mi = new MediaInfo.MediaInfo();
                    mi.Open(ofd.FileName);
                    vidFPS = mi.Get(MediaInfo.StreamKind.Video, 0, "FrameRate");
                    if (string.IsNullOrEmpty(vidFPS) || vidFPS == "0")
                    {
                        vidFPS = mi.Get(MediaInfo.StreamKind.Video, 0, "FrameRate");
                    }

                    // Assign the FPS to the TextBox
                    if (vidfileExt == ".gif")
                    {
                        frameDelay = frameDelay2;
                        txtFPS.Text = frameDelay2.ToString();
                        checkvidLength.Visible = false;
                        txtvidLength.Visible = false;
                        checkvidFPS.Text = "Delay :";
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(vidFPS) && vidFPS != "0")
                        {
                            txtFPS.Text = vidFPS; // Set the value once
                            vidFPS2 = vidFPS;
                            checkvidLength.Visible = true;
                            txtvidLength.Visible = true;
                            checkvidFPS.Text = "FPS :";
                        }
                    }


                    //Length
                    if (vidfileExt == ".gif")
                    {
                        vidlength2 = string.Empty;
                        txtvidLength.Text = vidlength2;
                        vidLength = vidlength2;
                    }
                    else
                    {
                        vidLength = mi.Get(StreamKind.General, 0, "Duration");
                    }

                    // Convert the duration to a more readable format (seconds)
                    if (long.TryParse(vidLength, out long durationMs))
                    {
                        TimeSpan videoLength = TimeSpan.FromMilliseconds(durationMs);
                        Console.WriteLine("Video Length: " + videoLength);
                        txtvidLength.Text = videoLength.ToString();
                        vidlength2 = videoLength.ToString();
                        vidLength = vidlength2;
                    }
                    else
                    {
                        Console.WriteLine("Could not retrieve video length.");
                    }

                    // Close the MediaInfo instance
                    mi.Close();
                }
            }
        }
        private void radimgX2_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.Checked)
            {
                if (rb == radimgX2)
                    radimgX22.Checked = true;
                else if (rb == radimgX22)
                    radimgX2.Checked = true;
            }

            if (radimgX2.Checked == true)
            {
                imgscale = "2";
                if (radimgN.Checked == true)
                {
                    dataimg = "realesr-animevideov3";
                }
                imgoutname = Path.GetFileNameWithoutExtension(imgfileName) + "-X" + imgscale + dataimg2;
                txtimgOutname.Text = imgoutname;
            }

        }

        private void radimgX3_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.Checked)
            {
                if (rb == radimgX3)
                    radimgX32.Checked = true;
                else if (rb == radimgX32)
                    radimgX3.Checked = true;
            }

            if (radimgX3.Checked == true)
            {
                imgscale = "3";
                if (radimgN.Checked == true)
                {
                    dataimg = "realesr-animevideov3";
                }
                imgoutname = Path.GetFileNameWithoutExtension(imgfileName) + "-X" + imgscale + dataimg2;
                txtimgOutname.Text = imgoutname;
            }
        }

        private void radimgX4_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.Checked)
            {
                if (rb == radimgX4)
                    radimgX42.Checked = true;
                else if (rb == radimgX42)
                    radimgX4.Checked = true;
            }

            if (radimgX4.Checked == true)
            {
                imgscale = "4";
                if (radimgN.Checked == true)
                {
                    dataimg = "realesrgan-x4plus-anime";
                }
                imgoutname = Path.GetFileNameWithoutExtension(imgfileName) + "-X" + imgscale + dataimg2;
                txtimgOutname.Text = imgoutname;
            }
        }
        private void radvidX2_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidX2.Checked == true)
            {
                vidscale = "2";
                vidoutname = Path.GetFileNameWithoutExtension(vidfileName) + "-X" + vidscale + datavid2;
                txtvidOutname.Text = vidoutname;
            }
        }

        private void radvidX3_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidX3.Checked == true)
            {
                vidscale = "3";
                vidoutname = Path.GetFileNameWithoutExtension(vidfileName) + "-X" + vidscale + datavid2;
                txtvidOutname.Text = vidoutname;
            }
        }

        private void radvidX4_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidX4.Checked == true)
            {
                vidscale = "4";
                vidoutname = Path.GetFileNameWithoutExtension(vidfileName) + "-X" + vidscale + datavid2;
                txtvidOutname.Text = vidoutname;
            }
        }

        private void txtvidPath_TextChanged(object sender, EventArgs e)
        {
            vidfilePath = '\u0022' + txtvidPath.Text + '\u0022';
            if (radvidOutloc1.Checked)
            {
                vidfileOut = viddirPath;
            }
            if (radvidOutloc2.Checked)
            {
                vidfileOut = vidoutputfolder;
            }
        }

        private void txtvidOutname_TextChanged(object sender, EventArgs e)
        {
            if (txtvidOutname.Enabled == true)
            {
                vidoutname = txtvidOutname.Text;
            }
            if (radimgOutloc1.Checked)
            {
                imgfileOut = imgdirPath;
            }
            if (radimgOutloc2.Checked)
            {
                imgfileOut = imgoutputfolder;
            }
        }

        private void radvidOutloc1_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidOutloc1.Checked)
            {
                vidfileOut = viddirPath;
            }
            if (radvidOutloc2.Checked)
            {
                vidfileOut = vidoutputfolder;
            }
        }

        private void btnvidOutdir_Click(object sender, EventArgs e)
        {
            if (radvidOutloc1.Checked)
            {
                Process.Start("explorer.exe", viddirPath);
            }
            if (radvidOutloc2.Checked)
            {
                Process.Start("explorer.exe", vidoutputfolder);
            }
            btnvidOutdir.BackColor = Color.DimGray;
            btnvidOutdir.ForeColor = Color.Transparent;
        }
        private void btnvidSubmit_Click(object sender, EventArgs e)
        {
            if (vidfilePath != string.Empty)
            {



                if (File.Exists($"{vidfileOut}\\{vidoutname}{vidfileExt}"))
                {
                    DialogResult result = MessageBox.Show("File already exist. Overwrite?", "Baca tah kontol", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    // Check the user's response
                    if (result == DialogResult.Yes)
                    {
                        panelimg.Enabled = false;
                        panelimgBatch.Enabled = false;
                        panelvid.Enabled = false;
                        btnframeCounter.BackColor = Color.Black;
                        btnframeCounter.FlatStyle = FlatStyle.Flat;
                        btnframeCounter.BackColor = Color.Transparent;
                        btnframeCounter.Font = new Font(btnframeCounter.Font.FontFamily, 8, FontStyle.Bold);
                        btnframeCounter.TextAlign = ContentAlignment.MiddleLeft;
                        StartCountingLoop(btnframeCounter);
                        File.Delete($"{vidfileOut}\\{vidoutname}{vidfileExt}");
                        TimerMulai();
                        RunVideoProgram();
                    }
                    else
                    {
                        // User chose not to overwrite, exit the method
                        return;
                    }

                }
                else
                {
                    panelimg.Enabled = false;
                    panelimgBatch.Enabled = false;
                    panelvid.Enabled = false;
                    btnframeCounter.BackColor = Color.Black;
                    btnframeCounter.FlatStyle = FlatStyle.Flat;
                    btnframeCounter.BackColor = Color.Transparent;
                    btnframeCounter.Font = new Font(btnframeCounter.Font.FontFamily, 8, FontStyle.Bold);
                    btnframeCounter.TextAlign = ContentAlignment.MiddleLeft;
                    StartCountingLoop(btnframeCounter);
                    File.Delete($"{vidfileOut}\\{vidoutname}{vidfileExt}");
                    TimerMulai();
                    RunVideoProgram();

                }
            }
            else
            {
                MessageBox.Show("File not found, please select a valid video file.", "Error");

            }
        }

        private async void RunVideoProgram()
        {
            // 1. BONGKAR: Extract frames from the video or GIF
            string exeFilePath = Path.Combine(twoLevelsUp, "Realesrgan", "ffmpeg", "bin", "ffmpeg");
            string tmpFrame = Path.Combine(twoLevelsUp, "Realesrgan", "tmp_frames");
            string datasubmit;

            if (vidfileExt == ".gif")
            {
                datasubmit = $"-i {vidfilePath} -f image2 \"{tmpFrame}\\frame%08d.png\"";
            }
            else
            {
                datasubmit = $"-i {vidfilePath} -qscale:v 1 -qmin 1 -qmax 1 -fps_mode cfr -f image2 \"{tmpFrame}\\frame%08d.png\"";
            }

            ProcessStartInfo ps = new ProcessStartInfo
            {
                FileName = exeFilePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = datasubmit,
            };

            // 2. UPSCALE: Upscale frames
            string out_frames = Path.Combine(twoLevelsUp, "Realesrgan", "out_frames");

            if (radvidS.Checked == true)
            {
                exeFilePath2 = Path.Combine(twoLevelsUp, "Realsr", "realsr-ncnn-vulkan.exe");
                modelz = "-m";
            }
            else
            {
                exeFilePath2 = Path.Combine(twoLevelsUp, "Realesrgan", "realesrgan-ncnn-vulkan.exe");
                modelz = "-n";
            }

            string datasubmit2 = $"-i \"{tmpFrame}\" -o \"{out_frames}\" {modelz} {datavid} -s {vidscale} -f png";
            ProcessStartInfo ps2 = new ProcessStartInfo
            {
                FileName = exeFilePath2,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = datasubmit2,
            };
            // 2.5 GIF Palette
            palettePath = $"{vidfileOut}\\palette.png";
            string datasubmit69 = $"-i \"{out_frames}\\frame%08d.png\" -vf \"palettegen\" \"{palettePath}\"";
            ProcessStartInfo ps69 = new ProcessStartInfo
            {
                FileName = exeFilePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = datasubmit69,
            };

            // 3. PASANG: Re-encode the video with upscaled frames

            string datasubmit3;
            if (vidfileExt == ".gif")
            {
                txtFPS.Text = frameDelay.ToString();
                datasubmit3 = $"-framerate {1000 / frameDelay} -i \"{out_frames}\\frame%08d.png\" -i \"{palettePath}\" -lavfi \"paletteuse\" -gifflags -transdiff \"{vidfileOut}\\{vidoutname}{vidfileExt}\"";
            }
            else
            {
                datasubmit3 = $"-framerate {vidFPS} -i \"{out_frames}\\frame%08d.png\" -i {vidfilePath} " +
                              $"-map 0:v:0 -map 1:a:0? -c:a copy -c:v libx264 -r {vidFPS} -pix_fmt yuv420p {customcmd} -t {vidLength} " +
                              $"\"{vidfileOut}\\{vidoutname}{vidfileExt}\"";
            }

            ProcessStartInfo ps3 = new ProcessStartInfo
            {
                FileName = exeFilePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = datasubmit3,
            };

            // 4. CLEAR: Clean up temporary files
            string datasubmit4 = $"del /S /q \"{tmpFrame}\" && del /S /q \"{out_frames}\" && del /S /q \"{vidfileOut}\\palette.png\"";

            ProcessStartInfo ps4 = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $"/c {datasubmit4}",
            };



            // Run processes and capture output in txtvidEND
            txtvidEND.Text = null;
            await RunProcessAndCaptureOutput(ps4, "\nCleaning up temporary files... \n");
            await RunProcessAndCaptureOutput(ps, "\nExtracting frames... \n");

            //Frame Counter
            string[] files = Directory.GetFiles(tmpFrame);
            int totalFiles = files.Length;
            btnframeCounter.Font = new Font(btnframeCounter.Font.FontFamily, 14, FontStyle.Bold);
            btnframeCounter.TextAlign = ContentAlignment.MiddleCenter;
            StopCountingLoop(btnframeCounter);
            btnframeCounter.Text = totalFiles.ToString();
            txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Total Frames : " + totalFiles.ToString() + Environment.NewLine);

            await RunProcessAndCaptureOutput(ps2, "\nUpscaling frames... \n");
            if (vidfileExt == ".gif")
            {
                await RunProcessAndCaptureOutput(ps69, "\nPaletting GIF... \n");
            }
            await RunProcessAndCaptureOutput(ps3, "\nRe-encoding video... \n");
            await RunProcessAndCaptureOutput(ps4, "\nCleaning up temporary files... \n");

            //THUMBNAIL

            if (checkThumb.Checked == true)
            {

                string fileExtension = Path.GetExtension(vidfileExt)?.TrimStart('.');
                string datasubmit5 = string.Empty;


                if (vidfileExt == ".mp4")
                {
                    datasubmit5 = $" -i \"{vidfileOut}\\{vidoutname}{vidfileExt}\" -i \"{thumbpath}\" -map 0 -map 1 -c copy -disposition:v:1 attached_pic  \"{vidfileOut}\\{vidoutname}-TN{vidfileExt}\"";

                }
                if (vidfileExt == ".mkv")
                {
                    destinationPath = Path.Combine(vidfileOut, "cover.png");
                    File.Copy(thumbpath, destinationPath, true);
                    datasubmit5 = $" -i \"{vidfileOut}\\{vidoutname}{vidfileExt}\" -c copy -attach \"{destinationPath}\" -metadata:s:t mimetype=image/png \"{vidfileOut}\\{vidoutname}-TN{vidfileExt}\"";



                    // Execute the ffmpeg command here (e.g., using Process.Start)
                }
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exeFilePath,
                    Arguments = datasubmit5,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                await RunProcessAndCaptureOutput(startInfo, "\nChanging Thumbnail... \n");
                File.Delete($"{vidfileOut}\\{vidoutname}{vidfileExt}");
            }

            // Indicate process completion

            txtvidEND.Invoke((MethodInvoker)delegate
            {
                if (checkThumb.Checked == false)
                {
                    if (File.Exists($"{vidfileOut}\\{vidoutname}{vidfileExt}"))
                    {
                        txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Process completed successfully." + Environment.NewLine);
                        btnvidOutdir.BackColor = Color.Lime;
                        btnvidOutdir.ForeColor = Color.Black;
                    }
                    else
                    {
                        txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Process failed, check Log file." + Environment.NewLine);
                        button1.BackColor = Color.Red;
                    }
                }
                if (checkThumb.Checked == true)
                {
                    if (File.Exists($"{vidfileOut}\\{vidoutname}-TN{vidfileExt}"))
                    {
                        // Create the folder if it doesn't exist


                        txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Process completed successfully." + Environment.NewLine);
                        btnvidOutdir.BackColor = Color.Lime;
                        btnvidOutdir.ForeColor = Color.Black;

                    }

                    else
                    {
                        txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Process failed, check Log file." + Environment.NewLine);
                        button1.BackColor = Color.Red;
                    }
                }
                txtvidEND.SelectionStart = txtvidEND.Text.Length;
                txtvidEND.ScrollToCaret();
            });

            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }
            TimerStop();
            btnframeCounter.BackColor = Color.Black;
            btnframeCounter.FlatStyle = FlatStyle.Flat;
            btnframeCounter.Font = new Font(btnframeCounter.Font.FontFamily, 14, FontStyle.Bold);
            panelimg.Enabled = true;
            panelimgBatch.Enabled = true;
            panelvid.Enabled = true;
            PlayCompletionSound();


            // Log file management
            textBoxContent = txtvidEND.Text;
            ManageLogFiles();
        }
        private void ManageLogFiles()
        {
            string[] logFiles = Directory.GetFiles(folderPath, "*.txt");

            if (logFiles.Length >= 10)
            {
                var oldestFile = logFiles.OrderBy(f => File.GetCreationTime(f)).First();
                try
                {
                    File.Delete(oldestFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting oldest file: " + ex.Message);
                    return;
                }
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"log_{timestamp}.txt";
            string filePath = Path.Combine(folderPath, fileName);
            File.WriteAllText(filePath, textBoxContent);
        }

        // Helper method to run processes and capture output in the TextBox
        private async Task RunProcessAndCaptureOutput(ProcessStartInfo ps, string processDescription)
        {
            using (Process process = new Process())
            {
                process.StartInfo = ps;
                process.OutputDataReceived += (outputSender, outputEvent) =>
                {
                    if (!string.IsNullOrEmpty(outputEvent.Data))
                    {
                        txtvidEND.Invoke((MethodInvoker)delegate
                        {
                            txtvidEND.AppendText(outputEvent.Data + Environment.NewLine);
                            txtvidEND.SelectionStart = txtvidEND.Text.Length;
                            txtvidEND.ScrollToCaret();
                        });
                    }
                };

                process.ErrorDataReceived += (errorSender, errorEvent) =>
                {
                    if (!string.IsNullOrEmpty(errorEvent.Data))
                    {
                        txtvidEND.Invoke((MethodInvoker)delegate
                        {
                            txtvidEND.AppendText(errorEvent.Data + Environment.NewLine);
                            txtvidEND.SelectionStart = txtvidEND.Text.Length;
                            txtvidEND.ScrollToCaret();
                        });
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                txtvidEND.Invoke((MethodInvoker)delegate
                {
                    txtvidEND.AppendText(processDescription + Environment.NewLine);
                    txtvidEND.SelectionStart = txtvidEND.Text.Length;
                    txtvidEND.ScrollToCaret();
                });

                await Task.Run(() => process.WaitForExit());
            }
        }

        // Method to get the GIF delay


        private void checkSound_CheckedChanged(object sender, EventArgs e)
        {
            if (checkimg.Checked == true)
            {
                label10.Visible = true;
                pictureBox1.Visible = true;
                pictureBox1.Enabled = true;
                this.Size = new Size(1011, 554);
            }
            if (checkimg.Checked == false)
            {
                label10.Visible = false;
                pictureBox1.Visible = false;
                pictureBox1.Enabled = false;
                this.Size = new Size(803, 554);
            }
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", folderPath);
            button1.BackColor = Color.Black;
        }


        public class CustomTabControl : TabControl
        {
            private Color _tabTextColor = Color.Black;
            private Color _tabBackgroundColor = Color.LightGray;
            private Color _selectedTabBackgroundColor = Color.LightBlue;
            private Color _borderColor = Color.Lime; // Default border color

            [Category("Custom Colors")]
            [Description("Sets the text color of the tabs.")]
            public Color TabTextColor
            {
                get { return _tabTextColor; }
                set { _tabTextColor = value; Invalidate(); }
            }

            [Category("Custom Colors")]
            [Description("Sets the background color of the tabs.")]
            public Color TabBackgroundColor
            {
                get { return _tabBackgroundColor; }
                set { _tabBackgroundColor = value; Invalidate(); }
            }

            [Category("Custom Colors")]
            [Description("Sets the background color of the selected tab.")]
            public Color SelectedTabBackgroundColor
            {
                get { return _selectedTabBackgroundColor; }
                set { _selectedTabBackgroundColor = value; Invalidate(); }
            }

            [Category("Custom Colors")]
            [Description("Sets the border color of the TabControl.")]
            public Color BorderColor
            {
                get { return _borderColor; }
                set { _borderColor = value; Invalidate(); }
            }

            public CustomTabControl()
            {
                this.DrawMode = TabDrawMode.OwnerDrawFixed; // Enable custom drawing
                this.DrawItem += CustomTabControl_DrawItem;
                this.SetStyle(ControlStyles.UserPaint, true);
                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                this.SetStyle(ControlStyles.ResizeRedraw, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                // Draw the border around the TabControl
                using (Pen borderPen = new Pen(BorderColor, 3)) // Change width as needed
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
                }
            }

            private void CustomTabControl_DrawItem(object sender, DrawItemEventArgs e)
            {
                Graphics g = e.Graphics;
                TabPage tabPage = this.TabPages[e.Index];
                Rectangle tabRect = this.GetTabRect(e.Index);

                // Set background color for selected and unselected tabs
                Color backgroundColor = (e.State == DrawItemState.Selected) ? _selectedTabBackgroundColor : _tabBackgroundColor;
                g.FillRectangle(new SolidBrush(backgroundColor), tabRect);

                // Set text color
                TextRenderer.DrawText(g, tabPage.Text, tabPage.Font, tabRect, _tabTextColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

            txtTimer.TextAlign = HorizontalAlignment.Center;
            txtTimer.Text = "Counter";
            txtPred.TextAlign = HorizontalAlignment.Center;
            txtPred.Text = "Estimate";
            btnIMG.FlatStyle = FlatStyle.Flat;
            btnVID.FlatStyle = FlatStyle.Popup;
            btnSingle.FlatStyle = FlatStyle.Flat;
            btnBatch.FlatStyle = FlatStyle.Popup;
            GoFront(panel3);
            GoFront(panelimg);
            if (File.Exists(txtimgPath.Text))
            {
                //IMAGE PREVIEW
                using (FileStream fs = new FileStream(txtimgPath.Text, FileMode.Open, FileAccess.Read))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);  // Copy the file stream to the memory stream
                        ms.Position = 0; // Reset the position of the memory stream to the beginning

                        // Create the image from the memory stream
                        pictureBox1.Image = Image.FromStream(ms);
                    }
                }
            }
            else
            {
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }

            }


        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            btnVID.FlatStyle = FlatStyle.Flat;
            btnIMG.FlatStyle = FlatStyle.Popup;
            GoFront(panelvid);
            txtPred.BringToFront();


            if (vidfileExt == ".gif")
            {
                if (File.Exists(txtvidPath.Text))
                {
                    LoadStandardImage(txtvidPath.Text);
                }
                else
                {
                    if (pictureBox1.Image != null)
                    {
                        pictureBox1.Image.Dispose();
                        pictureBox1.Image = null;
                    }
                }
            }
            else
            {
                if (File.Exists(thumbpath))
                {
                    //IMAGE PREVIEW
                    if (thumbext == ".webp")
                    {
                        // Handle .webp images using SkiaSharp
                        LoadWebpImage(thumbpath);
                    }
                    else if (thumbext == ".jpeg" || thumbext == ".jpg" || thumbext == ".png")
                    {
                        // Handle .gif, .jpg, .png images using the built-in .NET Image class
                        LoadStandardImage(thumbpath);
                    }

                }
                else
                {
                    if (pictureBox1.Image != null)
                    {
                        pictureBox1.Image.Dispose();
                        pictureBox1.Image = null;
                    }
                }
            }
        }

        private void panelimg_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private async void btnvidThumb_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "All files (*.*)|*.*|Image Files|*.jpg;*.jpeg;*.png";
                ofd.FilterIndex = 2;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {

                    thumbpath = ofd.FileName;
                    thumbext = Path.GetExtension(thumbpath);

                    //IMAGE PREVIEW

                    if (thumbext == ".jpg" || thumbext == ".png" || thumbext == ".jpeg")
                    {
                        // Handle .gif, .jpg, .png images using the built-in .NET Image class
                        LoadStandardImage(thumbpath);
                    }
                    else
                    {
                        MessageBox.Show("Unsupported file format.");
                    }

                    if (checkThumb.Checked == false)
                    {
                        DialogResult result = MessageBox.Show("Change current video thumbnail?", "Baca tah kontol", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        // Check the user's response
                        if (result == DialogResult.Yes)
                        {
                            panelimg.Enabled = false;
                            panelvid.Enabled = false;
                            string exeFilePath = Path.Combine(twoLevelsUp, "Realesrgan", "ffmpeg", "bin", "ffmpeg");
                            string fileExtension = Path.GetExtension(vidfileExt)?.TrimStart('.');
                            string datasubmit = string.Empty;
                            if (vidfileExt == ".mp4")
                            {
                                datasubmit = $" -i {vidfilePath} -i \"{thumbpath}\" -map 0 -map 1 -c copy -disposition:v:1 attached_pic  \"{vidfileOut}\\Output-{vidfileName}\"";

                            }
                            if (vidfileExt == ".mkv")
                            {
                                destinationPath = Path.Combine(vidfileOut, "cover.png");
                                File.Copy(thumbpath, destinationPath, true);
                                datasubmit = $" -i {vidfilePath} -c copy -attach \"{destinationPath}\" -metadata:s:t mimetype=image/png  \"{vidfileOut}\\Output-{vidfileName}\"";
                            }

                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = exeFilePath,
                                Arguments = datasubmit,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };
                            await RunProcessAndCaptureOutput(startInfo, "\nChanging Thumbnail... \n");
                            if (File.Exists($"{vidfileOut}\\Output-{vidfileName}") && new FileInfo($"{vidfileOut}\\Output-{vidfileName}").Length > 10 * 1024)
                            {
                                txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Process completed successfully." + Environment.NewLine);
                                btnvidOutdir.BackColor = Color.Lime;
                                btnvidOutdir.ForeColor = Color.Black;

                            }

                            else
                            {
                                txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Process failed, check Log file." + Environment.NewLine);
                                button1.BackColor = Color.Red;
                            }

                            txtvidEND.SelectionStart = txtvidEND.Text.Length;
                            txtvidEND.ScrollToCaret();
                            if (File.Exists(destinationPath))
                            {
                                File.Delete(destinationPath);
                            }
                            panelimg.Enabled = true;
                            panelvid.Enabled = true;
                            btnvidOutdir.BackColor = Color.Lime;
                            btnvidOutdir.ForeColor = Color.Black;
                            PlayCompletionSound();
                            textBoxContent = txtvidEND.Text;
                            ManageLogFiles();
                        }
                    }
                }
            }
        }



        private void checkThumb_CheckedChanged(object sender, EventArgs e)
        {
            if (checkThumb.Checked == true)
            {
                label11.Text = "To Upscaled Video";
            }
            if (checkThumb.Checked == false)
            {
                label11.Text = "To Selected Video";
            }
        }

        private void panelvid_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", folderPath);
            button1.BackColor = Color.Black;
        }

        private void txtFPS_TextChanged(object sender, EventArgs e)
        {
            vidFPS = txtFPS.Text;
            frameDelay = float.Parse(txtFPS.Text);
        }

        private void txtvidLength_TextChanged(object sender, EventArgs e)
        {
            vidLength = txtvidLength.Text;
        }

        private void radimgS_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.Checked)
            {
                if (rb == radimgS)
                    radimgS2.Checked = true;
                else if (rb == radimgS2)
                    radimgS.Checked = true;
            }

            if (radimgS.Checked == true)
            {
                radimgX2.Enabled = false;
                radimgX3.Enabled = false;
                radimgX4.Checked = true;
                dataimg = "models-DF2K_JPEG";
                dataimg2 = "-S";
                imgoutname = Path.GetFileNameWithoutExtension(imgfileName) + "-X" + imgscale + dataimg2;
                txtimgOutname.Text = imgoutname;
            }
        }

        private void radvidS_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidS.Checked == true)
            {
                labelHeavy.ForeColor = Color.Red;
                radvidX2.Enabled = false;
                radvidX3.Enabled = false;
                radvidX4.Checked = true;
                datavid = "models-DF2K_JPEG";
                datavid2 = "-S";
                vidoutname = Path.GetFileNameWithoutExtension(vidfileName) + "-X" + vidscale + datavid2;
                txtvidOutname.Text = vidoutname;
            }
            else
            {
                labelHeavy.ForeColor = Color.DimGray;
            }
        }

        private void framecounter()
        {
            if (File.Exists(txtvidPath.Text))

            {
                panelimg.Enabled = false;
                panelvid.Enabled = false;
                btnframeCounter.Font = new Font(btnframeCounter.Font.FontFamily, 8, FontStyle.Bold);
                btnframeCounter.TextAlign = ContentAlignment.MiddleLeft;
                btnframeCounter.Text = "Counting...";
                btnframeCounter.BackColor = Color.Black;
                btnframeCounter.FlatStyle = FlatStyle.Flat;
                txtvidEND.Text = null;
                btnframeCounter.BackColor = Color.Transparent;

                string exeFilePath = Path.Combine(twoLevelsUp, "Realesrgan", "ffmpeg", "bin", "ffprobe");

                int totalFiles = 0;
                Task.Run(() =>
                {
                    // Cara paling andal: hitung frame yang benar-benar terbaca
                    string argsCount = $"-v error -select_streams v:0 -count_frames " +
                                       $"-show_entries stream=nb_read_frames -of csv=p=0 \"{vidfilePath}\"";
                    string outCount = ExecProcess(exeFilePath, argsCount).Trim();

                    if (!int.TryParse(outCount, out totalFiles))
                    {
                        // Fallback cepat: metadata nb_frames (tidak selalu tersedia)
                        string argsMeta = $"-v error -select_streams v:0 " +
                                          $"-show_entries stream=nb_frames -of csv=p=0 \"{vidfilePath}\"";
                        string outMeta = ExecProcess(exeFilePath, argsMeta).Trim();
                        int.TryParse(outMeta, out totalFiles);
                    }

                    // Kembalikan ke UI thread untuk memperbarui label
                    btnframeCounter.BeginInvoke((Action)(() =>
                    {
                        StopCountingLoop(btnframeCounter);
                        btnframeCounter.Font = new Font(btnframeCounter.Font.FontFamily, 14, FontStyle.Bold);
                        btnframeCounter.TextAlign = ContentAlignment.MiddleCenter;
                        btnframeCounter.Text = totalFiles.ToString();
                        txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Total Frames : " + totalFiles.ToString() + Environment.NewLine);
                        
                        framedetector2 = 1;
                    }));
                });


                // Log file management
                textBoxContent = txtvidEND.Text;



                ManageLogFiles();

            }
            else
            {
                btnframeCounter.Text = "Click";
                btnframeCounter.BackColor = Color.DimGray;
                btnframeCounter.FlatStyle = FlatStyle.Standard;
            }
        }

        private static string ExecProcess(string fileName, string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var p = new Process { StartInfo = psi })
            {
                p.Start();
                string stdout = p.StandardOutput.ReadToEnd();
                string stderr = p.StandardError.ReadToEnd();
                p.WaitForExit();
                return string.IsNullOrWhiteSpace(stdout) ? stderr : stdout;
            }
        }

        private async void btnframeCounter_Click(object sender, EventArgs e)
        {
            if (vidfilePath == string.Empty)
            {
                MessageBox.Show("No video selected.");
                return;
            }
            else
            {
                StartCountingLoop(btnframeCounter);
                framecounter();
                if (framedetector2 == 1)
                {
                    panelimg.Enabled = true;
                    panelvid.Enabled = true;
                    
                    framedetector2 = 0;
                }
                else
                {
                    while (framedetector2 != 1)
                    {
                        await Task.Delay(100);
                    }
                    panelimg.Enabled = true;
                    panelvid.Enabled = true;
                   
                    framedetector2 = 0;
                }

            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "https://github.com/unamed666/RealesrganGUI";
            try
            {
                // Membuka URL di browser default
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Menyuruh sistem untuk membuka URL dengan browser
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi kesalahan: {ex.Message}");
            }
        }

        private void checkTop_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TOP = checkTop.Checked;
            this.TopMost = checkTop.Checked;
        }

        private void btnSingle_Click(object sender, EventArgs e)
        {
            GoFront(panelimg);
            btnSingle.FlatStyle = FlatStyle.Flat;
            btnBatch.FlatStyle = FlatStyle.Popup;
            txtTimer.TextAlign = HorizontalAlignment.Center;
            txtTimer.Text = "Counter";
            txtPred.TextAlign = HorizontalAlignment.Center;
            txtPred.Text = "Estimate";
        }

        private void btnBatch_Click(object sender, EventArgs e)
        {
            GoFront(panelimgBatch);
            btnSingle.FlatStyle = FlatStyle.Popup;
            btnBatch.FlatStyle = FlatStyle.Flat;
            txtPred.BringToFront();
        }

        private void RadioLinked_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.Checked)
            {
                if (rb == radimgX2) radimgX22.Checked = true;
                else if (rb == radimgX3) radimgX32.Checked = true;
                else if (rb == radimgX4) radimgX42.Checked = true;
                else if (rb == radimgX22) radimgX2.Checked = true;
                else if (rb == radimgX32) radimgX3.Checked = true;
                else if (rb == radimgX42) radimgX4.Checked = true;
            }
        }

        private void GoFront(Control ctrl)
        {
            ctrl.BringToFront();
            txtvidEND.BringToFront();
            button1.BringToFront();
            txtTimer.BringToFront();
        }
        private async Task TestFrame()
        {
            TimeSpan durasi = TimeSpan.Parse(vidlength2);
            TimeSpan mid = TimeSpan.FromMilliseconds(durasi.TotalMilliseconds / 2);

            // 1. BONGKAR: Extract frames from the video or GIF
            string exeFilePath = Path.Combine(twoLevelsUp, "Realesrgan", "ffmpeg", "bin", "ffmpeg");
            string tmpFrame = Path.Combine(twoLevelsUp, "Realesrgan", "tmp_frames");
            string datasubmit;


            datasubmit = $"-i {vidfilePath} -ss {mid} -frames:v 1 \"{tmpFrame}\\frame%08d.png\"";
            string out_frames = Path.Combine(twoLevelsUp, "Realesrgan", "out_frames");

            ProcessStartInfo ps = new ProcessStartInfo
            {
                FileName = exeFilePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = datasubmit,
            };

            // 2. UPSCALE: Upscale frames
            
            string modelz = string.Empty;
            if (radvidS.Checked == true)
            {
                exeFilePath2 = Path.Combine(twoLevelsUp, "Realsr", "realsr-ncnn-vulkan.exe");
                modelz = "-m";
            }
            else
            {
                exeFilePath2 = Path.Combine(twoLevelsUp, "Realesrgan", "realesrgan-ncnn-vulkan.exe");
                modelz = "-n";
            }

            string datasubmit2 = $"-i \"{tmpFrame}\" -o \"{out_frames}\" {modelz} {datavid} -s {vidscale} -f png";
            ProcessStartInfo ps2 = new ProcessStartInfo
            {
                FileName = exeFilePath2,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = datasubmit2,
            };
            // 2.5 GIF Palette
            palettePath = $"{vidfileOut}\\palette.png";
            string datasubmit69 = $"-i \"{out_frames}\\frame%08d.png\" -vf \"palettegen\" \"{palettePath}\"";
            ProcessStartInfo ps69 = new ProcessStartInfo
            {
                FileName = exeFilePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = datasubmit69,
            };

            // 4. CLEAR: Clean up temporary files
            string datasubmit4 = $"del /S /q \"{tmpFrame}\" && del /S /q \"{out_frames}\" && del /S /q \"{vidfileOut}\\palette.png\"";


            ProcessStartInfo ps4 = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $"/c {datasubmit4}",
            };

            // Run processes and capture output in txtvidEND
            txtvidEND.Text = null;

            await RunProcessAndCaptureOutput(ps4, "\nCleaning up temporary files... \n");
            await RunProcessAndCaptureOutput(ps, "\nExtracting Test Frame... \n");
            TimerMulai();
            await RunProcessAndCaptureOutput(ps2, "\nUpscaling frames... \n");
            TimerStop();
            if (vidfileExt == ".gif")
            {
                await RunProcessAndCaptureOutput(ps69, "\nPaletting GIF... \n");
            }
            await RunProcessAndCaptureOutput(ps4, "\nCleaning up temporary files... \n");
            framedetector = 1;
        }

        private void prediction()
        {
            TimeSpan durasi;
            if (!TimeSpan.TryParse(txtTimer.Text.Trim(), out durasi))
            {
                MessageBox.Show("Format waktu di textbox tidak valid.\nGunakan format hh:mm:ss atau hh:mm:ss.fff");
                return;
            }

            TimeSpan mid = TimeSpan.FromMilliseconds(durasi.TotalMilliseconds * durasivideo);
            TimeSpan mid2;
            if (string.Equals(datavid2, "-S", StringComparison.OrdinalIgnoreCase))
            {
                mid2 = mid;
            }
            else if (string.Equals(datavid2, "-N", StringComparison.OrdinalIgnoreCase))
            {
                mid2 = TimeSpan.FromTicks((long)Math.Round(mid.Ticks * 0.5));
            }
            else
            {
                mid2 = TimeSpan.FromTicks((long)Math.Round(mid.Ticks * 0.3));
            }



            txtPred.TextAlign = HorizontalAlignment.Left;
            txtPred.Text = mid2.ToString(@"d\.hh\:mm\:ss\.fff");
            framedetector3 = 1;
        }

        private void prediction2()
        {
            durasivideo = int.Parse(btnimgcounter.Text.Trim());
            TimeSpan durasi;
            if (!TimeSpan.TryParse(txtTimer.Text.Trim(), out durasi))
            {
                MessageBox.Show("Format waktu di textbox tidak valid.\nGunakan format hh:mm:ss atau hh:mm:ss.fff");
                return;
            }

            TimeSpan mid = TimeSpan.FromMilliseconds(durasi.TotalMilliseconds * durasivideo * 4);
            TimeSpan mid2;
            if (string.Equals(datavid2, "-S", StringComparison.OrdinalIgnoreCase))
            {
                mid2 = mid;
            }
            else if (string.Equals(datavid2, "-N", StringComparison.OrdinalIgnoreCase))
            {
                mid2 = TimeSpan.FromTicks((long)Math.Round(mid.Ticks * 0.5));
            }
            else
            {
                mid2 = TimeSpan.FromTicks((long)Math.Round(mid.Ticks * 0.3));
            }



            txtPred.TextAlign = HorizontalAlignment.Left;
            txtPred.Text = mid2.ToString(@"d\.hh\:mm\:ss\.fff");
        }

        private async Task CekFrameDetector()
        {
            if (framedetector == 1)
            {
                framecounter();
                framedetector = 0;
            }
            else
            {
                while (framedetector != 1)
                {
                    await Task.Delay(100);
                }
                framecounter();
                framedetector = 0;
            }
        }
        private async Task CekFrameDetector2()
        {
            if (framedetector2 == 1)
            {
                durasivideo = int.Parse(btnframeCounter.Text.Trim());
                prediction();
                StopCountingLoop(txtPred);
                framedetector2 = 0;
            }
            else
            {
                while (framedetector2 != 1)
                {
                    await Task.Delay(100);
                }
                durasivideo = int.Parse(btnframeCounter.Text.Trim());
                prediction();
                StopCountingLoop(txtPred);
                framedetector2 = 0;
            }
        }
        private async Task CekFrameDetector3()
        {
            if (framedetector3 == 1)
            {
                framedetector3 = 0;
                panelimg.Enabled = true;
                panelvid.Enabled = true;
                btnframeCounter.BackColor = Color.Black;
                btnframeCounter.FlatStyle = FlatStyle.Flat;
                btnframeCounter.Font = new Font(btnframeCounter.Font.FontFamily, 14, FontStyle.Bold);
                txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Estimated." + Environment.NewLine);
            }
            else
            {
                while (framedetector3 != 1)
                {
                    await Task.Delay(100);
                }
                framedetector3 = 0;
                PlayCompletionSound();
                panelimg.Enabled = true;
                panelimgBatch.Enabled = true;
                panelvid.Enabled = true;
                btnframeCounter.BackColor = Color.Black;
                btnframeCounter.FlatStyle = FlatStyle.Flat;
                btnframeCounter.Font = new Font(btnframeCounter.Font.FontFamily, 14, FontStyle.Bold);
                txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Estimated." + Environment.NewLine);
            }
        }
        private async void btnEsTestvid_Click(object sender, EventArgs e)
        {
            if (vidfilePath == string.Empty)
            {
                MessageBox.Show("No video selected.");
                return;
            }
            else
            {
                txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Estimating....." + Environment.NewLine);
                panelimg.Enabled = false;
                panelimgBatch.Enabled = false;
                panelvid.Enabled = false;
                txtPred.TextAlign = HorizontalAlignment.Left;                
                StartCountingLoop(txtPred);
                try
                {
                    // testframe
                    await TestFrame();
                    // framecounter
                    await CekFrameDetector();
                    // prediction
                    await CekFrameDetector2();
                    // finish
                    await CekFrameDetector3();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }
        private void HitungGambar()
        {
            // Set ekstensi diperbolehkan (case-insensitive), lokal pada metode
            var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp" };

            string root = imgdirPath; // atau: string root = C.Text;
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            {
                btnimgcounter.Text = "0";
                return;
            }

            long total = 0;
            var stack = new Stack<string>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                string dir = stack.Pop();
                try
                {
                    foreach (var sub in Directory.EnumerateDirectories(dir))
                    {
                        if (string.Equals(Path.GetFileName(sub), "UPSCALED",
                                          StringComparison.OrdinalIgnoreCase))
                            continue;
                        stack.Push(sub);
                    }

                    foreach (var file in Directory.EnumerateFiles(dir))
                    {
                        if (allowedExt.Contains(Path.GetExtension(file)))
                            total++;
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (PathTooLongException) { }
                catch (IOException) { }
            }

            btnimgcounter.Text = total.ToString();
        }
        private void PilihGambarTerbesar()
        {
            string root = imgdirPath; // Data C
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            {
                return;
            }

            // Jalankan di background agar UI tidak freeze
            Task.Run(() =>
            {
                var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".jpg", ".jpeg", ".png", ".webp" };

                long biggestSize = -1;

                var stack = new Stack<string>();
                stack.Push(root);

                while (stack.Count > 0)
                {
                    string dir = stack.Pop();
                    try
                    {
                        // Telusuri subfolder, kecuali "UPSCALED"
                        foreach (var sub in Directory.EnumerateDirectories(dir))
                        {
                            if (!string.Equals(Path.GetFileName(sub), "UPSCALED",
                                               StringComparison.OrdinalIgnoreCase))
                            {
                                stack.Push(sub);
                            }
                        }

                        // Periksa file gambar di folder saat ini
                        foreach (var file in Directory.EnumerateFiles(dir))
                        {
                            if (!allowedExt.Contains(Path.GetExtension(file))) continue;

                            long size;
                            try { size = new FileInfo(file).Length; }
                            catch { continue; } // lewati jika tidak dapat diakses

                            if (size > biggestSize)
                            {
                                biggestSize = size;
                                largestimg = file;
                            }
                        }
                    }
                    catch (UnauthorizedAccessException) { /* lewati */ }
                    catch (PathTooLongException) { /* lewati */ }
                    catch (IOException) { /* lewati */ }
                }

                // Kembali ke UI thread untuk menulis hasil ke Data B

            });
        }

        private async void btnEsTestbatch_Click(object sender, EventArgs e)
        {
            if (imgdirPath == string.Empty)
            {
                MessageBox.Show("No folder selected.");
                return;
            }
            else
            {
                txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Estimating....." + Environment.NewLine);
                txtPred.TextAlign = HorizontalAlignment.Left;
                StartCountingLoop(txtPred);
                PilihGambarTerbesar();
                await Task.Delay(100);
                imgfileExt = Path.GetExtension(largestimg);
                txtvidEND.AppendText(Environment.NewLine + largestimg + Environment.NewLine);


                // RealRS 
                if (radimgS.Checked == true)
                {
                    exeFilePath = Path.Combine(twoLevelsUp, "Realsr", "realsr-ncnn-vulkan.exe");
                    modelz = " -m ";
                }
                // Real-Esrgan
                else
                {
                    exeFilePath = Path.Combine(twoLevelsUp, "Realesrgan", "realesrgan-ncnn-vulkan.exe");
                    modelz = " -n ";
                }
                string datasubmit = " -i " + '\u0022' + largestimg + '\u0022' + modelz + dataimg
                   + " -s " + imgscale + " -o " + '\u0022' + imgfileOut + "\\" + imgoutname + imgfileExt + '\u0022';
                ProcessStartInfo ps = new ProcessStartInfo
                {
                    FileName = exeFilePath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = datasubmit
                };

                using (Process process = new Process())
                {
                    process.StartInfo = ps;
                    TimerMulai();

                    try
                    {
                        process.Start();

                        // Begin reading output and error asynchronously
                        process.OutputDataReceived += (outputSender, outputEvent) =>
                        {
                            if (!string.IsNullOrEmpty(outputEvent.Data))
                            {
                                // Use Invoke to update the TextBox safely from another thread
                                txtvidEND.Invoke((MethodInvoker)delegate
                                {
                                    TimerStop();
                                    txtvidEND.AppendText(outputEvent.Data + Environment.NewLine);
                                    txtvidEND.SelectionStart = txtvidEND.Text.Length;
                                    txtvidEND.ScrollToCaret();
                                });
                            }
                        };

                        process.ErrorDataReceived += (errorSender, errorEvent) =>
                        {
                            if (!string.IsNullOrEmpty(errorEvent.Data))
                            {
                                // Use Invoke to update the TextBox safely from another thread
                                txtvidEND.Invoke((MethodInvoker)delegate
                                {
                                    txtvidEND.AppendText(errorEvent.Data + Environment.NewLine);
                                    txtvidEND.SelectionStart = txtvidEND.Text.Length;
                                    txtvidEND.ScrollToCaret();
                                });
                            }
                        };


                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        await Task.Run(() => process.WaitForExit());  // Run WaitForExit on a background thread
                        string lagestout = $"{imgfileOut}\\{imgoutname}{imgfileExt}";
                        TimerStop();


                        txtvidEND.Invoke((MethodInvoker)delegate
                        {
                            if (File.Exists($"{imgfileOut}\\{imgoutname}{imgfileExt}"))
                            {
                                txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Estimated." + Environment.NewLine);

                                try
                                {
                                    // Hilangkan atribut read-only jika ada
                                    var attr = File.GetAttributes(lagestout);
                                    if ((attr & FileAttributes.ReadOnly) != 0)
                                        File.SetAttributes(lagestout, attr & ~FileAttributes.ReadOnly);

                                    File.Delete(lagestout); // hapus permanen (bukan ke Recycle Bin)


                                }
                                catch (UnauthorizedAccessException ex)
                                {
                                    txtvidEND.AppendText($"\nTidak memiliki izin untuk menghapus file.\n{lagestout}\nDetail: {ex.Message}\n");
                                }
                                catch (IOException ex)
                                {
                                    txtvidEND.AppendText($"\nGagal menghapus. File mungkin sedang dipakai aplikasi lain.\n{lagestout}\n{lagestout}\nDetail: {ex.Message}\n");
                                }
                                catch (Exception ex)
                                {
                                    txtvidEND.AppendText($"\nTerjadi kesalahan.\n{lagestout}\n{lagestout}\nDetail: {ex.Message}\n");
                                }
                            }
                            else
                            {
                                txtvidEND.AppendText(Environment.NewLine + Environment.NewLine + " Process failed, check Log file." + Environment.NewLine);

                            }
                            StopCountingLoop(txtPred);
                            prediction2();
                            txtvidEND.SelectionStart = txtvidEND.Text.Length;
                            txtvidEND.ScrollToCaret();
                        });
                    }
                    catch (Exception ex)
                    {
                        txtvidEND.Invoke((MethodInvoker)delegate
                        {
                            txtvidEND.Text = $"Error: {ex.Message}";
                        });
                    }
                }
            }
        }
        private static void StartCountingLoop(Control target)
        {
            StopCountingLoop(target); // hentikan loop lama pada kontrol yang sama

            var cts = new CancellationTokenSource();
            target.Tag = cts; // simpan CTS pada Tag

            int state = 0;

            Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    string text;
                    switch (state)
                    {
                        case 0: text = "counting."; break;
                        case 1: text = "counting.."; break;
                        case 2: text = "counting..."; break;
                        case 3: text = "counting...."; break;
                        default: text = "counting....."; break;
                    }
                    state = (state + 1) % 5;

                    if (target != null && !target.IsDisposed && target.IsHandleCreated)
                        target.BeginInvoke((Action)(() => target.Text = text));

                    try { await Task.Delay(500, cts.Token); }
                    catch (TaskCanceledException) { break; }
                }
            }, cts.Token);
        }

        private static void StopCountingLoop(Control target)
        {
            if (target == null) return;
            var cts = target.Tag as CancellationTokenSource;
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                target.Tag = null;
            }
        }
    }
}
