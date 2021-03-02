// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace WellFitPlus.Mobile.iOS.PlatformViews
{
    [Register ("VideoPlaybackRenderer")]
    partial class VideoPlaybackRenderer
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton BackToWorkButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton PauseButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView PauseButtonShadow { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton PlaybackToggleButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView PlayToggleImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ReplayButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView ReplayIcon { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton RewindButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton SoundToggleButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView SoundToggleShadow { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView SponsorImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView TipOfTheDayLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TitleLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TodoInfoLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (BackToWorkButton != null) {
                BackToWorkButton.Dispose ();
                BackToWorkButton = null;
            }

            if (PauseButton != null) {
                PauseButton.Dispose ();
                PauseButton = null;
            }

            if (PauseButtonShadow != null) {
                PauseButtonShadow.Dispose ();
                PauseButtonShadow = null;
            }

            if (PlaybackToggleButton != null) {
                PlaybackToggleButton.Dispose ();
                PlaybackToggleButton = null;
            }

            if (PlayToggleImage != null) {
                PlayToggleImage.Dispose ();
                PlayToggleImage = null;
            }

            if (ReplayButton != null) {
                ReplayButton.Dispose ();
                ReplayButton = null;
            }

            if (ReplayIcon != null) {
                ReplayIcon.Dispose ();
                ReplayIcon = null;
            }

            if (RewindButton != null) {
                RewindButton.Dispose ();
                RewindButton = null;
            }

            if (SoundToggleButton != null) {
                SoundToggleButton.Dispose ();
                SoundToggleButton = null;
            }

            if (SoundToggleShadow != null) {
                SoundToggleShadow.Dispose ();
                SoundToggleShadow = null;
            }

            if (SponsorImage != null) {
                SponsorImage.Dispose ();
                SponsorImage = null;
            }

            if (TipOfTheDayLabel != null) {
                TipOfTheDayLabel.Dispose ();
                TipOfTheDayLabel = null;
            }

            if (TitleLabel != null) {
                TitleLabel.Dispose ();
                TitleLabel = null;
            }

            if (TodoInfoLabel != null) {
                TodoInfoLabel.Dispose ();
                TodoInfoLabel = null;
            }
        }
    }
}