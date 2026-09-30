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
        }

    }
}