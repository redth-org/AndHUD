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

		ManualResetEvent waitDismiss = new ManualResetEvent(false);
		public Dialog CurrentDialog { get; private set; }

		ProgressWheel progressWheel = null;
		TextView statusText = null;
		ImageView imageView = null;

		object statusObj = null;

		readonly object dialogLock = new object();


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

		public void Dismiss(Context context = null)
		{
			DismissCurrent (context);
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
            if (context == null)
                throw new ArgumentNullException(nameof(context));

			if (timeout == null)
				timeout = TimeSpan.Zero;

			if (CurrentDialog != null && statusObj == null)
				DismissCurrent (context);

			lock (dialogLock)
			{
				if (CurrentDialog == null)
				{
					SetupDialog (context, maskType, cancelCallback, (a, d, m) => {
						View view = LayoutInflater.From (context).Inflate (Resource.Layout.loading, null);

						if (clickCallback != null)
							view.Click += (sender, e) => clickCallback();

						statusObj = new object();

						statusText = view.FindViewById<TextView>(Resource.Id.textViewStatus);

						if (!spinner)
							view.FindViewById<ProgressBar>(Resource.Id.loadingProgressBar).Visibility = ViewStates.Gone;

						if (maskType != MaskType.Black)
							view.SetBackgroundResource(Resource.Drawable.roundedbgdark);

						if (statusText != null)
						{
							statusText.Text = status ?? "";
							statusText.Visibility = string.IsNullOrEmpty(status) ? ViewStates.Gone : ViewStates.Visible;
						}

						if (!centered)
						{
							d.Window.SetGravity (GravityFlags.Bottom);
							var p = d.Window.Attributes;

							p.Y = DpToPx (context, 22);

							d.Window.Attributes = p;
						}

						return view;
					}, prepareDialogCallback, dialogShownCallback);

                    RunTimeout(context, timeout);
                }
				else
				{

					Application.SynchronizationContext.Send(state => {
						if (statusText != null)
							statusText.Text = status ?? "";
					}, null);
				}
			}
		}

		int DpToPx(Context context, int dp)
		{
			var displayMetrics = context.Resources.DisplayMetrics;
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, displayMetrics);
		}

		void showProgress(Context context, int progress, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			if (!timeout.HasValue || timeout == null)
				timeout = TimeSpan.Zero;

			if (CurrentDialog != null && progressWheel == null)
				DismissCurrent (context);

			lock (dialogLock)
			{
				if (CurrentDialog == null)
				{
					SetupDialog (context, maskType, cancelCallback, (a, d, m) => {
						var inflater = LayoutInflater.FromContext(context);
						var view = inflater.Inflate(Resource.Layout.loadingprogress, null);

						if (clickCallback != null)
							view.Click += (sender, e) => clickCallback();

						progressWheel = view.FindViewById<ProgressWheel>(Resource.Id.loadingProgressWheel);
						statusText = view.FindViewById<TextView>(Resource.Id.textViewStatus);

						if (maskType != MaskType.Black)
							view.SetBackgroundResource(Resource.Drawable.roundedbgdark);

						progressWheel.SetProgress(0);

						if (statusText != null)
						{
							statusText.Text = status ?? "";
							statusText.Visibility = string.IsNullOrEmpty(status) ? ViewStates.Gone : ViewStates.Visible;
						}

						return view;
					}, prepareDialogCallback, dialogShownCallback);

                    RunTimeout(context, timeout);
                }
				else
				{
					Application.SynchronizationContext.Send(state => {
						progressWheel?.SetProgress (progress);
						statusText.Text = status ?? "";
					}, null);
				}
			}
		}

		void showImage(Context context, Drawable image, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
		{
			if (timeout == null)
				timeout = TimeSpan.Zero;

			if (CurrentDialog != null && imageView == null)
				DismissCurrent (context);

			lock (dialogLock)
			{
				if (CurrentDialog == null)
				{
					SetupDialog (context, maskType, cancelCallback, (a, d, m) => {
						var inflater = LayoutInflater.FromContext(context);
						var view = inflater.Inflate(Resource.Layout.loadingimage, null);

						if (clickCallback != null)
							view.Click += (sender, e) => clickCallback();

						imageView = view.FindViewById<ImageView>(Resource.Id.loadingImage);
						statusText = view.FindViewById<TextView>(Resource.Id.textViewStatus);

						if (maskType != MaskType.Black)
							view.SetBackgroundResource(Resource.Drawable.roundedbgdark);

                        imageView?.SetImageDrawable(image);

						if (statusText != null)
						{
							statusText.Text = status ?? "";
							statusText.Visibility = string.IsNullOrEmpty(status) ? ViewStates.Gone : ViewStates.Visible;
						}

						return view;
					}, prepareDialogCallback, dialogShownCallback);

                    RunTimeout(context, timeout);
				}
				else
				{
					Application.SynchronizationContext.Send(state => {
                        imageView?.SetImageDrawable(image);
						statusText.Text = status ?? "";
					}, null);
				}
			}
		}

        void RunTimeout(Context context, TimeSpan? timeout)
        {
            if (timeout > TimeSpan.Zero)
            {
                Task.Run(() => {
                    if (!waitDismiss.WaitOne(timeout.Value))
                        DismissCurrent(context);

                }).ContinueWith(ct => {
                    var ex = ct.Exception;

                    if (ex != null)
                        Android.Util.Log.Error("AndHUD", ex.ToString());
                }, TaskContinuationOptions.OnlyOnFaulted);
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

		void DismissCurrent(Context context = null)
		{
			lock (dialogLock)
			{
				if (CurrentDialog != null)
				{
					waitDismiss.Set ();

					Action actionDismiss = () =>
					{
                        if (CurrentDialog != null)
                        {
                            CurrentDialog.Hide();
                            CurrentDialog.Dismiss();
                        }

						statusText = null;
						statusObj = null;
						imageView = null;
						progressWheel = null;
						CurrentDialog = null;

						waitDismiss.Reset ();
					};

					// First try the SynchronizationContext
					if (Application.SynchronizationContext != null)
					{
						Application.SynchronizationContext.Send (state => actionDismiss (), null);
						return;
					}

                    // Otherwise try OwnerActivity on dialog
                    var ownerActivity = CurrentDialog.OwnerActivity;
                    if (IsAlive(ownerActivity))
                    {
                        ownerActivity.RunOnUiThread(actionDismiss);
                        return;
                    }

                    // Otherwise try get it from the Window Context
                    if (IsAlive(CurrentDialog?.Window?.Context))
					{
                        if (CurrentDialog.Window.Context is Activity windowActivity)
                        {
                            windowActivity.RunOnUiThread(actionDismiss);
                            return;
                        }
                    }

                    // Finally if all else fails, let's see if someone passed in a context to dismiss and it
                    // happens to also be an Activity
                    if (context != null && context is Activity activity)
                    {
                        activity?.RunOnUiThread(actionDismiss);
                        return;
                    }
                }
			}
		}

        bool IsAlive(Java.Lang.Object @object)
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

