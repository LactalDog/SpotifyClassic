using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
using SpotifyClassic.ViewModels;

namespace SpotifyClassic.Pages
{
    public partial class PlaylistPage : PhoneApplicationPage
    {
        private const string BackendBaseUrl = "http://192.168.100.20:3000";
        private ApplicationBar barraDefault;
        private ApplicationBar barraSeleccion;
        private ObservableCollection<Canciones> _listaDeCanciones = new ObservableCollection<Canciones>();
        private bool _datosCargados = false;

        public PlaylistPage()
        {
            InitializeComponent();

            ConstruirBarrasDinamicas();
            this.ApplicationBar = barraDefault;

            ListaCanciones.ItemsSource = _listaDeCanciones;

            // Pre-poblar cabecera en el constructor para que TurnstileFeather anime con los datos correctos
            AplicarDatosPreliminares();

            ListaCanciones.IsSelectionEnabledChanged += ListaCanciones_IsSelectionEnabledChanged;
            ListaCanciones.SelectionChanged += ListaCanciones_SelectionChanged;
        }

        private void AplicarDatosPreliminares()
        {
            PlaylistModel pl = App.ViewModel.PlaylistSeleccionada;
            if (pl != null)
            {
                string creadorLimpio = string.IsNullOrWhiteSpace(pl.Creador) ? "Spotify" : pl.Creador.Trim();
                if (creadorLimpio.StartsWith("de ", StringComparison.InvariantCultureIgnoreCase))
                {
                    creadorLimpio = creadorLimpio.Substring(3).Trim();
                }

                Artista_titulo.Text = creadorLimpio.ToUpperInvariant();
                Tipo_titulo.Text = "playlist";
                Titulo.Text = pl.Titulo ?? "Sin título";
                Creador.Text = "de " + creadorLimpio;
                Detalles.Text = "Cargando canciones...";

                EstablecerPortada(pl.Portada);
            }
        }

        private void EstablecerPortada(string urlPortada)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(urlPortada))
                {
                    UriKind kind = urlPortada.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)
                        ? UriKind.Absolute
                        : UriKind.RelativeOrAbsolute;
                    Portada.Source = new BitmapImage(new Uri(urlPortada, kind));
                }
                else
                {
                    Portada.Source = new BitmapImage(new Uri("/Assets/MusicPreview.png", UriKind.Relative));
                }
            }
            catch
            {
                Portada.Source = new BitmapImage(new Uri("/Assets/MusicPreview.png", UriKind.Relative));
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_datosCargados) return;

            string uriParam = "";
            if (NavigationContext.QueryString.ContainsKey("uri"))
            {
                uriParam = NavigationContext.QueryString["uri"];
            }
            else if (App.ViewModel.PlaylistSeleccionada != null)
            {
                uriParam = App.ViewModel.PlaylistSeleccionada.Uri;
            }

            if (!string.IsNullOrWhiteSpace(uriParam))
            {
                await CargarDetallePlaylistAsync(uriParam);
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

        private async Task CargarDetallePlaylistAsync(string playlistUri)
        {
            try
            {
                string[] partes = playlistUri.Split(':');
                string playlistId = partes[partes.Length - 1];

                string url = string.Format("{0}/api/playlist/{1}", BackendBaseUrl, Uri.EscapeDataString(playlistId));
                string jsonString = await DownloadStringTaskAsync(url);

                JObject data = JObject.Parse(jsonString);

                string titulo = (string)data["titulo"] ?? "Sin título";
                string creador = (string)data["creador"] ?? "Spotify";
                string portada = (string)data["portada"] ?? "/Assets/MusicPreview.png";
                string detalles = (string)data["detalles"] ?? "";

                Artista_titulo.Text = creador.ToUpperInvariant();
                Tipo_titulo.Text = "playlist";
                Titulo.Text = titulo;
                Creador.Text = creador.StartsWith("de ", StringComparison.InvariantCultureIgnoreCase)
                    ? creador
                    : "de " + creador;
                Detalles.Text = detalles;

                EstablecerPortada(portada);

                _listaDeCanciones.Clear();
                JArray arrCanciones = data["canciones"] as JArray;
                if (arrCanciones != null)
                {
                    foreach (JObject c in arrCanciones)
                    {
                        string coverCancion = (string)c["portada"];
                        _listaDeCanciones.Add(new Canciones
                        {
                            Numero = (string)c["numero"] ?? "",
                            Titulo = (string)c["titulo"] ?? "Sin título",
                            Artista = (string)c["artista"] ?? "Desconocido",
                            Duracion = (string)c["duracion"] ?? "",
                            Portada = string.IsNullOrWhiteSpace(coverCancion) ? "/Assets/MusicPreview.png" : coverCancion,
                            Uri = (string)c["uri"] ?? ""
                        });
                    }
                }

                _datosCargados = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error cargando detalle de playlist: " + ex.Message);
                Detalles.Text = "No se pudieron cargar las canciones";
            }
        }

        private void ConstruirBarrasDinamicas()
        {
            barraDefault = new ApplicationBar();
            barraDefault.Mode = ApplicationBarMode.Default;
            barraDefault.Opacity = 1;

            ApplicationBarIconButton btnSeleccionar = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Select.png", UriKind.Relative));
            btnSeleccionar.Text = "seleccionar";
            btnSeleccionar.Click += BtnSeleccionar_Click;
            barraDefault.Buttons.Add(btnSeleccionar);

            barraSeleccion = new ApplicationBar();
            barraSeleccion.Mode = ApplicationBarMode.Default;
            barraSeleccion.Opacity = 1;

            ApplicationBarIconButton btnPlaylist = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Add.png", UriKind.Relative));
            btnPlaylist.Text = "añadir a playlist";
            btnPlaylist.Click += BtnPlaylist_Click;
            barraSeleccion.Buttons.Add(btnPlaylist);

            ApplicationBarIconButton btnCola = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.source.png", UriKind.Relative));
            btnCola.Text = "añadir a cola";
            btnCola.Click += BtnCola_Click;
            barraSeleccion.Buttons.Add(btnCola);
        }

        private void BtnSeleccionar_Click(object sender, EventArgs e)
        {
            ListaCanciones.IsSelectionEnabled = true;
        }

        private void BtnPlaylist_Click(object sender, EventArgs e)
        {
            ListaCanciones.IsSelectionEnabled = false;
        }

        private void BtnCola_Click(object sender, EventArgs e)
        {
            ListaCanciones.IsSelectionEnabled = false;
        }

        private void ListaCanciones_IsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ListaCanciones.IsSelectionEnabled)
            {
                this.ApplicationBar = barraSeleccion;
            }
            else
            {
                this.ApplicationBar = barraDefault;
                ListaCanciones.SelectedItems.Clear();
            }
        }

        private void ListaCanciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaCanciones.IsSelectionEnabled && ListaCanciones.SelectedItems.Count == 0)
            {
                ListaCanciones.IsSelectionEnabled = false;
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (ListaCanciones.IsSelectionEnabled)
            {
                ListaCanciones.IsSelectionEnabled = false;
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }
    }

    public class Canciones
    {
        public string Numero { get; set; }
        public string Titulo { get; set; }
        public string Artista { get; set; }
        public string Duracion { get; set; }
        public string Portada { get; set; }
        public string Uri { get; set; }
    }
}