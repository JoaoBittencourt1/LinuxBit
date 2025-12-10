using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LinuxHub // nome meio merda mas fazer oq 
{
    public partial class DistroWindow : Window
    {

        private string downloadLink;
        public DistroWindow(string name, string description, string imagePath, string link)
        {
            InitializeComponent();

            DistroName.Text = name;
            DistroDescription.Text = description;
            DistroImage.Source = new BitmapImage(new System.Uri(imagePath, System.UriKind.RelativeOrAbsolute)); // isso aqui ainda vai quebrar e eu não vou saber como

            downloadLink = link; // so link mudar em algum momento o problema não é meu
            DistroDownload.Text = link;
        }

        private void DistroDownload_Click(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(downloadLink))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadLink,
                        UseShellExecute = true
                    });
                }
                catch
                {
                    MessageBox.Show("Não foi possível abrir o link.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); // que isso nunca apareca
                }
            }
        }
    }
}
