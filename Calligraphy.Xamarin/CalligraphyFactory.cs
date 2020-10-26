using System;

using Android.Annotation;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;

using Java.Lang.Reflect;

namespace Calligraphy.Xamarin
{
	class CalligraphyFactory
	{
		const string ACTION_BAR_TITLE = "action_bar_title";
		const string ACTION_BAR_SUBTITLE = "action_bar_subtitle";

		readonly int[] attributeId;

		/// <summary>
		/// Some styles are in sub styles, such as actionBarTextStyle etc..
		/// </summary>
		/// <returns>2 element array, default to -1 unless a style has been found..</returns>
		/// <param name="view">View to check.</param>
		protected static int[] GetStyleForTextView(TextView view)
		{
			int[] styleIds = { -1, -1 };
			// Try to find the specific actionbar styles
			if (IsActionBarTitle(view))
			{
				styleIds[0] = Android.Resource.Attribute.ActionBarStyle;
				styleIds[1] = Android.Resource.Attribute.TitleTextStyle;
			}
			else if (IsActionBarSubTitle(view))
			{
				styleIds[0] = Android.Resource.Attribute.ActionBarStyle;
				styleIds[1] = Android.Resource.Attribute.SubtitleTextStyle;
			}
			if (styleIds[0] == -1)
			{
				// Use TextAppearance as default style
				styleIds[0] = CalligraphyConfig.Get().ClassStyleAttributeMap.ContainsKey(view.GetType())
											   ? CalligraphyConfig.Get().ClassStyleAttributeMap[view.GetType()]
											   : Android.Resource.Attribute.TextAppearance;
			}
			return styleIds;
		}

		/// <summary>
		/// An even dirtier way to see if the TextView is part of the ActionBar
		/// </summary>
		/// <returns><c>true</c>, if action bar title, <c>false</c> otherwise.</returns>
		/// <param name="view">TextView to check is Title.</param>
		[SuppressLint(Value = new string[] { "NewApi" })]
		protected static bool IsActionBarTitle(TextView view)
		{
			if (MatchesResourceIdName(view, ACTION_BAR_TITLE)) return true;
			if (ParentIsToolbarV7(view))
			{
                var parent = (Android.Support.V7.Widget.Toolbar)view.Parent;
				return view.Text.Equals(parent.Title, StringComparison.InvariantCultureIgnoreCase);
			}
			return false;
		}

		/// <summary>
		/// An even dirtier way to see if the TextView is part of the ActionBar
		/// </summary>
		/// <returns><c>true</c>, if action bar sub title, <c>false</c> otherwise.</returns>
		/// <param name="view">TextView to check is Title.</param>
		[SuppressLint(Value = new string[] { "NewApi" })]
		public static bool IsActionBarSubTitle(TextView view)
		{
			if (MatchesResourceIdName(view, ACTION_BAR_SUBTITLE)) return true;
			if (ParentIsToolbarV7(view))
			{
                var parent = (Android.Support.V7.Widget.Toolbar)view.Parent;
				return view.Text.Equals(parent.Subtitle, StringComparison.InvariantCultureIgnoreCase);
			}
			return false;
		}

		protected static bool ParentIsToolbarV7(View view) => CalligraphyUtils.CanCheckForV7Toolbar() && view.Parent != null && (view.Parent.GetType().IsAssignableFrom(Java.Lang.Class.ForName("android.support.v7.widget.Toolbar").GetType()));

		/// <summary>
		/// Use to match a view against a potential view id. Such as ActionBar title etc.
		/// </summary>
		/// <returns><c>true</c>, if resource identifier name matches, <c>false</c> otherwise.</returns>
		/// <param name="view">not null view you want to see has resource matching name..</param>
		/// <param name="matches">not null resource name to match against. Its not case sensitive..</param>
		protected static bool MatchesResourceIdName(View view, string matches)
		{
			if (view.Id == View.NoId) return false;
			string resourceEntryName = view.Resources.GetResourceEntryName(view.Id);
			return resourceEntryName.Equals(matches, StringComparison.InvariantCultureIgnoreCase);
		}

		public CalligraphyFactory(int attributeId) => this.attributeId = new int[] { attributeId };

        /// <summary>
        /// Handle the created view
        /// </summary>
		/// <returns>null if null is passed in.</returns>
		/// <param name="view">nullable.</param>
		/// <param name="context">shouldn't be null.</param>
		/// <param name="attrs">shouldn't be null.</param>
        public View OnViewCreated(View view, Context context, IAttributeSet attrs)
		{
			if(view != null && view.GetTag(Resource.Id.calligraphy_tag_id)?.ToString() != "true")
			{
				OnViewCreatedInternal(view, context, attrs);
				view.SetTag(Resource.Id.calligraphy_tag_id, "true");
			}
			return view;
		}

        void OnViewCreatedInternal(View view, Context context, IAttributeSet attrs)
		{
			if(view is TextView textView)
			{
				// Fast path the setting of TextView's font, means if we do some delayed setting of font,
				// which has already been set by use we skip this TextView (mainly for inflating custom,
				// TextView's inside the Toolbar/ActionBar).
				if (TypefaceUtils.IsLoaded(textView.Typeface))
					return;

				// Try to get typeface attribute value
				// Since we're not using namespace it's a little bit tricky

				// Check xml attrs, style attrs and text appearance for font path
				string textViewFont = ResolveFontPath(context, attrs);

				// Try theme attributes
                if(!string.IsNullOrEmpty(textViewFont))
				{
					int[] styleForTextView = GetStyleForTextView(textView);
					if (styleForTextView[1] != -1)
						textViewFont = CalligraphyUtils.PullFontPathFromTheme(context, styleForTextView[0], styleForTextView[1], attributeId);
					else
						textViewFont = CalligraphyUtils.PullFontPathFromTheme(context, styleForTextView[0], attributeId);
				}

				// Still need to defer the Native action bar, appcompat-v7:21+ uses the Toolbar underneath. But won't match these anyway.
				bool deferred = MatchesResourceIdName(view, ACTION_BAR_TITLE) || MatchesResourceIdName(view, ACTION_BAR_SUBTITLE);

				CalligraphyUtils.ApplyFontToTextView(context, textView, CalligraphyConfig.Get(), textViewFont, deferred);
			}

			// AppCompat API21+ The ActionBar doesn't inflate default Title/SubTitle, we need to scan the
			// Toolbar(Which underlies the ActionBar) for its children.
            if (CalligraphyUtils.CanCheckForV7Toolbar() && view is Android.Support.V7.Widget.Toolbar toolbar)
                ApplyFontToToolbar(toolbar);
            
			// Try to set typeface for custom views using interface method or via reflection if available
            if(view is IHasTypeface hasTypeface)
			{
				Typeface typeface = GetDefaultTypeface(context, ResolveFontPath(context, attrs));
				if (typeface != null)
					hasTypeface.SetTypeface(typeface);
			}
			else if(CalligraphyConfig.Get().CustomViewTypefaceSupport
			        && CalligraphyConfig.Get().IsCustomViewHasTypeface(view))
			{
				Method setTypeface = ReflectionUtils.GetMethod(view.Class, "setTypeface");
				string fontPath = ResolveFontPath(context, attrs);
				Typeface typeface = GetDefaultTypeface(context, fontPath);
				if (setTypeface != null && typeface != null)
					ReflectionUtils.InvokeMethod(view, setTypeface, typeface);
			}
		}

        Typeface GetDefaultTypeface(Context context, string fontPath)
		{
			if (string.IsNullOrEmpty(fontPath))
				fontPath = CalligraphyConfig.Get().FontPath;
			if (!string.IsNullOrEmpty(fontPath))
				return TypefaceUtils.Load(context.Assets, fontPath);
			return null;
		}

        /// <summary>
		/// Resolving font path from xml attrs, style attrs or text appearance
        /// </summary>
        /// <returns>The font path.</returns>
        /// <param name="context">Context.</param>
        /// <param name="attrs">Attrs.</param>
        string ResolveFontPath(Context context, IAttributeSet attrs)
		{
			// Try view xml attributes
			string textViewFont = CalligraphyUtils.PullFontPathFromView(context, attrs, attributeId);

			// Try view style attributes
			if (string.IsNullOrEmpty(textViewFont))
				textViewFont = CalligraphyUtils.PullFontPathFromStyle(context, attrs, attributeId);

			// Try View TextAppearance
			if (string.IsNullOrEmpty(textViewFont))
				textViewFont = CalligraphyUtils.PullFontPathFromAppearance(context, attrs, attributeId);

			return textViewFont;
		}

        /// <summary>
		/// Will forcibly set text on the views then remove ones that didn't have copy.
        /// </summary>
        /// <param name="view">View.</param>
        void ApplyFontToToolbar(Android.Support.V7.Widget.Toolbar view)
		{
			string previousTitle = view.Title;
			string previousSubtitle = view.Subtitle;
			// The toolbar inflates both the title and the subtitle views lazily but luckily they do it
			// synchronously when you set a title and a subtitle programmatically.
			// So we set a title and a subtitle to something, then get the views, then revert.
			view.Title = "calligraphy.xamarin:toolbar_title";
			view.Subtitle = "calligraphy.xamarin:toolbar_subtitle";

			// Iterate through the children to run post inflation on them
			int childCount = view.ChildCount;
			for (var i = 0; i < childCount; i++)
				OnViewCreated(view.GetChildAt(i), view.Context, null);
			// Remove views from view if they didn't have copy set.
			view.Title = previousTitle;
			view.Subtitle = previousSubtitle;
		}
	}
}
