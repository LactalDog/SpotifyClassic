using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using SpotifyClassic.ViewModels;

namespace SpotifyClassic
{
    public partial class Biblioteca : PhoneApplicationPage
    {
        public Biblioteca()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                if (!App.ViewModel.IsDataLoaded)
                {
                    App.ViewModel.LoadData();
                }

                DataContext = App.ViewModel;
                ActualizarEstadoVacio();
            }

            if (NavigationContext.QueryString.ContainsKey("seccion"))
            {
                string seccionSolicitada = NavigationContext.QueryString["seccion"].ToLowerInvariant();

                Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        if (seccionSolicitada == "playlists")
                            MainPivot.SelectedIndex = 0;
                        else if (seccionSolicitada == "álbumes" || seccionSolicitada == "albumes")
                            MainPivot.SelectedIndex = 1;
                        else if (seccionSolicitada == "me gusta" || seccionSolicitada == "tus me gusta")
                            MainPivot.SelectedIndex = 2;
                        else if (seccionSolicitada == "artistas")
                            MainPivot.SelectedIndex = 3;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Aviso al asignar pestaña en Pivot: " + ex.Message);
                    }
                });
            }
        }

        private void ActualizarEstadoVacio()
        {
            try
            {
                if (App.ViewModel.AlbumesAgrupados == null || App.ViewModel.AlbumesAgrupados.Count == 0)
                {
                    txtVacioAlbumes.Visibility = Visibility.Visible;
                    lstAlbumes.Visibility = Visibility.Collapsed;
                }
                else
                {
                    txtVacioAlbumes.Visibility = Visibility.Collapsed;
                    lstAlbumes.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al actualizar estado vacío: " + ex.Message);
            }
        }

        private void lstAlbumes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstAlbumes.SelectedItem == null) return;

            var album = lstAlbumes.SelectedItem as AlbumModel;
            if (album == null) return;

            // 1. Iniciamos la navegación de forma normal.
            NavigationService.Navigate(new Uri("/Pages/TurnstileFeatherEffectSample1.xaml", UriKind.Relative));

            // 2. Limpiamos la selección asíncronamente. 
            // Al encolarlo en el Dispatcher, el borrado de selección no invalida
            // el layout actual hasta que la transición de salida haya comenzado.
            // Al presionar el botón "Atrás", la lista ya estará deseleccionada
            // y no habrá cambios visuales que rompan la animación de retorno.
            Dispatcher.BeginInvoke(() =>
            {
                lstAlbumes.SelectedItem = null;
            });
        }
    }
}