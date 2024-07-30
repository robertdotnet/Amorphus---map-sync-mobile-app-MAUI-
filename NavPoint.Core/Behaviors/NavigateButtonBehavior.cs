using Microsoft.Maui.Controls.PlatformConfiguration;
using NavPoint.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NavPoint.Core.Behaviors
{
    public class NavigateButtonBehavior : Behavior<Button>
    {

        protected override void OnAttachedTo(Button bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.Clicked += OnButtonClicked;
        }

        protected override void OnDetachingFrom(Button bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.Clicked -= OnButtonClicked;
        }

        private void OnButtonClicked(object sender, EventArgs e)
        {


            if (sender is Button button)
            {
                var commandParameter = button.CommandParameter;
                if (commandParameter != null && commandParameter is LocationUnit)
                {
                    var locationUnit = (LocationUnit)commandParameter;
                    LaunchWaze(locationUnit.Coordinates.XCoordinate, locationUnit.Coordinates.YCoordinate);
                }
            }
        }

        private void LaunchWaze(double latitude, double longitude)
        {
            try
            {
                //WAZE NAVIGATION
                string wazeUri = $"waze://?ll={latitude},{longitude}&navigate=yes";
                Launcher.OpenAsync(wazeUri);
                
                //DEFAULT NAVIGATION
                //Map.Default.OpenAsync(latitude, longitude);

            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
        }
    }
}
