using System;

using Android.App;
using Android.Content;
using Android.Runtime;

namespace Calligraphy.Xamarin.Test
{
	[Application]
	public class App : Application
    {      
		public override void OnCreate()
		{
			base.OnCreate();
			CalligraphyConfig.InitDefault(new CalligraphyConfig.Builder()
											.SetDefaultFontPath("fonts/IndieFlower.ttf")
										    .Build());
		}

		public App(IntPtr intPtr, JniHandleOwnership jniHandleOwnership)
			: base(intPtr, jniHandleOwnership) { }
	}
}
