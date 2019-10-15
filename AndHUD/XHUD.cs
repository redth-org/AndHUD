using System;
using Android.App;
using AndroidHUD;

namespace XHUD
{
#pragma warning disable SA1649 // File name should match first type name
    public static class HUD
#pragma warning restore SA1649 // File name should match first type name
    {
        public static Activity MyActivity { get; set; }

        public static void Show(string message, int progress = -1, MaskType maskType = MaskType.Black)
        {
            AndHUD.Shared.Show(MyActivity, message, progress, (AndroidHUD.MaskType)maskType);
        }

        public static void Dismiss()
        {
            AndHUD.Shared.Dismiss(MyActivity);
        }

        public static void ShowToast(string message, bool showToastCentered = true, double timeoutMs = 1000)
        {
            AndHUD.Shared.ShowToast(MyActivity, message, (AndroidHUD.MaskType)MaskType.Black, TimeSpan.FromSeconds(timeoutMs / 1000), showToastCentered);
        }

        public static void ShowToast(string message, MaskType maskType, bool showToastCentered = true, double timeoutMs = 1000)
        {
            AndHUD.Shared.ShowToast(MyActivity, message, (AndroidHUD.MaskType)maskType, TimeSpan.FromSeconds(timeoutMs / 1000), showToastCentered);
        }
    }
}
