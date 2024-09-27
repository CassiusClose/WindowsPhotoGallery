using PhotoGalleryApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PhotoGalleryApp.Views
{
    /// <summary>
    /// Interaction logic for FolderTree.xaml
    /// </summary>
    public partial class FolderTree : UserControl
    {
        public FolderTree()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<FolderViewModel>), typeof(FolderTree));

        public ObservableCollection<EventFolderViewModel> ItemsSource
        {
            get { return (ObservableCollection<EventFolderViewModel>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
