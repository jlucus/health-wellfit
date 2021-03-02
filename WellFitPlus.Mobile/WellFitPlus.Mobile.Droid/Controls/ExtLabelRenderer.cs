using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Widget;
using Java.Lang;
using System;
using WellFitPlus.Mobile.Controls;
using WellFitPlus.Mobile.Droid.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(ExtLabel), typeof(ExtLabelRenderer))]
namespace WellFitPlus.Mobile.Droid.Controls
{
    public class ExtLabelRenderer : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            Control?.SetPadding(0, Control.PaddingTop, 0, Control.PaddingBottom);

            var label = (TextView)Control; // for example
            if (!string.IsNullOrEmpty(e.NewElement?.FontFamily))
            {
                try
                {
                    //Typeface font = Typeface.CreateFromAsset(Forms.Context.Assets, "OpenSans-Regular.ttf");  // font name specified here
                    Typeface font = Typeface.CreateFromAsset(Forms.Context.Assets, e.NewElement.FontFamily + ".ttf");
                    label.Typeface = font;
                }
                catch (System.Exception ex)
                {
                    Typeface font = Typeface.CreateFromAsset(Forms.Context.Assets, "Raleway-Regular.ttf");  // font name specified here
                    label.Typeface = font;
                }
            }
            else
            {
                Typeface font = Typeface.CreateFromAsset(Forms.Context.Assets, "Raleway-Regular.ttf");  // font name specified here
                label.Typeface = font;
            }

            //AutoResizeTextView tv = new AutoResizeTextView(Forms.Context);            
            //tv.TextSize = label.TextSize;
            //tv.Text = label.Text;
            //label.TextSize = tv.TextSize;
        }        
    }





//public class AutoResizeTextView : TextView
//    {

//        private interface SizeTester
//        {
//            int onTestSize(int suggestedSize, RectF availableSpace);
//        }
//        private class ST : SizeTester
//        {
//            public int onTestSize(int suggestedSize, RectF availableSpace)
//            {
//                return 10;
//            }
//        }

//        private RectF mTextRect = new RectF();

//        private RectF mAvailableSpaceRect;

//        private SparseIntArray mTextCachedSizes;

//        private TextPaint mPaint;

//        private float mMaxTextSize;

//        private float mSpacingMult = 1;

//        private float mSpacingAdd = 0;

//        private float mMinTextSize = 20;

//        private int mWidthLimit;

//        private static int NO_LINE_LIMIT = -1;

//        private int mMaxLines;

//        private bool mEnableSizeCache = true;

//        private bool mInitializedDimens;

//        public AutoResizeTextView(Context context) :
//                base(context)
//        {
//            this.initialize();
//        }

//        public AutoResizeTextView(Context context, IAttributeSet attrs) :
//                base(context, attrs)
//        {
//            //base(context, attrs);
//            this.initialize();
//        }

//        public AutoResizeTextView(Context context, IAttributeSet attrs, int defStyle) :
//                base(context, attrs, defStyle)
//        {
//            //base.(context, attrs, defStyle);
//            this.initialize();
//        }

//        private void initialize()
//        {
//            this.mPaint = new TextPaint(Paint);
//            this.mMaxTextSize = TextSize;
//            this.mAvailableSpaceRect = new RectF();
//            this.mTextCachedSizes = new SparseIntArray();
//            if ((this.mMaxLines == 0))
//            {
//                //  no value was assigned during construction
//                this.mMaxLines = NO_LINE_LIMIT;
//            }

//        }
        
//        public void SetTextSize(float size)
//        {
//            this.mMaxTextSize = size;
//            this.mTextCachedSizes.Clear();
//            this.adjustTextSize();
//        }

//        public override void SetMaxLines(int maxlines)
//        {
//            base.SetMaxLines(maxlines);
//            this.mMaxLines = maxlines;
//            this.adjustTextSize();
//        }

//        public int getMaxLines()
//        {
//            return this.mMaxLines;
//        }

//        public override void SetSingleLine()
//        {
//            base.SetSingleLine();
//            this.mMaxLines = 1;
//            this.adjustTextSize();
//        }

//        public override void SetSingleLine(bool singleLine)
//        {
//            base.SetSingleLine(singleLine);
//            if (singleLine)
//            {
//                this.mMaxLines = 1;
//            }
//            else
//            {
//                this.mMaxLines = NO_LINE_LIMIT;
//            }

//            this.adjustTextSize();
//        }


//        public override void SetLines(int lines)
//        {
//            base.SetLines(lines);
//            this.mMaxLines = lines;
//            this.adjustTextSize();
//        }


//        public void SetTextSize(int unit, float size)
//        {
//            Context c = Forms.Context;
//            Resources r;
//            if ((c == null))
//            {
//                r = Resources.System;
//            }
//            else
//            {
//                r = c.Resources;
//            }

//            this.mMaxTextSize = TypedValue.ApplyDimension( ComplexUnitType.Pt, size, r.DisplayMetrics);
//            this.mTextCachedSizes.Clear();
//            this.adjustTextSize();
//        }


//        public override void SetLineSpacing(float add, float mult)
//        {
//            //base.GetLineSpacing(add, mult);
//            this.mSpacingMult = mult;
//            this.mSpacingAdd = add;
//        }

//        public void setMinTextSize(float minTextSize)
//        {
//            this.mMinTextSize = minTextSize;
//            this.adjustTextSize();
//        }

//        private void adjustTextSize()
//        {
//            if (!this.mInitializedDimens)
//            {
//                return;
//            }

//            int startSize = ((int)(this.mMinTextSize));
//            int heightLimit = (MeasuredHeight
//                        - (PaddingBottom - PaddingTop));
//            this.mWidthLimit = (MeasuredWidth
//                        - (PaddingLeft - PaddingRight));
//            this.mAvailableSpaceRect.Right = this.mWidthLimit;
//            this.mAvailableSpaceRect.Bottom = heightLimit;
//            base.SetTextSize(TypedValue.DensityDefault, this.efficientTextSizeSearch(startSize, ((int)(this.mMaxTextSize)), mSizeTester, this.mAvailableSpaceRect));
//        }

//        private SizeTester mSizeTester = new ST();

//        public void enableSizeCache(bool enable)
//        {
//            this.mEnableSizeCache = enable;
//            this.mTextCachedSizes.Clear();
//            //this.adjustTextSize(Text.ToString());
//        }

//        private int efficientTextSizeSearch(int start, int end, SizeTester sizeTester, RectF availableSpace)
//        {
//            if (!this.mEnableSizeCache)
//            {
//                return AutoResizeTextView.binarySearch(start, end, sizeTester, availableSpace);
//            }

//            int key = Text.ToString().Length;
//            int size = this.mTextCachedSizes.IndexOfKey(key);
//            if ((size != 0))
//            {
//                return size;
//            }

//            size = AutoResizeTextView.binarySearch(start, end, sizeTester, availableSpace);
//            this.mTextCachedSizes.Put(key, size);
//            return size;
//        }

//        private static int binarySearch(int start, int end, SizeTester sizeTester, RectF availableSpace)
//        {
//            int lastBest = start;
//            int lo = start;
//            int hi = (end - 1);
//            int mid = 0;
//            while ((lo <= hi))
//            {
//                int midValCmp = sizeTester.onTestSize(mid, availableSpace);
//                if ((midValCmp < 0))
//                {
//                    lastBest = lo;
//                    lo = (mid + 1);
//                }
//                else if ((midValCmp > 0))
//                {
//                    hi = (mid - 1);
//                    lastBest = hi;
//                }
//                else
//                {
//                    return mid;
//                }

//            }

//            //  make sure to return last best
//            //  this is what should always be returned
//            return lastBest;
//        }


//        protected void OnTextChanged(ICharSequence text, int start, int before, int after)
//        {
//            base.OnTextChanged(text, start, before, after);
//            this.adjustTextSize();
//        }


//        protected override void OnSizeChanged(int width, int height, int oldwidth, int oldheight)
//        {
//            this.mInitializedDimens = true;
//            this.mTextCachedSizes.Clear();
//            base.OnSizeChanged(width, height, oldwidth, oldheight);
//            if (((width != oldwidth)
//                        || (height != oldheight)))
//            {
//                this.adjustTextSize();
//            }

//        }
//    }
}
