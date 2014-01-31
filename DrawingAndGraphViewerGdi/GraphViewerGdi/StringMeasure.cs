using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Msagl.Drawing;
using BBox = Microsoft.Msagl.Splines.Rectangle;

namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// 
    /// </summary>
    sealed public class StringMeasure {
        StringMeasure() { }

        static Graphics graphics;
        static Font defaultFont;
/// <summary>
/// 
/// </summary>
/// <param name="text"></param>
/// <param name="font"></param>
/// <param name="width"></param>
/// <param name="height"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        static public void MeasureWithFont(string text, Font font, out double width, out double height) {
            if (String.IsNullOrEmpty(text)) {
                width = 0;
                height = 0;
                return;
            }

            if (graphics == null)
                graphics = (new Form()).CreateGraphics();

            Measure(text, font, graphics, out width, out height);
        }


        static internal void Measure(string text,
          Font font, object graphics_, out double width, out double height) {
            Graphics graphics = graphics_ as Graphics;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            if (font == null)
                if (defaultFont == null)
                    font = defaultFont = new Font(Microsoft.Msagl.Drawing.Label.DefaultFontName, (float)(Microsoft.Msagl.Drawing.Label.DefaultFontSize * LayoutAlgorithmSettings.PointSize));
                else
                    font = defaultFont;

            using (StringFormat sf = StringFormat.GenericTypographic) {
                SizeF s = graphics.MeasureString(text, font, 1000000, sf);
                width = s.Width;
                height = s.Height;
            }

            //System.Drawing.StringFormat format = new System.Drawing.StringFormat();
            //System.Drawing.RectangleF rect = new System.Drawing.RectangleF(0, 0,
            //  1000, 1000);
            //System.Drawing.CharacterRange[] ranges =  { new System.Drawing.CharacterRange(0, text.Length) };
            //System.Drawing.Region[] regions = new System.Drawing.Region[1];

            //format.SetMeasurableCharacterRanges(ranges);

            //if (font == null)
            //{
            //  throw new Exception("font cannot be null");
            //}

            //if (font == null)
            //  throw new InvalidOperationException();//"font cannot be null");

            //regions = graphics.MeasureCharacterRanges(text, font, rect, format);
            //rect = regions[0].GetBounds(graphics);

            //Console.WriteLine("{0} {1}",rect.Width,width);

            //width = rect.Width;
            //height = rect.Height;
        }
    }
}
