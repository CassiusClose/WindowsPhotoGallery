using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace PhotoGalleryApp.Views
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window
    {
        public PopupWindow()
        {
            InitializeComponent();

            Loaded += PopupWindow_Loaded;
        }

        private void PopupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetBinding(PopupOpenProperty, new Binding("Open"));
        }


        public static readonly DependencyProperty PopupOpenProperty = DependencyProperty.Register("Open", typeof(bool), typeof(PopupWindow),
            new FrameworkPropertyMetadata { PropertyChangedCallback = PopupOpenChanged });

        public bool Open
        {
            get { return (bool)GetValue(PopupOpenProperty); }
            set { SetValue(PopupOpenProperty, value); }
        }


        private static void PopupOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PopupWindow window = (PopupWindow)d;
            if(window.Open == false)
                window.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }
    }
}
