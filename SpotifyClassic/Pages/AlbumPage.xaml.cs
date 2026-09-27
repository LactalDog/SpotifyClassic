using System;
using System.Windows;
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
            this.Loaded += AlbumPage_Loaded;
        }

        private void AlbumPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Asignamos la transición de llegada hacia adelante y de salida hacia atrás
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
                txtTituloDetalle.Text = NavigationContext.QueryString["title"];
            }
        }
    }
}