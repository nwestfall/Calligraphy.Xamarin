using System;
using System.Xml;

using Android.Annotation;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Java.Lang.Reflect;

namespace Calligraphy.Xamarin
{
	class CalligraphyLayoutInflater : LayoutInflater, ICalligraphyActivityFactory
    {
		static readonly string[] classPrefixList = {
			"android.widget",
			"android.webkit"
		};

		readonly int attributeId;
		readonly CalligraphyFactory calligraphyFactory;
		//Reflection Hax
		bool setPrivateFactory = false;
		Field constructorArgs = null;

		internal CalligraphyLayoutInflater(Context context, int attributeId) : base(context)
		{
			this.attributeId = attributeId;
			calligraphyFactory = new CalligraphyFactory(attributeId);
			SetUpLayoutFactories(false);
		}

		internal CalligraphyLayoutInflater(LayoutInflater original, Context newContext, int attributeId, bool cloned) : base(original, newContext)
		{
			this.attributeId = attributeId;
			calligraphyFactory = new CalligraphyFactory(attributeId);
			SetUpLayoutFactories(cloned);
		}

		public override LayoutInflater CloneInContext(Context newContext) => new CalligraphyLayoutInflater(this, newContext, attributeId, true);

		public override View Inflate(XmlReader parser, ViewGroup root, bool attachToRoot)
		{
			SetPrivateFactoryInternal();
			return base.Inflate(parser, root, attachToRoot);
		}

        /// <summary>
		/// We don't want to unnecessary create/set our factories if there are none there. We try to be
		/// as lazy as possible.
        /// </summary>
        /// <param name="cloned">If set to <c>true</c> cloned.</param>
        void SetUpLayoutFactories(bool cloned)
		{
			if (cloned) return;
			// If we are HC+ we get and set Factory2 otherwise we just wrap Factory1
            if(Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
			{
                if (Factory2 != null && !(Factory2 is WrapperFactory2))
                    SetFactory2(Factory2); // Sets both Factory/Factory2
			}
            // We can do this as setFactory2 is used for both methods.
            if (Factory != null && !(Factory is WrapperFactory))
                SetFactory(Factory);
		}

        void SetFactory(IFactory factory)
		{
			// Only set our factory and wrap calls to the Factory trying to be set!
			if (!(factory is WrapperFactory))
				Factory = new WrapperFactory(factory, this, calligraphyFactory);
			else
				Factory = factory;
		}

        [TargetApi(Value = (int)BuildVersionCodes.Honeycomb)]
        void SetFactory2(IFactory2 factory2)
		{
			// Only set our factory and wrap calls to the Factory2 trying to be set!
			if (!(factory2 is WrapperFactory2))
				Factory2 = new WrapperFactory2(factory2, calligraphyFactory);
			else
				Factory2 = factory2;
		}

        void SetPrivateFactoryInternal()
		{
			// Already tried to set the factory.
			if (setPrivateFactory) return;
			// Reflection (Or Old Device) skip.
			if (!CalligraphyConfig.Get().Reflection) return;
			// Skip if not attached to an activity.
            if(!(Context is IFactory2))
			{
				setPrivateFactory = true;
				return;
			}
            
			Method setPrivateFactoryMethod = ReflectionUtils.GetMethod(Java.Lang.Class.FromType(typeof(LayoutInflater)), "setPrivateFactory");
			if (setPrivateFactoryMethod != null)
				ReflectionUtils.InvokeMethod(this, setPrivateFactoryMethod, new PrivateWrapperFactory2((IFactory2)Context, this, calligraphyFactory));

			setPrivateFactory = true;
		}

		/// <summary>
		/// The Activity onCreateView (PrivateFactory) is the third port of call for LayoutInflation.
		/// We opted to manual injection over aggressive reflection, this should be less fragile.
		/// </summary>
		/// <returns>The activity create view.</returns>
		/// <param name="parent">Parent.</param>
		/// <param name="view">View.</param>
		/// <param name="name">Name.</param>
		/// <param name="context">Context.</param>
		/// <param name="attrs">Attrs.</param>
		[TargetApi(Value = (int)BuildVersionCodes.Honeycomb)]
		public View OnActivityCreateView(View parent, View view, string name, Context context, IAttributeSet attrs) => calligraphyFactory.OnViewCreated(CreateCustomViewInternal(parent, view, name, context, attrs), context, attrs);

        /// <summary>
		/// The LayoutInflater onCreateView is the fourth port of call for LayoutInflation.
		/// BUT only for none CustomViews.
        /// </summary>
        /// <returns>The create view.</returns>
        /// <param name="parent">Parent.</param>
        /// <param name="name">Name.</param>
        /// <param name="attrs">Attrs.</param>
		[TargetApi(Value = (int)BuildVersionCodes.Honeycomb)]
		protected override View OnCreateView(View parent, string name, IAttributeSet attrs) => calligraphyFactory.OnViewCreated(base.OnCreateView(parent, name, attrs), Context, attrs);

        /// <summary>
		/// The LayoutInflater onCreateView is the fourth port of call for LayoutInflation.
		/// BUT only for none CustomViews.
		/// Basically if this method doesn't inflate the View nothing probably will.
        /// </summary>
        /// <returns>The create view.</returns>
        /// <param name="name">Name.</param>
        /// <param name="attrs">Attrs.</param>
		protected override View OnCreateView(string name, IAttributeSet attrs)
		{
			// This mimics the <code>PhoneLayoutInflater</code> in the way it tries to inflate the base
			// classes, if this fails its pretty certain the app will fail at this point.
			View view = null;
            foreach(var prefix in classPrefixList)
			{
				try
				{
                    //It's weird, but it has to be done this way
                    //If prefix goes in the prefix spot, it errors out...
                    view = CreateView($"{prefix}.{name}", null, attrs);
				}
                catch(Java.Lang.ClassNotFoundException)
				{
					// Ignore
				}
			}
            // In this case we want to let the base class take a crack
            // at it.
            if (view == null) view = base.OnCreateView(name, attrs);

			return calligraphyFactory.OnViewCreated(view, view.Context, attrs);
		}

		/// <summary>
		/// Nasty method to inflate custom layouts that haven't been handled else where. If this fails it
		/// will fall back through to the PhoneLayoutInflater method of inflating custom views where
		/// Calligraphy will NOT have a hook into.
		/// </summary>
		/// <returns>view or the View we inflate in here.</returns>
		/// <param name="parent">Parent view.</param>
		/// <param name="view">view if it has been inflated by this point, if this is not null this method just returns this value.</param>
		/// <param name="name">name of the thing to inflate.</param>
		/// <param name="viewContext">Context to inflate by if parent is null</param>
		/// <param name="attrs">Attr for this view which we can steal fontPath from too.</param>
		internal View CreateCustomViewInternal(View parent, View view, string name, Context viewContext, IAttributeSet attrs)
		{
			// I by no means advise anyone to do this normally, but Google have locked down access to
			// the createView() method, so we never get a callback with attributes at the end of the
			// createViewFromTag chain (which would solve all this unnecessary rubbish).
			// We at the very least try to optimise this as much as possible.
			// We only call for customViews (As they are the ones that never go through onCreateView(...)).
			// We also maintain the Field reference and make it accessible which will make a pretty
			// significant difference to performance on Android 4.0+.

			// If CustomViewCreation is off skip this.
			if (!CalligraphyConfig.Get().CustomViewCreation) return view;
            if(view == null && name.IndexOf('.') > -1)
			{
				Java.Lang.Object[] constructorArgsArr = null;
				Java.Lang.Object lastContext = null;

				if (Build.VERSION.SdkInt <= BuildVersionCodes.P)
				{
					if (constructorArgs == null)
					{
                        Java.Lang.Class layoutInflaterClass = Java.Lang.Class.FromType(typeof(LayoutInflater));
						constructorArgs = layoutInflaterClass.GetDeclaredField("mConstructorArgs");
						constructorArgs.Accessible = true;
					}

					constructorArgsArr = (Java.Lang.Object[])constructorArgs.Get(this);
					lastContext = constructorArgsArr[0];

					// The LayoutInflater actually finds out the correct context to use. We just need to set
					// it on the mConstructor for the internal method.
					// Set the constructor args up for the createView, not sure why we can't pass these in.
					constructorArgsArr[0] = viewContext;
					constructorArgs.Set(this, constructorArgsArr);
				}
				try
				{
#if __ANDROID_29__
                        if (Build.VERSION.SdkInt > BuildVersionCodes.P)
                            view = CreateView(viewContext, name, null, attrs);
                        else
#endif
					view = CreateView(name, null, attrs);
				}
				catch (Java.Lang.ClassNotFoundException)
				{
				}
				finally
				{
					if (Build.VERSION.SdkInt <= BuildVersionCodes.P)
					{
						constructorArgsArr[0] = lastContext;
						constructorArgs.Set(this, constructorArgsArr);
					}
				}
			}
			return view;
		}

        /// <summary>
		/// Factory 1 is the first port of call for LayoutInflation
        /// </summary>
		class WrapperFactory : Java.Lang.Object, IFactory
		{
			readonly IFactory factory;
			readonly CalligraphyLayoutInflater inflater;
			readonly CalligraphyFactory calligraphyFactory;

            public WrapperFactory(IFactory factory, CalligraphyLayoutInflater inflater, CalligraphyFactory calligraphyFactory)
			{
				this.factory = factory;
				this.inflater = inflater;
				this.calligraphyFactory = calligraphyFactory;
			}

            public View OnCreateView(string name, Context context, IAttributeSet attrs)
			{
				if(Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
				{
					return calligraphyFactory.OnViewCreated(
							inflater.CreateCustomViewInternal(null,
															  factory.OnCreateView(name, context, attrs),
															 name,
															 context,
															 attrs),
							context,
						    attrs);
				}

				return calligraphyFactory.OnViewCreated(factory.OnCreateView(name, context, attrs), context, attrs);
			}
		}

        /// <summary>
		/// Factory 2 is the second port of call for LayoutInflation
        /// </summary>
        [TargetApi(Value = (int)BuildVersionCodes.Honeycomb)]
		class WrapperFactory2 : Java.Lang.Object, IFactory2
		{
			protected readonly IFactory2 factory2;
			protected readonly CalligraphyFactory calligraphyFactory;

            public WrapperFactory2(IFactory2 factory2, CalligraphyFactory calligraphyFactory)
			{
				this.factory2 = factory2;
				this.calligraphyFactory = calligraphyFactory;
			}

			public View OnCreateView(string name, Context context, IAttributeSet attrs) => calligraphyFactory.OnViewCreated(factory2.OnCreateView(name, context, attrs), context, attrs);

			public virtual View OnCreateView(View parent, string name, Context context, IAttributeSet attrs) => calligraphyFactory.OnViewCreated(factory2.OnCreateView(parent, name, context, attrs), context, attrs);
		}

        /// <summary>
		/// Private factory is step three for Activity Inflation, this is what is attached to the
		/// Activity on HC+ devices.
        /// </summary>
		[TargetApi(Value = (int)BuildVersionCodes.Honeycomb)]
		class PrivateWrapperFactory2 : WrapperFactory2
		{
			readonly CalligraphyLayoutInflater inflater;

			public PrivateWrapperFactory2(IFactory2 factory2, CalligraphyLayoutInflater inflater, CalligraphyFactory calligraphyFactory)
				: base(factory2, calligraphyFactory) => this.inflater = inflater;

			public override View OnCreateView(View parent, string name, Context context, IAttributeSet attrs) =>
				calligraphyFactory.OnViewCreated(
						inflater.CreateCustomViewInternal(parent,
														  factory2.OnCreateView(parent, name, context, attrs),
														  name,
														  context,
														  attrs),
        				context,
        				attrs);
		}
	}
}
