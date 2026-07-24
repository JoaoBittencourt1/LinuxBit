using System.Windows;
using System.Windows.Input;
using LinuxHub.Features.Catalog.ViewModels;

namespace LinuxHub.Features.Catalog.Views
{
    public partial class ImageViewerWindow : Window
    {
        public ImageViewerWindow(string imagePath) : this(new ImageViewerViewModel(imagePath))
        {
        }

        public ImageViewerWindow(ImageViewerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += Close;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) =>
            ((ImageViewerViewModel)DataContext).CloseCommand.Execute(null);
    }
}
