using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;
using SpotifyClassic.Data;
using SpotifyClassic.Resources;

namespace SpotifyClassic.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string BackendBaseUrl = "http://192.168.100.20:3000";
        private bool _isLibraryLoading = false;
        public bool IsLibraryLoaded { get; private set; }

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

        private Task<string> DownloadStringTaskAsync(string url)
        {
            var tcs = new TaskCompletionSource<string>();
            var client = new WebClient();
            client.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.TrySetException(e.Error);
                else if (e.Cancelled) tcs.TrySetCanceled();
                else tcs.TrySetResult(e.Result);
            };
            client.DownloadStringAsync(new Uri(url));
            return tcs.Task;
        }

        public async Task CargarBibliotecaDesdeServidorAsync(bool forzarRecarga = false)
        {
            if (_isLibraryLoading) return;
            if (IsLibraryLoaded && !forzarRecarga) return;

            _isLibraryLoading = true;

            try
            {
                string jsonString = await DownloadStringTaskAsync(BackendBaseUrl + "/api/library");
                JObject root = JObject.Parse(jsonString);

                JArray arrPlaylists = root["playlists"] as JArray;
                JArray arrAlbumes = root["albumes"] as JArray;
                JArray arrArtistas = root["artistas"] as JArray;
                JArray arrCanciones = root["cancionesMeGusta"] as JArray;

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // 1. Playlists (obtenidas con Shared Client ID)
                    Playlists.Clear();
                    if (arrPlaylists != null)
                    {
                        foreach (JObject obj in arrPlaylists)
                        {
                            string portada = (string)obj["portada"];
                            Playlists.Add(new PlaylistModel
                            {
                                Titulo = (string)obj["titulo"] ?? "Sin título",
                                Creador = (string)obj["creador"] ?? "Spotify",
                                Portada = string.IsNullOrWhiteSpace(portada) ? "/Assets/MusicPreview.png" : portada,
                                Uri = (string)obj["uri"] ?? ""
                            });
                        }
                    }

                    // 2. Álbumes (obtenidos con Personal Client ID)
                    Albumes.Clear();
                    if (arrAlbumes != null)
                    {
                        foreach (JObject obj in arrAlbumes)
                        {
                            string portada = (string)obj["portada"];
                            Albumes.Add(new AlbumModel
                            {
                                Titulo = (string)obj["titulo"] ?? "Sin título",
                                Artista = (string)obj["artista"] ?? "Desconocido",
                                Año = (string)obj["anio"] ?? "",
                                Tipo = (string)obj["tipo"] ?? "álbum",
                                Portada = string.IsNullOrWhiteSpace(portada) ? "/Assets/MusicPreview.png" : portada,
                                Uri = (string)obj["uri"] ?? ""
                            });
                        }
                    }

                    // 3. Artistas (obtenidos con Personal Client ID)
                    Artistas.Clear();
                    if (arrArtistas != null)
                    {
                        foreach (JObject obj in arrArtistas)
                        {
                            string portada = (string)obj["portada"];
                            Artistas.Add(new ArtistModel
                            {
                                Nombre = (string)obj["nombre"] ?? "Desconocido",
                                Portada = string.IsNullOrWhiteSpace(portada) ? "/Assets/MusicPreview.png" : portada,
                                Uri = (string)obj["uri"] ?? ""
                            });
                        }
                    }

                    // 4. Canciones "Me gusta" (obtenidas con Personal Client ID)
                    CancionesMeGusta.Clear();
                    if (arrCanciones != null)
                    {
                        foreach (JObject obj in arrCanciones)
                        {
                            string portada = (string)obj["portada"];
                            string artista = (string)obj["artista"] ?? "Desconocido";
                            CancionesMeGusta.Add(new TrackModel
                            {
                                Titulo = (string)obj["titulo"] ?? "Sin título",
                                Artista = artista,
                                Bajada = (string)obj["bajada"] ?? artista,
                                Portada = string.IsNullOrWhiteSpace(portada) ? "/Assets/MusicPreview.png" : portada,
                                Uri = (string)obj["uri"] ?? ""
                            });
                        }
                    }

                    // Reagrupar alfabéticamente para los LongListSelectors de Biblioteca.xaml
                    ActualizarAgrupacionPlaylists();
                    ActualizarAgrupacionAlbumes();
                    ActualizarAgrupacionArtistas();
                    ActualizarAgrupacionCancionesMeGusta();

                    IsLibraryLoaded = true;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error cargando biblioteca real: " + ex.Message);
            }
            finally
            {
                _isLibraryLoading = false;
            }
        }

        public void LoadData()
        {
            Items.Clear();
            Items.Add(new ItemViewModel() { LineOne = "playlists" });
            Items.Add(new ItemViewModel() { LineOne = "álbumes" });
            Items.Add(new ItemViewModel() { LineOne = "me gusta" });
            Items.Add(new ItemViewModel() { LineOne = "artistas" });

            TopArtistas.Clear();

            // EL PRIMERO ES GRANDE
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "1",
                Nombre = "Lady Gaga",
                CancionTop = "LoveDrug",
                Portada = "/Assets/Covers/thefame.png",
                VisibilidadGrande = Visibility.Visible,
                VisibilidadNormal = Visibility.Collapsed
            });

            // EL RESTO ES NORMAL
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "2",
                Nombre = "PinkPantheress",
                CancionTop = "Stateside + Zara Larsson",
                Portada = "/Assets/Covers/pinkp.jpg",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "3",
                Nombre = "Girls' Generation",
                CancionTop = "FLOWER POWER",
                Portada = "/Assets/NewsCovers/juno.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "4",
                Nombre = "TWICE",
                CancionTop = "LIKEY",
                Portada = "/Assets/Covers/TWICE.jpg",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "5",
                Nombre = "C418",
                CancionTop = "Moog City",
                Portada = "/Assets/Covers/mcost.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "6",
                Nombre = "Blake Neely",
                CancionTop = "",
                Portada = "/Assets/Covers/confident.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "7",
                Nombre = "Akira Yamaoka",
                CancionTop = "White Noise (Actual Noise)",
                Portada = "/Assets/Covers/sh2.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "8",
                Nombre = "Brian Reitzell",
                CancionTop = "",
                Portada = "/Assets/NewsCovers/1.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });
            TopArtistas.Add(new TopArtistModel
            {
                Puesto = "9",
                Nombre = "Shakira",
                CancionTop = "",
                Portada = "/Assets/NewsCovers/montana.png",
                VisibilidadGrande = Visibility.Collapsed,
                VisibilidadNormal = Visibility.Visible
            });

            // Inicializar agrupaciones vacías hasta que responda el servidor
            ActualizarAgrupacionArtistas();
            ActualizarAgrupacionAlbumes();
            ActualizarAgrupacionCancionesMeGusta();
            ActualizarAgrupacionPlaylists();

            IsDataLoaded = true;

            // Disparar carga de datos reales de la biblioteca en segundo plano
            var _ = CargarBibliotecaDesdeServidorAsync();
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
        public PlaylistModel PlaylistSeleccionada { get; set; }
        public AlbumModel AlbumSeleccionado { get; set; }
    }

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

        public string Uri { get; set; }

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
            get { return string.Format("Canción más escuchada: {0}", CancionTop); }
        }

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

        public string Uri { get; set; }

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

        private string _tipo;
        public string Tipo
        {
            get { return _tipo; }
            set { _tipo = value; NotifyPropertyChanged("Tipo"); }
        }

        public string Uri { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}