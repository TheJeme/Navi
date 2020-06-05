using System;
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

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        { 
            if (window.Title == "Create new Library")
            {
                if (LibNameLabel.Text.Length == 0)
                    MainWindow.libraryList.Add($"New library {MainWindow.libraryList.Count + 1}");
                else
                    MainWindow.libraryList.Add(LibNameLabel.Text);
            }
            else
            {
                if (LibNameLabel.Text.Length == 0)
                    MainWindow.libraryList[(this.Owner as MainWindow).libraryListView.SelectedIndex] = ("Unnamed library");
                else
                    MainWindow.libraryList[(this.Owner as MainWindow).libraryListView.SelectedIndex] = LibNameLabel.Text;
            }
            (this.Owner as MainWindow).libraryListView.Items.Refresh();
            this.Close();
        }
    }
}
