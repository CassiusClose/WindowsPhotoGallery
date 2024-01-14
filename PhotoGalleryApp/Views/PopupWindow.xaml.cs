using PhotoGalleryApp.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
            SetBinding(UserAcceptedProperty, new Binding("Accepted"));
        }


        public static readonly DependencyProperty PopupOpenProperty = DependencyProperty.Register("Open", typeof(bool), typeof(PopupWindow),
            new FrameworkPropertyMetadata { PropertyChangedCallback = PopupOpenChanged });

        /// <summary>
        /// Whether the popup window should be open or not. If false, then the
        /// window will close.
        /// </summary>
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


        public static readonly DependencyProperty UserAcceptedProperty = DependencyProperty.Register("UserAccepted", typeof(bool), typeof(PopupWindow),
            new PropertyMetadata(UserAcceptedPropertyChanged));

        /// <summary>
        /// Whether the user has accepted or cancelled the popup. Setting this
        /// will set Window.DialogResult, which is passed to the ViewModels.
        /// </summary>
        public bool UserAccepted
        {
            get { return (bool)GetValue(UserAcceptedProperty); }
            set { 
                SetValue(UserAcceptedProperty, value);
                DialogResult = value;
            }
        }

        private static void UserAcceptedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PopupWindow window = (PopupWindow)d;
            window.DialogResult = window.UserAccepted;
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Escape cancels the popup
            if (e.Key == Key.Escape)
            {
                PopupViewModel vm = (PopupViewModel)DataContext;
                vm.ClosePopup(false);
            }

            // Enter accepts the popup
            if(e.Key == Key.Enter)
            {
                PopupViewModel vm = (PopupViewModel)DataContext;
                vm.ClosePopup(true);
            }
        }
    }
}
