using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Navi.Properties;

using NAudio.Wave;

namespace Navi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static List<string> libraryList = new List<string>();
        public static List<MusicList> currentlyPlayingMusicList = new List<MusicList>();
        public static List<MusicList> currentlyViewingMusicList = new List<MusicList>();
        
        private MediaPlayer mediaPlayer;

        private bool isPlayingAudio;
        private bool isLooping;

        private int currentPlayingIndex;

        public MainWindow()
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += dtTicker;
            dt.Start();

            isPlayingAudio = false;
            isLooping = false;

            mediaPlayer = new MediaPlayer();

            CheckLibraryStatus();
            InitializeComponent();

            try
            {
                volumeSlider.Value = Convert.ToDouble(Settings.Default["volume"]);
            }
            catch (Exception)
            {
                volumeSlider.Value = volumeSlider.Maximum / 2;
                Settings.Default["volume"] = volumeSlider.Value.ToString();
                Settings.Default.Save();
            }
            libraryListView.ItemsSource = libraryList;
            musicListView.ItemsSource = currentlyViewingMusicList;
        }


        // Updates AudiopositionLabel and AudiopositionSlider every second.
        // If song is completed then loops or skips forward.
        private void dtTicker(object sender, EventArgs e) 
        {
            if (isPlayingAudio)
            {
                audioPositionLabel.Content = $"{mediaPlayer.Position.ToString(@"hh\:mm\:ss")} / {currentlyPlayingMusicList[currentPlayingIndex].Duration}";
                audioPositionSlider.Value = mediaPlayer.Position.TotalSeconds;

                if (mediaPlayer.Position >= currentlyPlayingMusicList[currentPlayingIndex].Duration)
                {
                    mediaPlayer.Pause();
                    audioPositionSlider.Value = 0; // Reset slider and label values back to zero.
                    audioPositionLabel.Content = $"{mediaPlayer.Position.ToString(@"hh\:mm\:ss")} / {currentlyPlayingMusicList[currentPlayingIndex].Duration}";

                    if (isLooping)
                    {
                        mediaPlayer.Play();
                    }
                    else
                    {
                        SkipForward();
                    }
                }
            }
        }


        // Check for music libraries in root library directory.
        private void CheckLibraryStatus() 
        {
            if (!Directory.Exists("./library")) // Creates a root directory for all music libraries.
            {
                Directory.CreateDirectory("./library");
            }
            var di = new DirectoryInfo("./library");
            var directories = di.EnumerateDirectories() // Gets directories in date added order
                                .OrderBy(d => d.CreationTime)
                                .Select(d => d.Name)
                                .ToList();
            foreach (var dir in directories)
            {
                libraryList.Add(dir);
            }
        }

        // Check all songs in selected library.
        public void CheckSongStatus()
        {
            currentlyViewingMusicList.Clear();
            musicListView.Items.Refresh();

            string filePath = $"./library/{libraryListView.SelectedValue.ToString()}/";
            FileInfo[] files = new DirectoryInfo(filePath) // Gets songs in date added order
                        .GetFiles("*.mp3")
                        .OrderBy(f => f.CreationTime)
                        .ToArray();

            foreach (var directoryPath in files)
            {
                try
                {
                    string titleAndDuration = directoryPath.Name.ToString();
                    string title = titleAndDuration.Substring(0, titleAndDuration.LastIndexOf('-') - 1);
                    string duration = titleAndDuration.Substring(titleAndDuration.LastIndexOf('-') + 2, 8).Replace('.', ':');

                    currentlyViewingMusicList.Add(new MusicList { Filename = $"{title} - {duration.Replace(':', '.')}.mp3", Title = title, Duration = TimeSpan.Parse(duration) });
                }

                catch (Exception) // Happens if file name doesn't include duration of song.
                {
                    Mp3FileReader reader = new Mp3FileReader($"{filePath}/{directoryPath}");
                    string title = directoryPath.Name.ToString().Remove(directoryPath.Name.ToString().Length - 4);
                    TimeSpan duration = TimeSpan.Parse(reader.TotalTime.ToString(@"hh\:mm\:ss"));                
                    currentlyViewingMusicList.Add(new MusicList { Filename = $"{title} - {duration.ToString().Replace(':', '.')}.mp3", Title = title, Duration = duration });
                    reader.Dispose();
                    File.Move($"{filePath}/{directoryPath}", $"{filePath}/{title} - {duration.ToString().Replace(':', '.')}.mp3");
                }
                musicListView.Items.Refresh();               
            }
        }

        //Loops the song
        private void Loop()
        {
            if (isLooping) // Enables looping
            {
                loopButton.Background = Brushes.Gray;
                loopMenuItem.IsChecked = false;
                isLooping = false;
            }
            else // Disables looping
            {
                loopButton.Background = Brushes.AliceBlue;
                loopMenuItem.IsChecked = true;
                isLooping = true;
            }
        }

        private void SkipForward()
        {
            if (currentPlayingIndex == currentlyPlayingMusicList.Count - 1) // If last song in list, then pause.
            {
                PauseAudio();
                audioPositionSlider.Value = 0; // Reset slider and label values back to zero.
                mediaPlayer.Position = TimeSpan.Zero;
                audioPositionLabel.Content = $"{mediaPlayer.Position.ToString(@"hh\:mm\:ss")} / {currentlyPlayingMusicList[currentPlayingIndex].Duration}";
                return;
            }

            currentPlayingIndex++;
            var mediaFile = new Uri(Environment.CurrentDirectory + $"/library/{libraryListView.SelectedValue.ToString()}/{currentlyPlayingMusicList[currentPlayingIndex].Filename}");
            mediaPlayer.Open(mediaFile);
            audioPositionSlider.Maximum = currentlyPlayingMusicList[currentPlayingIndex].Duration.TotalSeconds;
            if (isPlayingAudio)
            {
                mediaPlayer.Play();
            }
            currentPlayingLabel.Content = currentlyPlayingMusicList[currentPlayingIndex].Title;

            musicListView.SelectedItem = null;
        }

        private void SkipBackward()
        {
            if (currentPlayingIndex == 0) // If the first item, then doesn't accept to go negative.
            {
                PauseAudio();
                audioPositionSlider.Value = 0; // Reset slider and label values back to zero.
                mediaPlayer.Position = TimeSpan.Zero;
                audioPositionLabel.Content = $"{mediaPlayer.Position.ToString(@"hh\:mm\:ss")} / {currentlyPlayingMusicList[currentPlayingIndex].Duration}";
                return;
            }

            currentPlayingIndex--;
            var mediaFile = new Uri(Environment.CurrentDirectory + $"/library/{libraryListView.SelectedValue.ToString()}/{currentlyPlayingMusicList[currentPlayingIndex].Filename}");
            mediaPlayer.Open(mediaFile);
            audioPositionSlider.Maximum = currentlyPlayingMusicList[currentPlayingIndex].Duration.TotalSeconds;
            if (isPlayingAudio)
            {
                mediaPlayer.Play();
            }
            currentPlayingLabel.Content = currentlyPlayingMusicList[currentPlayingIndex].Title;

            musicListView.SelectedItem = null;
        }

        private void PlayAudio()
        {
            if (musicListView.Items.Count != 0 && musicListView.SelectedItem != null)
            {
                if (currentPlayingLabel.Content.ToString() != currentlyViewingMusicList[musicListView.SelectedIndex].Title)
                {

                    currentlyPlayingMusicList.Clear();
                    currentlyPlayingMusicList = new List<MusicList>(currentlyViewingMusicList);
                    currentPlayingIndex = musicListView.SelectedIndex;
                    currentPlayingLabel.Content = currentlyPlayingMusicList[currentPlayingIndex].Title;

                    var mediaFile = new Uri(Environment.CurrentDirectory + $"/library/{libraryListView.SelectedValue.ToString()}/{currentlyPlayingMusicList[currentPlayingIndex].Filename}");
                    mediaPlayer.Open(mediaFile);
                    audioPositionSlider.Maximum = currentlyPlayingMusicList[currentPlayingIndex].Duration.TotalSeconds;
                }
            }
            if (currentlyPlayingMusicList.Count == 0) return;

            mediaPlayer.Play();

            playMenuItem.IsEnabled = false;
            pauseMenuItem.IsEnabled = true;
            isPlayingAudio = true;
            playIcon.Source = new BitmapImage(new Uri("./Resources/pause.png", UriKind.Relative));
        }

        private void PauseAudio()
        {
            mediaPlayer.Pause();

            playMenuItem.IsEnabled = true;
            pauseMenuItem.IsEnabled = false;
            isPlayingAudio = false;
            playIcon.Source = new BitmapImage(new Uri("./Resources/play.png", UriKind.Relative));
        }

        private void PlayMedia()
        {
            if (!isPlayingAudio)
            {
                PlayAudio();
            }
            else
            {
                PauseAudio();
            }
        }

        private void PlayButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PlayMedia();
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
            Loop();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Settings.Default["volume"] = volumeSlider.Value.ToString();
            Settings.Default.Save();

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

        // For creating a music library
        private void NewLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            var newNameWin = new NewName_Window("Create new Library");
            newNameWin.Owner = this;
            newNameWin.ShowDialog();
        }

        // For renaming a music library
        private void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            var addSongWin = new AddSong_Window();
            addSongWin.Owner = this;
            addSongWin.ShowDialog();
        }

        // Checks songs in new selected library and updates them into currently viewing list.
        private void LibraryListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            addSongButton.IsEnabled = true; // So you can't add songs into null library, because nothing is selected.

            if (libraryListView.SelectedItem != null) // LibraryListView selected item can become unselected so just to be safe.
                CheckSongStatus();
        }

        private void MusicListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Console.WriteLine("MusicListView_SelectionChanged"); // Used only for debuggin purposes
        }

        // Changes the library order by one to up.
        private void MoveUpLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (libraryListView.SelectedIndex <= 0) return; // Can't move first item and handles pontetial error.

            var item = libraryList[libraryListView.SelectedIndex];
            libraryList.RemoveAt(libraryListView.SelectedIndex);
            libraryList.Insert(libraryListView.SelectedIndex - 1, item);
            libraryListView.Items.Refresh(); // Refresh the list to update the UI
        }

        // Changes the library order by one to down.
        private void MoveDownLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (libraryListView.SelectedIndex == libraryList.Count - 1 || libraryListView.SelectedIndex == -1) return; // Can't move last item

            var item = libraryList[libraryListView.SelectedIndex];
            libraryList.RemoveAt(libraryListView.SelectedIndex);
            libraryList.Insert(libraryListView.SelectedIndex + 1, item);
            libraryListView.Items.Refresh(); // Refresh the list to update the UI
        }

        private void RenameLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (libraryListView.SelectedIndex == -1) return;

            if (currentlyPlayingMusicList.SequenceEqual(currentlyViewingMusicList) && currentPlayingLabel.Content.ToString().Length != 0) // Can't rename currently playing list
            {
                MessageBox.Show("Can't rename library that is playing.", "Erorr");
            }
            else
            {
                var newNameWin = new NewName_Window("Rename Library", libraryListView.SelectedValue.ToString());
                newNameWin.Owner = this;
                newNameWin.ShowDialog();
            }
        }

        private void DeleteLibraryItem()
        {
            Directory.Delete("./library/" + libraryListView.SelectedValue.ToString(), true); // Deletes the directory and all the files inside it.
        
            libraryList.RemoveAt(libraryListView.SelectedIndex);
            currentlyViewingMusicList.Clear(); // Clears currently viewing list to ensure that errors get eliminated.
            musicListView.Items.Refresh();
            libraryListView.Items.Refresh();
            addSongButton.IsEnabled = false;
        }

        private void DeleteLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (libraryListView.SelectedIndex == -1) return;

            if (!Directory.Exists("./library/" + libraryListView.SelectedValue.ToString())) return; // Can't delete library that doesn't exist.

            if (currentlyPlayingMusicList.SequenceEqual(currentlyViewingMusicList) && currentPlayingLabel.Content.ToString().Length != 0) // Can't rename currently playing list
            {
                MessageBox.Show("Can't delete library that is playing.", "Erorr");
                return;
            }


            if (Directory.GetFileSystemEntries("./library/" + libraryListView.SelectedValue.ToString()).Length == 0) // If no songs in the list else warning
            {
                DeleteLibraryItem();
            }
            else
            {
                if (MessageBox.Show("Are you sure you want to delete the library?", "Navi", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
                else
                {
                    try
                    {
                        DeleteLibraryItem();
                    }
                    catch (InvalidOperationException)
                    {
                        MessageBox.Show("Error with deleting the library, try again.", "Error");
                    }
                }
            }
        }

        private void PlayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PlayMedia();
        }

        private void PauseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PlayMedia();
        }

        private void SkipForwardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SkipForward();
        }

        private void SkipBackwardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SkipBackward();
        }

        private void LoopMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Loop();
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

        private void SkipForward_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SkipForward();
        }

        private void SkipBackward_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SkipBackward();
        }

        private void AudioPositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (audioPositionSlider.IsFocused)
            {
                mediaPlayer.Position = TimeSpan.FromSeconds(audioPositionSlider.Value);
            }
        }

        private void PlaySongMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PlayAudio();
        }

        private void DeleteSongMenuItem_Click(object sender, RoutedEventArgs e)
        {       
            if (currentlyPlayingMusicList.Count == 0)
            {
                DeleteSongItem();
            }
            else
            {
                if (currentlyPlayingMusicList[musicListView.SelectedIndex].Equals(currentlyViewingMusicList[musicListView.SelectedIndex])
                    && currentlyViewingMusicList[musicListView.SelectedIndex].Title == currentPlayingLabel.Content.ToString()) // Can't delete already playing song
                {
                    MessageBox.Show("Can't delete song that is playing.", "Erorr");
                }
                else
                {
                    DeleteSongItem();
                }
            }
        }

        private void DeleteSongItem()
        {
            if (musicListView.SelectedIndex == -1) return;

            string songPath = $"./library/{libraryListView.SelectedValue.ToString()}/{currentlyViewingMusicList[musicListView.SelectedIndex].Filename}";

            if (currentlyPlayingMusicList.SequenceEqual(currentlyViewingMusicList) && currentPlayingLabel.Content.ToString().Length != 0)
            {
                currentlyPlayingMusicList.RemoveAt(musicListView.SelectedIndex);
            }

            currentlyViewingMusicList.RemoveAt(musicListView.SelectedIndex);
            musicListView.Items.Refresh();
            File.Delete(songPath);
        }
    }
}
