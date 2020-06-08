using System;
using System.IO;
using System.Windows;

namespace Navi
{
    /// <summary>
    /// Interaction logic for NewName_Window.xaml
    /// </summary>
    public partial class NewName_Window : Window
    {
        string selectedLibrary;
        string selectedSong;

        public NewName_Window(string winTitle)
        {
            InitializeComponent();

            window.Title = winTitle;
        }

        public NewName_Window(string winTitle, string selectedLibrary)
        {
            InitializeComponent();

            window.Title = winTitle;
            this.selectedLibrary = selectedLibrary;
        }

        public NewName_Window(string winTitle, string selectedLibrary, string selectedSong)
        {
            InitializeComponent();

            window.Title = winTitle;
            this.selectedLibrary = selectedLibrary;
            this.selectedSong = selectedSong;
        }

        private void CreateNewLibrary()
        {
            var libraryName = nameLabel.Text;

            if (!Directory.Exists("./library/" + libraryName))
            {
                MainWindow.libraryList.Add(libraryName);

                Directory.CreateDirectory("./library/" + libraryName);
                (this.Owner as MainWindow).libraryListView.Items.Refresh();
                this.Close();
            }
            else
            {
                MessageBox.Show("Library already exists with that name.", "Error");
            }
        }

        private void RenameLibrary()
        {
            if (nameLabel.Text == selectedLibrary) return;

            var libraryName = nameLabel.Text;

            if (!Directory.Exists("./library/" + libraryName))
            {
                Directory.Move("./library/" + selectedLibrary, "./library/" + libraryName); // Renames the directory.

                MainWindow.libraryList[(this.Owner as MainWindow).libraryListView.SelectedIndex] = libraryName;
                (this.Owner as MainWindow).libraryListView.Items.Refresh();
                this.Close();
            }
            else
            {
                MessageBox.Show("Library already exists with that name.", "Error");
            }
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (nameLabel.Text.Length == 0) return;

            switch (window.Title)
            {
                case ("Create new Library"):
                    CreateNewLibrary();
                    break;

                case ("Rename Library"):
                    RenameLibrary();
                    break;
            }
        }
    }
}
