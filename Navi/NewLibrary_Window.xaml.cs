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
        public NewLibrary_Window()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var libraryName = LibNameLabel.Text;
            if (window.Title == "Create new Library")
            {
                if (libraryName.Length == 0) return;

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
            else
            {
                if (libraryName.Length == 0) return;

                //TODO

                MainWindow.libraryList[(this.Owner as MainWindow).libraryListView.SelectedIndex] = libraryName;
                (this.Owner as MainWindow).libraryListView.Items.Refresh();
                this.Close();
            }
        }
    }
}
