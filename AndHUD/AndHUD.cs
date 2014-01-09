using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AndroidHUD
{
	public class AndHUD
	{
		private static AndHUD shared;
		private readonly object dialogLock = new object();

		private readonly ManualResetEvent waitDismiss = new ManualResetEvent(false);
		private ImageView imageView;
		private ProgressWheel progressWheel;

		private object statusObj;
		private TextView statusText;

		public static AndHUD Shared
		{
			get
			{
				if (shared == null)
					shared = new AndHUD();

				return shared;
			}
		}

		public Dialog CurrentDialog { get; private set; }


		public void Show(Context context, string status = null, int progress = -1, MaskType maskType = MaskType.Black, TimeSpan? timeout = null,
			Action clickCallback = null, Action cancelCallback = null)
		{
			if (progress >= 0)
				showProgress(context, progress, status, maskType, timeout, clickCallback, cancelCallback);
			else
				showStatus(context, true, status, maskType, timeout, clickCallback, cancelCallback: cancelCallback);
		}

		public void ShowSuccess(Context context, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null)
		{
			showImage(context, context.Resources.GetDrawable(Resource.Drawable.ic_successstatus), status, maskType, timeout, clickCallback);
		}

		public void ShowError(Context context, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null)
		{
			showImage(context, context.Resources.GetDrawable(Resource.Drawable.ic_errorstatus), status, maskType, timeout, clickCallback);
		}

		public void ShowSuccessWithStatus(Context context, string status, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null)
		{
			showImage(context, context.Resources.GetDrawable(Resource.Drawable.ic_successstatus), status, maskType, timeout, clickCallback);
		}

		public void ShowErrorWithStatus(Context context, string status, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null)
		{
			showImage(context, context.Resources.GetDrawable(Resource.Drawable.ic_errorstatus), status, maskType, timeout, clickCallback);
		}

		public void ShowImage(Context context, int drawableResourceId, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null,
			Action clickCallback = null)
		{
			showImage(context, context.Resources.GetDrawable(drawableResourceId), status, maskType, timeout, clickCallback);
		}

		public void ShowImage(Context context, Drawable drawable, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null,
			Action clickCallback = null)
		{
			showImage(context, drawable, status, maskType, timeout, clickCallback);
		}

		public void ShowToast(Context context, string status, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, bool centered = true,
			Action clickCallback = null)
		{
			showStatus(context, false, status, maskType, timeout, clickCallback, centered);
		}

		public void Dismiss(Context context = null)
		{
			DismissCurrent();
		}

		private void showStatus(Context context, bool spinner, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null,
			Action clickCallback = null, bool centered = true, Action cancelCallback = null)
		{
			if (timeout == null)
				timeout = TimeSpan.Zero;

			DismissCurrent(context);

			if (CurrentDialog != null && statusObj == null)
				DismissCurrent(context);

			lock (dialogLock)
			{
				if (CurrentDialog == null)
				{
					SetupDialog(context, maskType, cancelCallback, (a, d, m) =>
					{
						View view = LayoutInflater.From(context).Inflate(Resource.Layout.loading, null);

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

						return view;
					});

					if (!centered)
					{
						CurrentDialog.Window.SetGravity(GravityFlags.Bottom);
						WindowManagerLayoutParams p = CurrentDialog.Window.Attributes;

						p.Y = DpToPx(context, 22);

						CurrentDialog.Window.Attributes = p;
					}

					if (timeout > TimeSpan.Zero)
					{
						Task.Factory.StartNew(() =>
						{
							if (!waitDismiss.WaitOne(timeout.Value))
								DismissCurrent(context);
						}).ContinueWith(ct =>
						{
							AggregateException ex = ct.Exception;

							if (ex != null)
								Log.Error("AndHUD", ex.ToString());
						}, TaskContinuationOptions.OnlyOnFaulted);
					}
				}
				else
				{
					Application.SynchronizationContext.Post(state =>
					{
						if (statusText != null)
							statusText.Text = status ?? "";
					}, null);
				}
			}
		}

		private int DpToPx(Context context, int dp)
		{
			DisplayMetrics displayMetrics = context.Resources.DisplayMetrics;
			var px = (int) Math.Round(dp * (displayMetrics.Xdpi / (double) DisplayMetricsDensity.Default));
			return px;
		}

		private void showProgress(Context context, int progress, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null,
			Action clickCallback = null, Action cancelCallback = null)
		{
			if (timeout == null)
				timeout = TimeSpan.Zero;

			if (CurrentDialog != null && progressWheel == null)
				DismissCurrent(context);

			lock (dialogLock)
			{
				if (CurrentDialog == null)
				{
					SetupDialog(context, maskType, cancelCallback, (a, d, m) =>
					{
						LayoutInflater inflater = LayoutInflater.FromContext(context);
						View view = inflater.Inflate(Resource.Layout.loadingprogress, null);

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
					});

					if (timeout > TimeSpan.Zero)
					{
						Task.Factory.StartNew(() =>
						{
							if (!waitDismiss.WaitOne(timeout.Value))
								DismissCurrent(context);
						}).ContinueWith(ct =>
						{
							AggregateException ex = ct.Exception;

							if (ex != null)
								Log.Error("AndHUD", ex.ToString());
						}, TaskContinuationOptions.OnlyOnFaulted);
					}
				}
				else
				{
					Application.SynchronizationContext.Post(state =>
					{
						progressWheel.SetProgress(progress);
						statusText.Text = status ?? "";
					}, null);
				}
			}
		}


		private void showImage(Context context, Drawable image, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null,
			Action clickCallback = null, Action cancelCallback = null)
		{
			if (timeout == null)
				timeout = TimeSpan.Zero;

			if (CurrentDialog != null && imageView == null)
				DismissCurrent(context);

			lock (dialogLock)
			{
				if (CurrentDialog == null)
				{
					SetupDialog(context, maskType, cancelCallback, (a, d, m) =>
					{
						LayoutInflater inflater = LayoutInflater.FromContext(context);
						View view = inflater.Inflate(Resource.Layout.loadingimage, null);

						if (clickCallback != null)
							view.Click += (sender, e) => clickCallback();

						imageView = view.FindViewById<ImageView>(Resource.Id.loadingImage);
						statusText = view.FindViewById<TextView>(Resource.Id.textViewStatus);

						if (maskType != MaskType.Black)
							view.SetBackgroundResource(Resource.Drawable.roundedbgdark);

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
						Task.Factory.StartNew(() =>
						{
							if (!waitDismiss.WaitOne(timeout.Value))
								DismissCurrent(context);
						}).ContinueWith(ct =>
						{
							AggregateException ex = ct.Exception;

							if (ex != null)
								Log.Error("AndHUD", ex.ToString());
						}, TaskContinuationOptions.OnlyOnFaulted);
					}
				}
				else
				{
					Application.SynchronizationContext.Post(state =>
					{
						imageView.SetImageDrawable(image);
						statusText.Text = status ?? "";
					}, null);
				}
			}
		}


		private void SetupDialog(Context context, MaskType maskType, Action cancelCallback, Func<Context, Dialog, MaskType, View> customSetup)
		{
			if (Application.SynchronizationContext == null) return;
			Application.SynchronizationContext.Post(state =>
			{
				CurrentDialog = new Dialog(context);

				CurrentDialog.RequestWindowFeature((int) WindowFeatures.NoTitle);

				if (maskType != MaskType.Black)
					CurrentDialog.Window.ClearFlags(WindowManagerFlags.DimBehind);

				CurrentDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));


				View customView = customSetup(context, CurrentDialog, maskType);

				CurrentDialog.SetContentView(customView);

				CurrentDialog.SetCancelable(cancelCallback != null);
				if (cancelCallback != null)
					CurrentDialog.CancelEvent += (sender, args) => cancelCallback();

				CurrentDialog.Show();
			}, null);
		}

		private void DismissCurrent(Context context = null)
		{
			lock (dialogLock)
			{
				if (CurrentDialog != null)
				{
					waitDismiss.Set();

					Application.SynchronizationContext.Post(state =>
					{
						CurrentDialog.Hide();
						CurrentDialog.Cancel();

						statusText = null;
						statusObj = null;
						imageView = null;
						progressWheel = null;
						CurrentDialog = null;

						waitDismiss.Reset();
					}, null);
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
