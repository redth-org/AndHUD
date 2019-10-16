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
        private readonly SpinHandler _spinHandler;
        private readonly Android.OS.BuildVersionCodes _version = Android.OS.Build.VERSION.SdkInt;

        // Paints
        private readonly Paint _barPaint = new Paint();
        private readonly Paint _circlePaint = new Paint();
        private readonly Paint _rimPaint = new Paint();
        private readonly Paint _textPaint = new Paint();

        // Sizes (with defaults)
        private int _fullRadius = 100;

        // Rectangles
        private RectF _circleBounds = new RectF();

        private int _progress;

        /// <summary>
        /// Create an instance of ProgressWheel.
        /// </summary>
        /// <param name="context">Android <see cref="Context"/> to create view from.</param>
        public ProgressWheel(Context context)
            : this(context, null, 0)
        {
        }

        /// <summary>
        /// Create an instance of ProgressWheel, used by Android <see cref="LayoutInflater"/>.
        /// </summary>
        /// <param name="context">Android <see cref="Context"/> to create view from.</param>
        /// <param name="attrs">Android XML attributes from the layout.</param>
        public ProgressWheel(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        /// <summary>
        /// Create an instance of ProgressWheel, used by Android <see cref="LayoutInflater"/>.
        /// </summary>
        /// <param name="context">Android <see cref="Context"/> to create view from.</param>
        /// <param name="attrs">Android XML attributes from the layout.</param>
        /// <param name="defStyle">Android style to apply to view.</param>
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

        /// <summary>
        /// Get or set the radius of the circle to draw. Defaults to half of the
        /// width of the view.
        /// </summary>
        public int CircleRadius { get; set; }

        /// <summary>
        /// Get or set the length of the bar.
        /// </summary>
        public int BarLength { get; set; }

        /// <summary>
        /// Get or set the width of the bar.
        /// </summary>
        public int BarWidth { get; set; }

        /// <summary>
        /// Get or set the size of the text in px.
        /// </summary>
        public int TextSize { get; set; }

        /// <summary>
        /// Get or set the top padding of the progress wheel.
        /// </summary>
        public int WheelPaddingTop { get; set; }

        /// <summary>
        /// Get or set the bottom padding of the progress wheel.
        /// </summary>
        public int WheelPaddingBottom { get; set; }

        /// <summary>
        /// Get or set the left padding of the progress wheel.
        /// </summary>
        public int WheelPaddingLeft { get; set; }

        /// <summary>
        /// Get or set the right padding of the progress wheel.
        /// </summary>
        public int WheelPaddingRight { get; set; }

        /// <summary>
        /// Gets or sets the color of the bar.
        /// </summary>
        public Color BarColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the circle.
        /// </summary>
        public Color CircleColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the rim.
        /// </summary>
        public Color RimColor { get; set; }

        /// <summary>
        /// Gets or sets the shader of the rim.
        /// </summary>
        public Shader RimShader
        {
            get => _rimPaint.Shader;
            set => _rimPaint.SetShader(value);
        }

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        /// Gets or sets the spinning speed for the animation.
        /// </summary>
        public int SpinSpeed { get; set; }

        /// <summary>
        /// Gets or sets the rim width.
        /// </summary>
        public int RimWidth { get; set; }

        /// <summary>
        /// Gets or sets the delay in ms, between progress increases.
        /// </summary>
        public int DelayMillis { get; set; }

        /// <summary>
        /// Gets whether the spinning animation is running.
        /// </summary>
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
