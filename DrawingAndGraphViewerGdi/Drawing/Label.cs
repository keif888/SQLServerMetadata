using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using P2=Microsoft.Msagl.Point;
using Microsoft.Msagl.Drawing;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// Keep the information related to an object label
    /// </summary>
    [Serializable]
    public class Label: DrawingObject {
        
        /// <summary>
        /// an empty constructor
        /// </summary>
        public Label(){}

        /// <summary>
        /// a constructor with text
        /// </summary>
        /// <param name="textPar"></param>
        public Label(string textPar) { this.text = textPar; }

        /// <summary>
        /// the width of the label
        /// </summary>
        public double Width {
            get { return GeometryLabel.Width; }
            set { GeometryLabel.Width = value; }
        }

        /// <summary>
        /// the height of the label
        /// </summary>
        public double Height {
            get { return GeometryLabel.Height; }
            set { GeometryLabel.Height = value; }
        }

        /// <summary>
        /// left coordinate 
        /// </summary>
        public double Left { get { return Center.X - Width / 2; } }
        /// <summary>
        /// top coordinate
        /// </summary>
        public double Top { get { return Center.Y + Height / 2; } }

        /// <summary>
        /// left coordinate 
        /// </summary>
        public double Right { get { return Center.X + Width / 2; } }
        /// <summary>
        /// top coordinate
        /// </summary>
        public double Bottom { get { return Center.Y - Height / 2; } }

        /// <summary>
        /// gets the left top corner
        /// </summary>
        public P2 LeftTop { get{ return new P2(Left,Top);}}


        /// <summary>
        /// gets the right bottom corner
        /// </summary>
        public P2 RightBottom { get { return new P2(Right, Bottom); } }

        /// <summary>
        /// returns the bounding box of the label
        /// </summary>
        override public Microsoft.Msagl.Splines.Rectangle BoundingBox { 
            get { return new Microsoft.Msagl.Splines.Rectangle(LeftTop, RightBottom); }
        }

        /// <summary>
        /// gets or sets the label size
        /// </summary>
        virtual public Size Size {
            get { return new Size(GeometryLabel.Width, GeometryLabel.Height); }
            set {
                GeometryLabel.Width = value.Width;
                GeometryLabel.Height = value.Height;
            }
        }

        /// <summary>
        /// Center of the label
        /// </summary>
        public P2 Center { get { return GeometryLabel.Center; } set { GeometryLabel.Center = value; } }

        internal Color fontcolor = Color.Black;

        ///<summary>
        ///Label font color.
        ///</summary>
        [Description("type face color")]
        public Color FontColor {
            get { return fontcolor; }
            set {
                fontcolor = value;
            }
        }

        ///<summary>
        ///Type face font.
        ///</summary>
        string fontName = "";

        ///<summary>
        ///Type face font
        ///</summary>
        [Description("type face font"),
        DefaultValue("")]
        public string FontName {
            get {
                if (String.IsNullOrEmpty(fontName))
                    return DefaultFontName;
                else
                    return fontName;
            }
            set {
                fontName = value;
            }

        }



        string text;
        /// <summary>
        /// A label of the entity. The label is rendered opposite to the ID. 
        /// </summary>
        public string Text {
            get { return text; }
            set {
                if (value != null)
                    text = value.Replace("\\n", "\n");
                else
                    text = "";
            }
        }

       


        internal int fontsize = DefaultFontSize;

        ///<summary>
        ///The point size of the id.
        ///</summary>
        public int FontSize {
            get { return fontsize; }
            set { fontsize = value; }
        }


        internal static string defaultFontName = "Times-Roman";
        /// <summary>
        /// the name of the defaul font
        /// </summary>
        public static string DefaultFontName {
            get { return defaultFontName; }
            set { defaultFontName = value; }
        }

        static int defaultFontSize = 12;
        /// <summary>
        /// the default font size
        /// </summary>
        static public int DefaultFontSize {
            get { return defaultFontSize; }
            set { defaultFontSize = value; }
        }



        Microsoft.Msagl.Label geometryLabel=new Microsoft.Msagl.Label();

        /// <summary>
        /// gets or set geometry label
        /// </summary>
        public Microsoft.Msagl.Label GeometryLabel {
            get { return geometryLabel; }
            internal set { geometryLabel = value; }
        }
        /// <summary>
        /// gets the geometry of the label
        /// </summary>
        public override GeometryObject GeometryObject {
            get { return GeometryLabel; }
        }
    }
}

