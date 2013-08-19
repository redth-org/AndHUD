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
AndHUD.Show(myActivity, "Status Message");

//Show a progress with a filling circle representing the progress amount
AndHUD.ShowProgress(myActivity, "Completed: 15%", 15);

//Show a toast, similar to Android native toasts, but styled as AndHUD
AndHUD.ShowToast(myActivity, "Cheers!");

//Show a success image without a text message
AndHUD.ShowSuccess(myActivity, string.Empty);

//Show an error image with a message
AndHUD.ShowError(myActivity, "Invalid Password!");




```

Other Options
-------------



But Wait, there's more!
-----------------------


Thanks
------
Thanks to Nic Wise (@fastchicken) who inspired the creation of this with his component BTProgressHUD (https://components.xamarin.com/view/btprogresshud/).  It was so awesome for iOS that I needed to have it on Android as well :)

