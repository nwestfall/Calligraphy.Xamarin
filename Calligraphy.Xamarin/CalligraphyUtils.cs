using System;

using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Widget;

namespace Calligraphy.Xamarin
{
    public static class CalligraphyUtils
    {
		public static readonly int[] ANDROID_ATTR_TEXT_APPEARENCE = { Android.Resource.Attribute.TextAppearance };
		static bool? toolbarCheck = null;
		static bool? appCompatViewCheck = null;

        /// <summary>
		/// Applies a custom typeface span to the text.
        /// </summary>
		/// <returns>The passed in string.</returns>
		/// <param name="s">text to apply it too.</param>
        /// <param name="typeface">Typeface to apply.</param>
		public static string ApplyTypefaceSpan(string s, Typeface typeface)
		{
			if(!string.IsNullOrEmpty(s))
			{
				SpannableString span = new SpannableString(s);
				span.SetSpan(TypefaceUtils.GetSpan(typeface), 0, s.Length, SpanTypes.ExclusiveExclusive);
			}
			return s;
		}

		/// <summary>
        /// Applies a custom typeface span to the text.
        /// </summary>
        /// <returns>Either the passed in Object or new IEditable with the typeface span applied.</returns>
        /// <param name="s">text to apply it too.</param>
        /// <param name="typeface">Typeface to apply.</param>
        public static IEditable ApplyTypefaceSpan(IEditable s, Typeface typeface)
		{
			if (s?.Length() > 0)
				s.SetSpan(TypefaceUtils.GetSpan(typeface), 0, s.Length(), SpanTypes.ExclusiveExclusive);
			return s;
		}

        /// <summary>
		/// Applies a Typeface to a TextView.
		/// Defaults to false for deferring, if you are having issues with the textview keeping
		/// the custom Typeface, use with <paramref name="deferred"/> true
        /// </summary>
        /// <returns><c>true</c>, if font to text view was applyed, <c>false</c> otherwise.</returns>
		/// <param name="textView">Not null, TextView or child of.</param>
		/// <param name="typeface">Not null, Typeface to apply to the TextView.</param>
		/// <param name="deferred">If true we use Typefaces and TextChange listener to make sure font is always applied, but this sometimes conflicts with other <see cref="ISpannable"/></param>
        public static bool ApplyFontToTextView(TextView textView, Typeface typeface, bool deferred = false)
		{
			if (textView == null || typeface == null) return false;
			textView.PaintFlags = textView.PaintFlags | PaintFlags.SubpixelText | PaintFlags.AntiAlias;
			textView.SetTypeface(typeface, typeface.Style);
            if(deferred)
			{
				textView.SetText(ApplyTypefaceSpan(textView.Text, typeface), TextView.BufferType.Spannable);
                textView.AfterTextChanged += (object sender, AfterTextChangedEventArgs e) => 
				{
					ApplyTypefaceSpan(e.Editable, typeface);
				};
			}
			return true;
		}

        /// <summary>
		/// Useful for manually fonts to a TextView. Will not default back to font
		/// set in <see cref="CalligraphyConfig"/>
        /// </summary>
        /// <returns><c>true</c>, if font to text view was applyed, <c>false</c> otherwise.</returns>
        /// <param name="context">Context.</param>
		/// <param name="textView">Not null, TextView to apply to.</param>
		/// <param name="filePath">if null/empty will do nothing.</param>
        /// <param name="deferred">If set to <c>true</c> deferred.</param>
        public static bool ApplyFontToTextView(Context context, TextView textView, string filePath, bool deferred = false)
		{
			if (textView == null || context == null) return false;
			AssetManager assetManager = context.Assets;
			Typeface typeface = TypefaceUtils.Load(assetManager, filePath);
			return ApplyFontToTextView(textView, typeface, deferred);
		}

		internal static void ApplyFontToTextView(Context context, TextView textView, CalligraphyConfig config, bool deferred = false)
		{
			if (context == null || textView == null || config == null) return;
			if (!config.IsFontSet) return;
			ApplyFontToTextView(context, textView, config.FontPath, deferred);
		}

        /// <summary>
		/// Applies font to TextView. Will fall back to the default one if not set.
        /// </summary>
        /// <param name="context">Context.</param>
		/// <param name="textView">textView to apply to.</param>
        /// <param name="config">Default Config.</param>
		/// <param name="textViewFont">nullable, will use Default Config if null or fails to find the defined font</param>
        /// <param name="deferred">If set to <c>true</c> deferred.</param>
        public static void ApplyFontToTextView(Context context, TextView textView, CalligraphyConfig config, string textViewFont, bool deferred = false)
		{
			if (context == null || textView == null || config == null) return;
			if (!string.IsNullOrEmpty(textViewFont) && ApplyFontToTextView(context, textView, textViewFont, deferred))
				return;
			ApplyFontToTextView(context, textView, config, deferred);
		}

        /// <summary>
		/// Tries to pull the Custom Attribute directly from the TextView.
        /// </summary>
		/// <returns>null if attribute is not defined or added to View</returns>
        /// <param name="context">Context.</param>
        /// <param name="attrs">Attrs.</param>
		/// <param name="attributeId">if -1 returns null.</param>
		internal static string PullFontPathFromView(Context context, IAttributeSet attrs, int[] attributeId)
		{
			if (attributeId == null || attrs == null)
				return null;

			string attributeName;
			try
			{
				attributeName = context.Resources.GetResourceEntryName(attributeId[0]);            
			}
            catch(Android.Content.Res.Resources.NotFoundException)
			{
				// invalid attribute id
				return null;
			}

			int stringResourceId = attrs.GetAttributeResourceValue(null, attributeName, -1);
			return stringResourceId > 0
					? context.GetString(stringResourceId)
				    : attrs.GetAttributeValue(null, attributeName);
		}

        /// <summary>
		/// Tries to pull the Font Path from the View Style as this is the next decendent after being
		/// defined in the View's xml.
        /// </summary>
		/// <returns>null if attribute is not defined or found in the Style</returns>
        /// <param name="context">Context.</param>
        /// <param name="attrs">View Attrs.</param>
		/// <param name="attributeId">if -1 returns null.</param>
        internal static string PullFontPathFromStyle(Context context, IAttributeSet attrs, int[] attributeId)
		{
			if (attributeId == null || attrs == null)
				return null;

			TypedArray typedArray = context.ObtainStyledAttributes(attrs, attributeId);
            if(typedArray != null)
			{
				try
				{
					// first defined attribute
					string fontFromAttribute = typedArray.GetString(0);
					if (!string.IsNullOrEmpty(fontFromAttribute))
						return fontFromAttribute;
				}
                catch(Exception)
				{
					// failed for some reason
				}
                finally
				{
					typedArray.Recycle();
				}
			}
			return null;
		}

        /// <summary>
		/// Tries to pull the Font Path from the Text Appearance.
        /// </summary>
		/// <returns>returns null if attribute is not defined or if no TextAppearance is found.</returns>
        /// <param name="context">Context.</param>
        /// <param name="attrs">Attrs.</param>
		/// <param name="attributeId">if -1 returns null.</param>
        internal static string PullFontPathFromAppearance(Context context, IAttributeSet attrs, int[] attributeId)
		{
			if (attributeId == null || attrs == null)
				return null;

			int textAppearanceId = -1;
			TypedArray typedArray = context.ObtainStyledAttributes(attrs, ANDROID_ATTR_TEXT_APPEARENCE);
            if(typedArray != null)
			{
				try
				{
					textAppearanceId = typedArray.GetResourceId(0, -1);               
				}
                catch(Exception)
				{
					// ignore
				}
                finally
				{
					typedArray.Recycle();
				}
			}

			TypedArray textAppearanceAttrs = context.ObtainStyledAttributes(textAppearanceId, attributeId);
            if(textAppearanceAttrs != null)
			{
				try
				{
					return textAppearanceAttrs.GetString(0);
				}
                catch(Exception)
				{
					// ignore
				}
                finally
				{
					textAppearanceAttrs.Recycle();
				}
			}

			return null;
		}

        internal static string PullFontPathFromTheme(Context context, int styleAttrId, int[] attributeId)
		{
			if (styleAttrId == -1 || attributeId == null)
				return null;

			Android.Content.Res.Resources.Theme theme = context.Theme;
			var value = new TypedValue();
            
			theme.ResolveAttribute(styleAttrId, value, true);
			TypedArray typedArray = theme.ObtainStyledAttributes(value.ResourceId, attributeId);
			try
			{
				string font = typedArray.GetString(0);
				return font;
			}
            catch(Exception)
			{
				//failed for some reason
				return null;
			}
            finally
			{
				typedArray.Recycle();
			}
		}
      
        /// <summary>
		/// Last but not least, try to pull the Font Path from the Theme, which is defined.
        /// </summary>
		/// <returns>null if no theme or attribute defined.</returns>
        /// <param name="context">Activity Context.</param>
		/// <param name="styleAttrId">Theme style id.</param>
		/// <param name="subStyleAttrId">the sub style from the theme to look up after the first style.</param>
		/// <param name="attributeId">if -1 returns null.</param>
		internal static string PullFontPathFromTheme(Context context, int styleAttrId, int subStyleAttrId, int[] attributeId)
		{
			if (styleAttrId == -1 || attributeId == null)
				return null;

			Android.Content.Res.Resources.Theme theme = context.Theme;
			var value = new TypedValue();

			theme.ResolveAttribute(styleAttrId, value, true);
			int subStyleResId = -1;
			TypedArray parentTypedArray = theme.ObtainStyledAttributes(value.ResourceId, new int[] { subStyleAttrId });
			try
			{
				subStyleResId = parentTypedArray.GetResourceId(0, -1);
			}
            catch(Exception)
			{
				// Failed for some reason.
                return null;
			}
            finally
			{
				parentTypedArray.Recycle();
			}

			if (subStyleResId == -1) return null;
			TypedArray subTypedArray = context.ObtainStyledAttributes(subStyleResId, attributeId);
			if (subTypedArray != null)
			{
				try
				{
					return subTypedArray.GetString(0);
				}
				catch (Exception)
				{
					// Failed for some reason.
					return null;
				}
				finally
				{
					subTypedArray.Recycle();
				}
			}
			return null;
		}

        internal static bool CanCheckForV7Toolbar()
		{
			if(toolbarCheck == null)
			{
				try
				{
					Java.Lang.Class.ForName("android.support.v7.widget.Toolbar");
					toolbarCheck = true;
				}
                catch(Java.Lang.ClassNotFoundException)
				{
					toolbarCheck = false;
				}
			}
			return toolbarCheck.Value;
		}

        internal static bool CanAddV7AppCompatViews()
		{
			if(appCompatViewCheck == null)
			{
				try
				{
					Java.Lang.Class.ForName("android.support.v7.widget.AppCompatTextView");
					appCompatViewCheck = true;
				}
                catch(Java.Lang.ClassNotFoundException)
				{
					appCompatViewCheck = false;
				}
			}
			return appCompatViewCheck.Value;
		}
    }
}
