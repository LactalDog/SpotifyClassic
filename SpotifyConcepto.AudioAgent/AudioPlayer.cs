using System;
using System.Diagnostics;
using System.Net;
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

        private void NotificarServidor(string accion, string uri = null, int positionMs = 0)
        {
            try
            {
                string url = string.Format("{0}/api/player/sync?action={1}&positionMs={2}",
                    BackendBaseUrl, accion, Math.Max(0, positionMs));

                if (!string.IsNullOrEmpty(uri))
                {
                    url += "&uri=" + Uri.EscapeDataString(uri);
                }

                var client = new WebClient();
                client.UploadStringAsync(new Uri(url), "POST", "");
            }
            catch { }
        }

        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
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
                        break;

                    case PlayState.TrackEnded:
                        _trackEnded = true;
                        NotificarServidor("pause");
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
            finally
            {
                NotifyComplete();
            }
        }

        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            try
            {
                string trackUri = (track != null) ? track.Tag : null;

                switch (action)
                {
                    case UserAction.Play:
                        if (player.PlayerState == PlayState.Paused)
                        {
                            int posMs = 0;
                            try { posMs = (int)player.Position.TotalMilliseconds; } catch { }
                            player.Play();
                            NotificarServidor("play", trackUri, posMs);
                        }
                        else if (player.PlayerState == PlayState.Stopped && track != null)
                        {
                            if (_trackEnded || player.Position == TimeSpan.Zero)
                            {
                                try { player.Position = TimeSpan.Zero; } catch { }
                                player.Play();
                                _trackEnded = false;
                                NotificarServidor("play", trackUri, 0);
                            }
                        }
                        break;

                    case UserAction.Stop:
                        _trackEnded = false;
                        if (player.PlayerState != PlayState.Stopped)
                        {
                            player.Stop();
                            NotificarServidor("pause");
                        }
                        break;

                    case UserAction.Pause:
                        if (player.PlayerState == PlayState.Playing)
                        {
                            player.Pause();
                            NotificarServidor("pause");
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
                                NotificarServidor("seek", null, (int)newPos.TotalMilliseconds);
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
            finally
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