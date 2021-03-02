using Xamarin.Forms;

namespace WellFitPlus.Mobile
{
    public static class AppStyles
    {
        public static readonly Color DefaultTextColor = Device.OnPlatform(Color.White, Color.White, Color.White);
        public static readonly Color DefaultEntryTextColor = Device.OnPlatform(Color.Gray, Color.Gray, Color.Gray);

        public static readonly Thickness DefaultPagePadding = new Thickness(0, Device.OnPlatform(20, 0, 0), 0, 0);
        public static readonly Font LabelFont = Device.OnPlatform(Font.OfSize("Monaco", 12), Font.OfSize("Helvetica", 14), Font.OfSize("Comic Sans Ms", 24));
        
                
    }
}
