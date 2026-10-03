using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows;
using Microsoft.Phone.BackgroundAudio;

namespace SpotifyConcepto.AudioAgent
{
    public class AudioPlayer : AudioPlayerAgent
    {
        private static bool _trackEnded = false;
        private const string BackendBaseUrl = "http://192.168.100.20:3000";

        static AudioPlayer()
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Envía el estado al servidor y espera (máximo 2500ms) antes de que NotifyComplete() congele el agente.
        /// </summary>
        private void NotificarServidorSincrono(string accion, string uri = null, int positionMs = 0)
        {
            try
            {
                string url = string.Format("{0}/api/player/sync?action={1}&positionMs={2}&t={3}",
                    BackendBaseUrl, accion, Math.Max(0, positionMs), DateTime.UtcNow.Ticks);

                if (!string.IsNullOrEmpty(uri))
                {
                    url += "&uri=" + Uri.EscapeDataString(uri);
                }

                using (ManualResetEvent doneEvent = new ManualResetEvent(false))
                {
                    var client = new WebClient();
                    client.UploadStringCompleted += (s, e) =>
                    {
                        try { doneEvent.Set(); } catch { }
                    };
                    client.UploadStringAsync(new Uri(url), "POST", "");

                    // Esperamos hasta 2.5 segundos en un hilo fuera del Dispatcher para que el paquete salga por red
                    doneEvent.WaitOne(2500);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error en NotificarServidorSincrono: " + ex.Message);
            }
        }

        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
            string syncAction = null;
            string syncUri = (track != null) ? track.Tag : null;
            int syncPosMs = 0;

            try
            {
                switch (playState)
                {
                    case PlayState.TrackReady:
                        _trackEnded = false;
                        if (player.PlayerState != PlayState.Playing)
                        {
                            player.Play();
                        }
                        syncAction = "play";
                        syncPosMs = 0;
                        break;

                    case PlayState.TrackEnded:
                        _trackEnded = true;
                        syncAction = "pause";
                        try
                        {
                            player.Position = TimeSpan.Zero;
                        }
                        catch { }

                        if (player.PlayerState != PlayState.Stopped)
                        {
                            player.Stop();
                        }
                        break;

                    case PlayState.BufferingStopped:
                        if (!_trackEnded && player.PlayerState == PlayState.Paused)
                        {
                            player.Play();
                        }
                        break;

                    case PlayState.Playing:
                        _trackEnded = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error en OnPlayStateChanged: " + ex.Message);
            }

            if (syncAction != null)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    NotificarServidorSincrono(syncAction, syncUri, syncPosMs);
                    NotifyComplete();
                });
            }
            else
            {
                NotifyComplete();
            }
        }

        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            string syncAction = null;
            string syncUri = (track != null) ? track.Tag : null;
            int syncPosMs = 0;

            try
            {
                switch (action)
                {
                    case UserAction.Play:
                        if (player.PlayerState == PlayState.Paused)
                        {
                            try { syncPosMs = (int)player.Position.TotalMilliseconds; } catch { }
                            player.Play();
                            syncAction = "play";
                        }
                        else if (player.PlayerState == PlayState.Stopped && track != null)
                        {
                            if (_trackEnded || player.Position == TimeSpan.Zero)
                            {
                                try { player.Position = TimeSpan.Zero; } catch { }
                                player.Play();
                                _trackEnded = false;
                                syncAction = "play";
                                syncPosMs = 0;
                            }
                        }
                        break;

                    case UserAction.Stop:
                        _trackEnded = false;
                        if (player.PlayerState != PlayState.Stopped)
                        {
                            player.Stop();
                            syncAction = "pause";
                        }
                        break;

                    case UserAction.Pause:
                        if (player.PlayerState == PlayState.Playing)
                        {
                            player.Pause();
                            syncAction = "pause";
                        }
                        break;

                    case UserAction.FastForward:
                        if (player.PlayerState == PlayState.Playing || player.PlayerState == PlayState.Paused)
                        {
                            player.FastForward();
                        }
                        break;

                    case UserAction.Rewind:
                        if (player.PlayerState == PlayState.Playing || player.PlayerState == PlayState.Paused)
                        {
                            player.Rewind();
                        }
                        break;

                    case UserAction.Seek:
                        if (param is TimeSpan && track != null)
                        {
                            TimeSpan newPos = (TimeSpan)param;
                            if (newPos <= track.Duration)
                            {
                                player.Position = newPos;
                                syncAction = "seek";
                                syncPosMs = (int)newPos.TotalMilliseconds;
                            }
                        }
                        break;

                    case UserAction.SkipNext:
                        AudioTrack nextTrack = GetNextTrack();
                        if (nextTrack != null)
                        {
                            player.Track = nextTrack;
                        }
                        break;

                    case UserAction.SkipPrevious:
                        AudioTrack previousTrack = GetPreviousTrack();
                        if (previousTrack != null)
                        {
                            player.Track = previousTrack;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error en OnUserAction: " + ex.Message);
            }

            if (syncAction != null)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    NotificarServidorSincrono(syncAction, syncUri, syncPosMs);
                    NotifyComplete();
                });
            }
            else
            {
                NotifyComplete();
            }
        }

        private AudioTrack GetNextTrack()
        {
            return null;
        }

        private AudioTrack GetPreviousTrack()
        {
            return null;
        }

        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
        {
            Debug.WriteLine("--- ERROR EN AUDIO AGENT: " + error.Message + " ---");
            try
            {
                if (isFatal && player.PlayerState != PlayState.Stopped)
                {
                    player.Stop();
                }
            }
            catch { }
            finally
            {
                NotifyComplete();
            }
        }

        protected override void OnCancel()
        {
        }
    }
}