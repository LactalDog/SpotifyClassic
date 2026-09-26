using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using System.Net;
using Newtonsoft.Json.Linq;

namespace SpotifyClassic
{
    public class SpotifyTrackItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string CoverUrl { get; set; }
        public string Uri { get; set; }
        public double DurationMs { get; set; }
    }

    public partial class AcercaDe : PhoneApplicationPage
    {
        private const string BackendBaseUrl = "http://192.168.100.20:3000";
        private ObservableCollection<SpotifyTrackItem> _tracks = new ObservableCollection<SpotifyTrackItem>();

        private CancellationTokenSource _loadCancellationTokenSource;
        private string _desiredTrackUri;
        private int _playbackRetryCount = 0;
        private const int MaxPlaybackRetries = 3;

        private DispatcherTimer _timer;
        private bool _isSeeking = false;

        public AcercaDe()
        {
            InitializeComponent();
            lstForYou.ItemsSource = _tracks;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;

            slTimeline.AddHandler(UIElement.ManipulationStartedEvent, new EventHandler<ManipulationStartedEventArgs>(slTimeline_ManipulationStarted), true);
            slTimeline.AddHandler(UIElement.ManipulationCompletedEvent, new EventHandler<ManipulationCompletedEventArgs>(slTimeline_ManipulationCompleted), true);
        }

        // --- HELPERS DE WEBCLIENT COMPATIBLES CON WP8.0 ---
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

        private Task<string> UploadStringTaskAsync(string url, string data, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<string>();
            var client = new WebClient();

            // Si el token solicita cancelar, cancelamos la descarga del WebClient
            token.Register(() => client.CancelAsync());

            client.UploadStringCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.TrySetException(e.Error);
                else if (e.Cancelled) tcs.TrySetCanceled();
                else tcs.TrySetResult(e.Result);
            };
            client.UploadStringAsync(new Uri(url), "POST", data);
            return tcs.Task;
        }
        // ---------------------------------------------------

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_isSeeking && audioStreamPlayer.CurrentState == System.Windows.Media.MediaElementState.Playing)
            {
                TimeSpan currentPos = audioStreamPlayer.Position;

                if (currentPos.TotalSeconds <= slTimeline.Maximum)
                {
                    slTimeline.Value = currentPos.TotalSeconds;
                }

                txtCurrentTime.Text = string.Format("{0}:{1:00}", (int)currentPos.TotalMinutes, currentPos.Seconds);
            }
        }

        private async void lstForYou_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTrack = lstForYou.SelectedItem as SpotifyTrackItem;
            if (selectedTrack == null) return;

            _playbackRetryCount = 0;
            await PlayTrackAsync(selectedTrack, isRetry: false);

            lstForYou.SelectedItem = null;
        }

        private async Task PlayTrackAsync(SpotifyTrackItem track, bool isRetry)
        {
            if (track == null) return;

            if (_loadCancellationTokenSource != null)
            {
                _loadCancellationTokenSource.Cancel();
                _loadCancellationTokenSource.Dispose();
            }
            _loadCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _loadCancellationTokenSource.Token;

            _desiredTrackUri = track.Uri;

            slTimeline.Visibility = Visibility.Collapsed;
            pbLoading.Visibility = Visibility.Visible;

            if (!isRetry)
            {
                txtCurrentTitle.Text = track.Title;
                txtCurrentArtist.Text = track.Artist;

                if (!string.IsNullOrEmpty(track.CoverUrl))
                {
                    imgCurrentCover.Source = new BitmapImage(new Uri(track.CoverUrl, UriKind.Absolute));
                }

                double totalSeconds = track.DurationMs / 1000.0;
                slTimeline.Maximum = totalSeconds > 0 ? totalSeconds : 100;
                slTimeline.Value = 0;

                TimeSpan tsTotal = TimeSpan.FromSeconds(totalSeconds);
                txtTotalTime.Text = string.Format("{0}:{1:00}", (int)tsTotal.TotalMinutes, tsTotal.Seconds);
                txtCurrentTime.Text = "0:00";
            }

            try
            {
                txtStatus.Text = isRetry ? string.Format("Reintentando reproducción ({0}/{1})...", _playbackRetryCount, MaxPlaybackRetries) : "Descargando pista original...";
                audioStreamPlayer.Stop();
                audioStreamPlayer.Source = null;
                _timer.Stop();

                string loadUrl = BackendBaseUrl + "/api/player/load?uri=" + Uri.EscapeDataString(track.Uri);
                string jsonString = await UploadStringTaskAsync(loadUrl, "", cancellationToken);

                if (track.Uri != _desiredTrackUri) return;

                JObject obj = JObject.Parse(jsonString);
                string wavUrl = (string)obj["url"];

                if (track.Uri != _desiredTrackUri) return;

                string timestamp = DateTime.Now.Ticks.ToString();
                audioStreamPlayer.Source = new Uri(wavUrl + "?t=" + timestamp);
                audioStreamPlayer.Play();

                txtStatus.Text = "Reproduciendo: " + track.Title;
            }
            catch (WebException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    System.Diagnostics.Debug.WriteLine("Petición abortada por cambio rápido.");
                    return;
                }

                if (track.Uri == _desiredTrackUri)
                {
                    txtStatus.Text = "Error del servidor al procesar.";
                    pbLoading.Visibility = Visibility.Collapsed;
                    slTimeline.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                if (track.Uri == _desiredTrackUri)
                {
                    txtStatus.Text = "Error de red: " + ex.Message;
                    pbLoading.Visibility = Visibility.Collapsed;
                    slTimeline.Visibility = Visibility.Visible;
                }
            }
        }

        private async void audioStreamPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            txtStatus.Text = "Fallo en el flujo de audio local.";

            if (!string.IsNullOrEmpty(_desiredTrackUri) && _playbackRetryCount < MaxPlaybackRetries)
            {
                _playbackRetryCount++;
                SpotifyTrackItem targetTrack = null;

                foreach (var t in _tracks)
                {
                    if (t.Uri == _desiredTrackUri)
                    {
                        targetTrack = t;
                        break;
                    }
                }

                if (targetTrack != null)
                {
                    try
                    {
                        await Task.Delay(100);
                        if (_desiredTrackUri == targetTrack.Uri)
                        {
                            await PlayTrackAsync(targetTrack, isRetry: true);
                        }
                    }
                    catch (Exception)
                    {
                        // Si el usuario cambió de canción durante la pausa, ignoramos
                    }
                }
            }
            else
            {
                pbLoading.Visibility = Visibility.Collapsed;
                slTimeline.Visibility = Visibility.Visible;
            }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            audioStreamPlayer.Pause();
            txtStatus.Text = "Pausado.";
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            audioStreamPlayer.Play();
            txtStatus.Text = "Reproduciendo...";
        }

        private void slTimeline_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _isSeeking = true;
        }

        private void slTimeline_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (audioStreamPlayer.CurrentState == System.Windows.Media.MediaElementState.Playing ||
                audioStreamPlayer.CurrentState == System.Windows.Media.MediaElementState.Paused)
            {
                audioStreamPlayer.Position = TimeSpan.FromSeconds(slTimeline.Value);
            }
            _isSeeking = false;
        }

        private void slTimeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isSeeking)
            {
                TimeSpan previewTime = TimeSpan.FromSeconds(e.NewValue);
                txtCurrentTime.Text = string.Format("{0}:{1:00}", (int)previewTime.TotalMinutes, previewTime.Seconds);
            }
        }

        private void audioStreamPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            _timer.Start();

            pbLoading.Visibility = Visibility.Collapsed;
            slTimeline.Visibility = Visibility.Visible;
        }

        private async void btnRefreshForYou_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Consultando catálogo...";
            btnRefreshForYou.IsEnabled = false;

            try
            {
                string url = BackendBaseUrl + "/api/for-you";
                string jsonString = await DownloadStringTaskAsync(url);

                JArray items = JArray.Parse(jsonString);

                _tracks.Clear();
                foreach (JObject obj in items)
                {
                    _tracks.Add(new SpotifyTrackItem
                    {
                        Id = (string)obj["id"] ?? "",
                        Title = (string)obj["title"] ?? "Desconocido",
                        Artist = (string)obj["artist"] ?? "Desconocido",
                        Album = (string)obj["album"] ?? "",
                        CoverUrl = (string)obj["coverUrl"] ?? "",
                        Uri = (string)obj["uri"] ?? "",
                        DurationMs = obj["durationMs"] != null ? (double)obj["durationMs"] : 0
                    });
                }
                txtStatus.Text = string.Format("Se cargaron {0} recomendaciones.", _tracks.Count);
            }
            catch (WebException)
            {
                txtStatus.Text = "Error: Inicia sesión en el navegador (/login)";
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Fallo de conexión: " + ex.Message;
            }
            finally
            {
                btnRefreshForYou.IsEnabled = true;
            }
        }

        private void audioStreamPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (audioStreamPlayer.CurrentState == System.Windows.Media.MediaElementState.Buffering)
            {
                // Buffering...
            }
        }
    }
}