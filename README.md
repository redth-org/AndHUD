AndHUD
==========

AndHUD is a Progress / HUD library for Android which allows you to easily add amazing HUDs to your app!


Features
--------
 - Several types of HUDs
   - Spinner (with and without Text)
   - Progress (with and without Text)
   - Image (with and without Text)
   - Success / Error (with and without Text)
   - Toasts
 - Xamarin.Android Support
 - Xamarin Component store
 - Similar API and functionality to BTProgressHUD for iOS
 

Quick and Simple
----------------
```csharp
//Show a simple status message with an indeterminate spinner
AndHUD.Shared.Show(myActivity, "Status Message", MaskType.Clear);

//Show a progress with a filling circle representing the progress amount
AndHUD.Shared.ShowProgress(myActivity, "Loading… 60%", 60);

//Show a success image with a message
AndHUD.Shared.ShowSuccess(myActivity, "It Worked!", MaskType.Clear, TimeSpan.FromSeconds(2));

//Show an error image with a message
AndHUD.Shared.ShowError(myActivity, "It no worked :()", MaskType.Black, TimeSpan.FromSeconds(2));

//Show a toast, similar to Android native toasts, but styled as AndHUD
AndHUD.Shared.ShowToast(myActivity, "This is a non-centered Toast…", MaskType.Clear, TimeSpan.FromSeconds(2));

//Show a custom image with text
AndHUD.Shared.ShowImage(myActivity, Resource.Drawable.MyCustomImage, "Custom");

//Dismiss a HUD that will or will not be automatically timed out
AndHUD.Shared.Dismiss(myActivity);
```

![Collage of Possible HUDs](https://raw.github.com/Redth/AndHUD/master/Art/Collage.png)

Other Options
-------------
 - **MaskType:** By default, MaskType.Black dims the background behind the HUD.  Use MaskType.Clear to prevent the dimming
 - **Timeout:** If you provide a timeout, the HUD will automatically be dismissed after the timeout elapses, if you have not already dismissed it manually.



Thanks
------
Thanks to Nic Wise (@fastchicken) who inspired the creation of this with his component BTProgressHUD (https://components.xamarin.com/view/btprogresshud/).  It was so awesome for iOS that I needed to have it on Android as well :)

