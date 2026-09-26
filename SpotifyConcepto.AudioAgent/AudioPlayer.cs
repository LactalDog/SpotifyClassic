using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.BackgroundAudio;

namespace SpotifyConcepto.AudioAgent
{
    public class AudioPlayer : AudioPlayerAgent
    {
        private static bool _trackEnded = false;

        static AudioPlayer()
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // Evita que excepciones nativas de Media Foundation o de red cierren HeadlessHost
            e.Handled = true;
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
                        // Si tras un salto temporal el búfer se completó y no arrancó automáticamente
                        if (!_trackEnded && player.PlayerState == PlayState.Paused)
                        {
                            player.Play();
                        }
                        break;

                    case PlayState.Playing:
                        _trackEnded = false;
                        break;

                    case PlayState.Shutdown:
                    case PlayState.Unknown:
                    case PlayState.Stopped:
                    case PlayState.Paused:
                    case PlayState.BufferingStarted:
                    case PlayState.Rewinding:
                    case PlayState.FastForwarding:
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
                switch (action)
                {
                    case UserAction.Play:
                        if (player.PlayerState == PlayState.Paused)
                        {
                            player.Play();
                        }
                        else if (player.PlayerState == PlayState.Stopped && track != null)
                        {
                            if (_trackEnded || player.Position == TimeSpan.Zero)
                            {
                                try
                                {
                                    player.Position = TimeSpan.Zero;
                                }
                                catch { }

                                player.Play();
                                _trackEnded = false;
                            }
                        }
                        break;

                    case UserAction.Stop:
                        _trackEnded = false;
                        if (player.PlayerState != PlayState.Stopped)
                        {
                            player.Stop();
                        }
                        break;

                    case UserAction.Pause:
                        if (player.PlayerState == PlayState.Playing)
                        {
                            player.Pause();
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
                                // Asignamos la nueva posición dentro del entorno protegido del agente
                                player.Position = newPos;
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