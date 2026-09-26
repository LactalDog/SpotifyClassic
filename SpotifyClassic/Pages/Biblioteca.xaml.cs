using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace SpotifyClassic
{
    public partial class Biblioteca : PhoneApplicationPage
    {
        // El constructor debe llamarse exactamente igual que la clase
        public Biblioteca()
        {
            InitializeComponent();
        }

        // Este método se ejecuta automáticamente cuando se viaja hacia esta página
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Verificamos si la URL trae un parámetro oculto llamado "seccion"
            if (NavigationContext.QueryString.ContainsKey("seccion"))
            {
                string seccionSolicitada = NavigationContext.QueryString["seccion"];

                // Deslizamos el Pivot (MainPivot) según la palabra recibida
                // Nota: Los índices empiezan en 0 (0 es la primera pestaña, 1 la segunda, etc.)
                if (seccionSolicitada == "tus me gusta")
                    MainPivot.SelectedIndex = 0;
                else if (seccionSolicitada == "álbumes")
                    MainPivot.SelectedIndex = 1;
                else if (seccionSolicitada == "artistas")
                    MainPivot.SelectedIndex = 2;
                else if (seccionSolicitada == "playlists")
                    MainPivot.SelectedIndex = 3;
                else if (seccionSolicitada == "podcasts")
                    MainPivot.SelectedIndex = 4;
            }
        }
    }
}