using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Devices;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace DiscoRoboOfficial
{
    public partial class MainPage : PhoneApplicationPage
    {
        #region Variables
        private Accelerometer accelerometer;
        private PhotoCamera camera { get; set; }

        private const string ChangeModeSound = "Audio/ChangeMode.wav";
        private const string ShakeSound = "Audio/1200.wav";
        private const string BowSound = "Audio/1100.wav";
        private const string MoveFootSound = "Audio/1500.wav";
        // private const string ChangeLEDSound = "Audio/1000.wav"; // Reserve
        private const string ChangeSpeedSound = "Audio/800.wav";

        private const string GreetingSound = "Audio/category 1.wav";
        private const string FeelingSound = "Audio/category 2.wav";
        private const string EncouragementSound = "Audio/category 3.wav";
        private const string ActivitiesAnswerSound = "Audio/category 4.wav";
        private const string ActivitiesYesNoSound = "Audio/category 5.wav";
        private const string LoveAndFriendshipSound = "Audio/category 6.wav";
        private const string DirtyWordsSound = "Audio/category 7.wav";
        private const string UnknownSound = "Audio/category 8.wav";
        private const string FoodAndDrinksSound = "Audio/category 9.wav";
        private const string YesNoSound = "Audio/category 10.wav";
        private const string PopularPlacesSound = "Audio/category 11.wav";
        private const string SelfIntroductionSound = "Audio/category 12.wav";
        private const string CartoonSound = "Audio/category 13.wav";
        private const string CelebritiesSound = "Audio/category 14.wav";
        private const string AnimalsSound = "Audio/category 15.wav";
        private const string RandomSound = "Audio/category 16.wav";

        private const string D1Image = "/UIComponent/New/d1.png";
        private const string D2Image = "/UIComponent/New/d2.png";
        private const string D3Image = "/UIComponent/New/d3.png";
        private const string D4Image = "/UIComponent/New/d4.png";
        private const string D5Image = "/UIComponent/New/d5.png";
        private const string D6Image = "/UIComponent/New/d6.png";
        private const string D7Image = "/UIComponent/New/d7.png";
        private const string D8Image = "/UIComponent/New/d8.png";

        private const string ShakeIconPress = "/UIComponent/New/ic_shake_nor.png";
        private const string ShakeIconNormal = "/UIComponent/New/ic_shake_press.png";

        private const string CameraModeIcon = "/UIComponent/New/ic_camera_press.png";
        private const string ImageModeIcon = "/UIComponent/New/ic_gesture_press.png";

        private const string StopIcon = "UIComponent/New/ic_stop_nor.png";
        private const string ReplayIcon = "UIComponent/New/ic_play_nor.png";
        private const string RecordIcon = "UIComponent/New/ic_record_nor.png";
        private const string StopRecordIcon = "UIComponent/New/ic_record_press.png";

        private const string PlayMusicIcon = "UIComponent/ic_load_music_nor.png";

        private bool UseCamera = false;
        private bool UseAccelerometer = false;

        // Record parameters
        private DateTime RecordPivot;
        private bool IsRecording = false;
        private List<uint> TimePivots = new List<uint>(); // milliseconds
        private List<int> RobotMove = new List<int>(); // 1: head shake, 2: head bow, 3: move foot        

        // Replay parameters
        private DispatcherTimer ReplayTimer;
        private DateTime ReplayPivot;
        private double ReplayTiming = 0;
        private int ReplayMoveCount;
        private bool IsReplaying = false;
        private bool StopReplay = false;
        private const int TickInterval = 100;

        // Play music?
        private bool IsPlayingMusic = false;

        // Shake parameters
        private const double ShakeThresholdX = 0.8;
        private const double ShakeThresholdZ = 0.6;
        private const int ShakeDelay = 250;
        private DateTime ShakeTimePivot;


        // Change speed parameters
        private bool HighSpeed = true;
        private const string HighSpeedIcon = "UIComponent/New/ic_change_speed_press.png";
        private const string LowSpeedIcon = "UIComponent/New/ic_change_speed_nor.png";

        private RobotState state;
        private enum RobotState
        {
            D1, D2, D3, D4, D5, D6, D7, D8
        }

        public List<Keyword> ChatKeywords = new List<Keyword>();
        #endregion

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
            if (MediaPlayer.State == MediaState.Playing) MediaPlayer.Pause();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            //base.OnBackKeyPress(e);
            if (LoadPopup.IsOpen)
            {
                LoadPopup.IsOpen = false;
                e.Cancel = true;
                return;
            }
            if (SavePopup.IsOpen)
            {
                SavePopup.IsOpen = false;
                e.Cancel = true;
                return;
            }
            if (HelpPopup.IsOpen)
            {
                HelpPopup.IsOpen = false;
                e.Cancel = true;
                return;
            }
            if (MusicPopup.IsOpen)
            {
                MusicPopup.IsOpen = false;
                e.Cancel = true;
                return;
            }
            if (ChatPopup.IsOpen)
            {
                ChatPopup.IsOpen = false;
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
            if (MediaPlayer.State == MediaState.Paused) MediaPlayer.Resume();

            if (ChatKeywords.Count == 0)
            {
                ChatKeywords = Keyword.LoadKeywordsFromFile("categories.txt");
            }
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => AccelerometerChanged(e));
        }

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

        private void GestureListener_OnPinchCompleted(object sender, PinchGestureEventArgs e)
        {
            if (UseAccelerometer || LoadPopup.IsOpen || SavePopup.IsOpen || HelpPopup.IsOpen
                || MusicPopup.IsOpen || ChatPopup.IsOpen) return;
            PlayChatSound("random");
        }
        private void GestureListener_OnFlick(object sender, FlickGestureEventArgs e)
        {
            if (UseAccelerometer || LoadPopup.IsOpen || SavePopup.IsOpen || HelpPopup.IsOpen
                || MusicPopup.IsOpen || ChatPopup.IsOpen) return;
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
            var uri = new Uri("/UIComponent/New/ic_switch_mode_press.png", UriKind.Relative);
            SwitchModeButtonBackground.ImageSource = new BitmapImage(uri);
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
                LoadPopup.IsOpen = true;
            }
            else
            {
                StopReplay = true;
            }
        }

        private void MusicPlayButton_OnClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsPlayingMusic)
            {
                MusicList.ItemsSource = LoadMusicList();
                MusicPopup.IsOpen = true;
            }
            else
            {
                var uri = new Uri(PlayMusicIcon, UriKind.Relative);
                MusicPlayButtonBackground.ImageSource = new BitmapImage(uri);
                if (MediaPlayer.State != MediaState.Stopped) MediaPlayer.Stop();
                IsPlayingMusic = false;
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
                //if (ReplayTiming - time < TickInterval && ReplayTiming - time >= 0)
                if (Math.Abs(ReplayTiming - time) <= TickInterval / 2)
                {
                    Debug.WriteLine(ReplayTiming);
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

        private void SaveRecord(string filename)
        {
            var saveFileName = "Record\\" + filename;
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (myIsolatedStorage.FileExists(saveFileName)) myIsolatedStorage.DeleteFile(saveFileName);
                if (!myIsolatedStorage.DirectoryExists("Record"))
                {
                    myIsolatedStorage.CreateDirectory("Record");
                }
                using (
                    var writeFile =
                        new StreamWriter(new IsolatedStorageFileStream(saveFileName, FileMode.Create, FileAccess.Write,
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
            var storeList = new List<string>();
            if (!isoFile.DirectoryExists("Record"))
            {
                return storeList;
            }
            var list = isoFile.GetFileNames("./Record/*.*");
            foreach (var file in list)
            {
                var fileWithoutExtension = Path.GetFileNameWithoutExtension(file);
                storeList.Add(fileWithoutExtension);
            }
            return storeList;
        }

        private void LoadRecord(string filename)
        {
            TimePivots.Clear();
            RobotMove.Clear();
            var saveFileName = "Record\\" + filename;
            IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            if (!myIsolatedStorage.FileExists(saveFileName)) return;
            IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(saveFileName, FileMode.Open, FileAccess.Read);
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

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            SavePopup.IsOpen = false;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
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

        private void Delete_OnClick(object sender, RoutedEventArgs e)
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


        private void CancelLoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            LoadPopup.IsOpen = false;
        }

        private void LoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            LoadPopup.IsOpen = false;
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

        private void CloseHelpButton_OnClick(object sender, RoutedEventArgs e)
        {
            HelpPopup.IsOpen = false;
        }

        private void ShowTutorialButton_OnClick(object sender, RoutedEventArgs e)
        {
            HelpPopup.IsOpen = false;
            var uri = new Uri("/Tutorial/Tutorial1.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void HelpButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var uri = new Uri("UIComponent/New/ic_help_press.png", UriKind.Relative);
            HelpButton.Fill = new ImageBrush()
            {
                ImageSource = new BitmapImage(uri)
            };
            //HelpPopup.IsOpen = true;
            var uriPage = new Uri("/TutorialPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uriPage);
        }

        private void CancelMusicLoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            MusicPopup.IsOpen = false;
        }

        private void PlayMusicButton_OnClick(object sender, RoutedEventArgs e)
        {
            MusicPopup.IsOpen = false;
            if (MusicList.SelectedItem == null) return;
            string chosenMusic = MusicList.SelectedItem.ToString();
            using (var library = new MediaLibrary())
            {
                foreach (var song in library.Songs)
                {
                    if (song.Name == chosenMusic)
                    {
                        FrameworkDispatcher.Update();
                        MediaPlayer.Play(song);
                        var uri = new Uri(StopIcon, UriKind.Relative);
                        MusicPlayButtonBackground.ImageSource = new BitmapImage(uri);
                        IsPlayingMusic = true;
                        return;
                    }
                }
            }
        }

        private List<string> LoadMusicList()
        {
            var listSong = new List<string>();
            using (var library = new MediaLibrary())
            {
                listSong.AddRange(library.Songs.Select(song => song.Name));
            }
            return listSong;
        }

        private void SwitchModeButton_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var uri = new Uri("/UIComponent/New/ic_switch_mode_nor.png", UriKind.Relative);
            SwitchModeButtonBackground.ImageSource = new BitmapImage(uri);
        }

        private void HelpButton_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var uri = new Uri("UIComponent/New/ic_help_nor.png", UriKind.Relative);
            HelpButton.Fill = new ImageBrush()
                                  {
                                      ImageSource = new BitmapImage(uri)
                                  };
        }

        private void PlayChatSound(string category)
        {
            switch (category.ToLower())
            {
                case "greeting":
                    PlaySound(GreetingSound);
                    break;
                case "feeling":
                    PlaySound(FeelingSound);
                    break;
                case "encouragement":
                    PlaySound(EncouragementSound);
                    break;
                case "activities answer":
                    PlaySound(ActivitiesAnswerSound);
                    break;
                case "activities yes / no":
                    PlaySound(ActivitiesYesNoSound);
                    break;
                case "love and friendship and compliments":
                    PlaySound(LoveAndFriendshipSound);
                    break;
                case "bad words":
                    PlaySound(DirtyWordsSound);
                    break;
                case "food and drinks":
                    PlaySound(FoodAndDrinksSound);
                    break;
                case "yes/no":
                    PlaySound(YesNoSound);
                    break;
                case "popular places":
                    PlaySound(PopularPlacesSound);
                    break;
                case "self introduction":
                    PlaySound(SelfIntroductionSound);
                    break;
                case "cartoon":
                    PlaySound(CartoonSound);
                    break;
                case "celebrities":
                    PlaySound(CelebritiesSound);
                    break;
                case "animals":
                    PlaySound(AnimalsSound);
                    break;
                case "random":
                    PlaySound(RandomSound);
                    break;
                default:
                    PlaySound(UnknownSound);
                    break;
            }
        }

        private void ImageCanvas_OnDoubleTap(object sender, GestureEventArgs e)
        {
            ChatPopup.IsOpen = true;
            ChatTextBox.Focus();
        }

        private void TalkButton_OnClick(object sender, RoutedEventArgs e)
        {
            string chat = ChatTextBox.Text.ToLower();
            if (string.IsNullOrWhiteSpace(chat)) return;
            foreach (var keyword in ChatKeywords)
            {
                if (chat.Contains(keyword.Word))
                {
                    PlayChatSound(keyword.Category);
                    return;
                }
            }
            PlayChatSound("Unknown");
        }

        private void LayoutRoot_OnDoubleTap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            ChatPopup.IsOpen = true;
            ChatTextBox.Focus();
        }
    }
}