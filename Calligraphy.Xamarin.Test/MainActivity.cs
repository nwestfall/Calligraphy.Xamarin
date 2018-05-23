using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;

namespace Calligraphy.Xamarin.Test
{
    [Activity(Label = "Calligraphy.Xamarin.Test", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
        }

		protected override void AttachBaseContext(Context @base)
        {
            base.AttachBaseContext(CalligraphyContextWrapper.Wrap(@base));
        }
	}
}

