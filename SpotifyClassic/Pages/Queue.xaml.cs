using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace SpotifyClassic
{
    public partial class Queue : PhoneApplicationPage
    {
        public Queue()
        {
            InitializeComponent();

            CargarDatosDePrueba();
        }

        private void CargarDatosDePrueba()
        {
            List<Reproduciendo> ListaDeReproduccion = new List<Reproduciendo>
            {
                new Reproduciendo { Numero = "1", Titulo = "Introduction", Artista = "Solar Fields", Duracion = "5:22", Portada = "/Assets/MusicPreview.png" },
                new Reproduciendo { Numero = "2", Titulo = "Edge and Flight", Artista = "Solar Fields", Duracion = "6:55", Portada = "/Assets/MusicPreview.png" },
                new Reproduciendo { Numero = "3", Titulo = "Jacknife", Artista = "Solar Fields", Duracion = "4:30", Portada = "/Assets/MusicPreview.png" },
                new Reproduciendo { Numero = "4", Titulo = "Stepstones", Artista = "Solar Fields", Duracion = "7:02", Portada = "/Assets/MusicPreview.png" },
                new Reproduciendo { Numero = "5", Titulo = "Still", Artista = "Solar Fields", Duracion = "4:45", Portada = "/Assets/MusicPreview.png" },
                new Reproduciendo { Numero = "6", Titulo = "Mirror's Edge Theme", Artista = "Solar Fields", Duracion = "5:12", Portada = "/Assets/MusicPreview.png" }
            };

            ListaReproduciendo.ItemsSource = ListaDeReproduccion;
        }
    }

    public class Reproduciendo
    {
        public string Numero { get; set; }
        public string Titulo { get; set; }
        public string Artista { get; set; }
        public string Duracion { get; set; }
        public string Portada { get; set; }
    }
}