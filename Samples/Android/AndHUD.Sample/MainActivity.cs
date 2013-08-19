using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using AndroidHUD;

namespace Sample
{
	[Activity (Label = "AndHUD Sample", MainLauncher = true)]
	public class MainActivity : ListActivity
	{
		string[] demos = new string[] {
			"Status Indicator Only",
			"Status Indicator and Text",
			"Progress Only",
			"Progress and Text",
			"Success Image Only",
			"Success Image and Text",
			"Error Image Only",
			"Error Image and Text",
			"Toast",
			"Toast Non-Centered",
			"Custom Image"
		};

		ArrayAdapter<string> adapter;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			adapter = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleListItem1, demos);

			this.ListAdapter = adapter;

			ListView.ItemClick += (sender, e) => 
			{
				var demo = demos[e.Position];

				switch (demo)
				{
					case "Status Indicator Only":
						AndHUD.Shared.Show(this, null, -1, MaskType.Black, TimeSpan.FromSeconds(3));
						break;
					case "Status Indicator and Text":
						AndHUD.Shared.Show(this, "Loading...", -1, MaskType.Clear, TimeSpan.FromSeconds(3));
						break;
					case "Progress Only":
						ShowProgressDemo(progress => AndHUD.Shared.Show(this, null, progress, MaskType.Clear));
						break;
					case "Progress and Text":
						ShowProgressDemo(progress => AndHUD.Shared.Show(this, "Loading... " + progress + "%", progress, MaskType.Black));
						break;
					case "Success Image Only":
						AndHUD.Shared.ShowSuccessWithStatus(this, null, MaskType.Black, TimeSpan.FromSeconds(3));
						break;
					case "Success Image and Text":
						AndHUD.Shared.ShowSuccessWithStatus(this, "It Worked!", MaskType.Clear, TimeSpan.FromSeconds(3));
						break;
					case "Error Image Only":
						AndHUD.Shared.ShowErrorWithStatus(this, null, MaskType.Clear, TimeSpan.FromSeconds(3));
						break;
					case "Error Image and Text":
						AndHUD.Shared.ShowErrorWithStatus(this, "It no worked :(", MaskType.Black, TimeSpan.FromSeconds(3));
						break;
					case "Toast":
						AndHUD.Shared.ShowToast(this, "This is a toast... Cheers!", MaskType.Black, TimeSpan.FromSeconds(3), true);
						break;
					case "Toast Non-Centered":
						AndHUD.Shared.ShowToast(this, "This is a non-centered Toast...", MaskType.Clear, TimeSpan.FromSeconds(3), false);
						break;
					case "Custom Image":
						AndHUD.Shared.ShowImage(this, Resource.Drawable.ic_questionstatus, "Custom Image...", MaskType.Black, TimeSpan.FromSeconds(3));
						break;
				}
			
			};

		}

		void ShowProgressDemo(Action<int> action)
		{
			Task.Factory.StartNew (() => {

				int progress = 0;

				while (progress <= 100)
				{
					action (progress);

					Thread.Sleep (500);
					progress += 10;
				}

				AndHUD.Shared.Dismiss (this);
			});
		}

		void ShowDemo(Action action)
		{
			Task.Factory.StartNew(() => {

				action();

				Thread.Sleep(3000);

				AndHUD.Shared.Dismiss(this);
			});
		}
	}

}


