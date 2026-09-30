using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.Collections.Generic;

namespace SpotifyClassic.Pages
{
    public partial class AlbumPage : PhoneApplicationPage
    {
        public AlbumPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode != NavigationMode.Back)
            {
                CargarDatosDePrueba();
            }
        }

        private void CargarDatosDePrueba()
        {
            List<Cancion> listaDeCanciones = new List<Cancion>
            {
                new Cancion { Numero = "1", Titulo = "Introduction", Artista = "Solar Fields", Duracion = "5:22" },
                new Cancion { Numero = "2", Titulo = "Edge and Flight", Artista = "Solar Fields", Duracion = "6:55" },
                new Cancion { Numero = "3", Titulo = "Jacknife", Artista = "Solar Fields", Duracion = "4:30" },
                new Cancion { Numero = "4", Titulo = "Stepstones", Artista = "Solar Fields", Duracion = "7:02" },
                new Cancion { Numero = "5", Titulo = "Still", Artista = "Solar Fields", Duracion = "4:45" },
                new Cancion { Numero = "6", Titulo = "Mirror's Edge Theme", Artista = "Solar Fields", Duracion = "5:12" }
            };

            ListaCanciones.ItemsSource = listaDeCanciones;

            // LA CLAVE: Forzamos la construcción del árbol visual del LongListMultiSelector
            // ANTES de que TurnstileFeatherTransition capture la pantalla buscando los índices.
            this.UpdateLayout();
        }
    }

    public class Cancion
    {
        public string Numero { get; set; }
        public string Titulo { get; set; }
        public string Artista { get; set; }
        public string Duracion { get; set; }
    }
}