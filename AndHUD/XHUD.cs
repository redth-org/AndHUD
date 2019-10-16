using System;
using Android.App;
using AndroidHUD;

namespace XHUD
{
    /// <summary>
    /// HUD class to help interface between BTProgressHUD and AndHUD.
    /// </summary>
#pragma warning disable SA1649 // File name should match first type name
    public static class HUD
#pragma warning restore SA1649 // File name should match first type name
    {
        /// <summary>
        /// Get or set Activity used as context to show dialog in.
        /// </summary>
        public static Activity MyActivity { get; set; }

        /// <summary>
        /// Show a dialog.
        /// </summary>
        /// <param name="message">Message to show under loading indicator.</param>
        /// <param name="progress">If set between 1 and 100, the progress will be determinate.</param>
        /// <param name="maskType">Mask type used to dim background of dialog.</param>
        public static void Show(string message, int progress = -1, MaskType maskType = MaskType.Black)
        {
            AndHUD.Shared.Show(MyActivity, message, progress, (AndroidHUD.MaskType)maskType);
        }

        /// <summary>
        /// Dismiss currently shown dialog.
        /// </summary>
        public static void Dismiss()
        {
            AndHUD.Shared.Dismiss(MyActivity);
        }

        /// <summary>
        /// Show toast message.
        /// </summary>
        /// <param name="message">Message to show in toast.</param>
        /// <param name="showToastCentered">If true, toast will be centered on screen. Otherwise towards bottom of screen.</param>
        /// <param name="timeoutMs">Timeout in ms. Determines when to dismiss the toast.</param>
        public static void ShowToast(string message, bool showToastCentered = true, double timeoutMs = 1000)
        {
            AndHUD.Shared.ShowToast(MyActivity, message, (AndroidHUD.MaskType)MaskType.Black, TimeSpan.FromMilliseconds(timeoutMs), showToastCentered);
        }

        /// <summary>
        /// Show toast message.
        /// </summary>
        /// <param name="message">Message to show in toast.</param>
        /// <param name="maskType">Mask type used to dim background of dialog.</param>
        /// <param name="showToastCentered">If true, toast will be centered on screen. Otherwise towards bottom of screen.</param>
        /// <param name="timeoutMs">Timeout in ms. Determines when to dismiss the toast.</param>
        public static void ShowToast(string message, MaskType maskType, bool showToastCentered = true, double timeoutMs = 1000)
        {
            AndHUD.Shared.ShowToast(MyActivity, message, (AndroidHUD.MaskType)maskType, TimeSpan.FromMilliseconds(timeoutMs), showToastCentered);
        }
    }
}
