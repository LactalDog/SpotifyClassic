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

            // SOLUCIÓN CLAVE: Cargar los datos en el constructor.
            // Esto obliga al LongListSelector a encolar el dibujado de sus elementos
            // ANTES de que TransitionService tome la captura visual para animar.

            CargarDatosDePrueba();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Ya no cargamos los datos aquí para evitar el "choque" de hilos.
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