using System;
using Android.OS;
using Android.Views;
using Android.Graphics;
using Android.Util;
using Android.Content;
using Android.Runtime;

namespace AndroidHUD
{
    [Register("androidhud.ProgressWheel")]
	public class ProgressWheel : View
	{
		public ProgressWheel(Context context) 
			: this(context, null, 0)
		{
		}

		public ProgressWheel(Context context, IAttributeSet attrs) 
			: this(context, attrs, 0)
		{
		}

		public ProgressWheel (Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
			CircleRadius = 80;
			BarLength = 60;
			BarWidth = 20;
			RimWidth = 20;
			TextSize = 20;

			//Padding (with defaults)
			WheelPaddingTop = 5;
			WheelPaddingBottom = 5;
			WheelPaddingLeft = 5;
			WheelPaddingRight = 5;

			//Colors (with defaults)
			BarColor = Color.White;
			CircleColor = Color.Transparent;
			RimColor = Color.Gray;
			TextColor = Color.White;

			//Animation
			//The amount of pixels to move the bar by on each draw
			SpinSpeed = 2;
			//The number of milliseconds to wait inbetween each draw
			DelayMillis = 0;

			spinHandler = new SpinHandler(msg => {
				Invalidate ();

				if (isSpinning)
				{
					progress += SpinSpeed;
					if (progress > 360)
						progress = 0;

					spinHandler.SendEmptyMessageDelayed (0, DelayMillis);
				}
			});


			//ParseAttributes(context.ObtainStyledAttributes(attrs, null)); //TODO: swap null for R.styleable.ProgressWheel
		}

//		public string Text
//		{
//			get { return text; }
//			set { text = value; splitText = text.Split('\n'); }
//		}

		//public string[] SplitText { get { return splitText; } }


		public int CircleRadius { get;set; }
		public int BarLength { get;set; }
		public int BarWidth { get;set; }
		public int TextSize { get;set; }
		public int WheelPaddingTop { get;set; }
		public int WheelPaddingBottom { get; set; }
		public int WheelPaddingLeft { get;set; }
		public int WheelPaddingRight { get;set; }
		public Color BarColor { get;set; }
		public Color CircleColor { get;set; }
		public Color RimColor { get;set; }
		public Shader RimShader { get { return rimPaint.Shader; } set { rimPaint.SetShader(value); } }
		public Color TextColor { get;set; }
		public int SpinSpeed { get;set; }
		public int RimWidth { get;set; }
		public int DelayMillis { get;set; }

		public bool IsSpinning { get { return isSpinning; } }

		//Sizes (with defaults)
		int fullRadius = 100;

		//Paints
		Paint barPaint = new Paint();
		Paint circlePaint = new Paint();
		Paint rimPaint = new Paint();
		Paint textPaint = new Paint();

		//Rectangles
		//RectF rectBounds = new RectF();
		RectF circleBounds = new RectF();

		int progress = 0;
		bool isSpinning = false;
		SpinHandler spinHandler;

		Android.OS.BuildVersionCodes version = Android.OS.Build.VERSION.SdkInt;

		//Other
		//string text = "";
        //string[] splitText = new string[]{};

		protected override void OnAttachedToWindow ()
		{
			base.OnAttachedToWindow ();

			SetupBounds ();
			SetupPaints ();

			Invalidate ();
		}

		void SetupPaints() 
		{
			barPaint.Color = BarColor;
			barPaint.AntiAlias = true;
			barPaint.SetStyle (Paint.Style.Stroke);
			barPaint.StrokeWidth = BarWidth;

			rimPaint.Color = RimColor;
			rimPaint.AntiAlias = true;
			rimPaint.SetStyle (Paint.Style.Stroke);
			rimPaint.StrokeWidth = RimWidth;

			circlePaint.Color = CircleColor;
			circlePaint.AntiAlias = true;
			circlePaint.SetStyle(Paint.Style.Fill);

			textPaint.Color = TextColor;
			textPaint.SetStyle(Paint.Style.Fill);
			textPaint.AntiAlias = true;
			textPaint.TextSize = TextSize;
		}

		void SetupBounds() 
		{
			WheelPaddingTop = this.WheelPaddingTop;
			WheelPaddingBottom = this.WheelPaddingBottom;
			WheelPaddingLeft = this.WheelPaddingLeft;
			WheelPaddingRight = this.WheelPaddingRight;

//			rectBounds = new RectF(WheelPaddingLeft,
//			                       WheelPaddingTop,
//			                       this.LayoutParameters.Width - WheelPaddingRight,
//			                       this.LayoutParameters.Height - WheelPaddingBottom);
//
			circleBounds = new RectF(WheelPaddingLeft + BarWidth,
			                         WheelPaddingTop + BarWidth,
			                         this.LayoutParameters.Width - WheelPaddingRight - BarWidth,
			                         this.LayoutParameters.Height - WheelPaddingBottom - BarWidth);

			fullRadius = (this.LayoutParameters.Width - WheelPaddingRight - BarWidth)/2;
			CircleRadius = (fullRadius - BarWidth) + 1;
		}

//		void ParseAttributes(Android.Content.Res.TypedArray a) 
//		{
//			BarWidth = (int) a.GetDimension(R.styleable.ProgressWheel_barWidth, barWidth);
//			RimWidth = (int) a.GetDimension(R.styleable.ProgressWheel_rimWidth, rimWidth);
//			SpinSpeed = (int) a.GetDimension(R.styleable.ProgressWheel_spinSpeed, spinSpeed);
//			DelayMillis = (int) a.GetInteger(R.styleable.ProgressWheel_delayMillis, delayMillis);
//
//			if(DelayMillis < 0)
//				DelayMillis = 0;
//	
//			BarColor = a.GetColor(R.styleable.ProgressWheel_barColor, barColor);
//			BarLength = (int) a.GetDimension(R.styleable.ProgressWheel_barLength, barLength);
//			TextSize = (int) a.GetDimension(R.styleable.ProgressWheel_textSize, textSize);
//			TextColor = (int) a.GetColor(R.styleable.ProgressWheel_textColor, textColor);
//
//			Text = a.GetString(R.styleable.ProgressWheel_text);
//	
//			RimColor = (int) a.GetColor(R.styleable.ProgressWheel_rimColor, rimColor);
//			CircleColor = (int) a.GetColor(R.styleable.ProgressWheel_circleColor, circleColor);
//		}



		//----------------------------------
		//Animation stuff
		//----------------------------------
		protected override void OnDraw (Canvas canvas)
		{
			base.OnDraw (canvas);

			//Draw the rim
			canvas.DrawArc(circleBounds, 360, 360, false, rimPaint);

			//Draw the bar
			if(isSpinning) 
				canvas.DrawArc(circleBounds, progress - 90, BarLength, false, barPaint);
			else
				canvas.DrawArc(circleBounds, -90, progress, false, barPaint);

			//Draw the inner circle
			canvas.DrawCircle((circleBounds.Width() / 2) + RimWidth + WheelPaddingLeft, 
			                  (circleBounds.Height() / 2) + RimWidth + WheelPaddingTop, 
			                  CircleRadius, 
			                  circlePaint);

			//Draw the text (attempts to center it horizontally and vertically)
//			int offsetNum = 2;
//
//			foreach (var s in splitText) 
//			{
//				float offset = textPaint.MeasureText(s) / 2;
//
//				canvas.DrawText(s, this.Width / 2 - offset, 
//				                this.Height / 2 + (TextSize * (offsetNum)) 
//				                - ((splitText.Length - 1) * (TextSize / 2)), textPaint);
//				offsetNum++;
//			}
		}

		public void ResetCount() 
		{
			progress = 0;
			//Text = "0%";
			Invalidate();
		}

		public void StopSpinning() 
		{
			isSpinning = false;
			progress = 0;
			spinHandler.RemoveMessages(0);
		}


		public void Spin() 
		{
			isSpinning = true;
			spinHandler.SendEmptyMessage(0);
		}

		public void IncrementProgress() 
		{


			isSpinning = false;
			progress++;
			//Text = Math.Round(((float)progress/(float)360)*(float)100) + "%";
			spinHandler.SendEmptyMessage(0);
		}

		public void SetProgress(int i) {
			isSpinning = false;
			var newProgress = (int)((float)i / (float)100 * (float)360);

			if (version >= Android.OS.BuildVersionCodes.Honeycomb)
			{
				Android.Animation.ValueAnimator va = 
					(Android.Animation.ValueAnimator)Android.Animation.ValueAnimator.OfInt (progress, newProgress).SetDuration (250);

				va.Update += (sender, e) => {
					var interimValue = (int)e.Animation.AnimatedValue;

					progress = interimValue;

					//Text = Math.Round(((float)interimValue/(float)360)*(float)100) + "%";

					Invalidate ();
				};

				va.Start ();
			} else {
				progress = newProgress;
				Invalidate ();
			}

			spinHandler.SendEmptyMessage(0);
		}

        private sealed class SpinHandler : Handler
		{
			public SpinHandler(Action<Message> msgAction) : base(msgAction)
			{
				MessageAction = msgAction;
			}

			public Action<Message> MessageAction { get; }

			public override void HandleMessage (Message msg)
			{
				MessageAction (msg);
			}
		}
	}
}

