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
using AndroidHUD.Extensions;

namespace AndroidHUD
{
    /// <summary>
    /// Android HUD.
    /// </summary>
    public class AndHUD : IDisposable
    {
        private static AndHUD _shared;
        private readonly object _dialogLock = new object();
        private readonly ManualResetEvent _waitDismiss = new ManualResetEvent(false);

        private ProgressWheel _progressWheel;
        private TextView _statusText;
        private ImageView _imageView;

        private object _statusObj;

        private AndHUD()
        {
        }

        /// <summary>
        /// Gets the static instance of AndHUD.
        /// </summary>
        public static AndHUD Shared
        {
            get
            {
                if (_shared == null)
                {
                    _shared = new AndHUD();
                }

                return _shared;
            }
        }

        /// <summary>
        /// Gets the currently shown dialog.
        /// </summary>
        public Dialog CurrentDialog { get; private set; }

        /// <summary>
        /// <para>Show a dialog with a progress indicator.</para>
        /// <para>If <paramref name="progress"/> is provided it will be shown determinate, otherwise indeterminate.</para>
        /// </summary>
        /// <param name="context">Android <see cref="Context"/> needed to show the dialog.</param>
        /// <param name="status">Status text to show under progress or loading indicator.</param>
        /// <param name="progress">Progress, if over 0, then you should call this method again to update the progress.</param>
        /// <param name="maskType"><see cref="MaskType"/> indicating, whether background should be masked.</param>
        /// <param name="timeout">Timeout to automatically dismiss the dialog.</param>
        /// <param name="clickCallback">Callback for when dialog is clicked.</param>
        /// <param name="centered">Center dialog if true, otherwise align towards bottom.</param>
        /// <param name="cancelCallback">Callback for when dialog is dismissed by clicking outside of it.</param>
        /// <param name="prepareDialogCallback">Callback for preparing the dialog.</param>
        /// <param name="dialogShownCallback">Callback for when dialog is shown on the screen.</param>
        public void Show(Context context, string status = null, int progress = -1, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, bool centered = true, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
        {
            if (progress >= 0)
            {
                ShowProgress(context, progress, status, maskType, timeout, clickCallback, cancelCallback, prepareDialogCallback, dialogShownCallback);
            }
            else
            {
                ShowStatus(context, true, status, maskType, timeout, clickCallback, centered, cancelCallback, prepareDialogCallback, dialogShownCallback);
            }
        }

        /// <summary>
        /// Show a dialog with an success image.
        /// </summary>
        /// <param name="context">Android <see cref="Context"/> needed to show the dialog.</param>
        /// <param name="status">Status text to show under the success image.</param>
        /// <param name="maskType"><see cref="MaskType"/> indicating, whether background should be masked.</param>
        /// <param name="timeout">Timeout to automatically dismiss the dialog.</param>
        /// <param name="clickCallback">Callback for when dialog is clicked.</param>
        /// <param name="cancelCallback">Callback for when dialog is dismissed by clicking outside of it.</param>
        /// <param name="prepareDialogCallback">Callback for preparing the dialog.</param>
        /// <param name="dialogShownCallback">Callback for when dialog is shown on the screen.</param>
        public void ShowSuccess(Context context, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
        {
            ShowImage(context, Resource.Drawable.ic_successstatus, status, maskType, timeout, clickCallback, cancelCallback, prepareDialogCallback, dialogShownCallback);
        }

        /// <summary>
        /// Show a dialog with an error image.
        /// </summary>
        /// <param name="context">Android <see cref="Context"/> needed to show the dialog.</param>
        /// <param name="status">Status text to show under the error image.</param>
        /// <param name="maskType"><see cref="MaskType"/> indicating, whether background should be masked.</param>
        /// <param name="timeout">Timeout to automatically dismiss the dialog.</param>
        /// <param name="clickCallback">Callback for when dialog is clicked.</param>
        /// <param name="cancelCallback">Callback for when dialog is dismissed by clicking outside of it.</param>
        /// <param name="prepareDialogCallback">Callback for preparing the dialog.</param>
        /// <param name="dialogShownCallback">Callback for when dialog is shown on the screen.</param>
        public void ShowError(Context context, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
        {
            ShowImage(context, Resource.Drawable.ic_errorstatus, status, maskType, timeout, clickCallback, cancelCallback, prepareDialogCallback, dialogShownCallback);
        }

        /// <summary>
        /// Show a dialog with an image.
        /// </summary>
        /// <param name="context">Android <see cref="Context"/> needed to show the dialog.</param>
        /// <param name="drawableResourceId">Android drawable resource id.</param>
        /// <param name="status">Status text to show under the image.</param>
        /// <param name="maskType"><see cref="MaskType"/> indicating, whether background should be masked.</param>
        /// <param name="timeout">Timeout to automatically dismiss the dialog.</param>
        /// <param name="clickCallback">Callback for when dialog is clicked.</param>
        /// <param name="cancelCallback">Callback for when dialog is dismissed by clicking outside of it.</param>
        /// <param name="prepareDialogCallback">Callback for preparing the dialog.</param>
        /// <param name="dialogShownCallback">Callback for when dialog is shown on the screen.</param>
        public void ShowImage(Context context, int drawableResourceId, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
        {
            ShowImage(context, GetDrawable(context, drawableResourceId), status, maskType, timeout, clickCallback, cancelCallback, prepareDialogCallback, dialogShownCallback);
        }

        /// <summary>
        /// Show a dialog with an image.
        /// </summary>
        /// <param name="context">Android <see cref="Context"/> needed to show the dialog.</param>
        /// <param name="drawable">Android drawable resource.</param>
        /// <param name="status">Status text to show under the image.</param>
        /// <param name="maskType"><see cref="MaskType"/> indicating, whether background should be masked.</param>
        /// <param name="timeout">Timeout to automatically dismiss the dialog.</param>
        /// <param name="clickCallback">Callback for when dialog is clicked.</param>
        /// <param name="cancelCallback">Callback for when dialog is dismissed by clicking outside of it.</param>
        /// <param name="prepareDialogCallback">Callback for preparing the dialog.</param>
        /// <param name="dialogShownCallback">Callback for when dialog is shown on the screen.</param>
        public void ShowImage(Context context, Drawable drawable, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
        {
            ShowWithImage(context, drawable, status, maskType, timeout, clickCallback, cancelCallback, prepareDialogCallback, dialogShownCallback);
        }

        /// <summary>
        /// Show a toast dialog.
        /// </summary>
        /// <param name="context">Android <see cref="Context"/> needed to show the dialog.</param>
        /// <param name="status">Status text to show.</param>
        /// <param name="maskType"><see cref="MaskType"/> indicating, whether background should be masked.</param>
        /// <param name="timeout">Timeout to automatically dismiss the dialog.</param>
        /// <param name="centered">Center dialog if true, otherwise align towards bottom.</param>
        /// <param name="clickCallback">Callback for when dialog is clicked.</param>
        /// <param name="cancelCallback">Callback for when dialog is dismissed by clicking outside of it.</param>
        /// <param name="prepareDialogCallback">Callback for preparing the dialog.</param>
        /// <param name="dialogShownCallback">Callback for when dialog is shown on the screen.</param>
        public void ShowToast(Context context, string status, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, bool centered = true, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
        {
            ShowStatus(context, false, status, maskType, timeout, clickCallback, centered, cancelCallback, prepareDialogCallback, dialogShownCallback);
        }

        /// <summary>
        /// Dismiss currently shown dialog.
        /// </summary>
        /// <param name="context">Android <see cref="Context"/> needed to dismiss the dialog.</param>
        public void Dismiss(Context context = null)
        {
            DismissCurrent(context);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        /// <param name="disposing">Is disposing from <see cref="Dispose"/> call.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _progressWheel.Dispose();
                _statusText.Dispose();
                _imageView.Dispose();
                _waitDismiss.Dispose();
            }
        }

        private static Drawable GetDrawable(Context context, int drawableResourceId)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                return context.Resources.GetDrawable(drawableResourceId, context.Theme);
            }

#pragma warning disable CS0618 // Type or member is obsolete
            return context.Resources.GetDrawable(drawableResourceId);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static int DpToPx(Context context, int dp)
        {
            var displayMetrics = context.Resources.DisplayMetrics;
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, displayMetrics);
        }

        private void ShowStatus(Context context, bool spinner, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, bool centered = true, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (timeout == null)
            {
                timeout = TimeSpan.Zero;
            }

            if (CurrentDialog != null && _statusObj == null)
            {
                DismissCurrent(context);
            }

            lock (_dialogLock)
            {
                if (CurrentDialog == null)
                {
                    SetupDialog(
                        context,
                        maskType,
                        cancelCallback,
                        (_, d, __) =>
                        {
                            View view = LayoutInflater.From(context).Inflate(Resource.Layout.loading, null);

                            if (clickCallback != null)
                            {
                                view.Click += (sender, e) => clickCallback();
                            }

                            _statusObj = new object();

                            _statusText = view.FindViewById<TextView>(Resource.Id.textViewStatus);

                            if (!spinner)
                            {
                                view.FindViewById<ProgressBar>(Resource.Id.loadingProgressBar).Visibility = ViewStates.Gone;
                            }

                            if (maskType != MaskType.Black)
                            {
                                view.SetBackgroundResource(Resource.Drawable.roundedbgdark);
                            }

                            if (_statusText != null)
                            {
                                _statusText.Text = status ?? string.Empty;
                                _statusText.Visibility = string.IsNullOrEmpty(status) ? ViewStates.Gone : ViewStates.Visible;
                            }

                            if (!centered)
                            {
                                d.Window.SetGravity(GravityFlags.Bottom);
                                var p = d.Window.Attributes;

                                p.Y = DpToPx(context, 22);

                                d.Window.Attributes = p;
                            }

                            return view;
                        },
                        prepareDialogCallback,
                        dialogShownCallback);

                    RunTimeout(context, timeout);
                }
                else
                {
                    Application.SynchronizationContext.Send(
                        _ =>
                        {
                            if (_statusText != null)
                            {
                                _statusText.Text = status ?? string.Empty;
                            }
                        }, null);
                }
            }
        }

        private void ShowProgress(Context context, int progress, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
        {
            if (!timeout.HasValue || timeout == null)
            {
                timeout = TimeSpan.Zero;
            }

            if (CurrentDialog != null && _progressWheel == null)
            {
                DismissCurrent(context);
            }

            lock (_dialogLock)
            {
                if (CurrentDialog == null)
                {
                    SetupDialog(
                        context,
                        maskType,
                        cancelCallback,
                        (a, d, m) =>
                        {
                            var inflater = LayoutInflater.FromContext(context);
                            var view = inflater.Inflate(Resource.Layout.loadingprogress, null);

                            if (clickCallback != null)
                            {
                                view.Click += (sender, e) => clickCallback();
                            }

                            _progressWheel = view.FindViewById<ProgressWheel>(Resource.Id.loadingProgressWheel);
                            _statusText = view.FindViewById<TextView>(Resource.Id.textViewStatus);

                            if (maskType != MaskType.Black)
                            {
                                view.SetBackgroundResource(Resource.Drawable.roundedbgdark);
                            }

                            _progressWheel.SetProgress(0);

                            if (_statusText != null)
                            {
                                _statusText.Text = status ?? string.Empty;
                                _statusText.Visibility = string.IsNullOrEmpty(status) ? ViewStates.Gone : ViewStates.Visible;
                            }

                            return view;
                        },
                        prepareDialogCallback,
                        dialogShownCallback);

                    RunTimeout(context, timeout);
                }
                else
                {
                    Application.SynchronizationContext.Send(
                        _ =>
                        {
                            _progressWheel?.SetProgress(progress);
                            _statusText.Text = status ?? string.Empty;
                        }, null);
                }
            }
        }

        private void ShowWithImage(Context context, Drawable image, string status = null, MaskType maskType = MaskType.Black, TimeSpan? timeout = null, Action clickCallback = null, Action cancelCallback = null, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
        {
            if (timeout == null)
            {
                timeout = TimeSpan.Zero;
            }

            if (CurrentDialog != null && _imageView == null)
            {
                DismissCurrent(context);
            }

            lock (_dialogLock)
            {
                if (CurrentDialog == null)
                {
                    SetupDialog(
                        context,
                        maskType,
                        cancelCallback,
                        (a, d, m) =>
                        {
                            var inflater = LayoutInflater.FromContext(context);
                            var view = inflater.Inflate(Resource.Layout.loadingimage, null);

                            if (clickCallback != null)
                            {
                                view.Click += (sender, e) => clickCallback();
                            }

                            _imageView = view.FindViewById<ImageView>(Resource.Id.loadingImage);
                            _statusText = view.FindViewById<TextView>(Resource.Id.textViewStatus);

                            if (maskType != MaskType.Black)
                            {
                                view.SetBackgroundResource(Resource.Drawable.roundedbgdark);
                            }

                            _imageView?.SetImageDrawable(image);

                            if (_statusText != null)
                            {
                                _statusText.Text = status ?? string.Empty;
                                _statusText.Visibility = string.IsNullOrEmpty(status) ? ViewStates.Gone : ViewStates.Visible;
                            }

                            return view;
                        },
                        prepareDialogCallback,
                        dialogShownCallback);

                    RunTimeout(context, timeout);
                }
                else
                {
                    Application.SynchronizationContext.Send(
                        state =>
                        {
                            _imageView?.SetImageDrawable(image);
                            _statusText.Text = status ?? string.Empty;
                        }, null);
                }
            }
        }

        private void RunTimeout(Context context, TimeSpan? timeout)
        {
            if (timeout > TimeSpan.Zero)
            {
                _ = Task.Run(() =>
                {
                    if (!_waitDismiss.WaitOne(timeout.Value))
                    {
                        DismissCurrent(context);
                    }
                })
                .ContinueWith(
                    ct =>
                    {
                        var ex = ct.Exception;

                        if (ex != null)
                        {
                            Android.Util.Log.Error("AndHUD", ex.ToString());
                        }
                    },
                    TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        private void SetupDialog(Context context, MaskType maskType, Action cancelCallback, Func<Context, Dialog, MaskType, View> customSetup, Action<Dialog> prepareDialogCallback = null, Action<Dialog> dialogShownCallback = null)
        {
            Application.SynchronizationContext.Send(
                state =>
                {
                    var dialog = new Dialog(context);

                    dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);

                    if (maskType != MaskType.Black)
                    {
                        dialog.Window.ClearFlags(WindowManagerFlags.DimBehind);
                    }

                    if (maskType == MaskType.None)
                    {
                        dialog.Window.SetFlags(WindowManagerFlags.NotTouchModal, WindowManagerFlags.NotTouchModal);
                    }

                    dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));

                    var customView = customSetup(context, dialog, maskType);

                    dialog.SetContentView(customView);

                    dialog.SetCancelable(cancelCallback != null);
                    if (cancelCallback != null)
                    {
                        dialog.CancelEvent += (sender, e) => cancelCallback();
                    }

                    prepareDialogCallback?.Invoke(dialog);

                    CurrentDialog = dialog;

                    CurrentDialog.Show();

                    dialogShownCallback?.Invoke(CurrentDialog);
                }, null);
        }

        private void DismissCurrent(Context context = null)
        {
            lock (_dialogLock)
            {
                if (CurrentDialog != null)
                {
                    _waitDismiss.Set();

                    Action actionDismiss = () =>
                    {
                        if (CurrentDialog != null)
                        {
                            CurrentDialog.Hide();
                            CurrentDialog.Dismiss();
                        }

                        _statusText = null;
                        _statusObj = null;
                        _imageView = null;
                        _progressWheel = null;
                        CurrentDialog = null;

                        _waitDismiss.Reset();
                    };

                    // First try the SynchronizationContext
                    if (Application.SynchronizationContext != null)
                    {
                        Application.SynchronizationContext.Send(state => actionDismiss(), null);
                        return;
                    }

                    // Otherwise try OwnerActivity on dialog
                    var ownerActivity = CurrentDialog.OwnerActivity;
                    if (ownerActivity.IsAlive())
                    {
                        ownerActivity.RunOnUiThread(actionDismiss);
                        return;
                    }

                    // Otherwise try get it from the Window Context
                    if (CurrentDialog.Window.Context is Activity windowActivity && windowActivity.IsAlive())
                    {
                        windowActivity.RunOnUiThread(actionDismiss);
                        return;
                    }

                    // Finally if all else fails, let's see if someone passed in a context to dismiss and it
                    // happens to also be an Activity
                    if (context != null && context is Activity activity && activity.IsAlive())
                    {
                        activity.RunOnUiThread(actionDismiss);
                        return;
                    }
                }
            }
        }
    }
}
