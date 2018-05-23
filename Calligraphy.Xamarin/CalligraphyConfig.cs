using System;
using System.Linq;
using System.Collections.Generic;

using Android.OS;
using Android.Views;
using Android.Widget;

using Java.Lang;

namespace Calligraphy.Xamarin
{
	public class CalligraphyConfig
	{
		/// <summary>
		/// The default styles for the factory to lookup. The builder builds an extended immutable
		/// map of this with any additional custom styles.
        /// </summary>
		static readonly Dictionary<Type, int> DEFAULT_STYLES = new Dictionary<Type, int>();

		static CalligraphyConfig()
		{
			DEFAULT_STYLES.TryAdd(typeof(TextView), Android.Resource.Attribute.TextViewStyle);
			DEFAULT_STYLES.TryAdd(typeof(Button), Android.Resource.Attribute.ButtonStyle);
			DEFAULT_STYLES.TryAdd(typeof(EditText), Android.Resource.Attribute.EditTextStyle);
			DEFAULT_STYLES.TryAdd(typeof(AutoCompleteTextView), Android.Resource.Attribute.AutoCompleteTextViewStyle);
			DEFAULT_STYLES.TryAdd(typeof(MultiAutoCompleteTextView), Android.Resource.Attribute.AutoCompleteTextViewStyle);
			DEFAULT_STYLES.TryAdd(typeof(CheckBox), Android.Resource.Attribute.CheckboxStyle);
			DEFAULT_STYLES.TryAdd(typeof(RadioButton), Android.Resource.Attribute.RadioButtonStyle);
			DEFAULT_STYLES.TryAdd(typeof(ToggleButton), Android.Resource.Attribute.ButtonStyleToggle);
			if (CalligraphyUtils.CanAddV7AppCompatViews())
				AddAppCompatViews();
		}
        
        /// <summary>
		/// AppCompat will inflate special versions of views for Material tinting etc,
		/// this adds those classes to the style lookup map
        /// </summary>
        static void AddAppCompatViews()
		{
			DEFAULT_STYLES.TryAdd(Class.ForName("android.support.v7.widget.AppCompatTextView").GetType(), Android.Resource.Attribute.TextViewStyle);
			DEFAULT_STYLES.TryAdd(Class.ForName("android.support.v7.widget.AppCompatButton").GetType(), Android.Resource.Attribute.ButtonStyle);
			DEFAULT_STYLES.TryAdd(Class.ForName("android.support.v7.widget.AppCompatEditText").GetType(), Android.Resource.Attribute.EditTextStyle);
			DEFAULT_STYLES.TryAdd(Class.ForName("android.support.v7.widget.AppCompatAutoCompleteTextView").GetType(), Android.Resource.Attribute.AutoCompleteTextViewStyle);
			DEFAULT_STYLES.TryAdd(Class.ForName("android.support.v7.widget.AppCompatMultiAutoCompleteTextView").GetType(), Android.Resource.Attribute.AutoCompleteTextViewStyle);
			DEFAULT_STYLES.TryAdd(Class.ForName("android.support.v7.widget.AppCompatCheckBox").GetType(), Android.Resource.Attribute.CheckboxStyle);
			DEFAULT_STYLES.TryAdd(Class.ForName("android.support.v7.widget.AppCompatRadioButton").GetType(), Android.Resource.Attribute.RadioButtonStyle);
			DEFAULT_STYLES.TryAdd(Class.ForName("android.support.v7.widget.AppCompatCheckedTextView").GetType(), Android.Resource.Attribute.CheckedTextViewStyle);
		}

		static CalligraphyConfig instance;

        /// <summary>
		/// Set the default Calligraphy Config
        /// </summary>
		/// <param name="calligraphyConfig">the config build using the <see cref="Builder"/>.</param>
		public static void InitDefault(CalligraphyConfig calligraphyConfig) => instance = calligraphyConfig;

        /// <summary>
		/// The current Calligraphy Config.
		/// If not set it will create a default config.
        /// </summary>
        /// <returns>The get.</returns>
        public static CalligraphyConfig Get()
		{
			if (instance == null)
				instance = new CalligraphyConfig(new Builder());
			return instance;
		}
        
        /// <summary>
		/// Is a default font set?
        /// </summary>
        /// <value><c>true</c> if is font set; otherwise, <c>false</c>.</value>
		public bool IsFontSet
		{
			get;
			private set;
		}

        /// <summary>
		/// The default Font Path if nothing else is setup.
        /// </summary>
        /// <value>The font path.</value>
		public string FontPath
		{
			get;
			private set;
		}

        /// <summary>
		/// Default Font Path Attr Id to lookup
        /// </summary>
        /// <value>The attr identifier.</value>
        public int AttrId
		{
			get;
			private set;
		}

        /// <summary>
		/// Use Reflection to inject the private factory.
        /// </summary>
        /// <value><c>true</c> if reflection; otherwise, <c>false</c>.</value>
        public bool Reflection
		{
			get;
			private set;
		}

        /// <summary>
		/// Use Reflection to intercept CustomView inflation with the correct Context.
        /// </summary>
        /// <value><c>true</c> if custom view creation; otherwise, <c>false</c>.</value>
        public bool CustomViewCreation
		{
			get;
			private set;
		}

        /// <summary>
		/// Use Reflection to try to set typeface for custom views if they has setTypeface method
        /// </summary>
        /// <value><c>true</c> if custom view typeface support; otherwise, <c>false</c>.</value>
        public bool CustomViewTypefaceSupport
		{
			get;
			private set;
		}

        /// <summary>
		/// Class Styles. Build from DEFAULT_STYLES and the builder.
        /// </summary>
		public IReadOnlyDictionary<Type, int> ClassStyleAttributeMap
		{
			get;
			private set;
		}

		readonly IReadOnlyList<Type> hasTypefaceViews;

        /// <summary>
        /// If custom view has typeface.
        /// </summary>
        /// <returns><c>true</c>, if custom view has typeface, <c>false</c> otherwise.</returns>
        /// <param name="view">View.</param>
		public bool IsCustomViewHasTypeface(View view) => hasTypefaceViews.Contains(view.GetType());

		protected CalligraphyConfig(Builder builder)
		{
			IsFontSet = builder.isFontSet;
			FontPath = builder.fontAssetPath;
			AttrId = builder.attrId;
			Reflection = builder.reflection;
			CustomViewCreation = builder.customViewCreation;
			CustomViewTypefaceSupport = builder.customViewTypefaceSupport;
			var tempMap = new Dictionary<Type, int>(DEFAULT_STYLES);
			foreach (var style in builder.styleClassMap)
				tempMap.Add(style.Key, style.Value);
			ClassStyleAttributeMap = tempMap;
			hasTypefaceViews = builder.hasTypefaceClasses;
		}

        public class Builder
		{
			/// <summary>
            /// Default AttrId if not set
            /// </summary>
			public const int INVALID_ATTR_ID = -1;
            /// <summary>
            /// Use reflection to inject the private factory.  Doesn't exist pre HC.  So default to false
            /// </summary>
			internal bool reflection = Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb;
            /// <summary>
			/// Use Reflection to intercept CustomView inflation with the correct Context.
            /// </summary>
			internal bool customViewCreation = true;
            /// <summary>
			/// Use Reflection during view creation to try change typeface via setTypeface method if it exists
            /// </summary>
			internal bool customViewTypefaceSupport = false;
            /// <summary>
			/// The fontAttrId to look up the font path from.
            /// </summary>
			internal int attrId = Resource.Attribute.fontPath;
            /// <summary>
			/// Has the user set the default font path.
            /// </summary>
			internal bool isFontSet = false;
            /// <summary>
			/// The default fontPath
            /// </summary>
			internal string fontAssetPath = null;
            /// <summary>
			/// Additional Class Styles. Can be empty.
            /// </summary>
			internal Dictionary<Type, int> styleClassMap = new Dictionary<Type, int>();

			internal List<Type> hasTypefaceClasses = new List<Type>();

            /// <summary>
			/// This defaults to R.attr.fontPath. So only override if you want to use your own attrId.
            /// </summary>
            /// <returns>This builder.</returns>
			/// <param name="fontAssetAttrId">the custom attribute to look for fonts in assets.</param>
            public Builder SetFontAttrId(int fontAssetAttrId)
			{
				attrId = fontAssetAttrId;
				return this;
			}

            /// <summary>
			/// Set the default font if you don't define one else where in your styles.
            /// </summary>
            /// <returns>This builder.</returns>
			/// <param name="defaultFontAssetPath">a path to a font file in the assets folder, e.g. "fonts/Roboto-light.ttf",
			/// passing null will default to the device font-family.</param>
            public Builder SetDefaultFontPath(string defaultFontAssetPath)
			{
				isFontSet = !string.IsNullOrEmpty(defaultFontAssetPath);
				fontAssetPath = defaultFontAssetPath;
				return this;
			}

            /// <summary>
			/// Turn of the use of Reflection to inject the private factory.
			/// This has operational consequences! Please read and understand before disabling.
			/// 
			/// This is already disabled on pre Honeycomb devices. (API 11)
			/// 
			/// If you disable this you will need to override your <see cref="Android.App.Activity.OnCreateView(View, string, Android.Content.Context, Android.Util.IAttributeSet)"/>
			/// as this is set as the <see cref="Android.Views.LayoutInflater"/> private factory.
			/// 
			/// Use the following code in the Activity if you disable FactoryInjection:
			/// <code>
			/// public override View OnCreateView(View parent, string name, Context context, AttributeSet attrs) 
			/// {
			///     return CalligraphyContextWrapper.OnActivityCreateView(this, parent, base.OnCreateView(parent, name, context, attrs), name, context, attrs);
			/// }
			/// </code>
            /// </summary>
            /// <returns>This builder.</returns>
            public Builder DisablePrivateFactoryInjection()
			{
				reflection = false;
				return this;
			}

            /// <summary>
			/// Due to the poor inflation order where custom views are created and never returned inside an
			/// <code>OnCreateView(...)</code> method. We have to create CustomView's at the latest point in the
			/// overrideable injection flow.
			/// 
			/// On HoneyComb+ this is inside the <see cref="Android.App.Activity.OnCreateView(View, string, Android.Content.Context, Android.Util.IAttributeSet)"/>
			/// Pre HoneyComb this is in the <see cref="Android.Views.LayoutInflater.Factory.OnCreateView(string, Android.Util.IAttributeSet)"/>
			/// 
			/// We wrap base implementations, so if you LayoutInflater/Factory/Activity creates the
			/// custom view before we get to this point, your view is used. (Such is the case with the
			/// TintEditText etc)
			/// 
			/// The problem is, the native methods pass there parents context to the constructor in a really
			/// specific place. We have to mimic this in <see cref="CalligraphyLayoutInflater.CreateCustomViewInternal(View, View, string, Android.Content.Context, Android.Util.IAttributeSet)"/>
			/// To mimic this we have to use reflection as the Class constructor args are hidden to us.
			/// 
			/// We have discussed other means of doing this but this is the only semi-clean way of doing it.
			/// (Without having to do proxy classes etc).
			/// 
			/// Calling this will of course speed up inflation by turning off reflection, but not by much,
			/// But if you want Calligraphy to inject the correct typeface then you will need to make sure your CustomView's
			/// are created before reaching the LayoutInflater onViewCreated.
            /// </summary>
            /// <returns>This builder.</returns>
            public Builder DisableCustomViewInflation()
			{
				customViewCreation = false;
				return this;
			}

            /// <summary>
			/// Add a custom style to get looked up. If you use a custom class that has a parent style
			/// which is not part of the default android styles you will need to add it here.
			/// 
			/// The Calligraphy inflater is unaware of custom styles in your custom classes. We use
			/// the class type to look up the style attribute in the theme resources.
			/// 
			/// So if you had a <code>typeof(MyTextField)</code> which looked up it's default style as
			/// <code>Resource.Attribute.TextFieldStyle</code> you would add those here.
			/// 
			/// <code>builder.AddCustomStyle(typeof(MyTextField), Resource.Attribute.TextFieldStyle)</code>
            /// </summary>
            /// <returns>This builder.</returns>
			/// <param name="styleType">the class that related to the parent styleResource. null is ignored.  Must inherit <see cref=" TextView"/></param>
			/// <param name="styleResourceAttribute">0 is ignored..</param>
            public Builder AddCustomStyle(Type styleType, int styleResourceAttribute)
			{
				if (styleType == null || !styleType.IsAssignableFrom(typeof(TextView)) || styleResourceAttribute == 0) return this;
				styleClassMap.Add(styleType, styleResourceAttribute);
				return this;
			}

            /// <summary>
            /// Register custom non-<see cref=" TextView"/>'s which implement <see cref="IHasTypeface"/>
			/// so they can have the Typeface applied during inflation.
            /// </summary>
            /// <returns>This builder.</returns>
            /// <param name="type">The type.</param>
            public Builder AddCustomViewWithSetTypeface(Type @type)
			{
				customViewTypefaceSupport = true;
				hasTypefaceClasses.Add(type);
				return this;
			}

            public CalligraphyConfig Build()
			{
				this.isFontSet = !string.IsNullOrEmpty(fontAssetPath);
				return new CalligraphyConfig(this);
			}
		}
	}
}
