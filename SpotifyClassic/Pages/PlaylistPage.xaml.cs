using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace SpotifyClassic.Pages
{
    public partial class PlaylistPage : PhoneApplicationPage
    {
        private ApplicationBar barraDefault;
        private ApplicationBar barraSeleccion;

        public PlaylistPage()
        {
            InitializeComponent();

            // Configurar las barras de aplicación dinámicas
            ConstruirBarrasDinamicas();

            // Asignar la barra por defecto al iniciar
            this.ApplicationBar = barraDefault;

            // Cargar los datos en el constructor para evitar conflictos de animación
            CargarDatosDePrueba();

            // Suscribir eventos del LongListMultiSelector
            ListaCanciones.IsSelectionEnabledChanged += ListaCanciones_IsSelectionEnabledChanged;
            ListaCanciones.SelectionChanged += ListaCanciones_SelectionChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void CargarDatosDePrueba()
        {
            List<Canciones> listaDeCanciones = new List<Canciones>
            {
                new Canciones { Numero = "1", Titulo = "Introduction", Artista = "Solar Fields", Duracion = "5:22", Portada = "/Assets/MusicPreview.png" },
                new Canciones { Numero = "2", Titulo = "Edge and Flight", Artista = "Solar Fields", Duracion = "6:55", Portada = "/Assets/MusicPreview.png" },
                new Canciones { Numero = "3", Titulo = "Jacknife", Artista = "Solar Fields", Duracion = "4:30", Portada = "/Assets/MusicPreview.png" },
                new Canciones { Numero = "4", Titulo = "Stepstones", Artista = "Solar Fields", Duracion = "7:02", Portada = "/Assets/MusicPreview.png" },
                new Canciones { Numero = "5", Titulo = "Still", Artista = "Solar Fields", Duracion = "4:45", Portada = "/Assets/MusicPreview.png" },
                new Canciones { Numero = "6", Titulo = "Mirror's Edge Theme", Artista = "Solar Fields", Duracion = "5:12", Portada = "/Assets/MusicPreview.png" }
            };

            ListaCanciones.ItemsSource = listaDeCanciones;
        }

        private void ConstruirBarrasDinamicas()
        {
            // 1. Barra por defecto (Modo Normal)
            barraDefault = new ApplicationBar();
            barraDefault.Mode = ApplicationBarMode.Default;
            barraDefault.Opacity = 1;

            ApplicationBarIconButton btnSeleccionar = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Select.png", UriKind.Relative));
            btnSeleccionar.Text = "seleccionar";
            btnSeleccionar.Click += BtnSeleccionar_Click;
            barraDefault.Buttons.Add(btnSeleccionar);

            // 2. Barra de multiselección (Modo Activo)
            barraSeleccion = new ApplicationBar();
            barraSeleccion.Mode = ApplicationBarMode.Default;
            barraSeleccion.Opacity = 1;

            ApplicationBarIconButton btnPlaylist = new ApplicationBarIconButton(new Uri("/Toolkit.Content/ApplicationBar.Add.png", UriKind.Relative));
            btnPlaylist.Text = "añadir a playlist";
            btnPlaylist.Click += BtnPlaylist_Click;
            barraSeleccion.Buttons.Add(btnPlaylist);

            ApplicationBarIconButton btnCola = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.source.png", UriKind.Relative));
            btnCola.Text = "añadir a cola";
            btnCola.Click += BtnCola_Click;
            barraSeleccion.Buttons.Add(btnCola);
        }

        // --- MANEJADORES DE BOTONES ---

        private void BtnSeleccionar_Click(object sender, EventArgs e)
        {
            // Activar el modo multiselección
            ListaCanciones.IsSelectionEnabled = true;
        }

        private void BtnPlaylist_Click(object sender, EventArgs e)
        {
            // TODO: Lógica para añadir ListaCanciones.SelectedItems a la playlist
            ListaCanciones.IsSelectionEnabled = false;
        }

        private void BtnCola_Click(object sender, EventArgs e)
        {
            // TODO: Lógica para añadir ListaCanciones.SelectedItems a la cola
            ListaCanciones.IsSelectionEnabled = false;
        }

        // --- GESTIÓN DE ESTADOS DEL MULTISELECTOR ---

        private void ListaCanciones_IsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ListaCanciones.IsSelectionEnabled)
            {
                this.ApplicationBar = barraSeleccion;
            }
            else
            {
                this.ApplicationBar = barraDefault;
                ListaCanciones.SelectedItems.Clear(); // Limpiamos selección remanente
            }
        }

        private void ListaCanciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Si estamos en modo selección y desmarcamos absolutamente todas las casillas, salimos del modo
            if (ListaCanciones.IsSelectionEnabled && ListaCanciones.SelectedItems.Count == 0)
            {
                ListaCanciones.IsSelectionEnabled = false;
            }
        }

        // --- INTERCEPCIÓN DEL BOTÓN DE RETROCESO ---

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (ListaCanciones.IsSelectionEnabled)
            {
                // Salimos de la multiselección y cancelamos la navegación hacia atrás
                ListaCanciones.IsSelectionEnabled = false;
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }
    }

    public class Canciones
    {
        public string Numero { get; set; }
        public string Titulo { get; set; }
        public string Artista { get; set; }
        public string Duracion { get; set; }
        public string Portada { get; set; }
    }
}