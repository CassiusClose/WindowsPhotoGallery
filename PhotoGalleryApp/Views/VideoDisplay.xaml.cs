using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for VideoDisplay.xaml
    /// </summary>
    public partial class VideoDisplay : UserControl
    {
        public VideoDisplay()
        {
            InitializeComponent();

            VideoPlayer.Play();

            _positionUpdateTimer = new DispatcherTimer();
            _positionUpdateTimer.Tick += new EventHandler(UpdateSliderPosition);
            _positionUpdateTimer.Interval = TimeSpan.FromMilliseconds(250);
            _positionUpdateTimer.Start();

        }


        // Every so often, update the slider position so that it matches the video's position
        private DispatcherTimer _positionUpdateTimer;

        // Keep track of the position from the last timer update. This is used to distinguish
        // between when the timer updates the slider position and when the user manually does it.
        private double _lastUpdatedPosition = 0;



        /**
         * Called when the MediaElement that plays the video has finished loading the media. Duration
         * information will not be accurate until then.
         */
        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            PositionSlider.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
        }



        /**
         * Updates the slider position to match the video's current position
         */
        private void UpdateSliderPosition(object sender, EventArgs e)
        {
            // Only update if the video duration has been set
            if(PositionSlider.Maximum > 0)
            {
                _lastUpdatedPosition = VideoPlayer.Position.TotalMilliseconds;
                PositionSlider.Value = _lastUpdatedPosition;
            }
        }

        /**
         * Called when the value of the slider changes
         */
        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // If the value of slider has changed because the video is playing, then we don't need to do anything.
            // But if the value of the slider has changed because the user moved the thumb, then seek to that 
            // position on the video player
            if (e.NewValue != _lastUpdatedPosition)
                VideoPlayer.Position = TimeSpan.FromMilliseconds(e.NewValue);
        }



        /**
         * Called when the play/pause button is toggled. Pauses/plays the video as
         * directed and changes the button's display.
         */
        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton b = sender as ToggleButton;
            if (b.IsChecked == true)
            {
                VideoPlayer.Pause();
                b.Content = "Play";
                _positionUpdateTimer.Stop();
            }
            else if (b.IsChecked == false)
            {
                VideoPlayer.Play();
                b.Content = "Pause";
                _positionUpdateTimer.Start();
            }

        }

    }
}
