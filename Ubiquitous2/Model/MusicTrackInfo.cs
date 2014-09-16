using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class MusicTrackInfo : NotifyPropertyChangeBase
    {
        /// <summary>
        /// The <see cref="Artist" /> property's name.
        /// </summary>
        public const string ArtistPropertyName = "Artist";

        private string _artist = "";

        /// <summary>
        /// Sets and gets the Artist property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Artist
        {
            get
            {
                return _artist;
            }

            set
            {
                if (_artist == value)
                {
                    return;
                }

                RaisePropertyChanging(ArtistPropertyName);
                _artist = value;
                RaisePropertyChanged(ArtistPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private string _title = "";

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }

            set
            {
                if (_title == value)
                {
                    return;
                }

                RaisePropertyChanging(TitlePropertyName);
                _title = value;
                RaisePropertyChanged(TitlePropertyName);
            }
        }
        /// <summary>
        /// The <see cref="Album" /> property's name.
        /// </summary>
        public const string AlbumPropertyName = "Album";

        private string _album = "";

        /// <summary>
        /// Sets and gets the Album property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Album
        {
            get
            {
                return _album;
            }

            set
            {
                if (_album == value)
                {
                    return;
                }

                RaisePropertyChanging(AlbumPropertyName);
                _album = value;
                RaisePropertyChanged(AlbumPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="ImageURL" /> property's name.
        /// </summary>
        public const string ImagePropertyName = "ImageURL";

        private string _imageURL = null;

        /// <summary>
        /// Sets and gets the Image property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ImageURL
        {
            get
            {
                return _imageURL;
            }

            set
            {
                if (_imageURL == value)
                {
                    return;
                }

                RaisePropertyChanging(ImagePropertyName);
                _imageURL = value;
                RaisePropertyChanged(ImagePropertyName);
            }
        }
    }
}
