using WellFitPlus.Mobile.Controls;
using Xamarin.Forms;
using WellFitPlus.Mobile.Droid.Controls;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtScrollView), typeof(ExtScrollViewRenderer))]
namespace WellFitPlus.Mobile.Droid.Controls
{
    public class ExtScrollViewRenderer : ScrollViewRenderer
    {
        public ExtScrollViewRenderer() : base()
        {

        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
        }
    }
}