using System;
using System.IO;
using System.Linq;
using System.Net;
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

                (this.Owner as MainWindow).imgThumbnail.Source = new BitmapImage(new Uri(video.Thumbnails.StandardResUrl));

                CheckLibraryStatus();
                MainWindow.musicList.Add(video);
                (this.Owner as MainWindow).musicListView.Items.Refresh();
                var destinationPath = Path.Combine("./library/test1/", $"{video.Title}.mp3");
                DownloadImageAndAudio(youtubeID, destinationPath, video);

                (this.Owner as MainWindow).libraryListView.Items.Refresh();
            }
            catch (IOException)
            {
                MessageBox.Show("The song is already in the library", "Error");
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("SOMETHING HAPPANED", "Error"); //TODO: FIX ERRORGHANDLING
            }
            //catch (Exception er)
            //{
            //    MessageBox.Show("Invalid Youtube video id or url.", er.ToString());
            //}

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

        private async void DownloadImageAndAudio(string youtubeID, string destinationPath, YoutubeExplode.Videos.Video video)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFileAsync(new Uri(video.Thumbnails.StandardResUrl), $"./library/test1/{video.Title}.png");
            }
            await youtubeConverter.DownloadVideoAsync(youtubeID, destinationPath);
        }
    }
}
