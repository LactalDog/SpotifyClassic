using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using SpotifyClassic.Animations;

namespace SpotifyClassic
{
    public partial class Biblioteca : PhoneApplicationPage
    {
        // Guardamos el elemento seleccionado para garantizar que el retorno anime el mismo álbum
        private FrameworkElement _ultimoElementoSeleccionado = null;

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
                if (App.ViewModel.Albumes == null || App.ViewModel.Albumes.Count == 0)
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

            // Si estamos regresando de AlbumPage, reasignamos la animación de retorno al elemento original
            if (e.NavigationMode == NavigationMode.Back && _ultimoElementoSeleccionado != null)
            {
                var navIn = new NavigationInTransition();
                navIn.Backward = new ContinuumTransition(ContinuumTransitionMode.ContinuumBackwardInStoryboard, _ultimoElementoSeleccionado);
                TransitionService.SetNavigationInTransition(this, navIn);
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

        private void ItemAlbum_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (lstAlbumes.IsSelectionEnabled) return;

            var itemGrid = sender as FrameworkElement;
            if (itemGrid == null) return;

            var albumSeleccionado = itemGrid.DataContext as ViewModels.AlbumModel;
            if (albumSeleccionado == null) return;

            // Buscamos el TextBlock específico del título
            var txtTitulo = FindChild<TextBlock>(itemGrid, "txtTituloAlbum");
            _ultimoElementoSeleccionado = (FrameworkElement)txtTitulo ?? itemGrid;

            // Configuramos la transición de salida hacia adelante y la de retorno
            var navOut = new NavigationOutTransition();
            navOut.Forward = new ContinuumTransition(ContinuumTransitionMode.ContinuumForwardOutStoryboard, _ultimoElementoSeleccionado);

            var navIn = new NavigationInTransition();
            navIn.Backward = new ContinuumTransition(ContinuumTransitionMode.ContinuumBackwardInStoryboard, _ultimoElementoSeleccionado);

            TransitionService.SetNavigationOutTransition(this, navOut);
            TransitionService.SetNavigationInTransition(this, navIn);

            string url = string.Format("/Pages/AlbumPage.xaml?title={0}&artist={1}",
                Uri.EscapeDataString(albumSeleccionado.Titulo ?? ""),
                Uri.EscapeDataString(albumSeleccionado.Artista ?? ""));

            NavigationService.Navigate(new Uri(url, UriKind.Relative));
        }

        private static T FindChild<T>(DependencyObject parent, string name) where T : FrameworkElement
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
                var sub = FindChild<T>(child, name);
                if (sub != null) return sub;
            }
            return null;
        }
    }
}