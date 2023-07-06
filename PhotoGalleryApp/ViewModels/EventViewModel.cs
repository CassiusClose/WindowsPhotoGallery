using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoGalleryApp.Models;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// The viewmodel for a specific Event object.
    /// </summary>
    class EventViewModel : ICollectableViewModel
    {
        public EventViewModel(Event evnt, NavigatorViewModel nav)
        {
            _nav = nav;
            _event = evnt;

            _mediaCollectionVM = new MediaCollectionViewModel(nav, _event.Collection);
            _mediaCollectionVM.MediaOpened += MediaOpened;
        }

        /*
         * Open the media that was clicked in the MediaCollection - either a slideshow if clicked on
         * media, or the event page if clicked on event.
         */
        private void MediaOpened(ICollectableViewModel item)
        {
            if(item is MediaViewModel)
            {
                // Get a list of all the currently visible images
                //TODO Abstract here and galleryvm, and make efficient
                List<ICollectableViewModel> list = MediaCollectionVM.MediaView.Cast<ICollectableViewModel>().ToList();
                List<Media> media = new List<Media>(); //= MediaViewModel.GetMediaList(list);
                foreach (ICollectableViewModel vm in list)
                {
                    if (vm is MediaViewModel)
                        media.Add((Media)vm.GetModel());
                }

                // Get the clicked on photo's index in the list of Photos
                int index = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (item.GetModel() == media[i])
                    {
                        index = i;
                        break;
                    }
                }

                // Create a new page to view the clicked image
                SlideshowViewModel imagePage = new SlideshowViewModel(media, index, MediaCollectionVM.MediaCollectionModel);
                _nav.NewPage(imagePage);
            }
            else
            {
                _nav.NewPage(item);
            }
        }

        /// <summary>
        /// Returns the ICollectable model associated with this viewmodel
        /// </summary>
        /// <returns>The ICollectable model associated with this viewmodel</returns>
        public override ICollectable GetModel()
        {
            return Event;
        }

        private NavigatorViewModel _nav;

        private Event _event;
        public Event Event { get { return _event; } }


        private MediaCollectionViewModel _mediaCollectionVM;
        /// <summary>
        /// The media belonging to the event
        /// </summary>
        public MediaCollectionViewModel MediaCollectionVM { get { return _mediaCollectionVM; } }

        /// <summary>
        /// The event's display name
        /// </summary>
        public string Name
        {
            get { return _event.Name; }
        }



    }
}
