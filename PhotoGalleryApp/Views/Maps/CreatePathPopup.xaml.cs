using PhotoGalleryApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoGalleryApp.Views.Maps
{
    /// <summary>
    /// Interaction logic for CreatePathPopup.xaml
    /// </summary>
    public partial class CreatePathPopup : UserControl
    {
        public CreatePathPopup()
        {
            InitializeComponent();
        }

        /**
         * Prompt the user to pick a file that contains path data.
         */
        private void LoadFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text documents (.txt)|*.txt";

            bool? result = dialog.ShowDialog();

            // If the user picked a file
            if(result != null && result == true)
            {
                string filename = dialog.FileName;

                CreatePathPopupViewModel vm = (CreatePathPopupViewModel)DataContext;
                vm.Filename = filename;
            }
        }
    }
}
