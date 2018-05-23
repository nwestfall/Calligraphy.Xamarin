using Android.Graphics;

namespace Calligraphy.Xamarin
{
	/// <summary>
	/// There are two ways to set typeface for custom views:
	/// 
	/// <list type="bullet">
	///     <item>Implementing the interface.  You should only implement <see cref=" SetTypeface(Typeface)"/> method.</item>
	///     <item>Or via reflection.  If custom view already has setTypeface method you can register is during Calligraphy configuration.</item>
	/// </list>
	/// 
	/// First way is faster but encourage more effort from the developer to implement interface.  Second one requires less effort
	/// but works slowly cause reflection calls.
    /// </summary>
    public interface IHasTypeface
    {
		void SetTypeface(Typeface typeface);
    }
}
