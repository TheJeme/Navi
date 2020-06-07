using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace Navi
{
    /// <summary>
    /// Interaction logic for NewLibrary_Window.xaml
    /// </summary>
    public partial class NewLibrary_Window : Window
    {
        string oldDir;

        public NewLibrary_Window(string oldDir)
        {
            InitializeComponent();

            this.oldDir = oldDir;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (LibNameLabel.Text.Length == 0) return;

            var libraryName = LibNameLabel.Text;

            if (window.Title == "Create new Library")
            {

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
            else // Rename directory
            {

                Directory.Move("./library/" + oldDir, "./library/" + libraryName); // Renames the directory.

                MainWindow.libraryList[(this.Owner as MainWindow).libraryListView.SelectedIndex] = libraryName;
                (this.Owner as MainWindow).libraryListView.Items.Refresh();
                this.Close();
            }
        }
    }
}
