using Android.Graphics;
using Android.Widget;
using System;

using WellFitPlus.Mobile.Controls;
using WellFitPlus.Mobile.Droid.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtButton), typeof(ExtButtonRenderer))]
namespace WellFitPlus.Mobile.Droid.Controls
{
    public class ExtButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);

            Control?.SetPadding(0, Control.PaddingTop, 0, Control.PaddingBottom);
            
            var label = (TextView)Control; // for example
            if (!string.IsNullOrEmpty(e.NewElement?.FontFamily))
            {
                try
                {
                    //Typeface font = Typeface.CreateFromAsset(Forms.Context.Assets, "OpenSans-Regular.ttf");  // font name specified here
                    Typeface font = Typeface.CreateFromAsset(Forms.Context.Assets, e.NewElement.FontFamily + ".ttf");
                    label.Typeface = font;
                }
                catch (Exception ex)
                {                    
                    Typeface font = Typeface.CreateFromAsset(Forms.Context.Assets, "Raleway-Regular.ttf");  // font name specified here
                    label.Typeface = font;
                }
            }
            else
            {
                Typeface font = Typeface.CreateFromAsset(Forms.Context.Assets, "Raleway-Regular.ttf");  // font name specified here
                label.Typeface = font;
            }
        }
    }

}
