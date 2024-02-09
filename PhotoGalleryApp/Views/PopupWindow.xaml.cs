using PhotoGalleryApp.ViewModels;
using PhotoGalleryApp.Views.Behavior;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
            closeButtonVis = new CloseButtonVisibility(this);

            SetBinding(PopupOpenProperty, new Binding("Open"));
            SetBinding(UserAcceptedProperty, new Binding("Accepted"));

            SetBinding(AllowCloseProperty, new Binding("AllowClose"));
            if (AllowClose)
                closeButtonVis.EnableCloseButton();
            else
                closeButtonVis.DisableCloseButton();
        }



        #region Popup Open Property

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

        #endregion Popup Open Property


        #region User Accepted Property

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

        #endregion User Accepted Property


        #region Allow Close Property

        public static readonly DependencyProperty AllowCloseProperty = DependencyProperty.Register("AllowClose", typeof(bool), typeof(PopupWindow), 
            new FrameworkPropertyMetadata(AllowClosePropertyChanged));

        private static void AllowClosePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PopupWindow window = (PopupWindow)d;
            if(window.AllowClose)
                window.closeButtonVis.EnableCloseButton();
            else 
                window.closeButtonVis.DisableCloseButton();
        }

        // Whether the user should be allowed to close the popup-box, or only
        // the program. If this is false, the close button will be disabled,
        // and any shortcuts to close the box (like Alt-f4, etc.) will not
        // work.
        public bool AllowClose
        {
            get { return (bool)GetValue(AllowCloseProperty); }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!AllowClose)
                e.Cancel = true;
        }

        // Handles the windows menu interaction
        private CloseButtonVisibility closeButtonVis;


        #endregion  Allow Close Property



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
