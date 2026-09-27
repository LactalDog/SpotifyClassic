using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;

namespace SpotifyClassic
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ApplicationBar barraBiblioteca;
        private ApplicationBar barraReproduccion;
        private ApplicationBar barraDefault;
        private PanoramaItem itemReproduciendo;

        private const string BackendBaseUrl = "http://192.168.100.20:3000";
        private const string PlaceholderAsset = "/Assets/MusicPreview.png";

        // Claves de configuración en almacenamiento flash (IsolatedStorageSettings)
        private const string KeyLastCoverUrl = "LastCoverUrl";
        private const string KeyLastTitle = "LastTrackTitle";
        private const string KeyLastArtist = "LastArtistName";
        private const string ArchivoPortadaCache = "cache_ultima_portada.jpg";
        private const string ArchivoFondoCache = "cache_ultimo_fondo.jpg";

        private DispatcherTimer _timer;
        private bool _isSeeking = false;
        private bool _isLoadingTrack = false;
        private string _targetTrackTitulo = "";
        private string _targetTrackArtista = "";
        private int _playRequestId = 0;
        private int _backdropRequestId = 0;
        private ImageBrush _playPausaBrush;

        // Rastreo de la portada para evitar recargas innecesarias
        private string _urlPortadaActual = null;

        // Parámetros de animación fluida por GPU (UIElement.Opacity)
        private const int DuracionFadeMs = 500;
        private string _ultimoArtistaCargado = "";
        private Storyboard _storyboardFondo = null;
        private Image _imgFondoPanorama = null;

        // Control asíncrono para asegurar la secuencia Fade Out -> Fade In
        private Task _tareaFadeOutEnCurso = null;

        // Bandera y datos para diferir la transición del fondo hasta estar en la vista del reproductor
        private bool _transicionFondoPendiente = false;
        private string _uriPistaPendiente = "";
        private string _artistaPendiente = "";
        private int _backdropIdPendiente = 0;

        public MainPage()
        {
            InitializeComponent();

            MainPanorama.Opacity = 1.0;

            // Inicializamos el ImageBrush del Panorama con un ancho de 1024 px
            // para que BackgroundLayer configure inmediatamente su paralaje en 1024 px continuos.
            FondoPanoramaBrush.ImageSource = CrearDummyBackground(1024);

            itemReproduciendo = Reproduciendo;
            if (MainPanorama.Items.Contains(itemReproduciendo))
            {
                MainPanorama.Items.Remove(itemReproduciendo);
            }

            ConstruirBarrasDinamicas();
            this.ApplicationBar = barraBiblioteca;
            DataContext = App.ViewModel;

            // 1. Poblado de listas con placeholders de carga
            CargarPlaceholdersSiEstanVacios();

            // 2. Restauración de datos locales al iniciar si hay reproducción en curso
            RestaurarCacheLocalAlIniciar();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;

            Play_Pausa.Loaded += (s, args) => ActualizarIconoPlayPausa();

            BackgroundAudioPlayer.Instance.PlayStateChanged += (s, args) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    VerificarEstadoCargaPista();
                    ActualizarVisibilidadReproductor(false);
                    ActualizarIconoPlayPausa();

                    if (MainPanorama != null && MainPanorama.SelectedItem == itemReproduciendo)
                    {
                        SincronizarReproductorUI();
                        if (!_isSeeking && !_isLoadingTrack)
                        {
                            ActualizarProgresoReproduccion();
                        }
                    }
                });
            };

            slTimeline.AddHandler(UIElement.ManipulationStartedEvent, new EventHandler<ManipulationStartedEventArgs>(slTimeline_ManipulationStarted), true);
            slTimeline.AddHandler(UIElement.ManipulationCompletedEvent, new EventHandler<ManipulationCompletedEventArgs>(slTimeline_ManipulationCompleted), true);
        }

        // ============================================================================
        // GENERACIÓN DE BITMAP BASE DE 1024 PX PARA FIJAR EL ANCHO DEL PARALAJE
        // ============================================================================
        private static BitmapImage CrearDummyBackground(int ancho)
        {
            try
            {
                var wb = new WriteableBitmap(ancho, 1);
                using (var ms = new MemoryStream())
                {
                    wb.SaveJpeg(ms, ancho, 1, 0, 10);
                    ms.Seek(0, SeekOrigin.Begin);
                    var bmp = new BitmapImage();
                    bmp.SetSource(ms);
                    return bmp;
                }
            }
            catch
            {
                return new BitmapImage();
            }
        }

        // ============================================================================
        // FORMATEO DE TEXTO DE INTERFAZ
        // ============================================================================
        private static string FormatearArtista(string nombreArtista)
        {
            if (string.IsNullOrWhiteSpace(nombreArtista) || nombreArtista == "Desconocido")
            {
                return "Desconocido";
            }

            if (nombreArtista.StartsWith("por ", StringComparison.InvariantCultureIgnoreCase))
            {
                return nombreArtista;
            }

            return "por " + nombreArtista;
        }

        // ============================================================================
        // GESTIÓN DE ARCHIVOS EN ALMACENAMIENTO LOCAL (ISOLATED STORAGE)
        // ============================================================================
        private static void GuardarArchivoLocal(string nombreArchivo, byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return;
            try
            {
                using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var fs = iso.OpenFile(nombreArchivo, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            catch { }
        }

        private static byte[] LeerArchivoLocal(string nombreArchivo)
        {
            try
            {
                using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (iso.FileExists(nombreArchivo))
                    {
                        using (var fs = iso.OpenFile(nombreArchivo, FileMode.Open, FileAccess.Read))
                        {
                            byte[] buffer = new byte[fs.Length];
                            fs.Read(buffer, 0, buffer.Length);
                            return buffer;
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private static void EliminarArchivoLocal(string nombreArchivo)
        {
            try
            {
                using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (iso.FileExists(nombreArchivo))
                    {
                        iso.DeleteFile(nombreArchivo);
                    }
                }
            }
            catch { }
        }

        private Image ObtenerImagenFondoPanorama()
        {
            if (_imgFondoPanorama != null) return _imgFondoPanorama;

            try
            {
                _imgFondoPanorama = FindVisualChild<Image>(MainPanorama, "ImgFondoPanorama");
            }
            catch { }

            return _imgFondoPanorama;
        }

        private void RestaurarFondoArtistaDesdeCache()
        {
            try
            {
                byte[] bytesFondo = LeerArchivoLocal(ArchivoFondoCache);
                if (bytesFondo != null && bytesFondo.Length > 0)
                {
                    using (var ms = new MemoryStream(bytesFondo))
                    {
                        BitmapImage bmp = new BitmapImage();
                        bmp.DecodePixelWidth = 1024;
                        bmp.SetSource(ms);

                        DetenerAnimacionFondo();

                        var img = ObtenerImagenFondoPanorama();
                        if (img != null)
                        {
                            img.Source = bmp;
                            img.Opacity = 1.0;
                        }

                        if (IsolatedStorageSettings.ApplicationSettings.Contains(KeyLastArtist))
                        {
                            _ultimoArtistaCargado = IsolatedStorageSettings.ApplicationSettings[KeyLastArtist] as string;
                        }
                    }
                }
                else
                {
                    var img = ObtenerImagenFondoPanorama();
                    if (img != null)
                    {
                        img.Source = null;
                        img.Opacity = 0.0;
                    }
                }
            }
            catch
            {
                var img = ObtenerImagenFondoPanorama();
                if (img != null)
                {
                    img.Source = null;
                    img.Opacity = 0.0;
                }
            }
        }

        private void RestaurarCacheLocalAlIniciar()
        {
            try
            {
                bool hayPistaActiva = (SafeGetTrack() != null);

                if (hayPistaActiva)
                {
                    string coverUrl = null;
                    if (IsolatedStorageSettings.ApplicationSettings.Contains(KeyLastCoverUrl))
                    {
                        coverUrl = IsolatedStorageSettings.ApplicationSettings[KeyLastCoverUrl] as string;
                    }

                    byte[] bytesPortada = LeerArchivoLocal(ArchivoPortadaCache);
                    if (bytesPortada != null && bytesPortada.Length > 0)
                    {
                        using (var ms = new MemoryStream(bytesPortada))
                        {
                            BitmapImage bmp = new BitmapImage();
                            bmp.DecodePixelWidth = 400;
                            bmp.SetSource(ms);

                            ImageBrush brush = Portada.Background as ImageBrush;
                            if (brush == null)
                            {
                                brush = new ImageBrush { Stretch = Stretch.UniformToFill };
                                Portada.Background = brush;
                            }
                            brush.ImageSource = bmp;
                            _urlPortadaActual = coverUrl;
                        }
                    }
                    else if (!string.IsNullOrEmpty(coverUrl))
                    {
                        CargarPortadaEnReproductor(coverUrl);
                    }
                    else
                    {
                        AsignarPlaceholderPortada();
                    }

                    RestaurarFondoArtistaDesdeCache();

                    if (IsolatedStorageSettings.ApplicationSettings.Contains(KeyLastTitle))
                    {
                        string t = IsolatedStorageSettings.ApplicationSettings[KeyLastTitle] as string;
                        if (!string.IsNullOrEmpty(t)) Título.Text = t;
                    }
                    if (IsolatedStorageSettings.ApplicationSettings.Contains(KeyLastArtist))
                    {
                        string a = IsolatedStorageSettings.ApplicationSettings[KeyLastArtist] as string;
                        if (!string.IsNullOrEmpty(a)) Artista.Text = FormatearArtista(a);
                    }
                }
                else
                {
                    _ultimoArtistaCargado = "";
                    DetenerAnimacionFondo();
                    var img = ObtenerImagenFondoPanorama();
                    if (img != null)
                    {
                        img.Source = null;
                        img.Opacity = 0.0;
                    }
                    AsignarPlaceholderPortada();
                }
            }
            catch { }
        }

        // ============================================================================
        // PRECARGA DE PLACEHOLDERS (SKELETON LOADING)
        // ============================================================================
        private void PoblarPlaceholdersRecientes()
        {
            App.ViewModel.Recientes.Clear();
            for (int i = 0; i < 8; i++)
            {
                App.ViewModel.Recientes.Add(new ViewModels.TrackModel
                {
                    Titulo = "...",
                    Bajada = "...",
                    Portada = "",
                    Uri = ""
                });
            }
        }

        private void CargarPlaceholdersSiEstanVacios()
        {
            if (App.ViewModel.Recientes == null || App.ViewModel.Recientes.Count == 0)
            {
                PoblarPlaceholdersRecientes();
            }

            if (App.ViewModel.Sugerencias == null || App.ViewModel.Sugerencias.Count == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    App.ViewModel.Sugerencias.Add(new ViewModels.SuggestionsModel
                    {
                        Titulo = "...",
                        Tipo = "...",
                        Artista = "...",
                        Portada = ""
                    });
                }
            }

            if (App.ViewModel.Novedades == null || App.ViewModel.Novedades.Count == 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    App.ViewModel.Novedades.Add(new ViewModels.NewsModel
                    {
                        Titulo = "...",
                        Tipo = "...",
                        Artista = "...",
                        Portada = ""
                    });
                }
            }
        }

        // ============================================================================
        // MÉTODOS SEGUROS PARA CONSULTAR EL AGENTE DE AUDIO
        // ============================================================================
        private PlayState SafeGetPlayerState()
        {
            try
            {
                return BackgroundAudioPlayer.Instance.PlayerState;
            }
            catch
            {
                return PlayState.Stopped;
            }
        }

        private AudioTrack SafeGetTrack()
        {
            try
            {
                return BackgroundAudioPlayer.Instance.Track;
            }
            catch
            {
                return null;
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var fe = child as FrameworkElement;
                if (fe != null && fe is T && (string.IsNullOrEmpty(name) || fe.Name == name))
                {
                    return (T)fe;
                }

                var subChild = FindVisualChild<T>(child, name);
                if (subChild != null)
                {
                    return subChild;
                }
            }
            return null;
        }

        private void ActualizarIconoPlayPausa()
        {
            try
            {
                bool isPlaying = !_isLoadingTrack && (SafeGetPlayerState() == PlayState.Playing);
                string uriStr = isPlaying ? "/Assets/AppBar/transport.pause.png" : "/transport.play.png";

                if (_playPausaBrush == null && Play_Pausa != null)
                {
                    Play_Pausa.ApplyTemplate();
                    var border = FindVisualChild<Border>(Play_Pausa, "border");
                    if (border != null)
                    {
                        _playPausaBrush = border.Background as ImageBrush;
                        if (_playPausaBrush == null)
                        {
                            _playPausaBrush = new ImageBrush { Stretch = Stretch.Uniform };
                            border.Background = _playPausaBrush;
                        }
                    }
                }

                if (_playPausaBrush != null)
                {
                    BitmapImage bmp = _playPausaBrush.ImageSource as BitmapImage;
                    if (bmp == null || bmp.UriSource == null || bmp.UriSource.OriginalString != uriStr)
                    {
                        _playPausaBrush.ImageSource = new BitmapImage(new Uri(uriStr, UriKind.Relative));
                    }
                }
            }
            catch { }
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

        private Task<string> UploadStringTaskAsync(string url, string data)
        {
            var tcs = new TaskCompletionSource<string>();
            var client = new WebClient();
            client.UploadStringCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.TrySetException(e.Error);
                else if (e.Cancelled) tcs.TrySetCanceled();
                else tcs.TrySetResult(e.Result);
            };
            client.UploadStringAsync(new Uri(url), "POST", data);
            return tcs.Task;
        }

        private Task<Stream> OpenReadTaskAsync(string url)
        {
            var tcs = new TaskCompletionSource<Stream>();
            var client = new WebClient();
            client.OpenReadCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.TrySetException(e.Error);
                else if (e.Cancelled) tcs.TrySetCanceled();
                else tcs.TrySetResult(e.Result);
            };
            client.OpenReadAsync(new Uri(url));
            return tcs.Task;
        }

        private void MostrarCargaReproduccion()
        {
            if (pbCargando != null)
            {
                pbCargando.Visibility = Visibility.Visible;
                pbCargando.IsIndeterminate = true;
            }
            if (slTimeline != null)
            {
                slTimeline.Visibility = Visibility.Collapsed;
                slTimeline.Value = 0;
            }
            if (textBlock != null) textBlock.Text = "0:00";
            if (textBlock_Copy != null) textBlock_Copy.Text = "-0:00";
            ActualizarIconoPlayPausa();
        }

        private void MostrarSliderReproduccion()
        {
            if (pbCargando != null)
            {
                pbCargando.IsIndeterminate = false;
                pbCargando.Visibility = Visibility.Collapsed;
            }
            if (slTimeline != null)
            {
                slTimeline.Visibility = Visibility.Visible;
            }
            ActualizarIconoPlayPausa();
        }

        private void VerificarEstadoCargaPista()
        {
            if (!_isLoadingTrack) return;

            try
            {
                AudioTrack currentTrack = SafeGetTrack();
                var state = SafeGetPlayerState();

                if (currentTrack != null &&
                    !string.IsNullOrEmpty(_targetTrackTitulo) &&
                    currentTrack.Title == _targetTrackTitulo &&
                    (state == PlayState.Playing || state == PlayState.Paused))
                {
                    _isLoadingTrack = false;
                    MostrarSliderReproduccion();
                }
            }
            catch { }
        }

        private void NavegarAReproductor()
        {
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (MainPanorama == null || itemReproduciendo == null) return;
                    if (MainPanorama.SelectedItem == itemReproduciendo) return;

                    if (MainPanorama.DefaultItem == itemReproduciendo)
                    {
                        MainPanorama.DefaultItem = MainPanorama.SelectedItem ?? Biblioteca;
                        Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                MainPanorama.DefaultItem = itemReproduciendo;
                            }
                            catch { }
                        });
                    }
                    else
                    {
                        MainPanorama.DefaultItem = itemReproduciendo;
                    }
                }
                catch { }
            });
        }

        private void ActualizarVisibilidadReproductor(bool enfocarEnReproductor = false)
        {
            try
            {
                bool hayPistaActiva = (SafeGetTrack() != null) || _isLoadingTrack;

                if (hayPistaActiva)
                {
                    if (!MainPanorama.Items.Contains(itemReproduciendo))
                    {
                        MainPanorama.Items.Insert(0, itemReproduciendo);
                    }

                    if (!_isLoadingTrack)
                    {
                        SincronizarReproductorUI();

                        var img = ObtenerImagenFondoPanorama();
                        if (img != null && img.Source == null)
                        {
                            RestaurarFondoArtistaDesdeCache();
                        }
                    }

                    if (enfocarEnReproductor)
                    {
                        NavegarAReproductor();
                    }
                }
                else
                {
                    if (MainPanorama.Items.Contains(itemReproduciendo))
                    {
                        MainPanorama.Items.Remove(itemReproduciendo);
                    }

                    if (!_isLoadingTrack)
                    {
                        _transicionFondoPendiente = false;
                        _tareaFadeOutEnCurso = AtenuarFondoANegroAsync();
                        _ultimoArtistaCargado = "";
                    }
                }

                ActualizarBarraSegunSeleccion();
            }
            catch { }
        }

        private void SincronizarReproductorUI()
        {
            try
            {
                if (_isLoadingTrack)
                {
                    if (!string.IsNullOrEmpty(_targetTrackTitulo) && Título.Text != _targetTrackTitulo)
                    {
                        Título.Text = _targetTrackTitulo;
                        Artista.Text = FormatearArtista(_targetTrackArtista);
                    }
                    return;
                }

                AudioTrack track = SafeGetTrack();
                if (track != null)
                {
                    if (Título.Text != track.Title && !string.IsNullOrEmpty(track.Title))
                    {
                        Título.Text = track.Title;
                        Artista.Text = FormatearArtista(track.Artist);
                    }

                    RestaurarPortadaReproductor();
                }
            }
            catch { }
        }

        private void RestaurarPortadaReproductor()
        {
            try
            {
                string coverUrl = null;
                if (IsolatedStorageSettings.ApplicationSettings.Contains(KeyLastCoverUrl))
                {
                    coverUrl = IsolatedStorageSettings.ApplicationSettings[KeyLastCoverUrl] as string;
                }

                if (string.IsNullOrEmpty(coverUrl))
                {
                    AsignarPlaceholderPortada();
                    return;
                }

                if (_urlPortadaActual == coverUrl)
                {
                    return;
                }

                byte[] bytes = LeerArchivoLocal(ArchivoPortadaCache);
                if (bytes != null && bytes.Length > 0)
                {
                    using (var ms = new MemoryStream(bytes))
                    {
                        BitmapImage bmp = new BitmapImage();
                        bmp.DecodePixelWidth = 400;
                        bmp.SetSource(ms);

                        ImageBrush brush = Portada.Background as ImageBrush;
                        if (brush == null)
                        {
                            brush = new ImageBrush { Stretch = Stretch.UniformToFill };
                            Portada.Background = brush;
                        }
                        brush.ImageSource = bmp;
                        _urlPortadaActual = coverUrl;
                        return;
                    }
                }

                CargarPortadaEnReproductor(coverUrl);
            }
            catch
            {
                AsignarPlaceholderPortada();
            }
        }

        private async void CargarPortadaEnReproductor(string coverUrl)
        {
            ImageBrush brush = Portada.Background as ImageBrush;
            if (brush == null)
            {
                brush = new ImageBrush { Stretch = Stretch.UniformToFill };
                Portada.Background = brush;
            }

            if (string.IsNullOrEmpty(coverUrl) || coverUrl == PlaceholderAsset)
            {
                AsignarPlaceholderPortada();
                return;
            }

            _urlPortadaActual = coverUrl;

            bool esRemoto = coverUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) ||
                            coverUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase);

            if (esRemoto)
            {
                brush.ImageSource = new BitmapImage(new Uri(PlaceholderAsset, UriKind.Relative));
            }
            else
            {
                try
                {
                    brush.ImageSource = new BitmapImage(new Uri(coverUrl, UriKind.RelativeOrAbsolute));
                    return;
                }
                catch { }
            }

            try
            {
                var stream = await OpenReadTaskAsync(coverUrl);
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);
                    bytes = ms.ToArray();
                }
                stream.Dispose();

                if (_urlPortadaActual != coverUrl)
                {
                    return;
                }

                GuardarArchivoLocal(ArchivoPortadaCache, bytes);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (_urlPortadaActual != coverUrl) return;

                    try
                    {
                        using (var msBmp = new MemoryStream(bytes))
                        {
                            BitmapImage bmp = new BitmapImage();
                            bmp.DecodePixelWidth = 400;
                            bmp.SetSource(msBmp);
                            brush.ImageSource = bmp;
                        }
                    }
                    catch
                    {
                        AsignarPlaceholderPortada();
                    }
                });
            }
            catch
            {
                if (_urlPortadaActual == coverUrl)
                {
                    AsignarPlaceholderPortada();
                }
            }
        }

        private void AsignarPlaceholderPortada()
        {
            try
            {
                _urlPortadaActual = PlaceholderAsset;
                ImageBrush brush = Portada.Background as ImageBrush;
                if (brush == null)
                {
                    brush = new ImageBrush { Stretch = Stretch.UniformToFill };
                    Portada.Background = brush;
                }
                brush.ImageSource = new BitmapImage(new Uri(PlaceholderAsset, UriKind.Relative));
            }
            catch { }
        }

        private void ActualizarProgresoReproduccion()
        {
            try
            {
                if (_isLoadingTrack) return;

                var state = SafeGetPlayerState();

                if (state == PlayState.Playing || state == PlayState.Paused)
                {
                    AudioTrack track = SafeGetTrack();
                    if (track != null)
                    {
                        TimeSpan currentPos = TimeSpan.Zero;
                        try { currentPos = BackgroundAudioPlayer.Instance.Position; } catch { }
                        TimeSpan total = track.Duration;

                        if (total.TotalSeconds > 0)
                        {
                            slTimeline.Maximum = total.TotalSeconds;

                            if (currentPos.TotalSeconds <= slTimeline.Maximum)
                            {
                                slTimeline.Value = currentPos.TotalSeconds;
                            }

                            textBlock.Text = string.Format("{0}:{1:00}", (int)currentPos.TotalMinutes, currentPos.Seconds);

                            TimeSpan restante = total - currentPos;
                            textBlock_Copy.Text = string.Format("-{0}:{1:00}", (int)restante.TotalMinutes, restante.Seconds);
                        }
                    }
                }
                else if (state == PlayState.Stopped)
                {
                    if (!slTimeline.Value.Equals(0))
                    {
                        slTimeline.Value = 0;
                        textBlock.Text = "0:00";
                        AudioTrack track = SafeGetTrack();
                        if (track != null && track.Duration.TotalSeconds > 0)
                        {
                            TimeSpan total = track.Duration;
                            textBlock_Copy.Text = string.Format("-{0}:{1:00}", (int)total.TotalMinutes, total.Seconds);
                        }
                        else
                        {
                            textBlock_Copy.Text = "-0:00";
                        }
                    }
                }
            }
            catch { }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (MainPanorama == null || MainPanorama.SelectedItem != itemReproduciendo)
                {
                    return;
                }

                if (_isLoadingTrack)
                {
                    VerificarEstadoCargaPista();
                    if (_isLoadingTrack) return;
                }

                SincronizarReproductorUI();
                ActualizarIconoPlayPausa();

                if (!_isSeeking)
                {
                    ActualizarProgresoReproduccion();
                }
            }
            catch { }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }

            base.OnNavigatedTo(e);

            CargarPlaceholdersSiEstanVacios();
            ActualizarVisibilidadReproductor(false);

            var state = SafeGetPlayerState();
            if (state == PlayState.Playing || state == PlayState.Paused)
            {
                _isLoadingTrack = false;
                MostrarSliderReproduccion();
            }

            ActualizarIconoPlayPausa();

            CargarRecientesAsync();
            CargarSugerenciasAsync();
            CargarNovedadesAsync();

            try
            {
                _timer.Start();
            }
            catch { }
        }

        private async void CargarNovedadesAsync()
        {
            try
            {
                string jsonString = await DownloadStringTaskAsync(BackendBaseUrl + "/api/home/made-for-you");
                JArray items = JArray.Parse(jsonString);

                App.ViewModel.Novedades.Clear();

                foreach (JObject obj in items)
                {
                    string portada = (string)obj["portada"];
                    App.ViewModel.Novedades.Add(new ViewModels.NewsModel
                    {
                        Titulo = (string)obj["titulo"] ?? "Desconocido",
                        Tipo = (string)obj["tipo"] ?? "Playlist",
                        Artista = (string)obj["artista"] ?? "Spotify",
                        Portada = string.IsNullOrWhiteSpace(portada) ? "" : portada
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Novedades: " + ex.Message);
            }
        }

        private async void CargarRecientesAsync()
        {
            try
            {
                if (App.ViewModel.Recientes.Count == 0)
                {
                    PoblarPlaceholdersRecientes();
                }

                string jsonString = await DownloadStringTaskAsync(BackendBaseUrl + "/api/home/recently-played");
                JArray items = JArray.Parse(jsonString);

                App.ViewModel.Recientes.Clear();

                foreach (JObject obj in items)
                {
                    string portada = (string)obj["portada"];
                    App.ViewModel.Recientes.Add(new ViewModels.TrackModel
                    {
                        Titulo = (string)obj["titulo"] ?? "Desconocido",
                        Bajada = (string)obj["bajada"] ?? "",
                        Portada = string.IsNullOrWhiteSpace(portada) ? "" : portada,
                        Uri = (string)obj["uri"] ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Recientes: " + ex.Message);
            }
        }

        private async void CargarSugerenciasAsync()
        {
            try
            {
                string jsonString = await DownloadStringTaskAsync(BackendBaseUrl + "/api/home/suggestions");
                JArray items = JArray.Parse(jsonString);

                App.ViewModel.Sugerencias.Clear();

                foreach (JObject obj in items)
                {
                    string portada = (string)obj["portada"];
                    string rawArtista = (string)obj["artista"];

                    // Formateamos el creador para que muestre "de <Nombre>"
                    string artistaFormateado = "Spotify";
                    if (!string.IsNullOrWhiteSpace(rawArtista))
                    {
                        artistaFormateado = rawArtista.Trim();
                        if (!artistaFormateado.StartsWith("de ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            artistaFormateado = "de " + artistaFormateado;
                        }
                    }
                    else
                    {
                        artistaFormateado = "de Spotify";
                    }

                    App.ViewModel.Sugerencias.Add(new ViewModels.SuggestionsModel
                    {
                        Titulo = (string)obj["titulo"] ?? "Desconocido",
                        Tipo = (string)obj["tipo"] ?? "Playlist",
                        Artista = artistaFormateado,
                        Portada = string.IsNullOrWhiteSpace(portada) ? "" : portada
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Sugerencias: " + ex.Message);
            }
        }
        private async void lstRecientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTrack = lstRecientes.SelectedItem as ViewModels.TrackModel;
            if (selectedTrack == null) return;

            if (string.IsNullOrEmpty(selectedTrack.Uri))
            {
                lstRecientes.SelectedItem = null;
                return;
            }

            lstRecientes.SelectedItem = null;
            await PlayTrackAsync(selectedTrack);
        }

        // ============================================================================
        // ANIMACIÓN FLUIDA SECUENCIAL A 60 FPS (FADE OUT AWAITABLE -> FADE IN)
        // ============================================================================
        private void DetenerAnimacionFondo()
        {
            if (_storyboardFondo != null)
            {
                _storyboardFondo.Stop();
                _storyboardFondo = null;
            }
        }

        /// <summary>
        /// Realiza la atenuación a negro de forma asíncrona garantizada con limpieza estricta de memoria.
        /// </summary>
        private Task AtenuarFondoANegroAsync()
        {
            if (_tareaFadeOutEnCurso != null && !_tareaFadeOutEnCurso.IsCompleted)
            {
                return _tareaFadeOutEnCurso;
            }

            var tcs = new TaskCompletionSource<bool>();

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var img = ObtenerImagenFondoPanorama();
                    if (img == null)
                    {
                        tcs.TrySetResult(true);
                        return;
                    }

                    if (img.Source != null)
                    {
                        DetenerAnimacionFondo();

                        DoubleAnimation fadeOut = new DoubleAnimation
                        {
                            From = 1.0,
                            To = 0.0,
                            Duration = new Duration(TimeSpan.FromMilliseconds(DuracionFadeMs)),
                            EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
                        };

                        Storyboard sbOut = new Storyboard();
                        sbOut.Children.Add(fadeOut);
                        Storyboard.SetTarget(fadeOut, img);
                        Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));

                        EventHandler onCompleted = null;
                        onCompleted = (s, e) =>
                        {
                            sbOut.Completed -= onCompleted;
                            DetenerAnimacionFondo();
                            img.Source = null;
                            img.Opacity = 0.0;
                            tcs.TrySetResult(true);
                        };

                        sbOut.Completed += onCompleted;
                        _storyboardFondo = sbOut;
                        sbOut.Begin();
                    }
                    else
                    {
                        DetenerAnimacionFondo();
                        img.Source = null;
                        img.Opacity = 0.0;
                        tcs.TrySetResult(true);
                    }
                }
                catch
                {
                    tcs.TrySetResult(false);
                }
            });

            _tareaFadeOutEnCurso = tcs.Task;
            return _tareaFadeOutEnCurso;
        }

        /// <summary>
        /// Muestra la nueva imagen con una transición suave hacia 1.0 de opacidad liberando los buffers de memoria.
        /// </summary>
        private void MostrarNuevaImagenConFadeIn(byte[] bytesNuevaImagen)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var img = ObtenerImagenFondoPanorama();
                    if (img == null) return;

                    DetenerAnimacionFondo();

                    if (bytesNuevaImagen != null && bytesNuevaImagen.Length > 0)
                    {
                        BitmapImage bmp = new BitmapImage();
                        bmp.DecodePixelWidth = 1024;

                        using (var ms = new MemoryStream(bytesNuevaImagen))
                        {
                            bmp.SetSource(ms);
                        }

                        img.Source = bmp;
                        img.Opacity = 0.0;

                        DoubleAnimation fadeIn = new DoubleAnimation
                        {
                            From = 0.0,
                            To = 1.0,
                            Duration = new Duration(TimeSpan.FromMilliseconds(DuracionFadeMs)),
                            EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn }
                        };

                        Storyboard sbIn = new Storyboard();
                        sbIn.Children.Add(fadeIn);
                        Storyboard.SetTarget(fadeIn, img);
                        Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));

                        EventHandler onCompleted = null;
                        onCompleted = (s, e) =>
                        {
                            sbIn.Completed -= onCompleted;
                            img.Opacity = 1.0;
                            _storyboardFondo = null;
                        };

                        sbIn.Completed += onCompleted;
                        _storyboardFondo = sbIn;
                        sbIn.Begin();
                    }
                    else
                    {
                        img.Source = null;
                        img.Opacity = 0.0;
                    }
                }
                catch
                {
                    var img = ObtenerImagenFondoPanorama();
                    if (img != null)
                    {
                        img.Source = null;
                        img.Opacity = 0.0;
                    }
                }
            });
        }

        private async Task CargarFondoArtistaAsync(string trackUri, string nombreArtista, int backdropId)
        {
            if (string.IsNullOrEmpty(trackUri)) return;

            try
            {
                string endpoint = string.Format("{0}/api/player/backdrop?uri={1}",
                    BackendBaseUrl, Uri.EscapeDataString(trackUri));

                string jsonString = await DownloadStringTaskAsync(endpoint);

                if (backdropId != _backdropRequestId) return;

                JObject data = JObject.Parse(jsonString);
                string backdropUrl = (string)data["backdropUrl"];

                if (!string.IsNullOrEmpty(backdropUrl))
                {
                    byte[] bytes;
                    using (var stream = await OpenReadTaskAsync(backdropUrl))
                    {
                        if (backdropId != _backdropRequestId) return;

                        using (var ms = new MemoryStream())
                        {
                            await stream.CopyToAsync(ms);
                            bytes = ms.ToArray();
                        }
                    }

                    if (backdropId != _backdropRequestId) return;

                    GuardarArchivoLocal(ArchivoFondoCache, bytes);

                    IsolatedStorageSettings.ApplicationSettings[KeyLastArtist] = nombreArtista ?? "";
                    IsolatedStorageSettings.ApplicationSettings.Save();

                    if (_tareaFadeOutEnCurso != null)
                    {
                        await _tareaFadeOutEnCurso;
                    }

                    if (backdropId != _backdropRequestId) return;

                    MostrarNuevaImagenConFadeIn(bytes);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al obtener fondo de artista: " + ex.Message);
            }
        }

        // ============================================================================
        // REPRODUCCIÓN CONTROLADA Y PERSISTENCIA DE DATOS
        // ============================================================================
        private async Task PlayTrackAsync(ViewModels.TrackModel track)
        {
            if (track == null) return;

            int currentRequestId = ++_playRequestId;

            _isLoadingTrack = true;
            _targetTrackTitulo = string.IsNullOrEmpty(track.Titulo) ? "Desconocido" : track.Titulo;
            _targetTrackArtista = string.IsNullOrEmpty(track.Bajada) ? "Desconocido" : track.Bajada;

            _timer.Stop();
            MostrarCargaReproduccion();

            // 1. Asignación inmediata de textos
            Título.Text = _targetTrackTitulo;
            Artista.Text = FormatearArtista(_targetTrackArtista);

            // 2. Cambio inmediato de portada
            CargarPortadaEnReproductor(track.Portada);

            // 3. Persistencia de metadatos en almacenamiento flash
            IsolatedStorageSettings.ApplicationSettings[KeyLastCoverUrl] = track.Portada ?? "";
            IsolatedStorageSettings.ApplicationSettings[KeyLastTitle] = _targetTrackTitulo;
            IsolatedStorageSettings.ApplicationSettings.Save();

            ActualizarVisibilidadReproductor(true);

            // 4. Actualización coordinada del fondo panorámico
            bool esMismoArtista = !string.IsNullOrEmpty(_ultimoArtistaCargado) &&
                                  _ultimoArtistaCargado.Equals(_targetTrackArtista, StringComparison.InvariantCultureIgnoreCase);

            if (!esMismoArtista)
            {
                _ultimoArtistaCargado = _targetTrackArtista;
                int currentBackdropId = ++_backdropRequestId;
                EliminarArchivoLocal(ArchivoFondoCache);

                if (MainPanorama != null && MainPanorama.SelectedItem == itemReproduciendo)
                {
                    _transicionFondoPendiente = false;
                    _tareaFadeOutEnCurso = AtenuarFondoANegroAsync();
                    var tareaFondo = CargarFondoArtistaAsync(track.Uri, _targetTrackArtista, currentBackdropId);
                }
                else
                {
                    _transicionFondoPendiente = true;
                    _uriPistaPendiente = track.Uri;
                    _artistaPendiente = _targetTrackArtista;
                    _backdropIdPendiente = currentBackdropId;
                }
            }

            try
            {
                var currentState = SafeGetPlayerState();
                if (currentState == PlayState.Playing || currentState == PlayState.Paused)
                {
                    BackgroundAudioPlayer.Instance.Stop();
                    await Task.Delay(250);
                }
            }
            catch { }

            await IniciarCargaYReproduccionAsync(track, currentRequestId, 1);
        }

        private async Task IniciarCargaYReproduccionAsync(ViewModels.TrackModel track, int requestId, int intento)
        {
            const int MaxIntentos = 3;

            try
            {
                System.Diagnostics.Debug.WriteLine(string.Format("=== CARGANDO PISTA (Intento {0}/{1}) ===", intento, MaxIntentos));

                string loadUrl = BackendBaseUrl + "/api/player/load?uri=" + Uri.EscapeDataString(track.Uri);
                string jsonString = await UploadStringTaskAsync(loadUrl, "");

                if (requestId != _playRequestId) return;

                JObject obj = JObject.Parse(jsonString);
                string m4aUrl = (string)obj["url"];
                if (string.IsNullOrEmpty(m4aUrl))
                {
                    throw new Exception("URL vacía devuelta por el backend.");
                }

                Uri audioUri = new Uri(m4aUrl, UriKind.Absolute);

                AudioTrack audioTrack = new AudioTrack(
                    audioUri,
                    _targetTrackTitulo,
                    _targetTrackArtista,
                    "Spotify Classic",
                    null
                );

                if (requestId != _playRequestId) return;

                System.Diagnostics.Debug.WriteLine("=== ASIGNANDO PISTA AL AGENTE ===");

                try
                {
                    BackgroundAudioPlayer.Instance.Track = audioTrack;
                }
                catch (Exception exTrack)
                {
                    System.Diagnostics.Debug.WriteLine("Aviso al asignar Track (reintentando): " + exTrack.Message);
                    await Task.Delay(300);
                    if (requestId != _playRequestId) return;
                    BackgroundAudioPlayer.Instance.Track = audioTrack;
                }

                _timer.Start();

                int comprobaciones = 0;
                while (comprobaciones < 16)
                {
                    await Task.Delay(500);

                    if (requestId != _playRequestId) return;

                    var state = SafeGetPlayerState();
                    var currentTrack = SafeGetTrack();

                    if (currentTrack != null && currentTrack.Title == _targetTrackTitulo &&
                        (state == PlayState.Playing || state == PlayState.Paused))
                    {
                        _isLoadingTrack = false;
                        MostrarSliderReproduccion();
                        return;
                    }

                    comprobaciones++;
                }

                throw new TimeoutException("Tiempo de espera agotado sin confirmación de reproducción.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Fallo en intento {0}: {1}", intento, ex.Message));

                if (requestId != _playRequestId) return;

                if (intento < MaxIntentos)
                {
                    try
                    {
                        var state = SafeGetPlayerState();
                        if (state == PlayState.Playing || state == PlayState.Paused)
                        {
                            BackgroundAudioPlayer.Instance.Stop();
                        }
                    }
                    catch { }

                    await Task.Delay(500);
                    await IniciarCargaYReproduccionAsync(track, requestId, intento + 1);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Se agotaron los 3 intentos para cargar la pista.");
                    _isLoadingTrack = false;
                    MostrarSliderReproduccion();
                }
            }
        }

        private void Play_Pausa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoadingTrack) return;

                var state = SafeGetPlayerState();
                if (state == PlayState.Playing)
                {
                    BackgroundAudioPlayer.Instance.Pause();
                }
                else if (state == PlayState.Paused)
                {
                    BackgroundAudioPlayer.Instance.Play();
                }
                else if (state == PlayState.Stopped)
                {
                    if (SafeGetTrack() != null)
                    {
                        try { BackgroundAudioPlayer.Instance.Position = TimeSpan.Zero; } catch { }
                        BackgroundAudioPlayer.Instance.Play();
                    }
                }

                ActualizarIconoPlayPausa();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al presionar Play/Pausa: " + ex.Message);
            }
        }

        private void slTimeline_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _isSeeking = true;
            if (_timer != null && _timer.IsEnabled)
            {
                _timer.Stop();
            }
        }

        private void slTimeline_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            try
            {
                var state = SafeGetPlayerState();
                if (state == PlayState.Playing || state == PlayState.Paused)
                {
                    double segundosObjetivo = slTimeline.Value;
                    TimeSpan nuevaPosicion = TimeSpan.FromSeconds(segundosObjetivo);

                    AudioTrack pistaActual = SafeGetTrack();
                    if (pistaActual != null && nuevaPosicion <= pistaActual.Duration)
                    {
                        // Asignamos la nueva posición de forma atómica en el despachador
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                BackgroundAudioPlayer.Instance.Position = nuevaPosicion;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("Error al asignar Position: " + ex.Message);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ManipulationCompleted: " + ex.Message);
            }
            finally
            {
                // Margen de estabilización para permitir que el hardware del altavoz se asiente
                var timerEspera = new DispatcherTimer();
                timerEspera.Interval = TimeSpan.FromMilliseconds(300);
                timerEspera.Tick += (s, args) =>
                {
                    timerEspera.Stop();
                    _isSeeking = false;
                    if (_timer != null && !_timer.IsEnabled)
                    {
                        _timer.Start();
                    }
                };
                timerEspera.Start();
            }
        }

        // Se ejecuta en cada movimiento del slider para actualizar los números en pantalla
        private void slTimeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Solo actualizamos las etiquetas de texto visuales durante el arrastre manual
            if (_isSeeking)
            {
                TimeSpan tiempoPrevio = TimeSpan.FromSeconds(e.NewValue);
                if (textBlock != null)
                {
                    textBlock.Text = string.Format("{0}:{1:00}", (int)tiempoPrevio.TotalMinutes, tiempoPrevio.Seconds);
                }

                AudioTrack track = SafeGetTrack();
                if (track != null && track.Duration.TotalSeconds > 0 && textBlock_Copy != null)
                {
                    TimeSpan restante = track.Duration - tiempoPrevio;
                    textBlock_Copy.Text = string.Format("-{0}:{1:00}", (int)restante.TotalMinutes, restante.Seconds);
                }
            }
        }

        private void IrAcercaDe(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/AcercaDe.xaml", UriKind.Relative));
        }

        private void AbrirSeccionBiblioteca(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var itemSeleccionado = (ViewModels.ItemViewModel)e.AddedItems[0];
            if (itemSeleccionado == null) return;

            string ruta = "/Pages/Biblioteca.xaml?seccion=" + itemSeleccionado.LineOne;
            NavigationService.Navigate(new Uri(ruta, UriKind.Relative));

            ((Microsoft.Phone.Controls.LongListSelector)sender).SelectedItem = null;
        }

        private void ConstruirBarrasDinamicas()
        {
            barraBiblioteca = new ApplicationBar();
            barraBiblioteca.Mode = ApplicationBarMode.Default;
            barraBiblioteca.Opacity = 0.95;

            ApplicationBarIconButton btnAleatorio = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.shuffle.png", UriKind.Relative));
            btnAleatorio.Text = "orden aleatorio";
            barraBiblioteca.Buttons.Add(btnAleatorio);

            ApplicationBarIconButton btnBuscar = new ApplicationBarIconButton(new Uri("/Assets/AppBar/feature.search.png", UriKind.Relative));
            btnBuscar.Text = "buscar";
            barraBiblioteca.Buttons.Add(btnBuscar);

            ApplicationBarMenuItem menuConfigBiblio = new ApplicationBarMenuItem("configuración");
            barraBiblioteca.MenuItems.Add(menuConfigBiblio);

            ApplicationBarMenuItem menuAcercaBiblio = new ApplicationBarMenuItem("acerca de");
            menuAcercaBiblio.Click += IrAcercaDe;
            barraBiblioteca.MenuItems.Add(menuAcercaBiblio);

            barraReproduccion = new ApplicationBar();
            barraReproduccion.Mode = ApplicationBarMode.Minimized;
            barraReproduccion.Opacity = 0.95;

            ApplicationBarMenuItem menuConfigRepro = new ApplicationBarMenuItem("configuración");
            barraReproduccion.MenuItems.Add(menuConfigRepro);

            ApplicationBarMenuItem menuAcercaDeRepro = new ApplicationBarMenuItem("acerca de");
            menuAcercaDeRepro.Click += IrAcercaDe;
            barraReproduccion.MenuItems.Add(menuAcercaDeRepro);

            barraDefault = new ApplicationBar();
            barraDefault.Mode = ApplicationBarMode.Default;
            barraDefault.Opacity = 0.95;

            ApplicationBarIconButton btnBuscarDef = new ApplicationBarIconButton(new Uri("/Assets/AppBar/feature.search.png", UriKind.Relative));
            btnBuscarDef.Text = "buscar";
            barraDefault.Buttons.Add(btnBuscarDef);

            ApplicationBarMenuItem menuConfigDef = new ApplicationBarMenuItem("configuración");
            barraDefault.MenuItems.Add(menuConfigDef);

            ApplicationBarMenuItem menuAcercaDeDef = new ApplicationBarMenuItem("acerca de");
            menuAcercaDeDef.Click += IrAcercaDe;
            barraDefault.MenuItems.Add(menuAcercaDeDef);
        }

        private void ActualizarBarraSegunSeleccion()
        {
            if (MainPanorama == null) return;

            if (MainPanorama.SelectedItem == Biblioteca)
            {
                this.ApplicationBar = barraBiblioteca;
            }
            else if (MainPanorama.SelectedItem == itemReproduciendo)
            {
                this.ApplicationBar = barraReproduccion;
            }
            else
            {
                this.ApplicationBar = barraDefault;
            }
        }

        private void CambioDeVistaPanorama(object sender, SelectionChangedEventArgs e)
        {
            ActualizarBarraSegunSeleccion();

            if (MainPanorama != null && MainPanorama.SelectedItem == itemReproduciendo)
            {
                SincronizarReproductorUI();
                ActualizarIconoPlayPausa();
                if (!_isSeeking && !_isLoadingTrack)
                {
                    ActualizarProgresoReproduccion();
                }

                // Al llegar a la vista del reproductor, ejecutamos la transición pendiente
                if (_transicionFondoPendiente)
                {
                    _transicionFondoPendiente = false;
                    _tareaFadeOutEnCurso = AtenuarFondoANegroAsync();
                    var tareaFondo = CargarFondoArtistaAsync(_uriPistaPendiente, _artistaPendiente, _backdropIdPendiente);
                }
            }
        }
    }
}