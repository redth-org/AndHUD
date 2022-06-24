using AndroidHUD;
using AndroidX.AppCompat.App;

namespace SampleNet6;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : AppCompatActivity
{
    private readonly string[] _demos = {
        "Status Indicator Only",
        "Status Indicator and Text",
        "Non-Modal Indicator and Text",
        "Progress Only",
        "Progress and Text",
        "Success Image Only",
        "Success Image and Text",
        "Error Image Only",
        "Error Image and Text",
        "Toast",
        "Toast Non-Centered",
        "Custom Image",
        "Click Callback",
        "Cancellable Callback",
        "Long Message",
        "Really Long Message"
    };

    private ListView _listView;
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set our view from the "main" layout resource
        SetContentView(Resource.Layout.activity_main);
        
        var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
        SetSupportActionBar(toolbar);

        var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, _demos);

        _listView = FindViewById<ListView>(Resource.Id.listview);
        _listView.Adapter = adapter;
        _listView.ItemClick += OnItemClick;
    }
    
    protected override void OnDestroy()
    {
        if (_listView != null)
            _listView.ItemClick -= OnItemClick;

        base.OnDestroy();
    }
    
    private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var demo = _demos[e.Position];

            switch (demo)
            {
                case "Status Indicator Only":
                    AndHUD.Shared.Show(this, null, -1, MaskType.Black, TimeSpan.FromSeconds(3));
                    break;
                case "Status Indicator and Text":
                    AndHUD.Shared.Show(this, "Loading...", -1, MaskType.Clear, TimeSpan.FromSeconds(3));
                    break;
                case "Non-Modal Indicator and Text":
                    AndHUD.Shared.Show(this, "Loading...", -1, MaskType.None, TimeSpan.FromSeconds(5));
                    break;
                case "Progress Only":
                    ShowProgressDemo(progress => AndHUD.Shared.Show(this, null, progress, MaskType.Clear));
                    break;
                case "Progress and Text":
                    ShowProgressDemo(progress => AndHUD.Shared.Show(this, "Loading... " + progress + "%", progress, MaskType.Clear));
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
                case "Click Callback":
                    AndHUD.Shared.ShowToast(this, "Click this toast to close it!", MaskType.Clear, null, true, () => AndHUD.Shared.Dismiss(this));
                    break;
                case "Cancellable Callback":
                    AndHUD.Shared.ShowToast(this, "Click back button to cancel/close it!", MaskType.None, null, true, null, () => AndHUD.Shared.Dismiss(this));
                    break;
                case "Long Message":
                    AndHUD.Shared.Show(this, "This is a longer message to display!", -1, MaskType.Black, TimeSpan.FromSeconds(3));
                    break;
                case "Really Long Message":
                    AndHUD.Shared.Show(this, "This is a really really long message to display as a status indicator, so you should shorten it!", -1, MaskType.Black, TimeSpan.FromSeconds(3));
                    break;
            }
        }

        void ShowProgressDemo(Action<int> action)
        {
            Task.Run(() => {
                int progress = 0;

                while (progress <= 100)
                {
                    action(progress);

                    new ManualResetEvent(false).WaitOne(500);
                    progress += 10;
                }

                AndHUD.Shared.Dismiss(this);
            });
        }

        void ShowDemo(Action action)
        {
            Task.Run(() => {

                action();

                new ManualResetEvent(false).WaitOne(3000);

                AndHUD.Shared.Dismiss(this);
            });
        }
}
