using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace Calligraphy.Xamarin
{
	public class CalligraphyContextWrapper : ContextWrapper
    {
		CalligraphyLayoutInflater inflater;

		readonly int attributeId;

        /// <summary>
		/// Uses the default configuration from <see cref="CalligraphyConfig"/>
		/// 
		/// Remember if you are defining default in the
		/// <see cref="CalligraphyConfig"/> make sure this is initialised before
		/// the activity is created.
        /// </summary>
		/// <returns>ContextWrapper to pass back to the activity.</returns>
		/// <param name="base">ContextBase to Wrap.</param>
		public static ContextWrapper Wrap(Context @base) => new CalligraphyContextWrapper(@base);

        /// <summary>
		/// You only need to call this IF you call
		/// <see cref="CalligraphyConfig.Builder.DisablePrivateFactoryInjection()"/>
		/// This will need to be called from the
		/// <see cref="Activity.OnCreateView(View, string, Context, IAttributeSet)"/>
		/// method to enable view font injection if the view is created inside the activity onCreateView.
		/// 
		/// You would implement this method like so in you base activity.
		/// <code>
		/// public View OnCreateView(View parent, String name, Context context, AttributeSet attrs) {
        ///     return CalligraphyContextWrapper.OnActivityCreateView(this, parent, super.onCreateView(parent, name, context, attrs), name, context, attrs);
        /// }
		/// </code>
        /// </summary>
		/// <returns>The same view passed in, or null if null passed in.</returns>
		/// <param name="activity">The activity the original that the ContextWrapper was attached too.</param>
		/// <param name="parent">Parent view from onCreateView</param>
		/// <param name="view">The View Created inside OnCreateView or from base.OnCreateView</param>
		/// <param name="name">The View name from OnCreateView</param>
		/// <param name="context">The context from OnCreateView</param>
		/// <param name="attrs">The AttributeSet from OnCreateView</param>
		public static View OnActivityCreateView(Activity activity, View parent, View view, string name, Context context, IAttributeSet attrs) =>
		    Get(activity).OnActivityCreateView(parent, view, name, context, attrs);

        /// <summary>
		/// Get the Calligraphy Activity Fragment Instance to allow callbacks for when views are created.
        /// </summary>
		/// <returns>Interface allowing you to call OnActivityViewCreated</returns>
		/// <param name="activity">The activity the original that the ContextWrapper was attached too.</param>
        static ICalligraphyActivityFactory Get(Activity activity)
		{
			if (!(activity.LayoutInflater is CalligraphyLayoutInflater))
				throw new NotSupportedException("This activity does not wrap the Base Context! See CalligraphyContextWrapper.Wrap(Context)");
			return (ICalligraphyActivityFactory)activity.LayoutInflater;
		}

        /// <summary>
		/// Uses the default configuration from <see cref="CalligraphyConfig"/>
		/// 
		/// Remember if you are defining default in the
        /// <see cref="CalligraphyConfig"/> make sure this is initialised before
        /// the activity is created.
        /// </summary>
		/// <param name="base">ContextBase to Wrap.</param>
		CalligraphyContextWrapper(Context @base) : base(@base) => attributeId = CalligraphyConfig.Get().AttrId;

		[Obsolete("Use Wrap(Context)")]
		public CalligraphyContextWrapper(Context @base, int attributeId) : base(@base) => this.attributeId = attributeId;
        

		public override Java.Lang.Object GetSystemService([StringDef(Type = "Android.Content.Context", Fields = new[] { "PowerService", "WindowService", "LayoutInflaterService", "AccountService", "ActivityService", "AlarmService", "NotificationService", "AccessibilityService", "CaptioningService", "KeyguardService", "LocationService", "SearchService", "SensorService", "StorageService", "StorageStatsService", "WallpaperService", "VibratorService", "ConnectivityService", "NetworkStatsService", "WifiService", "WifiAwareService", "WifiP2pService", "NsdService", "AudioService", "FingerprintService", "MediaRouterService", "TelephonyService", "TelephonySubscriptionService", "CarrierConfigService", "TelecomService", "ClipboardService", "InputMethodService", "TextServicesManagerService", "TextClassificationService", "AppwidgetService", "DropboxService", "DevicePolicyService", "UiModeService", "DownloadService", "NfcService", "BluetoothService", "UsbService", "LauncherAppsService", "InputService", "DisplayService", "UserService", "RestrictionsService", "AppOpsService", "CameraService", "PrintService", "ConsumerIrService", "TvInputService", "UsageStatsService", "MediaSessionService", "BatteryService", "JobSchedulerService", "MediaProjectionService", "MidiService", "HardwarePropertiesService", "ShortcutService", "SystemHealthService", "CompanionDeviceService" }), StringDef(Type = "Android.Content.Context", Fields = new[] { "PowerService", "WindowService", "LayoutInflaterService", "AccountService", "ActivityService", "AlarmService", "NotificationService", "AccessibilityService", "CaptioningService", "KeyguardService", "LocationService", "SearchService", "SensorService", "StorageService", "StorageStatsService", "WallpaperService", "VibratorService", "ConnectivityService", "NetworkStatsService", "WifiService", "WifiAwareService", "WifiP2pService", "NsdService", "AudioService", "FingerprintService", "MediaRouterService", "TelephonyService", "TelephonySubscriptionService", "CarrierConfigService", "TelecomService", "ClipboardService", "InputMethodService", "TextServicesManagerService", "TextClassificationService", "AppwidgetService", "DropboxService", "DevicePolicyService", "UiModeService", "DownloadService", "NfcService", "BluetoothService", "UsbService", "LauncherAppsService", "InputService", "DisplayService", "UserService", "RestrictionsService", "AppOpsService", "CameraService", "PrintService", "ConsumerIrService", "TvInputService", "UsageStatsService", "MediaSessionService", "BatteryService", "JobSchedulerService", "MediaProjectionService", "MidiService", "HardwarePropertiesService", "ShortcutService", "SystemHealthService", "CompanionDeviceService" })] string name)
		{
			if(LayoutInflaterService.Equals(name, StringComparison.InvariantCultureIgnoreCase))
			{
				if (inflater == null)
					inflater = new CalligraphyLayoutInflater(LayoutInflater.FromContext(BaseContext), this, attributeId, false);
				return inflater;
			}

			return base.GetSystemService(name);         
		}
	}
}
