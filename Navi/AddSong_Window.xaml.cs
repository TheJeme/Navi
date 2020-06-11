using System;
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

            if (youtubeID.Length == 0) this.Close(); // Youtube URL/ID can't be blank.

            try // Tries to download audio if valid url/id.
            {
                var video = await youtube.Videos.GetAsync(youtubeID);
                var title = CleanTitle(video.Title); // Cleans illegal characters to bypass errors
                CheckLibraryStatus(); // Check that root directory exists
                MainWindow.currentlyViewingMusicList.Add(new MusicList { Title = video.Title, Duration = video.Duration});
                (this.Owner as MainWindow).musicListView.Items.Refresh();
                var destinationPath = Path.Combine($"./library/{(this.Owner as MainWindow).libraryListView.SelectedValue.ToString()}/", $"{title}.mp3");

                okButton.IsEnabled = false; // Disables buttons to alert user.
                closeButton.IsEnabled = false;
                songLabel.IsEnabled = false;
                downloadingLabel.Visibility = Visibility.Visible;


                await DownloadAudio(youtubeID, destinationPath, video); // Waits till download is complete and then continues the code.

                (this.Owner as MainWindow).libraryListView.Items.Refresh(); // Refresh libraryListView to make sure song is added to list and is visible.
            }
            catch (IOException)
            {
                MessageBox.Show("The song is already in the library", "Error");
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("SOMETHING HAPPANED", "Error");
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

        private string CleanTitle(string title)
        {
            return string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
        }

        private async Task DownloadAudio(string youtubeID, string destinationPath, YoutubeExplode.Videos.Video video)
        {
            await youtubeConverter.DownloadVideoAsync(youtubeID, destinationPath);
        }
    }
}
