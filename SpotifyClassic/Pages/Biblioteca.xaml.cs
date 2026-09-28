using System;
using System.Windows;
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

            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }

            DataContext = App.ViewModel;
            this.Loaded += Biblioteca_Loaded;
        }

        private void Biblioteca_Loaded(object sender, RoutedEventArgs e)
        {
            ActualizarEstadoVacio();
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

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

        private void ItemAlbum_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var itemGrid = sender as FrameworkElement;
            if (itemGrid == null) return;

            var album = itemGrid.DataContext as AlbumModel;
            if (album == null) return;

            string url = string.Format("/Pages/AlbumPage.xaml?title={0}&artist={1}",
                Uri.EscapeDataString(album.Titulo ?? ""),
                Uri.EscapeDataString(album.Artista ?? ""));

            NavigationService.Navigate(new Uri(url, UriKind.Relative));
        }
    }
}