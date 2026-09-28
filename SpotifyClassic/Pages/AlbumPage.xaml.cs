using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

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

            if (NavigationContext.QueryString.ContainsKey("title"))
            {
                Titulo.Text = Uri.UnescapeDataString(NavigationContext.QueryString["title"]);
            }
            if (NavigationContext.QueryString.ContainsKey("artist"))
            {
                Artista.Text = Uri.UnescapeDataString(NavigationContext.QueryString["artist"]);
            }
        }
    }
}