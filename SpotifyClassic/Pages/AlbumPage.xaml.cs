using System;
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

            // Llamamos a la función que cargará los datos al iniciar la página
            CargarDatosDePrueba();
        }

        private void CargarDatosDePrueba()
        {
            // 1. Creamos una lista de prueba con la estructura de nuestra clase
            List<Cancion> listaDeCanciones = new List<Cancion>
            {
                new Cancion { Numero = "1", Titulo = "Introduction", Artista = "Solar Fields", Duracion = "5:22" },
                new Cancion { Numero = "2", Titulo = "Edge and Flight", Artista = "Solar Fields", Duracion = "6:55" },
                new Cancion { Numero = "3", Titulo = "Jacknife", Artista = "Solar Fields", Duracion = "4:30" },
                new Cancion { Numero = "4", Titulo = "Stepstones", Artista = "Solar Fields", Duracion = "7:02" },
                new Cancion { Numero = "5", Titulo = "Still", Artista = "Solar Fields", Duracion = "4:45" },
                new Cancion { Numero = "6", Titulo = "Mirror's Edge Theme", Artista = "Solar Fields", Duracion = "5:12" }
            };

            // 2. Asignamos la lista al origen de datos del selector
            // Nota: "ListaCanciones" será el nombre que le daremos al control en el XAML en el Paso 2
            ListaCanciones.ItemsSource = listaDeCanciones;
        }
    }

    // Clase sencilla para estructurar los datos de cada canción
    public class Cancion
    {
        public string Numero { get; set; }
        public string Titulo { get; set; }
        public string Artista { get; set; }
        public string Duracion { get; set; }
    }
}