using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using YoutubeExplode;
using YoutubeExplode.Converter;


namespace Navi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /**
        private YoutubeClient youtube;
        private YoutubeConverter youtubeConverter;
        private MediaPlayer snd;        

        private bool isPlayingAudio;

        private readonly string youtubeID = "https://www.youtube.com/watch?v=_JL33DgClrI&list=RD_JL33DgClrI&start_radio=1";

        public MainWindow()
        {
            InitializeComponent();

            isPlayingAudio = false;

            youtube = new YoutubeClient();
            youtubeConverter = new YoutubeConverter(youtube);
        }

        private void checkLibraryStatus()
        {
            if (!Directory.Exists("./library"))
            {
                Directory.CreateDirectory("./library");
            }
        }

        private async void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            //if (txtBox.Text == "") return; TODO: COMMENT THIS OUT

            try
            {
                var video = await youtube.Videos.GetAsync(youtubeID);
                var title = cleanTitle(video.Title);
                labelResult.Content = title;
                imgThumbnail.Source = new BitmapImage(new Uri(video.Thumbnails.StandardResUrl));

                checkLibraryStatus();

                var destinationPath = Path.Combine("./library/test1/", $"{title}.mp3");
                downloadImageAndAudio(youtubeID, destinationPath, video);

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
        }

        private string cleanTitle(string title)
        {
            return string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlayingAudio)
            {
                snd = new MediaPlayer();
                snd.Volume = 0.05f;
                snd.Open(new Uri(Environment.CurrentDirectory + "/library/test1/Hanatan - Ghost Rule.mp3"));
                snd.Play();
                isPlayingAudio = true;
            }
        }


        private async void downloadImageAndAudio(string youtubeID, string destinationPath, YoutubeExplode.Videos.Video video)
        {
            using (WebClient webClient = new WebClient())
            { 
                webClient.DownloadFileAsync(new Uri(video.Thumbnails.StandardResUrl), $"./library/test1/{video.Title}.png");
            }
            await youtubeConverter.DownloadVideoAsync(youtubeID, destinationPath);
        }**/
    }
}
