using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SpotifyClassic.ViewModels;

namespace SpotifyClassic
{
    public partial class Biblioteca : PhoneApplicationPage
    {
        private ApplicationBar barraPlaylists; // NUEVO
        private ApplicationBar barraCancionesDefault;
        private ApplicationBar barraSeleccion;

        public Biblioteca()
        {
            InitializeComponent();
            ConstruirBarrasDinamicas();
            MainPivot.SelectionChanged += MainPivot_SelectionChanged;
        }

        private void ConstruirBarrasDinamicas()
        {
            // 0. NUEVO: Barra específica para Playlists
            barraPlaylists = new ApplicationBar();
            barraPlaylists.Mode = ApplicationBarMode.Default;
            barraPlaylists.Opacity = 0.99;

            ApplicationBarIconButton btnCrearPlaylist = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Add.png", UriKind.Relative));
            btnCrearPlaylist.Text = "crear";
            btnCrearPlaylist.Click += BtnCrearPlaylist_Click;
            barraPlaylists.Buttons.Add(btnCrearPlaylist);

            // 1. Barra específica para la pestaña "Me gusta"
            barraCancionesDefault = new ApplicationBar();
            barraCancionesDefault.Mode = ApplicationBarMode.Default;
            barraCancionesDefault.Opacity = 0.99;

            ApplicationBarIconButton btnSeleccionar = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Select.png", UriKind.Relative));
            btnSeleccionar.Text = "seleccionar";
            btnSeleccionar.Click += BtnSeleccionar_Click;

            ApplicationBarIconButton btnBuscar = new ApplicationBarIconButton(new Uri("/Assets/AppBar/feature.search.png", UriKind.Relative));
            btnBuscar.Text = "buscar";

            barraCancionesDefault.Buttons.Add(btnSeleccionar);
            barraCancionesDefault.Buttons.Add(btnBuscar);

            // 2. Barra activa durante la multiselección
            barraSeleccion = new ApplicationBar();
            barraSeleccion.Mode = ApplicationBarMode.Default;
            barraSeleccion.Opacity = 0.99;

            ApplicationBarIconButton btnPlaylist = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Add.png", UriKind.Relative));
            btnPlaylist.Text = "añadir a playlist";
            btnPlaylist.Click += BtnPlaylist_Click;
            barraSeleccion.Buttons.Add(btnPlaylist);

            ApplicationBarIconButton btnCola = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.source.png", UriKind.Relative));
            btnCola.Text = "añadir a cola";
            btnCola.Click += BtnCola_Click;
            barraSeleccion.Buttons.Add(btnCola);
        }

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarBarraSegunSeleccion();
        }

        private void ActualizarBarraSegunSeleccion()
        {
            if (MainPivot.SelectedIndex == 0) // Pestaña Playlists
            {
                if (ListaCancionesMeGusta != null && ListaCancionesMeGusta.IsSelectionEnabled)
                {
                    ListaCancionesMeGusta.IsSelectionEnabled = false;
                }
                this.ApplicationBar = barraPlaylists;
            }
            else if (MainPivot.SelectedIndex == 2) // Pestaña Me Gusta
            {
                if (ListaCancionesMeGusta != null && ListaCancionesMeGusta.IsSelectionEnabled)
                    this.ApplicationBar = barraSeleccion;
                else
                    this.ApplicationBar = barraCancionesDefault;
            }
            else // Pestañas Álbumes, Artistas
            {
                if (ListaCancionesMeGusta != null && ListaCancionesMeGusta.IsSelectionEnabled)
                {
                    ListaCancionesMeGusta.IsSelectionEnabled = false;
                }
                this.ApplicationBar = null;
            }
        }

        // --- MANEJADORES DE BOTONES ---
        private void BtnCrearPlaylist_Click(object sender, EventArgs e)
        {
            // TODO: Lógica para mostrar diálogo o ir a página de creación de playlist
            MessageBox.Show("Crear nueva playlist (En desarrollo)");
        }

        private void BtnSeleccionar_Click(object sender, EventArgs e)
        {
            ListaCancionesMeGusta.IsSelectionEnabled = true;
        }

        private void BtnPlaylist_Click(object sender, EventArgs e)
        {
            ListaCancionesMeGusta.IsSelectionEnabled = false;
        }

        private void BtnCola_Click(object sender, EventArgs e)
        {
            ListaCancionesMeGusta.IsSelectionEnabled = false;
        }

        // --- EVENTOS DE LISTAS ---
        private void lstPlaylists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstPlaylists.SelectedItem == null) return;
            var playlist = lstPlaylists.SelectedItem as PlaylistModel;
            if (playlist == null) return;

            NavigationService.Navigate(new Uri("/Pages/PlaylistPage.xaml", UriKind.Relative));
            lstPlaylists.SelectedItem = null; // Limpiar selección para permitir volver a clickear
        }

        private void lstAlbumes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstAlbumes.SelectedItem == null) return;
            var album = lstAlbumes.SelectedItem as AlbumModel;
            if (album == null) return;

            NavigationService.Navigate(new Uri("/Pages/AlbumPage.xaml", UriKind.Relative));
            lstAlbumes.SelectedItem = null;
        }

        private void lstArtistas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstArtistas.SelectedItem == null) return;
            var artista = lstArtistas.SelectedItem as ArtistModel;
            if (artista == null) return;

            // Navegar a la página del perfil del artista
            NavigationService.Navigate(new Uri("/Pages/ArtistProfile.xaml", UriKind.Relative));

            lstArtistas.SelectedItem = null; // Limpiar selección
        }

        
        private void ListaCancionesMeGusta_IsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ActualizarBarraSegunSeleccion();
            if (!ListaCancionesMeGusta.IsSelectionEnabled)
            {
                ListaCancionesMeGusta.SelectedItems.Clear();
            }
        }

        private void ListaCancionesMeGusta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaCancionesMeGusta.IsSelectionEnabled && ListaCancionesMeGusta.SelectedItems.Count == 0)
            {
                ListaCancionesMeGusta.IsSelectionEnabled = false;
            }
        }

        // --- RETROCESO (HARDWARE BACK BUTTON) ---
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (MainPivot.SelectedIndex == 2 && ListaCancionesMeGusta != null && ListaCancionesMeGusta.IsSelectionEnabled)
            {
                ListaCancionesMeGusta.IsSelectionEnabled = false;
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Suscribirnos a cambios del ViewModel para refrescar la visibilidad cuando llegue el JSON
            App.ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            App.ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            if (e.NavigationMode != NavigationMode.Back)
            {
                if (!App.ViewModel.IsDataLoaded)
                {
                    App.ViewModel.LoadData();
                }

                DataContext = App.ViewModel;
                ActualizarEstadoVacio();

                if (!App.ViewModel.IsLibraryLoaded)
                {
                    await App.ViewModel.CargarBibliotecaDesdeServidorAsync();
                    ActualizarEstadoVacio();
                }
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

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PlaylistsAgrupadas" ||
                e.PropertyName == "AlbumesAgrupados" ||
                e.PropertyName == "ArtistasAgrupados" ||
                e.PropertyName == "CancionesMeGustaAgrupadas")
            {
                Dispatcher.BeginInvoke(() => ActualizarEstadoVacio());
            }
        }

        private void ActualizarEstadoVacio()
        {
            try
            {
                // Validación para Playlists (verificamos la colección base porque AlphaKeyGroup siempre crea 28 cabeceras)
                if (App.ViewModel.Playlists == null || App.ViewModel.Playlists.Count == 0)
                {
                    txtVacioPlaylists.Visibility = Visibility.Visible;
                    lstPlaylists.Visibility = Visibility.Collapsed;
                }
                else
                {
                    txtVacioPlaylists.Visibility = Visibility.Collapsed;
                    lstPlaylists.Visibility = Visibility.Visible;
                }

                // Validación para Álbumes
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

                // Validación para Artistas
                if (App.ViewModel.Artistas == null || App.ViewModel.Artistas.Count == 0)
                {
                    txtVacioArtistas.Visibility = Visibility.Visible;
                    lstArtistas.Visibility = Visibility.Collapsed;
                }
                else
                {
                    txtVacioArtistas.Visibility = Visibility.Collapsed;
                    lstArtistas.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al actualizar estado vacío: " + ex.Message);
            }
        }
    }
}