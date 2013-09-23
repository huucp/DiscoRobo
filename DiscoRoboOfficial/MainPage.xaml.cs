using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Devices;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace DiscoRoboOfficial
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Accelerometer accelerometer;
        private PhotoCamera camera { get; set; }

        private const string ChangeModeSound = "Audio/ChangeMode.wav";
        private const string ShakeSound = "Audio/1200.wav";
        private const string BowSound = "Audio/1100.wav";
        private const string MoveFootSound = "Audio/1500.wav";
        private const string ChangeLEDSound = "Audio/1000.wav";
        private const string ChangeSpeedSound = "Audio/800.wav";

        //private const string D1Image = "/UIComponent/d1.png";
        //private const string D2Image = "/UIComponent/d2.png";
        //private const string D3Image = "/UIComponent/d3.png";
        //private const string D4Image = "/UIComponent/d4.png";
        //private const string D5Image = "/UIComponent/d5.png";
        //private const string D6Image = "/UIComponent/d6.png";
        //private const string D7Image = "/UIComponent/d7.png";
        //private const string D8Image = "/UIComponent/d8.png";
        private const string D1Image = "/UIComponent/New/d1.png";
        private const string D2Image = "/UIComponent/New/d2.png";
        private const string D3Image = "/UIComponent/New/d3.png";
        private const string D4Image = "/UIComponent/New/d4.png";
        private const string D5Image = "/UIComponent/New/d5.png";
        private const string D6Image = "/UIComponent/New/d6.png";
        private const string D7Image = "/UIComponent/New/d7.png";
        private const string D8Image = "/UIComponent/New/d8.png";

        //private const string ShakeIconPress = "/UIComponent/ic_shake_nor.png";
        //private const string ShakeIconNormal = "/UIComponent/ic_shake_press_1.png";
        private const string ShakeIconPress = "/UIComponent/New/ic_shake_nor.png";
        private const string ShakeIconNormal = "/UIComponent/New/ic_shake_press.png";

        //private const string CameraModeIcon = "/UIComponent/ic_camera_nor_1.png";
        //private const string ImageModeIcon = "/UIComponent/ic_gesture_nor_1.png";
        private const string CameraModeIcon = "/UIComponent/New/ic_camera_press.png";
        private const string ImageModeIcon = "/UIComponent/New/ic_gesture_press.png";

        //private const string StopIcon = "UIComponent/ic_stop_nor.png";
        //private const string RecordIcon = "UIComponent/ic_record_nor.png";
        //private const string ReplayIcon = "UIComponent/ic_play_nor.png";
        private const string StopIcon = "UIComponent/New/ic_stop_nor.png";        
        private const string ReplayIcon = "UIComponent/New/ic_play_nor.png";
        private const string RecordIcon = "UIComponent/New/ic_record_nor.png";
        private const string StopRecordIcon = "UIComponent/New/ic_record_press.png";

        private bool UseCamera = false;
        private bool UseAccelerometer = false;

        // Record parameters
        private DateTime RecordPivot;
        private bool IsRecording = false;
        private List<uint> TimePivots = new List<uint>(); // milliseconds
        private List<int> RobotMove = new List<int>(); // 1: head shake, 2: head bow, 3: move foot        

        // Reply parameters
        private DispatcherTimer ReplayTimer;
        private DateTime ReplayPivot;
        private int ReplayMoveCount;
        private bool IsReplaying = false;
        private bool StopReplay = false;
        private const int TickInterval = 100;

        // Change speed parameters
        private bool HighSpeed = true;
        private const string HighSpeedIcon = "UIComponent/New/ic_change_speed_press.png";
        private const string LowSpeedIcon = "UIComponent/New/ic_change_speed_nor.png";

        private RobotState state;
        private enum RobotState
        {
            D1, D2, D3, D4, D5, D6, D7, D8
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            state = RobotState.D1;
            ReplayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(TickInterval)
            };
            ReplayTimer.Tick += ReplayTimer_Tick;
        }


        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (accelerometer != null)
            {
                accelerometer.Stop();
#pragma warning disable 612,618
                accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
#pragma warning restore 612,618
            }
            if (camera != null) camera.Dispose();
            ReplayTimer.Tick -= ReplayTimer_Tick;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            //base.OnBackKeyPress(e);
            if (LoadPopUp.IsOpen)
            {
                LoadPopUp.IsOpen = false;
                e.Cancel = true;
                return;
            }
            if (SavePopup.IsOpen)
            {
                SavePopup.IsOpen = false;
                e.Cancel = true;
                return;
            }
            if (HelpPopUp.IsOpen)
            {
                HelpPopUp.IsOpen = false;
                e.Cancel = true;
                return;
            }
            if (NavigationService.CanGoBack)
            {
                while (NavigationService.RemoveBackEntry() != null)
                {
                    NavigationService.RemoveBackEntry();
                }
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (UseCamera)
            {
                camera = new PhotoCamera(CameraType.Primary);
                CameraView.SetSource(camera);
            }
            if (UseAccelerometer)
            {
#pragma warning disable 612,618
                accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
#pragma warning restore 612,618
                accelerometer.Start();
            }
            ReplayTimer.Tick += ReplayTimer_Tick;
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => AccelerometerChanged(e));
        }

        private const double ShakeThresholdX = 0.8;
        private const double ShakeThresholdZ = 0.6;
        private const int ShakeDelay = 250;
        private DateTime ShakeTimePivot;


        private void AccelerometerChanged(AccelerometerReadingEventArgs e)
        {

            var t = e.Timestamp.DateTime;
            if ((t - ShakeTimePivot).TotalMilliseconds > ShakeDelay)
            {
                if (e.X > ShakeThresholdX)
                {
                    MoveFoot();
                    ShakeTimePivot = DateTime.Now;
                }
                else if (e.Z > ShakeThresholdZ)
                {
                    BowHead();
                    ShakeTimePivot = DateTime.Now;
                }
            }
        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            if (UseAccelerometer || LoadPopUp.IsOpen || SavePopup.IsOpen || HelpPopUp.IsOpen) return;
            if (Math.Abs(e.HorizontalVelocity - 0) > 0.001 &&
                e.Direction == System.Windows.Controls.Orientation.Horizontal)
            {
                if (e.GetPosition(LayoutRoot).Y < 400)
                {
                    ShakeHead();
                }
                else
                {
                    MoveFoot();
                }
            }
            if (Math.Abs(e.VerticalVelocity - 0) > 0.001 &&
                e.Direction == System.Windows.Controls.Orientation.Vertical)
            {
                BowHead();
            }
        }

        private void MoveFoot()
        {
            PlaySound(MoveFootSound);
            var tmpState = state;
            switch (tmpState)
            {
                case RobotState.D1:
                    state = RobotState.D5;
                    break;
                case RobotState.D2:
                    state = RobotState.D6;
                    break;
                case RobotState.D3:
                    state = RobotState.D7;
                    break;
                case RobotState.D4:
                    state = RobotState.D8;
                    break;
                case RobotState.D5:
                    state = RobotState.D1;
                    break;
                case RobotState.D6:
                    state = RobotState.D2;
                    break;
                case RobotState.D7:
                    state = RobotState.D3;
                    break;
                case RobotState.D8:
                    state = RobotState.D4;
                    break;
            }
            if (!UseCamera) ChangeImage(state);
            if (IsRecording)
            {
                var now = DateTime.Now;
                var duration = (now - RecordPivot).TotalMilliseconds;
                TimePivots.Add((uint)duration);
                RobotMove.Add(3);
            }
        }

        private void ShakeHead()
        {
            PlaySound(ShakeSound);
            var tmpState = state;
            switch (tmpState)
            {
                case RobotState.D1:
                    state = RobotState.D3;
                    break;
                case RobotState.D2:
                    state = RobotState.D4;
                    break;
                case RobotState.D3:
                    state = RobotState.D1;
                    break;
                case RobotState.D4:
                    state = RobotState.D2;
                    break;
                case RobotState.D5:
                    state = RobotState.D7;
                    break;
                case RobotState.D6:
                    state = RobotState.D8;
                    break;
                case RobotState.D7:
                    state = RobotState.D5;
                    break;
                case RobotState.D8:
                    state = RobotState.D6;
                    break;
            }
            if (!UseCamera) ChangeImage(state);
            if (IsRecording)
            {
                var now = DateTime.Now;
                var duration = (now - RecordPivot).TotalMilliseconds;
                TimePivots.Add((uint)duration);
                RobotMove.Add(1);
            }
        }

        private void BowHead()
        {
            PlaySound(BowSound);
            var tmpState = state;
            switch (tmpState)
            {
                case RobotState.D1:
                    state = RobotState.D2;
                    break;
                case RobotState.D2:
                    state = RobotState.D1;
                    break;
                case RobotState.D3:
                    state = RobotState.D4;
                    break;
                case RobotState.D4:
                    state = RobotState.D3;
                    break;
                case RobotState.D5:
                    state = RobotState.D6;
                    break;
                case RobotState.D6:
                    state = RobotState.D5;
                    break;
                case RobotState.D7:
                    state = RobotState.D8;
                    break;
                case RobotState.D8:
                    state = RobotState.D7;
                    break;
            }
            if (!UseCamera) ChangeImage(state);
            if (IsRecording)
            {
                var now = DateTime.Now;
                var duration = (now - RecordPivot).TotalMilliseconds;
                TimePivots.Add((uint)duration);
                RobotMove.Add(2);
            }
        }

        private void PlaySound(string path)
        {
            Stream stream = TitleContainer.OpenStream(path);
            SoundEffect effect = SoundEffect.FromStream(stream);            
            FrameworkDispatcher.Update();            
            effect.Play();
            stream.Close();
        }

        // Change ImageView source based on state
        private void ChangeImage(RobotState s)
        {
            var uri = new Uri(@D1Image, UriKind.Relative);
            var image = new BitmapImage(uri);
            switch (s)
            {
                case RobotState.D1:
                    uri = new Uri(@D1Image, UriKind.Relative);
                    image = new BitmapImage(uri);
                    break;
                case RobotState.D2:
                    uri = new Uri(D2Image, UriKind.Relative);
                    image = new BitmapImage(uri);
                    break;
                case RobotState.D3:
                    uri = new Uri(@D3Image, UriKind.Relative);
                    image = new BitmapImage(uri);
                    break;
                case RobotState.D4:
                    uri = new Uri(@D4Image, UriKind.Relative);
                    image = new BitmapImage(uri);
                    break;
                case RobotState.D5:
                    uri = new Uri(@D5Image, UriKind.Relative);
                    image = new BitmapImage(uri);
                    break;
                case RobotState.D6:
                    uri = new Uri(@D6Image, UriKind.Relative);
                    image = new BitmapImage(uri);
                    break;
                case RobotState.D7:
                    uri = new Uri(@D7Image, UriKind.Relative);
                    image = new BitmapImage(uri);
                    break;
                case RobotState.D8:
                    uri = new Uri(@D8Image, UriKind.Relative);
                    image = new BitmapImage(uri);
                    break;
            }
            ImageView.ImageSource = image;
        }

        private void ChangeMode_OnClick(object sender, EventArgs e)
        {
            PlaySound(ChangeModeSound);
        }

        private void ShakeEnable_Click(object sender, RoutedEventArgs e)
        {

            if (!UseAccelerometer)
            {
                var pressUri = new Uri(ShakeIconNormal, UriKind.Relative);
                ShakeEnableButtonBackground.ImageSource = new BitmapImage(pressUri);
                
                accelerometer = new Accelerometer
                                    {
                                        TimeBetweenUpdates = TimeSpan.FromMilliseconds(100)
                                    };
#pragma warning disable 612,618
                accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
#pragma warning restore 612,618
                ShakeTimePivot = DateTime.Now;
                accelerometer.Start();
            }
            else
            {
                var norUri = new Uri(ShakeIconPress, UriKind.Relative);

                ShakeEnableButtonBackground.ImageSource = new BitmapImage(norUri);
#pragma warning disable 612,618
                accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
#pragma warning restore 612,618
                accelerometer.Stop();
                accelerometer = null;
            }
            UseAccelerometer = !UseAccelerometer;
        }

        private void CameraEnable_Click(object sender, MouseButtonEventArgs e)
        {
            if (!UseCamera)
            {
                //if (camera == null)
                //{
                if (Camera.IsCameraTypeSupported(CameraType.Primary))
                {
                    camera = new PhotoCamera(CameraType.Primary);
                    CameraView.SetSource(camera);

                    ImageCanvas.Visibility = Visibility.Collapsed;
                    VideoCanvas.Visibility = Visibility.Visible;

                    var pressUri = new Uri(ImageModeIcon, UriKind.Relative);
                    CameraEnableButtonBackground.ImageSource = new BitmapImage(pressUri);

                    UseCamera = true;
                }
                else
                {
                    MessageBox.Show("Camera is not supported in this device.");
                }
                //}
                //else
                //{                    
                //    ImageCanvas.Visibility = Visibility.Collapsed;
                //    VideoCanvas.Visibility = Visibility.Visible;

                //    var pressUri = new Uri(ImageModeIcon, UriKind.Relative);
                //    CameraEnableButtonBackground.ImageSource = new BitmapImage(pressUri);
                //    UseCamera = false;
                //}
            }
            else
            {
                camera.Dispose();

                ImageCanvas.Visibility = Visibility.Visible;
                VideoCanvas.Visibility = Visibility.Collapsed;

                var norUri = new Uri(CameraModeIcon, UriKind.Relative);
                CameraEnableButtonBackground.ImageSource = new BitmapImage(norUri);

                UseCamera = false;
            }
        }


        private void RecordButton_OnClick(object sender, EventArgs e)
        {
            IsRecording = !IsRecording;
            if (IsRecording)
            {
                RecordPivot = DateTime.Now;
                TimePivots.Clear();
                RobotMove.Clear();
                var uri = new Uri(StopRecordIcon, UriKind.Relative);
                RecordButtonBackground.ImageSource = new BitmapImage(uri);
            }
            else
            {
                if (TimePivots.Any())
                {
                    var first = TimePivots[0];
                    for (int index = 0; index < TimePivots.Count; index++)
                    {
                        TimePivots[index] -= (first - 500);
                    }
                    SavePopup.IsOpen = true;
                }                
                var uri = new Uri(RecordIcon, UriKind.Relative);
                RecordButtonBackground.ImageSource = new BitmapImage(uri);
            }
        }


        private void ReplayButton_OnClick(object sender, EventArgs e)
        {


            if (IsRecording) return;
            if (!IsReplaying)
            {
                LoadList.ItemsSource = LoadRecordList();
                LoadPopUp.IsOpen = true;
            }
            else
            {
                StopReplay = true;
            }
        }

        private double ReplayTiming = 0;
        private void ReplayTimer_Tick(object sender, EventArgs e)
        {
            if (Math.Abs(ReplayTiming - 0) < 0.01) ReplayTiming = (DateTime.Now - ReplayPivot).TotalMilliseconds;
            else ReplayTiming += TickInterval;
            for (int index = 0; index < TimePivots.Count; index++)
            {
                var time = TimePivots[index];
                if (ReplayTiming > TimePivots.Last() + TickInterval)
                {
                    ReplayMoveCount = TimePivots.Count;
                    break;
                }
                if (ReplayTiming - time < TickInterval && ReplayTiming - time >= 0)
                {
                    switch (RobotMove[index])
                    {
                        case 1:
                            ShakeHead();
                            break;
                        case 2:
                            BowHead();
                            break;
                        case 3:
                            MoveFoot();
                            break;
                    }
                    ReplayMoveCount++;
                    break;
                }
            }
            if (ReplayMoveCount == TimePivots.Count || StopReplay)
            {
                ReplayTimer.Stop();
                IsReplaying = false;
                ReplayTiming = 0;
                var uri = new Uri(ReplayIcon, UriKind.Relative);
                ReplayButtonBackground.ImageSource = new BitmapImage(uri);
            }
        }

        private void ChangeSpeedButton_OnCllick(object sender, MouseButtonEventArgs e)
        {
            PlaySound(ChangeSpeedSound);
            HighSpeed = !HighSpeed;
            if (HighSpeed)
            {
                var uri = new Uri(HighSpeedIcon, UriKind.Relative);
                ChangeSpeedButtonBackground.ImageSource = new BitmapImage(uri);
            }
            else
            {
                var uri = new Uri(LowSpeedIcon, UriKind.Relative);
                ChangeSpeedButtonBackground.ImageSource = new BitmapImage(uri);
            }
        }

        private void SaveRecord(string SaveFileName)
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (myIsolatedStorage.FileExists(SaveFileName)) myIsolatedStorage.DeleteFile(SaveFileName);
                using (
                    var writeFile =
                        new StreamWriter(new IsolatedStorageFileStream(SaveFileName, FileMode.Create, FileAccess.Write,
                                                                       myIsolatedStorage)))
                {
                    for (int i = 0; i < TimePivots.Count; i++)
                    {
                        string text = string.Format("{0}-{1}", TimePivots[i], RobotMove[i]);
                        writeFile.WriteLine(text);
                    }
                    writeFile.Close();
                }
            }
        }

        private List<string> LoadRecordList()
        {
            var isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            var list = isoFile.GetFileNames();
            var storeList = new List<string>();
            foreach (var file in list)
            {
                var fileWithoutExtension = Path.GetFileNameWithoutExtension(file);
                storeList.Add(fileWithoutExtension);
            }
            return storeList;
        }

        private void LoadRecord(string SaveFileName)
        {
            TimePivots.Clear();
            RobotMove.Clear();
            IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            if (!myIsolatedStorage.FileExists(SaveFileName)) return;
            IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(SaveFileName, FileMode.Open, FileAccess.Read);
            using (var reader = new StreamReader(fileStream))
            {
                while (!reader.EndOfStream)
                {
                    string text = reader.ReadLine();
                    if (text != null)
                    {
                        string[] split = Regex.Split(text, "-");
                        if (split.Count() > 1)
                        {
                            uint timePivot;
                            int robotMove;
                            if (uint.TryParse(split[0], out timePivot) && int.TryParse(split[1], out robotMove))
                            {
                                TimePivots.Add(timePivot);
                                RobotMove.Add(robotMove);
                            }
                        }
                    }
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SavePopup.IsOpen = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var filename = SaveTextBox.Text;
            if (string.IsNullOrWhiteSpace(filename))
            {
                MessageBox.Show("Filename is invalid");
            }
            else
            {
                filename += ".txt";
                SaveRecord(filename);
                SavePopup.IsOpen = false;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var item = (string)menuItem.DataContext;
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                if (!myIsolatedStorage.FileExists(item + ".txt")) return;
                myIsolatedStorage.DeleteFile(item + ".txt");
                var list = myIsolatedStorage.GetFileNames();
                var storeList = new List<string>();
                foreach (var file in list)
                {
                    var fileWithoutExtension = Path.GetFileNameWithoutExtension(file);
                    storeList.Add(fileWithoutExtension);
                }
                LoadList.ItemsSource = storeList;
            }
        }


        private void CancelLoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPopUp.IsOpen = false;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPopUp.IsOpen = false;
            if (LoadList.SelectedItem == null) return;
            string chosenRecord = LoadList.SelectedItem.ToString();
            LoadRecord(chosenRecord + ".txt");
            ReplayPivot = DateTime.Now;
            ReplayMoveCount = 0;
            ReplayTimer.Start();
            IsReplaying = true;
            StopReplay = false;
            var uri = new Uri(StopIcon, UriKind.Relative);
            ReplayButtonBackground.ImageSource = new BitmapImage(uri);
        }

        private void CloseHelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpPopUp.IsOpen = false;
        }

        private void ShowTutorialButton_OnClick(object sender, RoutedEventArgs e)
        {
            HelpPopUp.IsOpen = false;
            var uri = new Uri("/Tutorial/Tutorial1.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void HelpButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HelpPopUp.IsOpen = true;
        }
    }
}