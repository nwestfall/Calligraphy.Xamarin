using Android.Util;

using Java.Lang;
using Java.Lang.Reflect;

namespace Calligraphy.Xamarin
{
    static class ReflectionUtils
    {
		const string TAG = "Calligraphy.Xamarin.ReflectionUtils";

		internal static Field GetField(Class @class, string fieldName)
		{
			try
			{
				Field field = @class.GetDeclaredField(fieldName);
				field.Accessible = true;
				return field;
			}
			catch(NoSuchFieldException){ }
			return null;
		}

        internal static Object GetValue(Field field, Object @object)
		{
			try
			{
				return field?.Get(@object);
			}
			catch(IllegalAccessException) { }
			return null;
		}

        internal static void SetValue(Field field, Object @object, Object value)
		{
			try
			{
				field?.Set(@object, value);
			}
			catch(IllegalAccessException) { }
		}

        internal static Method GetMethod(Class @class, string methodName)
		{
			Method[] methods = @class.GetMethods();
            foreach(var method in methods)
			{
				if(method.Name.Equals(methodName, System.StringComparison.InvariantCultureIgnoreCase))
				{
					method.Accessible = true;
					return method;
				}
			}
			return null;
		}
        
		internal static void InvokeMethod(Object @object, Method method, params Object[] args)
		{
			try
			{
				method?.Invoke(@object, args);
			}
            catch(Exception ex)
			{
				if (ex is IllegalAccessException || ex is InvocationTargetException)
					Log.Debug(TAG, Throwable.FromException(ex), "Can't invoke method using reflection");
				else
					throw;
			}
		}
    }
}