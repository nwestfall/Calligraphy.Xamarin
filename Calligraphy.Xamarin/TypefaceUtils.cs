using System;
using System.Collections.Generic;

using Android.Content.Res;
using Android.Graphics;
using Android.Util;

namespace Calligraphy.Xamarin
{
	/// <summary>
    /// A helper loading <see cref=" Typeface"/> avoiding the leak of the font when loaded
	/// by multiple calls to <see cref=" Typeface.CreateFromAsset(AssetManager, string)"/> on pre-ICS versions.
	/// 
	/// More details can be found here https://code.google.com/p/android/issues/detail?id=9904
    /// </summary>
    public static class TypefaceUtils
    {
		static readonly Dictionary<string, Typeface> cachedFonts = new Dictionary<string, Typeface>();
		static readonly Dictionary<Typeface, CalligraphyTypefaceSpan> cachedSpans = new Dictionary<Typeface, CalligraphyTypefaceSpan>();

        /// <summary>
        /// A helper loading a custom font
        /// </summary>
        /// <returns>Return <see cref=" Typeface"/> or null if the path is invalid</returns>
        /// <param name="assetManager">App's asset manager.</param>
        /// <param name="filePath">The path of the file.</param>
		public static Typeface Load(AssetManager assetManager, string filePath)
		{
			lock(cachedFonts)
			{
				try
				{
					if(!cachedFonts.ContainsKey(filePath))
					{
						Typeface typeface = Typeface.CreateFromAsset(assetManager, filePath);
						cachedFonts.Add(filePath, typeface);
						return typeface;
					}
				}
				catch(Exception ex)
				{
					Log.Warn("Calligraphy.Xamarin", Java.Lang.Throwable.FromException(ex), $"Can't create asset from {filePath}.  Make sure you have passed in the correct path and file name");
					cachedFonts.Add(filePath, null);
					return null;
				}
				return cachedFonts[filePath];            
			}
		}

        /// <summary>
        /// A helping loading custom spans so we don't have to keep creating hundreds of spans.
        /// </summary>
        /// <returns>Will return null if typeface passed in is null.</returns>
        /// <param name="typeface">Typeface not null typeface.</param>
        public static CalligraphyTypefaceSpan GetSpan(Typeface typeface)
		{
			if (typeface == null) return null;
            lock(cachedSpans)
			{
				if(!cachedSpans.ContainsKey(typeface))
				{
					var span = new CalligraphyTypefaceSpan(typeface);
					cachedSpans.Add(typeface, span);
					return span;
				}
				return cachedSpans[typeface];
			}
		}

        /// <summary>
        /// Is the passed in typeface one of ours?
        /// </summary>
		/// <returns>true if we have loaded it false otherwise.</returns>
		/// <param name="typeface">typeface nullable, the typeface to check if ours.</param>
		public static bool IsLoaded(Typeface typeface) => typeface != null && cachedFonts.ContainsValue(typeface);      
    }
}
