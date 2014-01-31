using System;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Msagl;
using P2=Microsoft.Msagl.Point;


namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// Edge layout attributes.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Attr"), Description("Edge layout attributes."),
    TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    [Serializable]
    public sealed class EdgeAttr : AttributeBase {
       
        int separation=1;
/// <summary>
/// The separation of the edge in layers. The default is 1.
/// </summary>
        public int Separation {
            get { return separation; }
            set { separation = value; }
        }
  
       
        int weight = 1;

        /// <summary>
        /// Greater weight keeps the edge short
        /// </summary>
        public int Weight {
            get { return weight; }
            set { weight = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        public EdgeAttr() {
            Color = new Color(0, 0, 0);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EdgeAttr Clone() {
            return this.MemberwiseClone() as EdgeAttr;
        }


        ArrowStyle arrowheadAtSource = ArrowStyle.NonSpecified;

        /// <summary>
        /// Arrow style; for the moment only the Normal and None are supported.
        /// </summary>
        public ArrowStyle ArrowheadAtSource {
            get { return arrowheadAtSource; }
            set { arrowheadAtSource = value; }
        }


        /// <summary>
        /// Arrow style; for the moment only a few are supported.
        /// </summary>
        ArrowStyle arrowheadAtTarget = ArrowStyle.NonSpecified;

        /// <summary>
        /// Arrow style; for the moment only the Normal and None are supported.
        /// </summary>
        public ArrowStyle ArrowheadAtTarget {
            get { return arrowheadAtTarget; }
            set { arrowheadAtTarget = value; }
        }

        float arrowheadLength=10;
        /// <summary>
        /// the length of an arrow head of the edge
        /// </summary>
        public float ArrowheadLength {
            get { return arrowheadLength; }
            set { arrowheadLength = value; }
        }
        

        /// <summary>
        /// ToString with a parameter.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
        public string ToString(string text) {
            string ret = "";
            if (!String.IsNullOrEmpty(text)) {
                text = text.Replace("\r\n", "\\n");
                ret += "label=" + Utils.Quote(text);
            }


            if (this.arrowheadAtSource != ArrowStyle.NonSpecified)
                ret = Utils.ConcatWithComma(ret, "arrowhead=" + this.arrowheadAtSource.ToString().ToLower());


            ret = Utils.ConcatWithComma(ret, Utils.ColorToString("color=", Color.ToString()),
                                StylesToString(","),                              
                                IdToString()
                                );


            return ret;

        }

        Microsoft.Msagl.Edge geometryEdge;
/// <summary>
/// gets and sets the geometry edge
/// </summary>
        public Microsoft.Msagl.Edge GeometryEdge {
            get { return geometryEdge; }
            set { geometryEdge = value; }
        }
      
   
   /// <summary>
   /// gets and sets the edge curve
   /// </summary>
        public Splines.ICurve EdgeCurve {
            get {
                if (this.GeometryEdge == null)
                    return null;
                return this.GeometryEdge.Curve;
            }
            set { this.GeometryEdge.Curve = value; }
        }
       
        /// <summary>
        /// Signals if an arrow should be drawn at the end.
        /// </summary>
        public bool ArrowAtTarget {
            get { return ArrowheadAtTarget != Microsoft.Msagl.Drawing.ArrowStyle.None; }
        }
        /// <summary>
        /// 
        /// </summary>
      
        public P2 ArrowAtTargetPosition {
            get {
                if (this.GeometryEdge == null)
                    return new P2();
                return this.GeometryEdge.ArrowheadAtTargetPosition; 
            }
            set { this.GeometryEdge.ArrowheadAtTargetPosition = value; }
        }
    /// <summary>
    /// is true if need to draw an arrow at the source
    /// </summary>
        public bool ArrowAtSource {
            get { return ! (ArrowheadAtSource == Microsoft.Msagl.Drawing.ArrowStyle.NonSpecified || ArrowheadAtSource ==Microsoft.Msagl.Drawing.ArrowStyle.None); }
        }
    /// <summary>
    /// gets or sets the position of the arrow head at the source
    /// </summary>
        public P2 ArrowAtSourcePosition {
            get {
                if (this.GeometryEdge == null)
                    return new P2();
                return this.GeometryEdge.ArrowheadAtSourcePosition;
            }
            set { this.GeometryEdge.ArrowheadAtSourcePosition = value; }
        }

        double length=1;
        /// <summary>
        /// applicable for MDS layouts
        /// </summary>
        public double Length {
            get { return length; }
            set { length = value; }
        }

    }
}
