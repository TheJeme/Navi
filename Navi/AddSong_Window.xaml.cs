using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

using YoutubeExplode;
using YoutubeExplode.Converter;

namespace Navi
{
    /// <summary>
    /// Interaction logic for AddSong_Window.xaml
    /// </summary>
    public partial class AddSong_Window : Window
    {
        private YoutubeClient youtube;
        private YoutubeConverter youtubeConverter;

        public AddSong_Window()
        {
            youtube = new YoutubeClient();
            youtubeConverter = new YoutubeConverter(youtube);

            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Closes the window
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string youtubeID = songLabel.Text;

            if (youtubeID.Length == 0) this.Close(); // Youtube URL/ID can't be null.

            try // Tries to download audio if valid url/id.
            {
                var video = await youtube.Videos.GetAsync(youtubeID);
                var cleanTitle = CleanTitle(video.Title); // Cleans illegal characters to bypass errors
                var duration = video.Duration;
                var cleanDuration = CleanDuration(duration); // Converts duration to legal filename e.g (00.37.12) instead of (00:37:12)
                CheckLibraryStatus(); // Check that root directory exists

                if (File.Exists($"./library/{(this.Owner as MainWindow).libraryListView.SelectedValue.ToString()}/{cleanTitle} - {cleanDuration}.mp3"))
                {
                    MessageBox.Show("The song is already in the library", "Error");
                    return;
                }

                MainWindow.currentlyViewingMusicList.Add(new MusicList { Filename = $"{cleanTitle} - {cleanDuration}.mp3", Title = cleanTitle, Duration = duration });
                var destinationPath = Path.Combine($"./library/{(this.Owner as MainWindow).libraryListView.SelectedValue.ToString()}/{cleanTitle} - {cleanDuration}.mp3");

                okButton.IsEnabled = false; // Disables buttons to notify user and to keep errors away.
                closeButton.IsEnabled = false;
                songLabel.IsEnabled = false;
                downloadingLabel.Visibility = Visibility.Visible;

                await youtubeConverter.DownloadVideoAsync(youtubeID, destinationPath);
                //await DownloadAudio(youtubeID, destinationPath); // Waits till download is complete and then continues the code.

                MainWindow.currentlyPlayingMusicList = new List<MusicList>(MainWindow.currentlyViewingMusicList);
                (this.Owner as MainWindow).musicListView.Items.Refresh();
                (this.Owner as MainWindow).libraryListView.Items.Refresh(); // Refresh libraryListView to make sure song is added to list and is visible.
            }
            catch (IOException)
            {
                MessageBox.Show("The song is already in the library", "Error");
            }
            catch (InvalidOperationException er)
            {
                MessageBox.Show(er.ToString(), "error");
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Invalid Youtube video id or url.", "Error");
            }
            catch (Exception er)
            {
                MessageBox.Show("Invalid Youtube video id or url.", er.ToString());
            }

            this.Close(); // Lastly closes the window
        }


        private void CheckLibraryStatus()
        {
            if (!Directory.Exists("./library"))
            {
                Directory.CreateDirectory("./library");
            }
        }

        private string CleanDuration(TimeSpan duration)
        {
            return duration.ToString().Replace(':', '.');
        }

        private string CleanTitle(string title)
        {
            return string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
        }

        private async Task DownloadAudio(string youtubeID, string destinationPath)
        {
            await youtubeConverter.DownloadVideoAsync(youtubeID, destinationPath);            
        }
    }
}
