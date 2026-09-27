using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SpotifyClassic.Resources;

namespace SpotifyClassic.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.Items = new ObservableCollection<ItemViewModel>();
            this.Recientes = new ObservableCollection<TrackModel>();
            this.Novedades = new ObservableCollection<NewsModel>();
            this.Sugerencias = new ObservableCollection<SuggestionsModel>();
            this.Reproduciendo = new ObservableCollection<PlayingModel>();
            this.Albumes = new ObservableCollection<AlbumModel>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ItemViewModel> Items { get; private set; }
        public ObservableCollection<AlbumModel> Albumes { get; private set; }

        private string _sampleProperty = "Sample Runtime Property Value";
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding
        /// </summary>
        /// <returns></returns>
        public string SampleProperty
        {
            get
            {
                return _sampleProperty;
            }
            set
            {
                if (value != _sampleProperty)
                {
                    _sampleProperty = value;
                    NotifyPropertyChanged("SampleProperty");
                }
            }
        }

        /// <summary>
        /// Sample property that returns a localized string
        /// </summary>
        public string LocalizedSampleProperty
        {
            get
            {
                return AppResources.SampleProperty;
            }
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            // Sample data; replace with real data
            Items.Add(new ItemViewModel() { LineOne = "playlists" });
            Items.Add(new ItemViewModel() { LineOne = "álbumes"});
            Items.Add(new ItemViewModel() { LineOne = "me gusta" });
            Items.Add(new ItemViewModel() { LineOne = "artistas"});
            
            Recientes.Clear();

            Reproduciendo.Add(new PlayingModel() { Portada = "/Assets/NewsCovers/mf.png", Tipo = "Álbum", Titulo = "King of Having Fun", Artista = "Medium Build" });

            Albumes.Add(new AlbumModel { Titulo = "King of Having Fun", Artista = "Medium Build", Año = "2024", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "The New Abnormal", Artista = "The Strokes", Año = "2020", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "Currents", Artista = "Tame Impala", Año = "2015", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "AM", Artista = "Arctic Monkeys", Año = "2013", Portada = "/Assets/MusicPreview.png" });


            IsDataLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public ObservableCollection<TrackModel> Recientes { get; private set; }
        public ObservableCollection<NewsModel> Novedades { get; private set; }
        public ObservableCollection<SuggestionsModel> Sugerencias { get; private set; }
        public ObservableCollection<PlayingModel> Reproduciendo { get; private set; }
    }

    public class TrackModel
    {
        public string Titulo { get; set; }
        public string Portada { get; set; }
        public string Bajada { get; set; }
        public string Uri { get; set; } // ¡Propiedad requerida!
    }

    public class NewsModel
    {
        public string Titulo { get; set; }
        public string Tipo { get; set; }
        public string Artista { get; set; }
        public string Portada { get; set; }
    }

    public class SuggestionsModel
    {
        public string Titulo { get; set; }
        public string Tipo { get; set; }
        public string Artista { get; set; }
        public string Portada { get; set; }
    }

    public class PlayingModel
    {
        public string Titulo { get; set; }
        public string Tipo { get; set; }
        public string Artista { get; set; }
        public string Portada { get; set; }
        public string Duracion { get; set; }
        public string TiempoTranscurrido { get; set; }
        public string TiempoRestante { get; set; }
        public string UltimoTiempo { get; set; }
        public string EnPausa { get; set; }
    }

    public class AlbumModel : INotifyPropertyChanged
    {
        private string _titulo;
        public string Titulo
        {
            get { return _titulo; }
            set { _titulo = value; NotifyPropertyChanged("Titulo"); }
        }

        private string _artista;
        public string Artista
        {
            get { return _artista; }
            set { _artista = value; NotifyPropertyChanged("Artista"); }
        }

        private string _portada;
        public string Portada
        {
            get { return _portada; }
            set { _portada = value; NotifyPropertyChanged("Portada"); }
        }

        private string _año;
        public string Año
        {
            get { return _año; }
            set { _año = value; NotifyPropertyChanged("Año"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}