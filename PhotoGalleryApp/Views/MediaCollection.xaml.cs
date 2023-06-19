using PhotoGalleryApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    /// Interaction logic for MediaCollection.xaml
    /// </summary>
    public partial class MediaCollection : UserControl
    {
        public MediaCollection()
        {
            InitializeComponent();
        }



        /*
         * In order to size the images properly, the collection needs to know the max thumbnail height in the code behind. To do this, make it a DependencyProperty.
         */
        public static readonly DependencyProperty ThumbnailHeightProperty = DependencyProperty.Register("ThumbnailHeight", typeof(int), typeof(MediaCollection));
        public int ThumbnailHeight
        {
            get { return (int)GetValue(ThumbnailHeightProperty); }
            set { SetValue(ThumbnailHeightProperty, value); }
        }



        /**
         * If the user scrolls the mouse wheel when the mouse is anywhere within the ScrollViewer,
         * scroll the ScrollViewer.
         */
        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int offset = e.Delta;
            ImagesScrollViewer.ScrollToVerticalOffset(ImagesScrollViewer.VerticalOffset - offset);
            e.Handled = true;
        }




        /* 
         * When the list of images changes, update the image layout
         */
        public void CollectionListBox_ItemsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            RecalcImageSizes();
        }

        /*
         * Called when the user control is loaded. Before this point, the ListBox that holds the images has not been initialized, but the ListBox
         * ItemsChanged listener will be called anyway. 
         */
        public void UserControl_Loaded(object sender, RoutedEventArgs args)
        {
            RecalcImageSizes();
            ((INotifyCollectionChanged)CollectionListBox.Items).CollectionChanged += CollectionListBox_ItemsChanged;
        }

        /*
         * If the size of the container is changed, the images will fit differently.
         */
        public void UserControl_SizeChanged(object sender, RoutedEventArgs args)
        {
            RecalcImageSizes();
        }





        private const int COLLECTION_RIGHT_PADDING = 25;

        /*
         * The images are displayed in rows, but could be different sizes. This takes an approach similar to the way Google Photos does it,
         * resizing each image so that every image in a row has the same height, and every row has the same width. This should get called any time
         * the window changes size or the list of images changes.
         */
        private void RecalcImageSizes()
        {
            // What should be combined width of every image in each row at the end of this function. Subtract some from the container's width to account
            // for padding between images and possibly the scroll bar as well. There's probably a better way to do this, but I'm not sure what the
            // padding on the images is right now.
            double containerWidth = this.ActualWidth - COLLECTION_RIGHT_PADDING;

            int itemsCount = CollectionListBox.Items.Count;
            for (int i = 0; i < itemsCount;)
            {

                /* At their default size, figure out how many images fit in the row. */

                double widthInRow = 0; // The total width of the images added to the row so far
                int rowItemCount = 0; // How many images have been added to the row

                // Go through until we've filled up the row. Do this by adding images until the next image's width would
                // exceed the row maximum.
                for (int j = i; j < itemsCount; j++)
                {
                    ListBoxItem lbi = (ListBoxItem)CollectionListBox.ItemContainerGenerator.ContainerFromIndex(j);
                    MediaViewModel vm = (MediaViewModel)lbi.Content;
                    double ar = GetAR(vm);

                    if (widthInRow + ThumbnailHeight * ar > containerWidth)
                    {
                        break;
                    }

                    widthInRow += ThumbnailHeight * ar;

                    rowItemCount++;
                }


                /* If needed, scale the images in the row up to take up the entire row's width. */
                double scale = containerWidth / widthInRow;

                // Setting the max amount the scale can be makes a max height to the images in the collection.
                // Practically, this means the last row of images may not extend to fit the entire row. This
                // is an aesthetic choice.
                /*if (scale > 1.5)
                {
                    scale = 1.5;
                }*/

                // Now go through and resize each image in the row as determined in the previous loop. Note that
                // this increments the outer loop index (i) as well.
                for (int j = 0; j < rowItemCount; j++, i++)
                {
                    ListBoxItem lbi = (ListBoxItem)CollectionListBox.ItemContainerGenerator.ContainerFromIndex(i);

                    MediaViewModel vm = (MediaViewModel)lbi.Content;
                    double ar = GetAR(vm);

                    // Scale each image
                    // If ceiling, the rounding might push the actual width over the boundaries of the rows,
                    // and the last images will be put in the next row. So instead, floor.
                    lbi.MaxWidth = Math.Floor(ThumbnailHeight * ar * scale);
                    lbi.Width = Math.Floor(ThumbnailHeight * ar * scale);

                    // Setting width isn't enough, it will still try to display the image at ThumbnailHeight,
                    // I guess? It will result in the full height being displayed, but only a small fraction
                    // of the width.
                    lbi.MaxHeight = Math.Floor(ThumbnailHeight * scale);
                    lbi.Height = Math.Floor(ThumbnailHeight * scale);
                }
            }
        }


        /*
         * Returns the aspect ratio of a media VM object. Aspect ratio is stored without rotation info, so
         * apply that if needed.
         */
        private double GetAR(MediaViewModel vm)
        {
            double ar;
            if (vm.MediaType == Utils.MediaFileType.Image)
            {
                ar = (vm as ImageViewModel).AspectRatio;
            }
            else
            {
                ar = (vm as VideoViewModel).ThumbnailViewModel.AspectRatio;
            }

            if (vm.Rotation == Rotation.Rotate90 || vm.Rotation == Rotation.Rotate270)
            {
                ar = 1 / ar;
            }

            return ar;
        }
    }
}
