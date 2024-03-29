﻿using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UIKit;
using WellFitPlus.Mobile.Controls;
using WellFitPlus.Mobile.iOS.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly: ExportRenderer(typeof(ExtLabel), typeof(ExtLabelRenderer))]

namespace WellFitPlus.Mobile.iOS.Controls
{
    public class ExtLabelRenderer : LabelRenderer
    {
        /// <summary>
        /// The on element changed callback.
        /// </summary>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            var view = e.NewElement as ExtLabel;

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

            var view = Element as ExtLabel;

            if (view != null &&
                e.PropertyName == Label.TextProperty.PropertyName ||
                e.PropertyName == Label.FormattedTextProperty.PropertyName ||
                e.PropertyName == ExtLabel.PlaceholderProperty.PropertyName ||
                e.PropertyName == ExtLabel.FormattedPlaceholderProperty.PropertyName ||
                e.PropertyName == ExtLabel.IsDropShadowProperty.PropertyName ||
                e.PropertyName == ExtLabel.IsStrikeThroughProperty.PropertyName ||
                e.PropertyName == ExtLabel.IsUnderlineProperty.PropertyName)
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
        private void UpdateUi(ExtLabel view)
        {
            // Prefer font set through Font property.
            if (view.Font == Font.Default)
            {
                if (view.FontSize > 0)
                {
                    this.Control.Font = UIFont.FromName(this.Control.Font.Name, (float)view.FontSize);
                }

                if (!string.IsNullOrEmpty(view.FontName))
                {
                    var fontName = Path.GetFileNameWithoutExtension(view.FontName);

                    var font = UIFont.FromName(fontName, this.Control.Font.PointSize);

                    if (font != null)
                    {
                        this.Control.Font = font;
                    }
                }

                #region ======= This is for backward compatability with obsolete attrbute 'FontNameIOS' ========
                if (!string.IsNullOrEmpty(view.FontNameIOS))
                {
                    var font = UIFont.FromName(view.FontNameIOS, (view.FontSize > 0) ? (float)view.FontSize : 12.0f);

                    if (font != null)
                    {
                        this.Control.Font = font;
                    }
                }
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

            var underline = view.IsUnderline ? NSUnderlineStyle.Single : NSUnderlineStyle.None;
            var strikethrough = view.IsStrikeThrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None;

            NSShadow dropShadow = null;

            if (view.IsDropShadow)
            {
                dropShadow = new NSShadow
                {
                    ShadowColor = view.DropShadowColor.ToUIColor(),
                    ShadowBlurRadius = 1.4f,
                    ShadowOffset = new CoreGraphics.CGSize(new CoreGraphics.CGPoint(0.3f, 0.8f))
                };
            }

            // For some reason, if we try and convert Color.Default to a UIColor, the resulting color is
            // either white or transparent. The net result is the ExtendedLabel does not display.
            // Only setting the control's TextColor if is not Color.Default will prevent this issue.
            if (view.TextColor != Color.Default)
            {
                this.Control.TextColor = view.TextColor.ToUIColor();
            }

            this.Control.AttributedText = new NSMutableAttributedString(view.Text,
                this.Control.Font,
                underlineStyle: underline,
                strikethroughStyle: strikethrough,
                shadow: dropShadow);
            ;
        }

        private void SetPlaceholder(ExtLabel view)
        {
            if (view == null)
            {
                return;
            }

            if (view.FormattedText != null)
            {
                this.Control.AttributedText = view.FormattedText.ToAttributed(view.Font, view.TextColor);
                LayoutSubviews();
                return;
            }

            if (!string.IsNullOrEmpty(view.Text))
            {
                this.UpdateUi(view);
                LayoutSubviews();
                return;
            }

            if (string.IsNullOrWhiteSpace(view.Placeholder) && view.FormattedPlaceholder == null)
            {
                return;
            }

            var formattedPlaceholder = view.FormattedPlaceholder ?? view.Placeholder;

            Control.AttributedText = formattedPlaceholder.ToAttributed(view.Font, view.TextColor);

            LayoutSubviews();
        }
    }
}
