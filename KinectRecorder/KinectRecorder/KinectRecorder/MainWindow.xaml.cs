using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using KinectRecorder.src;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;
using Timer = System.Timers.Timer;

namespace KinectRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker _bgWorker;
        private string _exerciseName;
        private string _secondsToRecord;
        private string _colorStream;
        private string _outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string DEFAULT_KINECT_UTILITY_PATH =
            "C:\\Program Files\\Microsoft SDKs\\Kinect\\v2.0_1409\\Tools\\KinectStudio\\KSUtil.exe";
        private string _fileName;
        private readonly Process _recordingProcess;
        private CustomMessageBox _messageBox;
        private readonly Timer _progressTimer = new Timer();

        public MainWindow()
        {
            InitializeComponent();
            UpdateOutFolderLabel();
            InitializeBackgroundWorker();
            InitializeTimer();
            InitializeMessageBox();
            _recordingProcess = new Process();
        }

        private void InitializeTimer()
        {
            _progressTimer.Interval = 1000;
            _progressTimer.Elapsed += OnTimedEvent;
        }

        private void InitializeMessageBox()
        {
            _messageBox = new CustomMessageBox("Wait!", "Capture In Progress\n");
            _messageBox.UpdateButton("Cancel", StopKinectRecording);
        }

        private void InitializeBackgroundWorker()
        {
            _bgWorker = new BackgroundWorker();
            _bgWorker.DoWork += StartKinectRecording;
            _bgWorker.RunWorkerCompleted += OnRecordingFinished;
            _bgWorker.ProgressChanged += UpdateMessageBox;
            _bgWorker.WorkerReportsProgress = true;
        }

        private void UpdateOutFolderLabel()
        {
            OutputLabel.Content = "Out folder: " + _outputFolder;
        }

        private void PlayClicked(object sender, RoutedEventArgs e)
        {
            if (Exercise.Text.Length == 0 || Seconds.Text.Length == 0)
            {
                MessageBox.Show(this, "You have to specify name and seconds!","Error!");
                return;
            }
            _bgWorker.RunWorkerAsync();
            _messageBox.ShowDialog();
        }

        private void OnRecordingFinished(object sender, EventArgs eventArgs)
        {
            if (!_progressTimer.Enabled)
            {
                return;
            }
            _progressTimer.Enabled = false;
            UnsubscribeButtonEvents();
            _messageBox.UpdateMessage(" DONE!");
            _messageBox.UpdateButton("Ok", ResetMessageBox);
        }

        private void ResetMessageBox(object sender, EventArgs eventArgs)
        {
            _messageBox.Hide();
            UnsubscribeButtonEvents();
            _messageBox.ResetMessage();
            _messageBox.UpdateButton("Cancel", StopKinectRecording);
        }

        private void UnsubscribeButtonEvents()
        {
            _messageBox.UnsubscribeButtonEvent(StopKinectRecording);
            _messageBox.UnsubscribeButtonEvent(ResetMessageBox);
        }

        private void UpdateMessageBox(object sender, EventArgs eventArgs)
        {
            _messageBox.UpdateMessage(".");
        }

        private void StopKinectRecording(object sender, EventArgs eventArgs)
        {
            _recordingProcess.Kill();
            _progressTimer.Enabled = false;

            ResetMessageBox(null, null);
            try
            {
                var filepath = _outputFolder + "\\" + _fileName;
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ExerciseNameChanged(object sender, TextChangedEventArgs e)
        {
            if (ValidateExerciseName(sender as TextBox))
            {
                _exerciseName = UpdateValue(sender as TextBox);
            }
        }

        private void SecondsChanged(object sender, TextChangedEventArgs e)
        {
            if (ValidateSeconds(sender as TextBox))
            {
                _secondsToRecord = UpdateValue(sender as TextBox);
            }
        }

        private static string UpdateValue(TextBox sender)
        {
            if (sender != null)
            {
                return sender.Text.TrimEnd('\r', '\n');
            }
            return string.Empty;
        }

        private static bool ValidateSeconds(TextBox sender)
        {
            if (sender != null)
            {
                Regex regex = new Regex("^[0-9]*$");
                if (!regex.IsMatch(sender.Text))
                {
                    MessageBox.Show("You can insert only numbers", "Error!");
                    sender.Text = null;
                    return false;
                }
                return true;
            }
            return false;
        }

        private static bool ValidateExerciseName(TextBox sender)
        {
            if (sender != null)
            {
                Regex regex = new Regex("^[a-zA-Z0-9_.-]*$");
                if (!regex.IsMatch(sender.Text))
                {
                    MessageBox.Show("You can insert only numbers, letters and underscores", "Error!");
                    sender.Text = null;
                    return false;
                }
                return true;
            }
            return false;
        }

        private void StartKinectRecording(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            StringBuilder sb = new StringBuilder();

            _fileName = _exerciseName + "-" + DateTime.Now.ToString("hh:mm:ss").Replace(':', '_') + ".xef ";

            sb.Append("-record ")
              .Append(_outputFolder)
              .Append("\\")
              .Append(_fileName)
              .Append(_secondsToRecord)
              .Append(" -stream depth ir ")
              .Append(_colorStream)
              .Append("body");

            ProcessStartInfo kinectRecordingProcess = new ProcessStartInfo(DEFAULT_KINECT_UTILITY_PATH, sb.ToString())
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            try
            {
                _progressTimer.Enabled = true;
                _recordingProcess.StartInfo = kinectRecordingProcess;
                _recordingProcess.Start();

                PrintOutput(_recordingProcess);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _bgWorker.ReportProgress(0);
        }

        private static void PrintOutput(Process process)
        {
            while (!process.StandardOutput.EndOfStream)
            {
                string line = process.StandardOutput.ReadLine();
                Console.Write(line);
            }
        }

        private void BrowseButton(object sender, RoutedEventArgs e)
        {
            using (
                var dialog = new FolderBrowserDialog()
                {
                    Description = "Current output folder: " + _outputFolder,
                    SelectedPath = _outputFolder
                })
            {
                var result = dialog.ShowDialog();
                _outputFolder = dialog.SelectedPath;
                UpdateOutFolderLabel();
            }
        }

        private void ColorStreamChecked(object sender, RoutedEventArgs e)
        {
            _colorStream = "color ";
        }

        private void ColorStreamUnchecked(object sender, RoutedEventArgs e)
        {
            _colorStream = "";
        }
    }
}
