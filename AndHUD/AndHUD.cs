using System;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Content;
using System.Threading;
using System.Threading.Tasks;

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


		public void Show (Activity activity, string status = null, int progress = -1, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null)
		{
			if (progress >= 0)
				showProgress (activity, progress, status, maskType, timeout, clickCallback);
			else
				showStatus (activity, true, status, maskType, timeout, clickCallback);
		}

		public void ShowSuccess(Activity activity, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null)
		{
			showImage (activity, activity.Resources.GetDrawable (Resource.Drawable.ic_successstatus), status, maskType, timeout, clickCallback);
		}

		public void ShowError(Activity activity, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null)
		{
			showImage (activity, activity.Resources.GetDrawable (Resource.Drawable.ic_errorstatus), status, maskType, timeout, clickCallback);
		}

		public void ShowSuccessWithStatus(Activity activity, string status, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null)
		{
			showImage (activity, activity.Resources.GetDrawable (Resource.Drawable.ic_successstatus), status, maskType, timeout, clickCallback);
		}

		public void ShowErrorWithStatus(Activity activity, string status, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null)
		{
			showImage (activity, activity.Resources.GetDrawable (Resource.Drawable.ic_errorstatus), status, maskType, timeout, clickCallback);
		}

		public void ShowImage(Activity activity, int drawableResourceId, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null)
		{
			showImage (activity, activity.Resources.GetDrawable(drawableResourceId), status, maskType, timeout, clickCallback);
		}

		public void ShowImage(Activity activity, Android.Graphics.Drawables.Drawable drawable, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null)
		{
			showImage (activity, drawable, status, maskType, timeout, clickCallback);
		}

		public void ShowToast(Activity activity, string status, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, bool centered = true, Action clickCallback = null)
		{
			showStatus (activity, false, status, maskType, timeout, clickCallback, centered);
		}

		public void Dismiss(Activity activity)
		{
			DismissCurrent (activity);
		}

		void showStatus (Activity activity, bool spinner, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, bool centered = true)
		{
			if (timeout == null)
				timeout = TimeSpan.Zero;

			DismissCurrent (activity);

			if (CurrentDialog != null && statusObj == null)
				DismissCurrent (activity);

			lock (dialogLock)
			{
				if (CurrentDialog == null)
				{
					SetupDialog (activity, maskType, (a, d, m) => {
						View view;

						view = LayoutInflater.From (activity).Inflate (Resource.Layout.Loading, null);

						if (clickCallback != null)
							view.Click += (sender, e) => clickCallback();

						statusObj = new object();

						statusText = view.FindViewById<TextView>(Resource.Id.textViewStatus);

						if (!spinner)
							view.FindViewById<ProgressBar>(Resource.Id.LoadingProgressBar).Visibility = ViewStates.Gone;

						if (maskType != MaskType.Black)
							view.SetBackgroundResource(Resource.Drawable.RoundedBgDark);

						if (statusText != null)
						{
							statusText.Text = status ?? "";
							statusText.Visibility = string.IsNullOrEmpty(status) ? ViewStates.Gone : ViewStates.Visible;
						}

						return view;
					});

					if (!centered)
					{
						CurrentDialog.Window.SetGravity (GravityFlags.Bottom);
						var p = CurrentDialog.Window.Attributes;

						p.Y = DpToPx (activity, 22);

						CurrentDialog.Window.Attributes = p;

					}

					if (timeout > TimeSpan.Zero)
					{
						Task.Factory.StartNew (() => {
							if (!waitDismiss.WaitOne (timeout.Value))
								DismissCurrent (activity);
						});
					}
				}
				else
				{
					activity.RunOnUiThread(() => {
						if (statusText != null)
							statusText.Text = status ?? "";
					});
				}
			}
		}

		int DpToPx(Context context, int dp) 
		{
			var displayMetrics = context.Resources.DisplayMetrics;
			int px = (int)Math.Round((double)dp * ((double)displayMetrics.Xdpi / (double)Android.Util.DisplayMetricsDensity.Default));       
			return px;
		}

		void showProgress(Activity activity, int progress, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null)
		{
			if (timeout == null)
				timeout = TimeSpan.Zero;

			if (CurrentDialog != null && progressWheel == null)
				DismissCurrent (activity);

			lock (dialogLock)
			{
				if (CurrentDialog == null)
				{
					SetupDialog (activity, maskType, (a, d, m) => {
						var view = activity.LayoutInflater.Inflate(Resource.Layout.LoadingProgress, null);

						if (clickCallback != null)
							view.Click += (sender, e) => clickCallback();

						progressWheel = view.FindViewById<ProgressWheel>(Resource.Id.LoadingProgressWheel);
						statusText = view.FindViewById<TextView>(Resource.Id.textViewStatus);

						if (maskType != MaskType.Black)
							view.SetBackgroundResource(Resource.Drawable.RoundedBgDark);

						progressWheel.SetProgress(0);

						if (statusText != null)
						{
							statusText.Text = status ?? "";
							statusText.Visibility = string.IsNullOrEmpty(status) ? ViewStates.Gone : ViewStates.Visible;
						}

						return view;
					});

					if (timeout > TimeSpan.Zero)
					{
						Task.Factory.StartNew (() => {
							if (!waitDismiss.WaitOne (timeout.Value))
								DismissCurrent (activity);
						});
					}
				}
				else
				{
					activity.RunOnUiThread(() => {
						progressWheel.SetProgress (progress);
						statusText.Text = status ?? "";
					});
				}
			}
		}


		void showImage(Activity activity, Android.Graphics.Drawables.Drawable image, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null)
		{
			if (timeout == null)
				timeout = TimeSpan.Zero;

			if (CurrentDialog != null && imageView == null)
				DismissCurrent (activity);

			lock (dialogLock)
			{
				if (CurrentDialog == null)
				{
					SetupDialog (activity, maskType, (a, d, m) => {
						var view = activity.LayoutInflater.Inflate(Resource.Layout.LoadingImage, null);

						if (clickCallback != null)
							view.Click += (sender, e) => clickCallback();

						imageView = view.FindViewById<ImageView>(Resource.Id.LoadingImage);
						statusText = view.FindViewById<TextView>(Resource.Id.textViewStatus);

						if (maskType != MaskType.Black)
							view.SetBackgroundResource(Resource.Drawable.RoundedBgDark);

						imageView.SetImageDrawable(image);

						if (statusText != null)
						{
							statusText.Text = status ?? "";
							statusText.Visibility = string.IsNullOrEmpty(status) ? ViewStates.Gone : ViewStates.Visible;
						}

						return view;
					});

					if (timeout > TimeSpan.Zero)
					{
						Task.Factory.StartNew (() => {
							if (!waitDismiss.WaitOne (timeout.Value))
								DismissCurrent (activity);
						});
					}
				}
				else
				{
					activity.RunOnUiThread(() => {
						imageView.SetImageDrawable(image);
						statusText.Text = status ?? "";
					});
				}
			}
		}



		void SetupDialog(Activity activity, MaskType maskType, Func<Activity, Dialog, MaskType, View> customSetup)
		{
			activity.RunOnUiThread (() => {

				CurrentDialog = new Dialog(activity);

				CurrentDialog.RequestWindowFeature((int)WindowFeatures.NoTitle);

				if (maskType != MaskType.Black)
					CurrentDialog.Window.ClearFlags(WindowManagerFlags.DimBehind);

				CurrentDialog.Window.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable(Android.Graphics.Color.Transparent));

				var customView = customSetup(activity, CurrentDialog, maskType);

				CurrentDialog.SetContentView (customView);
				CurrentDialog.SetCancelable (false);	

				CurrentDialog.Show ();

			});
		}

		void DismissCurrent(Activity activity)
		{
			lock (dialogLock)
			{
				if (CurrentDialog != null)
				{
					waitDismiss.Set ();

					activity.RunOnUiThread (() => {
						CurrentDialog.Hide ();
						CurrentDialog.Cancel ();

						statusText = null;
						statusObj = null;
						imageView = null;
						progressWheel = null;
						CurrentDialog = null;

						waitDismiss.Reset ();
					});

				}


			}
		}
	}

	public enum MaskType
	{
		Clear = 2,
		Black = 3
	}
}

