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
    /// Interaction logic for Gallery.xaml
    /// </summary>
    public partial class GalleryCopy : UserControl
    {
        public GalleryCopy()
        {
            InitializeComponent();

            Loaded += GalleryLoaded;
            SizeChanged += Gallery_SizeChanged;
        }

        public void GalleryLoaded(object sender, RoutedEventArgs args)
        {
            MeasureSizes();
        }

        public void Gallery_SizeChanged(object sender, RoutedEventArgs args)
        {
            MeasureSizes();
        }

        public void MeasureSizes()
        {
            double galleryWidth = GalleryImageListBox.ActualWidth - 5;
            int thumbHeight = 200;
            List<MediaViewModel> list = GalleryImageListBox.Items.Cast<MediaViewModel>().ToList();
            int mediaIndex = 0;
            int margin = 8;

            //Console.WriteLine("Gallery width: " + galleryWidth);

            while(mediaIndex < list.Count)
            {
                int count = 0;
                double totWidth = 0;
                double arSums = 0;
                while (totWidth < galleryWidth)
                {

                    MediaViewModel mediaVM = list[mediaIndex+count];
                    ImageViewModel vm;
                    if (mediaVM.MediaType == Utils.MediaFileType.Image)
                    {
                        vm = (mediaVM as ImageViewModel);
                    }
                    else
                    {
                        vm = (mediaVM as VideoViewModel).ThumbnailViewModel;
                    }
                    //Console.WriteLine("\nPhoto: " + vm.Filepath);

                    double displayWidth = thumbHeight * vm.Photo.AspectRatio + 2*margin;

                    //Console.WriteLine("DisplayWidth: " + displayWidth);

                    if (count == 0 || totWidth + displayWidth < galleryWidth)
                    {
                        totWidth += displayWidth;
                        //Console.WriteLine("New tot width: " + totWidth);
                        count++;
                        arSums += vm.Photo.AspectRatio;
                        if (mediaIndex + count >= list.Count)
                        {
                            //Console.WriteLine("Item break:");
                            break;
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Width break: ");
                        break;
                    }
                }


                //double diff = galleryWidth - totWidth;
                //double scale = diff / count;


                // totWidth = newHeight * ar + margin + newHeight * ar + margin


                // (newHeight * ar1 + newHeight * ar2 + newHeight * ar3) + margins = galleryWidth

                double newHeight = (galleryWidth - count*2*margin) / arSums;


                //Console.WriteLine("\n\nnewheight: " + newHeight);

                for(int i = 0; i < count; i++)
                {
                    MediaViewModel mediaVM = list[mediaIndex + i];
                    ImageViewModel vm;
                    if (mediaVM.MediaType == Utils.MediaFileType.Image)
                    {
                        vm = (mediaVM as ImageViewModel);
                    }
                    else
                    {
                        vm = (mediaVM as VideoViewModel).ThumbnailViewModel;
                    }

                    //Console.WriteLine("OldSize: " + (thumbHeight * vm.Photo.AspectRatio) + ", " + thumbHeight);


                    /*
                    double newWidth = (thumbHeight * vm.Photo.AspectRatio) + scale;
                    double newHeight = newWidth / vm.Photo.AspectRatio;*/
                    //Console.WriteLine("NewSize: " + newWidth + ", " + newHeight);
                    mediaVM.DisplayHeight =  newHeight;
                    /*Console.WriteLine("\nPhoto: " + vm.Filepath);
                    Console.WriteLine("DispHeight: " + vm.DisplayHeight);
                    Console.WriteLine("DispWidth: " + vm.DisplayHeight * vm.AspectRatio);
                    */
                }


                mediaIndex += count;
            }

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

    }
}
