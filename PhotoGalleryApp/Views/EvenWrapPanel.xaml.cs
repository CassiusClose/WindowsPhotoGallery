using PhotoGalleryApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoGalleryApp.Views
{
    /// <summary>
    /// Interaction logic for EvenWrapPanel.xaml
    /// </summary>
    public partial class EvenWrapPanel : UserControl
    {
        public EvenWrapPanel()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ICollectionView),
            typeof(EvenWrapPanel),
            new FrameworkPropertyMetadata
            {
                PropertyChangedCallback = ItemsSourcePropertyChanged
            }
        );
        /// <summary>
        /// The text that is in the control's TextBox
        /// </summary>
        public ICollectionView ItemsSource
        {
            get { return (ICollectionView)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        private static void ItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EvenWrapPanel control = (EvenWrapPanel)d;
            if (control != null)
                control.ItemsSourceChanged();
        }

        public void ItemsSourceChanged()
        {
            /*List<MediaViewModel> list = ItemsSource.Cast<MediaViewModel>().ToList();
            foreach(MediaViewModel vm in list)
            {
                vm.DisplayHeight = 200;
            }*/
        }



        /// <summary>
        /// An Event which gets triggered anytime the user chooses to select an item on the
        /// drop-down list or create a new item. Arguments are stored in ItemChosenEventArgs.
        /// </summary>
        public event EventHandler<PanelItemClickedEventArgs> ItemClicked;

        public event ScrollChangedEventHandler ScrollChanged
        {
            add { EvenWrapPanel_ScrollViewer.ScrollChanged += value; }
            remove { EvenWrapPanel_ScrollViewer.ScrollChanged -= value; }
        }




        private void ItemClickedOn(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            if (item != null)
            {
                MediaViewModel vm = (MediaViewModel)item.Content;
                ItemClicked.Invoke(this, new PanelItemClickedEventArgs(vm));
            }

        }

        /**
         * If the user scrolls the mouse wheel when the mouse is anywhere within the ScrollViewer,
         * scroll the ScrollViewer.
         */
        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int offset = e.Delta;
            EvenWrapPanel_ScrollViewer.ScrollToVerticalOffset(EvenWrapPanel_ScrollViewer.VerticalOffset - offset);
            e.Handled = true;
        }





    }




    /// <summary>
    /// A type of WPF Event Arguments for events that involve choosing an item from a ChooserDropDown. The item
    /// might already have existed on the list or not.
    /// </summary>
    public class PanelItemClickedEventArgs : EventArgs
    {
        // The ChooserDropDown item that was selected
        public MediaViewModel Item;

        /// <summary>
        /// Creates ItemChosenEventArgs for and event when the given string item is picked from a ChooserDropDown,
        /// either choosing an existing tag or creating a new one.
        /// </summary>
        /// <param name="item">The item chosen during this event.</param>
        /// <param name="itemNew">Whether the item is being created by this event or already existed in the list.</param>
        public PanelItemClickedEventArgs(MediaViewModel item)
        {
            Item = item;
        }
    }

}
