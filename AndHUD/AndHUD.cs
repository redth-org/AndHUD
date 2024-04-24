using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AndroidHUD
{
    public class AndHUD
    {
        private const string TagName = nameof(AndHUD);
        
		static AndHUD shared;

		public static AndHUD Shared
		{
			get
			{
				if (shared == null)
					shared = new AndHUD ();

				return shared;
			}
		}

		public AndHUD()
		{
		}

		public Dialog CurrentDialog { get; private set; }

		ProgressWheel progressWheel = null;
		TextView statusText = null;
		ImageView imageView = null;

		object statusObj = null;

        private readonly SemaphoreSlim _semaphoreSlim = new(1);

		public void Show (Context context, string status = null, int progress = -1, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, bool centered = true, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			if (progress >= 0)
				showProgress (context, progress, status, maskType, timeout, clickCallback, cancelCallback, prepareDialogCallback, dialogShownCallback);
			else
				showStatus (context, true, status, maskType, timeout, clickCallback, centered, cancelCallback, prepareDialogCallback, dialogShownCallback);
		}

		public void ShowSuccess(Context context, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			showImage (context, GetDrawable(context, Resource.Drawable.ic_successstatus), status, maskType, timeout, clickCallback, cancelCallback, prepareDialogCallback, dialogShownCallback);
		}

		public void ShowError(Context context, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			showImage (context, GetDrawable(context, Resource.Drawable.ic_errorstatus), status, maskType, timeout, clickCallback, cancelCallback, prepareDialogCallback, dialogShownCallback);
		}

		public void ShowSuccessWithStatus(Context context, string status, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			showImage (context, GetDrawable(context, Resource.Drawable.ic_successstatus), status, maskType, timeout, clickCallback, cancelCallback, prepareDialogCallback, dialogShownCallback);
		}

		public void ShowErrorWithStatus(Context context, string status, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			showImage (context, GetDrawable(context, Resource.Drawable.ic_errorstatus), status, maskType, timeout, clickCallback, cancelCallback, prepareDialogCallback, dialogShownCallback);
		}

		public void ShowImage(Context context, int drawableResourceId, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			showImage (context, GetDrawable(context, drawableResourceId), status, maskType, timeout, clickCallback, cancelCallback, prepareDialogCallback, dialogShownCallback);
		}

		public void ShowImage(Context context, Drawable drawable, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			showImage (context, drawable, status, maskType, timeout, clickCallback, cancelCallback, prepareDialogCallback, dialogShownCallback);
		}

		public void ShowToast(Context context, string status, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, bool centered = true, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			showStatus (context, false, status, maskType, timeout, clickCallback, centered, cancelCallback, prepareDialogCallback, dialogShownCallback);
		}

		public void Dismiss()
		{
            if (!_semaphoreSlim.Wait(1000))
            {
                Log.Warn(TagName, "Timed out getting semaphore on Dismiss()");
                return;
            }
            try
            {
                DismissCurrent ();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
		}

        Drawable GetDrawable(Context context, int drawableResourceId)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                return context.Resources.GetDrawable(drawableResourceId, context.Theme);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                return context.Resources.GetDrawable(drawableResourceId);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        void showStatus (Context context, bool spinner, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, bool centered = true, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			if (timeout == null)
				timeout = TimeSpan.Zero;

            if (!_semaphoreSlim.Wait(1000))
            {
                Log.Warn(TagName, "Timed out getting semaphore on showStatus()");
                return;
            }

            if (CurrentDialog != null && statusObj == null)
                DismissCurrent ();

            try
            {
                if (CurrentDialog == null)
                {
                    SetupDialog (context, maskType, cancelCallback, (a, d, m) => {
                        var view = LayoutInflater.From (context)?.Inflate (Resource.Layout.loading, null);

                        if (clickCallback != null && view is not null)
                            view.Click += (sender, e) => clickCallback();

                        statusObj = new object();

                        statusText = view?.FindViewById<TextView>(Resource.Id.textViewStatus);

                        if (!spinner)
                        {
                            var progressBar = view?.FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);
                            if (progressBar != null)
                                progressBar.Visibility = ViewStates.Gone;
                        }

                        if (maskType != MaskType.Black)
                            view?.SetBackgroundResource(Resource.Drawable.roundedbgdark);

                        if (statusText != null)
                        {
                            statusText.Text = status ?? "";
                            statusText.Visibility = string.IsNullOrEmpty(status) ? ViewStates.Gone : ViewStates.Visible;
                        }

                        if (!centered && d.Window is not null)
                        {
                            d.Window.SetGravity (GravityFlags.Bottom);
                            var p = d.Window.Attributes;

                            p.Y = DpToPx (context, 22);

                            d.Window.Attributes = p;
                        }

                        return view;
                    }, prepareDialogCallback, dialogShownCallback);

                    RunTimeout(timeout);
                }
                else
                {

                    Application.SynchronizationContext.Send(_ => {
                        if (statusText != null)
                            statusText.Text = status ?? "";
                    }, null);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
		}

		int DpToPx(Context context, int dp) 
		{
			var displayMetrics = context.Resources.DisplayMetrics;
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, displayMetrics);
		}

		void showProgress(Context context, int progress, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			timeout ??= TimeSpan.Zero;

            if (!_semaphoreSlim.Wait(1000))
            {
                Log.Warn(TagName, "Timed out getting semaphore on showProgress()");
                return;
            }
            
            if (CurrentDialog != null && progressWheel == null)
                DismissCurrent ();

            try
            {
                if (CurrentDialog == null)
                {
                    SetupDialog (context, maskType, cancelCallback, (a, d, m) => {
                        var inflater = LayoutInflater.FromContext(context);
                        var view = inflater?.Inflate(Resource.Layout.loadingprogress, null);

                        if (clickCallback != null && view is not null)
                            view.Click += (sender, e) => clickCallback();

                        progressWheel = view?.FindViewById<ProgressWheel>(Resource.Id.loadingProgressWheel);
                        statusText = view?.FindViewById<TextView>(Resource.Id.textViewStatus);

                        if (maskType != MaskType.Black)
                            view?.SetBackgroundResource(Resource.Drawable.roundedbgdark);

                        progressWheel?.SetProgress(0);

                        if (statusText != null)
                        {
                            statusText.Text = status ?? "";
                            statusText.Visibility = string.IsNullOrEmpty(status) ? ViewStates.Gone : ViewStates.Visible;
                        }

                        return view;
                    }, prepareDialogCallback, dialogShownCallback);

                    RunTimeout(timeout);
                }
                else
                {
                    Application.SynchronizationContext.Send(state => {
                        progressWheel?.SetProgress (progress);
                        statusText.Text = status ?? "";
                    }, null);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
		}

		void showImage(Context context, Drawable image, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			if (timeout == null)
				timeout = TimeSpan.Zero;

            if (!_semaphoreSlim.Wait(1000))
            {
                Log.Warn(TagName, "Timed out getting semaphore on showImage()");
                return;
            }

            if (CurrentDialog != null && imageView == null)
                DismissCurrent ();
            try
            {
                if (CurrentDialog == null)
                {
                    SetupDialog (context, maskType, cancelCallback, (a, d, m) => {
                        var inflater = LayoutInflater.FromContext(context);
                        var view = inflater?.Inflate(Resource.Layout.loadingimage, null);

                        if (clickCallback != null && view is not null)
                            view.Click += (sender, e) => clickCallback();

                        imageView = view?.FindViewById<ImageView>(Resource.Id.loadingImage);
                        statusText = view?.FindViewById<TextView>(Resource.Id.textViewStatus);

                        if (maskType != MaskType.Black)
                            view?.SetBackgroundResource(Resource.Drawable.roundedbgdark);
                            
                        imageView?.SetImageDrawable(image);

                        if (statusText != null)
                        {
                            statusText.Text = status ?? "";
                            statusText.Visibility = string.IsNullOrEmpty(status) ? ViewStates.Gone : ViewStates.Visible;
                        }

                        return view;
                    }, prepareDialogCallback, dialogShownCallback);

                    RunTimeout(timeout);
                }
                else
                {
                    Application.SynchronizationContext.Send(state => {
                        imageView?.SetImageDrawable(image);
                        statusText.Text = status ?? "";
                    }, null);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
		}

        async void RunTimeout(TimeSpan? timeout)
        {
            if (timeout > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(timeout.Value).ConfigureAwait(false);
                    DismissCurrent();
                }
                catch (Exception e)
                {
                    Log.Error(TagName, e.ToString());
                }
            }
        }

        void SetupDialog(Context context, MaskType maskType, Action cancelCallback, Func<Context, Dialog, MaskType, View> customSetup, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			Application.SynchronizationContext.Send(state => {

				var dialog = new Dialog(context);

                dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);

				if (maskType != MaskType.Black)
                    dialog.Window.ClearFlags(WindowManagerFlags.DimBehind);

				if (maskType == MaskType.None)
                    dialog.Window.SetFlags(WindowManagerFlags.NotTouchModal, WindowManagerFlags.NotTouchModal);

                dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));

				var customView = customSetup(context, dialog, maskType);

                dialog.SetContentView (customView);

                dialog.SetCancelable (cancelCallback != null);
				if (cancelCallback != null)
                    dialog.CancelEvent += (sender, e) => cancelCallback();

                prepareDialogCallback?.Invoke(dialog);

                CurrentDialog = dialog;

				CurrentDialog.Show ();

                dialogShownCallback?.Invoke(CurrentDialog);

			}, null);
		}

		void DismissCurrent()
        {
            if (CurrentDialog != null)
            {
                void ActionDismiss()
                {
                    try
                    {
                        if (!IsAlive(CurrentDialog) || !IsAlive(CurrentDialog.Window))
                            return;

                        CurrentDialog.Hide();
                        CurrentDialog.Dismiss();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(TagName, "Failed to dismiss dialog {0}", ex.ToString());
                    }
                    finally
                    {
                        statusText = null;
                        statusObj = null;
                        imageView = null;
                        progressWheel = null;
                        CurrentDialog = null;
                    }
                }

                Application.SynchronizationContext.Send(_ => ActionDismiss(), null);
            }
        }

        static bool IsAlive(Java.Lang.Object @object)
        {
            if (@object == null)
                return false;

            if (@object.Handle == IntPtr.Zero)
                return false;

            if (@object is Activity activity)
            {
                if (activity.IsFinishing)
                    return false;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1 && 
                    activity.IsDestroyed)
                    return false;
            }

            return true;
        }
    }

	public enum MaskType
	{
		None = 1,
		Clear = 2,
		Black = 3
	}
}
