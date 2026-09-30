// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Samples
{
    public partial class TurnstileFeatherEffectSample1 : PhoneApplicationPage
    {
        
        public TurnstileFeatherEffectSample1()
        {
            InitializeComponent();
        }

        

        // Initiate a forward navigation.
        private void Item_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Samples/FeatheredTransitionsSample2.xaml", UriKind.Relative));
        }
    }
}