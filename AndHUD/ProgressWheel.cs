using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidHUD.Extensions;

namespace AndroidHUD
{
    /// <summary>
    /// Progress Wheel View for showing circular progress.
    /// </summary>
    [Register("androidhud.ProgressWheel")]
    public class ProgressWheel : View
    {
        // Sizes (with defaults)
        private int _fullRadius = 100;

        // Paints
        private Paint _barPaint = new Paint();
        private Paint _circlePaint = new Paint();
        private Paint _rimPaint = new Paint();
        private Paint _textPaint = new Paint();

        // Rectangles
        private RectF _circleBounds = new RectF();

        private int _progress = 0;
        private SpinHandler _spinHandler;

        private Android.OS.BuildVersionCodes _version = Android.OS.Build.VERSION.SdkInt;

        public ProgressWheel(Context context)
            : this(context, null, 0)
        {
        }

        public ProgressWheel(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public ProgressWheel(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            CircleRadius = 80;
            BarLength = 60;
            BarWidth = 20;
            RimWidth = 20;
            TextSize = 20;

            // Padding (with defaults)
            WheelPaddingTop = 5;
            WheelPaddingBottom = 5;
            WheelPaddingLeft = 5;
            WheelPaddingRight = 5;

            // Colors (with defaults)
            BarColor = Color.White;
            CircleColor = Color.Transparent;
            RimColor = Color.Gray;
            TextColor = Color.White;

            // Animation
            // The amount of pixels to move the bar by on each draw
            SpinSpeed = 2;

            // The number of milliseconds to wait inbetween each draw
            DelayMillis = 0;

            _spinHandler = new SpinHandler(msg =>
            {
                Invalidate();

                if (IsSpinning)
                {
                    _progress += SpinSpeed;
                    if (_progress > 360)
                    {
                        _progress = 0;
                    }

                    _spinHandler.SendEmptyMessageDelayed(0, DelayMillis);
                }
            });
        }

        public int CircleRadius { get; set; }

        public int BarLength { get; set; }

        public int BarWidth { get; set; }

        public int TextSize { get; set; }

        public int WheelPaddingTop { get; set; }

        public int WheelPaddingBottom { get; set; }

        public int WheelPaddingLeft { get; set; }

        public int WheelPaddingRight { get; set; }

        public Color BarColor { get; set; }

        public Color CircleColor { get; set; }

        public Color RimColor { get; set; }

        public Shader RimShader
        {
            get => _rimPaint.Shader;
            set => _rimPaint.SetShader(value);
        }

        public Color TextColor { get; set; }

        public int SpinSpeed { get; set; }

        public int RimWidth { get; set; }

        public int DelayMillis { get; set; }

        public bool IsSpinning { get; private set; }

        /// <summary>
        /// Reset progress to 0.
        /// </summary>
        public void ResetCount()
        {
            _progress = 0;

            // Text = "0%";
            Invalidate();
        }

        /// <summary>
        /// Stop spinning animation.
        /// </summary>
        public void StopSpinning()
        {
            IsSpinning = false;
            _progress = 0;
            _spinHandler.RemoveMessages(0);
        }

        /// <summary>
        /// Start spinning animation.
        /// </summary>
        public void Spin()
        {
            IsSpinning = true;
            _spinHandler.SendEmptyMessage(0);
        }

        /// <summary>
        /// Increment progress by 1.
        /// </summary>
        public void IncrementProgress()
        {
            IsSpinning = false;
            _progress++;
            _spinHandler.SendEmptyMessage(0);
        }

        /// <summary>
        /// Set progress.
        /// </summary>
        /// <param name="amount">Amount to increase progress.</param>
        public void SetProgress(int amount)
        {
            IsSpinning = false;
            var newProgress = (int)((float)amount / 100 * 360);

            if (_version >= Android.OS.BuildVersionCodes.Honeycomb)
            {
                var va =
                    (Android.Animation.ValueAnimator)Android.Animation.ValueAnimator.OfInt(_progress, newProgress).SetDuration(250);

                va.Update += (sender, e) =>
                {
                    _progress = (int)e.Animation.AnimatedValue;

                    Invalidate();
                };

                va.Start();
            }
            else
            {
                _progress = newProgress;
                Invalidate();
            }

            _spinHandler.SendEmptyMessage(0);
        }

        /// <inheritdoc/>
        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            SetupBounds();
            SetupPaints();

            Invalidate();
        }

        // ----------------------------------
        // Animation stuff
        // ----------------------------------

        /// <inheritdoc/>
        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (!canvas.IsAlive())
            {
                return;
            }

            // Draw the rim
            canvas.DrawArc(_circleBounds, 360, 360, false, _rimPaint);

            // Draw the bar
            if (IsSpinning)
            {
                canvas.DrawArc(_circleBounds, _progress - 90, BarLength, false, _barPaint);
            }
            else
            {
                canvas.DrawArc(_circleBounds, -90, _progress, false, _barPaint);
            }

            // Draw the inner circle
            canvas.DrawCircle(
                (_circleBounds.Width() / 2) + RimWidth + WheelPaddingLeft,
                (_circleBounds.Height() / 2) + RimWidth + WheelPaddingTop,
                CircleRadius,
                _circlePaint);
        }

        private void SetupPaints()
        {
            _barPaint.Color = BarColor;
            _barPaint.AntiAlias = true;
            _barPaint.SetStyle(Paint.Style.Stroke);
            _barPaint.StrokeWidth = BarWidth;

            _rimPaint.Color = RimColor;
            _rimPaint.AntiAlias = true;
            _rimPaint.SetStyle(Paint.Style.Stroke);
            _rimPaint.StrokeWidth = RimWidth;

            _circlePaint.Color = CircleColor;
            _circlePaint.AntiAlias = true;
            _circlePaint.SetStyle(Paint.Style.Fill);

            _textPaint.Color = TextColor;
            _textPaint.SetStyle(Paint.Style.Fill);
            _textPaint.AntiAlias = true;
            _textPaint.TextSize = TextSize;
        }

        private void SetupBounds()
        {
            _circleBounds =
                new RectF(
                    WheelPaddingLeft + BarWidth,
                    WheelPaddingTop + BarWidth,
                    LayoutParameters.Width - WheelPaddingRight - BarWidth,
                    LayoutParameters.Height - WheelPaddingBottom - BarWidth);

            _fullRadius = (LayoutParameters.Width - WheelPaddingRight - BarWidth) / 2;
            CircleRadius = (_fullRadius - BarWidth) + 1;
        }

        private class SpinHandler : Android.OS.Handler
        {
            public SpinHandler(Action<Android.OS.Message> msgAction)
            {
                MessageAction = msgAction;
            }

            public Action<Android.OS.Message> MessageAction { get; private set; }

            public override void HandleMessage(Android.OS.Message msg)
            {
                MessageAction(msg);
            }
        }
    }
}
