using System;

using Android.Graphics;
using Android.Text;
using Android.Text.Style;

namespace Calligraphy.Xamarin
{
	public class CalligraphyTypefaceSpan : MetricAffectingSpan
    {
		readonly Typeface typeface;

		public CalligraphyTypefaceSpan(Typeface typeface) => this.typeface = typeface ?? throw new ArgumentException(nameof(typeface));

		public override void UpdateDrawState(TextPaint tp) => Apply(tp);

		public override void UpdateMeasureState(TextPaint p) => Apply(p);

        void Apply(Paint paint)
		{
			Typeface oldTypeface = paint.Typeface;
			TypefaceStyle oldStyle = oldTypeface?.Style ?? TypefaceStyle.Normal;
			TypefaceStyle fakeStyle = oldStyle & ~typeface.Style;

			if ((fakeStyle & TypefaceStyle.Bold) != 0)
				paint.FakeBoldText = true;

			if ((fakeStyle & TypefaceStyle.Italic) != 0)
				paint.TextSkewX = -0.25f;

			paint.SetTypeface(typeface);
		}
	}
}
