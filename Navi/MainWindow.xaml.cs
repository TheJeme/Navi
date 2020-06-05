using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Threading;
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

        List<string> saa = new List<string>();
        List<YoutubeExplode.Videos.Video> musicList = new List<YoutubeExplode.Videos.Video>();
        
        private YoutubeClient youtube;
        private YoutubeConverter youtubeConverter;
        private MediaPlayer mediaPlayer;


        private bool isPlayingAudio;
        private bool isLooping;
        private bool isTimerEnabled;

        private readonly string youtubeID = "https://www.youtube.com/watch?v=_JL33DgClrI&list=RD_JL33DgClrI&start_radio=1";

        public MainWindow()
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += dtTicker;
            dt.Start();

            isPlayingAudio = false;

            youtube = new YoutubeClient();
            youtubeConverter = new YoutubeConverter(youtube);

            mediaPlayer = new MediaPlayer();

            saa.Add("Content1");
            saa.Add("Content2");
            saa.Add("Content3");

            InitializeComponent();

            libraryListView.ItemsSource = saa;
            musicListView.ItemsSource = musicList;
        }

        private void dtTicker(object sender, EventArgs e)
        {      
            if (isTimerEnabled)
            {
                audioPositionLabel.Content = $"{mediaPlayer.Position.ToString().Split('.')[0]} / {musicList[0].Duration}";
            }
            buttonSearch.Content = (musicList.Count);
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
                imgThumbnail.Source = new BitmapImage(new Uri(video.Thumbnails.StandardResUrl));

                checkLibraryStatus();
                musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video); musicList.Add(video);
                musicListView.Items.Refresh();
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

        private async void downloadImageAndAudio(string youtubeID, string destinationPath, YoutubeExplode.Videos.Video video)
        {
           using (WebClient webClient = new WebClient())
           { 
               webClient.DownloadFileAsync(new Uri(video.Thumbnails.StandardResUrl), $"./library/test1/{video.Title}.png");
           }
           await youtubeConverter.DownloadVideoAsync(youtubeID, destinationPath);
        }

        private void playButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!isPlayingAudio)
            {
                mediaPlayer.Open(new Uri(Environment.CurrentDirectory + "/library/test1/Hanatan - Ghost Rule.mp3")); //TODO: MOVE FROM HERE
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
    }
}
