using Android.Content;
using Android.Util;
using Android.Views;

namespace Calligraphy.Xamarin
{
    interface ICalligraphyActivityFactory
    {
		/// <summary>
		/// Used to Wrap the Activity onCreateView method.
		/// You implement this method like so in your base activity.
		/// 
		/// <code>
		/// public View OnCreateView(View parent, string name, Context context, IAttributeSet attrs)
		/// {
		///     return CalligraphyContextWrapper.Get(BaseContext).OnActivityCreateView(base.OnCreateView(parent, name, context, attrs), attrs);
		/// }
		/// </code>
        /// </summary>
		/// <returns>The view passed in, or null if nothing was passed in.</returns>
		/// <param name="parent">parent view, can be null.</param>
		/// <param name="view">result of <code>base.OnCreateView(parent, name, context, attrs)</code>, this might be null, which is fine</param>
		/// <param name="name">Name of View we are trying to inflate</param>
		/// <param name="context">Current context (normally the Activity's)</param>
        /// <param name="attrs">Attrs.</param>
		View OnActivityCreateView(View parent, View view, string name, Context context, IAttributeSet attrs);
    }
}
