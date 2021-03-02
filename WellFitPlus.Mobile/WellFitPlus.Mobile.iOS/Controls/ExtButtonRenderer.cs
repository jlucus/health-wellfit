using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UIKit;
using WellFitPlus.Mobile.Controls;
using WellFitPlus.Mobile.iOS.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly: ExportRenderer(typeof(ExtButton), typeof(ExtButtonRenderer))]
namespace WellFitPlus.Mobile.iOS.Controls
{
    public class ExtButtonRenderer : ButtonRenderer
    {
        /// <summary>
        /// The on element changed callback.
        /// </summary>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            var view = e.NewElement as ExtButton;

            //UpdateUi(view, this.Control);
            if (view != null)
            {
                SetPlaceholder(view);
            }
        }

        /// <summary>
        /// Raises the element property changed event.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">The event arguments</param>
        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var view = Element as ExtButton;

            if (view != null &&
                e.PropertyName == Button.TextProperty.PropertyName) ; // ||
                //e.PropertyName == Button.FormattedTextProperty.PropertyName ||
                //e.PropertyName == ExtButton.PlaceholderProperty.PropertyName ||
                //e.PropertyName == ExtButton.FormattedPlaceholderProperty.PropertyName ||
                //e.PropertyName == ExtButton.IsDropShadowProperty.PropertyName ||
                //e.PropertyName == ExtButton.IsStrikeThroughProperty.PropertyName ||
                //e.PropertyName == ExtButton.IsUnderlineProperty.PropertyName)
            {
                SetPlaceholder(view);
            }
        }

        /// <summary>
        /// Updates the UI.
        /// </summary>
        /// <param name="view">
        /// The view.
        /// </param>
        private void UpdateUi(ExtButton view)
        {
            // Prefer font set through Font property.
            if (view.Font == Font.Default)
            {
                if (view.FontSize > 0)
                {
                    this.Control.Font = UIFont.FromName(this.Control.Font.Name, (float)view.FontSize);
                }

                if (!string.IsNullOrEmpty(view.FontFamily))
                {
                    var fontName = Path.GetFileNameWithoutExtension(view.FontFamily);

                    var font = UIFont.FromName(fontName, this.Control.Font.PointSize);

                    if (font != null)
                    {
                        this.Control.Font = font;
                    }
                }

                #region ======= This is for backward compatability with obsolete attrbute 'FontNameIOS' ========
                //if (!string.IsNullOrEmpty(view.FontNameIOS))
                //{
                //    var font = UIFont.FromName(view.FontNameIOS, (view.FontSize > 0) ? (float)view.FontSize : 12.0f);

                //    if (font != null)
                //    {
                //        this.Control.Font = font;
                //    }
                //}
                #endregion ====== End of obsolete section ==========================================================
            }
            else
            {
                try
                {
                    var font = UIFont.FromName(view.FontFamily, (float)view.FontSize);
                    if (font != null)
                        this.Control.Font = font;
                }
                catch (Exception ex)
                {
                    var x = ex;
                }
            }

            //Do not create attributed string if it is not necesarry
            //if (!view.IsUnderline && !view.IsStrikeThrough && !view.IsDropShadow)
            //{
            //    return;
            //}

            //var underline = view.IsUnderline ? NSUnderlineStyle.Single : NSUnderlineStyle.None;
            //var strikethrough = view.IsStrikeThrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None;

            //NSShadow dropShadow = null;

            //if (view.IsDropShadow)
            //{
            //    dropShadow = new NSShadow
            //    {
            //        ShadowColor = view.DropShadowColor.ToUIColor(),
            //        ShadowBlurRadius = 1.4f,
            //        ShadowOffset = new CoreGraphics.CGSize(new CoreGraphics.CGPoint(0.3f, 0.8f))
            //    };
            //}

            // For some reason, if we try and convert Color.Default to a UIColor, the resulting color is
            // either white or transparent. The net result is the ExtendedLabel does not display.
            // Only setting the control's TextColor if is not Color.Default will prevent this issue.
            //if (view.TextColor != Color.Default)
            //{
            //    this.Control.TextColor = view.TextColor.ToUIColor();
            //}

            //this.Control.AttributedText = new NSMutableAttributedString(view.Text,
            //    this.Control.Font,
            //    underlineStyle: underline,
            //    strikethroughStyle: strikethrough,
            //    shadow: dropShadow);
            //;
        }

        private void SetPlaceholder(ExtButton view)
        {
            if (view == null)
            {
                return;
            }

            //if (view.FormattedText != null)
            //{
            //    this.Control.AttributedText = view.FormattedText.ToAttributed(view.Font, view.TextColor);
            //    LayoutSubviews();
            //    return;
            //}

            if (!string.IsNullOrEmpty(view.Text))
            {
                this.UpdateUi(view);
                LayoutSubviews();
                return;
            }

            //if (string.IsNullOrWhiteSpace(view.Placeholder) && view.FormattedPlaceholder == null)
            //{
            //    return;
            //}

            //var formattedPlaceholder = view.FormattedPlaceholder ?? view.Placeholder;

            //Control.AttributedText = formattedPlaceholder.ToAttributed(view.Font, view.TextColor);

            LayoutSubviews();
        }

    }

}
