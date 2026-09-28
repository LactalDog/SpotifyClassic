using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using SpotifyClassic.Animations;

namespace SpotifyClassic.Pages
{
    public partial class AlbumPage : PhoneApplicationPage
    {
        public AlbumPage()
        {
            InitializeComponent();

            // Asignamos las transiciones de entrada y salida inmediatamente en el constructor
            // para que no haya un marco en negro entre la salida de Biblioteca y la llegada a AlbumPage
            var navIn = new NavigationInTransition();
            navIn.Forward = new ContinuumTransition(ContinuumTransitionMode.ContinuumForwardInStoryboard, txtTituloDetalle);

            var navOut = new NavigationOutTransition();
            navOut.Backward = new ContinuumTransition(ContinuumTransitionMode.ContinuumBackwardOutStoryboard, txtTituloDetalle);

            TransitionService.SetNavigationInTransition(this, navIn);
            TransitionService.SetNavigationOutTransition(this, navOut);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.ContainsKey("title"))
            {
                txtTituloDetalle.Text = Uri.UnescapeDataString(NavigationContext.QueryString["title"]);
            }
        }
    }
}