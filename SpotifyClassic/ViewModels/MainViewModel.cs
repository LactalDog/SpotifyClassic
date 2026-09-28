using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using SpotifyClassic.Data;
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

        public ObservableCollection<ItemViewModel> Items { get; private set; }
        public ObservableCollection<AlbumModel> Albumes { get; private set; }

        private List<AlphaKeyGroup<AlbumModel>> _albumesAgrupados;
        public List<AlphaKeyGroup<AlbumModel>> AlbumesAgrupados
        {
            get { return _albumesAgrupados; }
            set
            {
                if (_albumesAgrupados != value)
                {
                    _albumesAgrupados = value;
                    NotifyPropertyChanged("AlbumesAgrupados");
                }
            }
        }

        public bool IsDataLoaded { get; private set; }

        public void ActualizarAgrupacionAlbumes()
        {
            if (Albumes != null)
            {
                AlbumesAgrupados = AlphaKeyGroup<AlbumModel>.CreateGroups(
                    Albumes,
                    CultureInfo.CurrentUICulture,
                    (AlbumModel s) => string.IsNullOrEmpty(s.Titulo) ? "#" : s.Titulo,
                    true);
            }
        }

        public void LoadData()
        {
            Items.Clear();
            Items.Add(new ItemViewModel() { LineOne = "playlists" });
            Items.Add(new ItemViewModel() { LineOne = "álbumes" });
            Items.Add(new ItemViewModel() { LineOne = "me gusta" });
            Items.Add(new ItemViewModel() { LineOne = "artistas" });

            Albumes.Clear();
            // Colección con imágenes y metadatos adaptados al estilo de PicturesAlbum del Toolkit
            Albumes.Add(new AlbumModel { Titulo = "AM", Artista = "Arctic Monkeys", Año = "2013", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "Currents", Artista = "Tame Impala", Año = "2015", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "King of Having Fun", Artista = "Medium Build", Año = "2024", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "The New Abnormal", Artista = "The Strokes", Año = "2020", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "Favourite Worst Nightmare", Artista = "Arctic Monkeys", Año = "2007", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "Is This It", Artista = "The Strokes", Año = "2001", Portada = "/Assets/MusicPreview.png" });

            ActualizarAgrupacionAlbumes();

            IsDataLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
        public string Uri { get; set; }
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