using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using SpotifyClassic.Data;
using SpotifyClassic.Resources;
using System.Windows;

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
            this.CancionesMeGusta = new ObservableCollection<TrackModel>();
            this.Playlists = new ObservableCollection<PlaylistModel>();
            this.Artistas = new ObservableCollection<ArtistModel>();
            this.TopArtistas = new ObservableCollection<TopArtistModel>();
        }

        public ObservableCollection<ItemViewModel> Items { get; private set; }
        public ObservableCollection<AlbumModel> Albumes { get; private set; }
        public ObservableCollection<TrackModel> CancionesMeGusta { get; private set; }
        public ObservableCollection<PlaylistModel> Playlists { get; private set; }
        public ObservableCollection<TopArtistModel> TopArtistas { get; private set; }

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

        private List<AlphaKeyGroup<PlaylistModel>> _playlistsAgrupadas;
        public List<AlphaKeyGroup<PlaylistModel>> PlaylistsAgrupadas
        {
            get { return _playlistsAgrupadas; }
            set
            {
                if (_playlistsAgrupadas != value)
                {
                    _playlistsAgrupadas = value;
                    NotifyPropertyChanged("PlaylistsAgrupadas");
                }
            }
        }

        private List<AlphaKeyGroup<TrackModel>> _cancionesMeGustaAgrupadas;
        public List<AlphaKeyGroup<TrackModel>> CancionesMeGustaAgrupadas
        {
            get { return _cancionesMeGustaAgrupadas; }
            set
            {
                if (_cancionesMeGustaAgrupadas != value)
                {
                    _cancionesMeGustaAgrupadas = value;
                    NotifyPropertyChanged("CancionesMeGustaAgrupadas");
                }
            }
        }

        public ObservableCollection<ArtistModel> Artistas { get; private set; }

        private List<AlphaKeyGroup<ArtistModel>> _artistasAgrupados;
        public List<AlphaKeyGroup<ArtistModel>> ArtistasAgrupados
        {
            get { return _artistasAgrupados; }
            set
            {
                if (_artistasAgrupados != value)
                {
                    _artistasAgrupados = value;
                    NotifyPropertyChanged("ArtistasAgrupados");
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

        public void ActualizarAgrupacionCancionesMeGusta()
        {
            if (CancionesMeGusta != null)
            {
                CancionesMeGustaAgrupadas = AlphaKeyGroup<TrackModel>.CreateGroups(
                    CancionesMeGusta,
                    CultureInfo.CurrentUICulture,
                    (TrackModel s) => string.IsNullOrEmpty(s.Titulo) ? "#" : s.Titulo,
                    true);
            }
        }

        public void ActualizarAgrupacionPlaylists()
        {
            if (Playlists != null)
            {
                PlaylistsAgrupadas = AlphaKeyGroup<PlaylistModel>.CreateGroups(
                    Playlists,
                    CultureInfo.CurrentUICulture,
                    (PlaylistModel s) => string.IsNullOrEmpty(s.Titulo) ? "#" : s.Titulo,
                    true);
            }
        }

        public void ActualizarAgrupacionArtistas()
        {
            if (Artistas != null)
            {
                ArtistasAgrupados = AlphaKeyGroup<ArtistModel>.CreateGroups(
                    Artistas,
                    CultureInfo.CurrentUICulture,
                    (ArtistModel s) => string.IsNullOrEmpty(s.Nombre) ? "#" : s.Nombre,
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
            Albumes.Add(new AlbumModel { Titulo = "Alan Wake 2 - The Lake House - 6 Deep Breaths by POE 1-52 screenshot", Artista = "Arctic Monkeys", Año = "2013", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "American Horror Stories", Artista = "Lin Manuel Miranda", Año = "2013", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "Currents", Artista = "taylor swift, lin manuel miranda, and Electronic arts music dicord label from 2015 Feat. Kesha from the black eyed peas", Año = "2015", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "King of Having Fun", Artista = "Medium Build", Año = "2024", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "The New Abnormal", Artista = "The Strokes", Año = "2020", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "Favourite Worst Nightmare", Artista = "Arctic Monkeys", Año = "2007", Portada = "/Assets/MusicPreview.png" });
            Albumes.Add(new AlbumModel { Titulo = "Is This It", Artista = "The Strokes", Año = "2001", Portada = "/Assets/MusicPreview.png" });

            CancionesMeGusta.Add(new TrackModel { Titulo = "A new dawn: I like it", Artista = "Arctic Monkeys", Portada = "/Assets/MusicPreview.png" });
            CancionesMeGusta.Add(new TrackModel { Titulo = "American in my heart", Artista = "Lin Manuel Miranda", Portada = "/Assets/MusicPreview.png" });
            CancionesMeGusta.Add(new TrackModel { Titulo = "Currents", Artista = "taylor swift, lin manuel miranda, and Electronic arts music dicord label from 2015 Feat. Kesha from the black eyed peas", Portada = "/Assets/MusicPreview.png" });
            CancionesMeGusta.Add(new TrackModel { Titulo = "King of Having Fun", Artista = "Medium Build", Portada = "/Assets/MusicPreview.png" });
            CancionesMeGusta.Add(new TrackModel { Titulo = "The New Big Day", Artista = "The Strokes", Portada = "/Assets/MusicPreview.png" });
            CancionesMeGusta.Add(new TrackModel { Titulo = "Fest in the night", Artista = "Arctic Monkeys", Portada = "/Assets/MusicPreview.png" });
            CancionesMeGusta.Add(new TrackModel { Titulo = "Silents In The Wild", Artista = "The Strokes", Portada = "/Assets/MusicPreview.png" });

            Playlists.Clear();
            Playlists.Add(new PlaylistModel { Titulo = "Topsify Argentina", Creador = "Spotify", Portada = "/Assets/MusicPreview.png" });
            Playlists.Add(new PlaylistModel { Titulo = "Rock Classics", Creador = "Juan Pérez", Portada = "/Assets/MusicPreview.png" });
            Playlists.Add(new PlaylistModel { Titulo = "Focus para Programar", Creador = "Ivanna", Portada = "/Assets/MusicPreview.png" });
            Playlists.Add(new PlaylistModel { Titulo = "Discover Weekly", Creador = "Spotify", Portada = "/Assets/MusicPreview.png" });
            Playlists.Add(new PlaylistModel { Titulo = "Gym Motivation", Creador = "Ricardo", Portada = "/Assets/MusicPreview.png" });
            Playlists.Add(new PlaylistModel { Titulo = "Viaje al Sur", Creador = "Godoy Tiago Joaquín", Portada = "/Assets/MusicPreview.png" });

            Artistas.Clear();
            Artistas.Add(new ArtistModel { Nombre = "Arctic Monkeys", Portada = "/Assets/MusicPreview.png" });
            Artistas.Add(new ArtistModel { Nombre = "Lin Manuel Miranda", Portada = "/Assets/MusicPreview.png" });
            Artistas.Add(new ArtistModel { Nombre = "Medium Build", Portada = "/Assets/MusicPreview.png" });
            Artistas.Add(new ArtistModel { Nombre = "The Strokes", Portada = "/Assets/MusicPreview.png" });
            Artistas.Add(new ArtistModel { Nombre = "Solar Fields", Portada = "/Assets/MusicPreview.png" });
            Artistas.Add(new ArtistModel { Nombre = "Taylor Swift", Portada = "/Assets/MusicPreview.png" });

            TopArtistas.Clear();

            // EL PRIMERO ES GRANDE
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "1",
                Nombre = "Lady Gaga",
                CancionTop = "Do I Wanna Know?",
                Portada = "/Assets/MusicPreview.png",
                VisibilidadGrande = Visibility.Visible,
                VisibilidadNormal = Visibility.Collapsed
            });

            // EL RESTO ES NORMAL
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "2",
                Nombre = "AKIRA YAMAOKA",
                CancionTop = "Reptilia",
                Portada = "/Assets/MusicPreview.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "3",
                Nombre = "Solar Fields",
                CancionTop = "Mirror's Edge Theme",
                Portada = "/Assets/MusicPreview.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "4",
                Nombre = "Lin Manuel Miranda",
                CancionTop = "Alexander Hamilton",
                Portada = "/Assets/MusicPreview.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "5",
                Nombre = "Taylor Swift",
                CancionTop = "Anti-Hero",
                Portada = "/Assets/MusicPreview.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "6",
                Nombre = "Medium Build",
                CancionTop = "King of Having Fun",
                Portada = "/Assets/MusicPreview.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "7",
                Nombre = "Daft Punk",
                CancionTop = "Get Lucky",
                Portada = "/Assets/MusicPreview.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "8",
                Nombre = "Tame Impala",
                CancionTop = "The Less I Know The Better",
                Portada = "/Assets/MusicPreview.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });

            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "9",
                Nombre = "Juliana Gattas",
                CancionTop = "Soy Así",
                Portada = "/Assets/MusicPreview.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });

            ActualizarAgrupacionArtistas();
            ActualizarAgrupacionAlbumes();
            ActualizarAgrupacionCancionesMeGusta();
            ActualizarAgrupacionPlaylists();

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

    // 1. Añadir el nuevo modelo al final del archivo (fuera de la clase MainViewModel)
    public class PlaylistModel : INotifyPropertyChanged
    {
        private string _titulo;
        public string Titulo
        {
            get { return _titulo; }
            set { _titulo = value; NotifyPropertyChanged("Titulo"); }
        }

        private string _creador;
        public string Creador
        {
            get { return _creador; }
            set { _creador = value; NotifyPropertyChanged("Creador"); }
        }

        private string _portada;
        public string Portada
        {
            get { return _portada; }
            set { _portada = value; NotifyPropertyChanged("Portada"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TopArtistModel
    {
        public string Puesto { get; set; }
        public string Nombre { get; set; }
        public string CancionTop { get; set; }
        public string Portada { get; set; }

        public string TituloDisplay
        {
            get { return string.Format("#{0}\n{1}", Puesto, Nombre); }
        }

        public string MensajeDisplay
        {
            get { return string.Format("{0}", CancionTop); }
        }

        // NUEVAS PROPIEDADES PARA EL TAMAÑO
        public Visibility VisibilidadGrande { get; set; }
        public Visibility VisibilidadNormal { get; set; }
    }

    public class ArtistModel : INotifyPropertyChanged
    {
        private string _nombre;
        public string Nombre
        {
            get { return _nombre; }
            set { _nombre = value; NotifyPropertyChanged("Nombre"); }
        }

        private string _portada;
        public string Portada
        {
            get { return _portada; }
            set { _portada = value; NotifyPropertyChanged("Portada"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TrackModel
    {
        public string Titulo { get; set; }
        public string Portada { get; set; }
        public string Artista { get; set; }
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