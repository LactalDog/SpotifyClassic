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
    public partial class AlbumPage : PhoneApplicationPage
    {
        private const string BackendBaseUrl = "http://192.168.100.20:3000";
        private ApplicationBar barraDefault;
        private ApplicationBar barraSeleccion;
        private ObservableCollection<Cancion> _listaDeCanciones = new ObservableCollection<Cancion>();
        private bool _datosCargados = false;

        public AlbumPage()
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
            AlbumModel alb = App.ViewModel.AlbumSeleccionado;
            if (alb != null)
            {
                string artistaStr = string.IsNullOrWhiteSpace(alb.Artista) ? "DESCONOCIDO" : alb.Artista;
                Artista_titulo.Text = artistaStr.ToUpperInvariant();
                Tipo_titulo.Text = string.IsNullOrWhiteSpace(alb.Tipo) ? "álbum" : alb.Tipo.ToLowerInvariant();

                Año.Text = alb.Año ?? "";
                Titulo.Text = alb.Titulo ?? "Sin título";
                Artista.Text = alb.Artista ?? "Desconocido";
                Detalles.Text = "Cargando canciones...";

                EstablecerPortada(alb.Portada);
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
            else if (App.ViewModel.AlbumSeleccionado != null)
            {
                uriParam = App.ViewModel.AlbumSeleccionado.Uri;
            }

            if (!string.IsNullOrWhiteSpace(uriParam))
            {
                await CargarDetalleAlbumAsync(uriParam);
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

        private async Task CargarDetalleAlbumAsync(string albumUri)
        {
            try
            {
                string[] partes = albumUri.Split(':');
                string albumId = partes[partes.Length - 1];

                string url = string.Format("{0}/api/album/{1}", BackendBaseUrl, Uri.EscapeDataString(albumId));
                string jsonString = await DownloadStringTaskAsync(url);

                JObject data = JObject.Parse(jsonString);

                string titulo = (string)data["titulo"] ?? "Sin título";
                string artista = (string)data["artista"] ?? "Desconocido";
                string artistaPrincipal = (string)data["artistaPrincipal"] ?? artista;
                string anio = (string)data["anio"] ?? "";
                string tipo = (string)data["tipo"] ?? "álbum";
                string portada = (string)data["portada"] ?? "/Assets/MusicPreview.png";
                string detalles = (string)data["detalles"] ?? "";

                Artista_titulo.Text = artistaPrincipal.ToUpperInvariant();
                Tipo_titulo.Text = tipo.ToLowerInvariant();

                Año.Text = anio;
                Titulo.Text = titulo;
                Artista.Text = artista;
                Detalles.Text = detalles;

                EstablecerPortada(portada);

                _listaDeCanciones.Clear();
                JArray arrCanciones = data["canciones"] as JArray;
                if (arrCanciones != null)
                {
                    foreach (JObject c in arrCanciones)
                    {
                        _listaDeCanciones.Add(new Cancion
                        {
                            Numero = (string)c["numero"] ?? "",
                            Titulo = (string)c["titulo"] ?? "Sin título",
                            Artista = (string)c["artista"] ?? artista,
                            Duracion = (string)c["duracion"] ?? "",
                            Portada = portada,
                            Uri = (string)c["uri"] ?? ""
                        });
                    }
                }

                _datosCargados = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error cargando detalle de álbum: " + ex.Message);
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

    public class Cancion
    {
        public string Numero { get; set; }
        public string Titulo { get; set; }
        public string Artista { get; set; }
        public string Duracion { get; set; }
        public string Portada { get; set; }
        public string Uri { get; set; }
    }
}