using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using NAudio.Wave;

namespace Navi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static List<string> libraryList = new List<string>();
        public static List<MusicList> musicList = new List<MusicList>();
        
        private MediaPlayer mediaPlayer;


        private bool isPlayingAudio;
        private bool isLooping;
        private bool isTimerEnabled;

        public MainWindow()
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += dtTicker;
            dt.Start();

            isPlayingAudio = false;

            mediaPlayer = new MediaPlayer();

            CheckLibraryStatus();
            InitializeComponent();

            libraryListView.ItemsSource = libraryList;
            musicListView.ItemsSource = musicList;
        }

        private void dtTicker(object sender, EventArgs e) // Updates Audioposition every second.
        {      
            if (isTimerEnabled)
            {
                audioPositionLabel.Content = $"{mediaPlayer.Position.ToString().Split('.')[0]} / {musicList[0].Duration}"; 
            }
        }

        private void CheckLibraryStatus()
        {
            if (!Directory.Exists("./library"))
            {
                Directory.CreateDirectory("./library");
            }

            foreach (var directoryPath in Directory.GetDirectories("./library/"))
            {
                libraryList.Add(new DirectoryInfo(directoryPath).Name);
            }
        }

        private void CheckSongStatus()
        {
            foreach (var directoryPath in Directory.GetFiles($"./library/{libraryListView.SelectedValue.ToString()}/"))
            {
                Console.WriteLine(directoryPath);
                if (!directoryPath.EndsWith(".mp3")) continue; // Does not include anyother type files than mp3.

                Mp3FileReader reader = new Mp3FileReader($"{directoryPath}");
                string duration = reader.TotalTime.ToString().Split('.')[0]; // Splits duration and takes milliseconds off.
                musicList.Add(new MusicList { Title = new DirectoryInfo(directoryPath).Name.ToString(), Duration = duration });
            }
            musicListView.Items.Refresh();
        }

        private void PlayButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (musicListView.SelectedItem == null) return; // If not selected any item in the list then can't play it.

            if (!isPlayingAudio)
            {
                mediaPlayer.Open(new Uri(Environment.CurrentDirectory + $"/library/{libraryListView.SelectedValue.ToString()}/{musicList[0].Title}"));
                mediaPlayer.Play();
                
                isTimerEnabled = true;
                isPlayingAudio = true;
                playIcon.Source = new BitmapImage(new Uri("./Resources/pause.png", UriKind.Relative));
            }
            else
            {
                mediaPlayer.Pause();
                isTimerEnabled = false;
                isPlayingAudio = false;
                playIcon.Source = new BitmapImage(new Uri("./Resources/play.png", UriKind.Relative));
            }
        }

        private void VolumeButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (volumePanel.IsVisible)
            {
                volumePanel.Visibility = Visibility.Collapsed;
                volumeButton.Background = Brushes.Gray;
            }
            else
            {
                volumePanel.Visibility = Visibility.Visible;
                volumeButton.Background = Brushes.AliceBlue;
            }
        }

        private void LoopButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isLooping)
            {
                loopButton.Background = Brushes.Gray;
                isLooping = false;
            }
            else
            {
                loopButton.Background = Brushes.AliceBlue;
                isLooping = true;
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = volumeSlider.Value;

            if (volumeSlider.Value == 0)
            {
                volumeIcon.Source = new BitmapImage(new Uri("./Resources/volume-mute.png", UriKind.Relative));
            }
            else if (volumeSlider.Value < volumeSlider.Maximum / 2)
            {
                volumeIcon.Source = new BitmapImage(new Uri("./Resources/volume-low.png", UriKind.Relative));
            }
            else
            {
                volumeIcon.Source = new BitmapImage(new Uri("./Resources/volume-high.png", UriKind.Relative));
            }
        }

        private void NewLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            var newLibWin = new NewLibrary_Window();
            newLibWin.Owner = this;
            newLibWin.Show();
        }

        private void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            var addSongWin = new AddSong_Window();
            addSongWin.Owner = this;
            addSongWin.Show();
        }

        private void LibraryListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            addSongButton.IsEnabled = true;

            CheckSongStatus();
        }

        private void MusicListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Console.WriteLine("MusicListView_SelectionChanged");
        }

        private void MoveUpLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (libraryListView.SelectedIndex == 0) return;

            var item = libraryList[libraryListView.SelectedIndex];
            libraryList.RemoveAt(libraryListView.SelectedIndex);
            libraryList.Insert(libraryListView.SelectedIndex - 1, item);
            libraryListView.Items.Refresh();
        }

        private void MoveDownLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (libraryListView.SelectedIndex == libraryList.Count - 1) return;

            var item = libraryList[libraryListView.SelectedIndex];
            libraryList.RemoveAt(libraryListView.SelectedIndex);
            libraryList.Insert(libraryListView.SelectedIndex + 1, item);
            libraryListView.Items.Refresh();
        }

        private void RenameLibrary_Click(object sender, RoutedEventArgs e)
        {
            var newLibWin = new NewLibrary_Window();
            newLibWin.Owner = this;
            newLibWin.Title = "Edit Library name";
            newLibWin.Show();
        }

        private void DeleteLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists("./library/" + libraryListView.SelectedValue.ToString())) return;

            if (Directory.GetFileSystemEntries("./library/" + libraryListView.SelectedValue.ToString()).Length == 0)
            {
                Directory.Delete("./library/" + libraryListView.SelectedValue.ToString(), true);
                libraryList.RemoveAt(libraryListView.SelectedIndex);
                libraryListView.Items.Refresh();
                addSongButton.IsEnabled = false;
            }
            else
            {
                if (MessageBox.Show("Are you sure you want to delete the library?", "Navi", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
                else
                {
                    Directory.Delete("./library/" + libraryListView.SelectedValue.ToString(), true);
                    libraryList.RemoveAt(libraryListView.SelectedIndex);
                    libraryListView.Items.Refresh();
                    addSongButton.IsEnabled = false;
                }
            }
        }

        private void PlayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            playMenuItem.IsEnabled = false;
            pauseMenuItem.IsEnabled = true;

            mediaPlayer.Play();
        }

        private void PauseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            playMenuItem.IsEnabled = true;
            pauseMenuItem.IsEnabled = false;

            mediaPlayer.Pause();
        }

        private void SkipForwardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }

        private void SkipBackwardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }

        private void LoopMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (loopMenuItem.IsChecked)
            {
                loopMenuItem.IsChecked = false;
            }
            else
            {
                loopMenuItem.IsChecked = true;
            }
        }

        private void Volume100MenuItem_Click(object sender, RoutedEventArgs e)
        {
            volumeSlider.Value = volumeSlider.Maximum;
        }

        private void Volume75MenuItem_Click(object sender, RoutedEventArgs e)
        {
            volumeSlider.Value = volumeSlider.Maximum * 0.75f;
        }

        private void Volume50MenuItem_Click(object sender, RoutedEventArgs e)
        {
            volumeSlider.Value = volumeSlider.Maximum * 0.5f;
        }

        private void Volume25MenuItem_Click(object sender, RoutedEventArgs e)
        {
            volumeSlider.Value = volumeSlider.Maximum * 0.25f;
        }

        private void Volume0MenuItem_Click(object sender, RoutedEventArgs e)
        {
            volumeSlider.Value = 0;
        }
    }
}
