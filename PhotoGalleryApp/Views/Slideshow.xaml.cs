﻿using System;
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

namespace PhotoGalleryApp.Views
{
    /// <summary>
    /// Interaction logic for Slideshow.xaml
    /// </summary>
    public partial class Slideshow : UserControl
    {
        public Slideshow()
        {
            InitializeComponent();
        }

        // Seize focus when the control is loaded (when the navigator opens this page).
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }
    }
}
