using AndroidX.AppCompat.App;
using Sample;

namespace SampleNet6;

[Activity(Label = "Second")]
public class SecondActivity : AppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        SetContentView(Resource.Layout.activity_second);
    }
}
