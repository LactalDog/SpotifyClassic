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
        private ApplicationBar barraCancionesDefault;
        private ApplicationBar barraSeleccion;

        public Biblioteca()
        {
            InitializeComponent();
            ConstruirBarrasDinamicas();

            // Asignar el evento SelectionChanged al Pivot principal
            MainPivot.SelectionChanged += MainPivot_SelectionChanged;
        }

        private void ConstruirBarrasDinamicas()
        {
            // 1. Barra específica para la pestaña "Me gusta" (Permite activar selección)
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

        // --- GESTIÓN DE VISTAS (PIVOT) ---
        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarBarraSegunSeleccion();
        }

        private void ActualizarBarraSegunSeleccion()
        {
            // El índice 2 corresponde a la pestaña "me gusta"
            if (MainPivot.SelectedIndex == 2)
            {
                if (ListaCancionesMeGusta != null && ListaCancionesMeGusta.IsSelectionEnabled)
                    this.ApplicationBar = barraSeleccion;
                else
                    this.ApplicationBar = barraCancionesDefault;
            }
            else
            {
                // Apagar el modo selección automáticamente al cambiar de pestaña
                if (ListaCancionesMeGusta != null && ListaCancionesMeGusta.IsSelectionEnabled)
                {
                    ListaCancionesMeGusta.IsSelectionEnabled = false;
                }

                // Ocultar la AppBar en el resto de las pestañas de la biblioteca
                this.ApplicationBar = null;
            }
        }

        // --- MANEJADORES DE BOTONES ---
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

        // --- EVENTOS DEL LONG LIST MULTI SELECTOR ---
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

            NavigationService.Navigate(new Uri("/Pages/AlbumPage.xaml", UriKind.Relative));
        }
    }
}