using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

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
            this.Close();
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string youtubeID = songLabel.Text;

            if (youtubeID.Length == 0) this.Close();

            try
            {
                var video = await youtube.Videos.GetAsync(youtubeID);
                var title = CleanTitle(video.Title); // Cleans illegal characters to bypass errors
                CheckLibraryStatus(); // Check that root directory exists
                MainWindow.currentlyViewingMusicList.Add(new MusicList { Title = video.Title, Duration = video.Duration});
                (this.Owner as MainWindow).musicListView.Items.Refresh();
                var destinationPath = Path.Combine($"./library/{(this.Owner as MainWindow).libraryListView.SelectedValue.ToString()}/", $"{title}.mp3");
                await DownloadAudio(youtubeID, destinationPath, video);

                (this.Owner as MainWindow).libraryListView.Items.Refresh(); // Refresh libraryListView to make sure song is added to list and is visible.
            }
            catch (IOException)
            {
                MessageBox.Show("The song is already in the library", "Error");
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("SOMETHING HAPPANED", "Error"); //TODO: FIX ERRORGHANDLING
            }
            catch (Exception er)
            {
                MessageBox.Show("Invalid Youtube video id or url.", er.ToString());
            }

            this.Close();
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
