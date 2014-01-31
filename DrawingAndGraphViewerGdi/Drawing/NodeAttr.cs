using System;
using System.Collections;
using System.ComponentModel;
using P2=Microsoft.Msagl.Point;
using Microsoft.Msagl;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// Attribute of a Node.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Attr"), Description("Node layout attributes."),
    TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    [Serializable]
    public class NodeAttr : AttributeBase {


/// <summary>
/// overrides the Node line width
/// </summary>
        public override int LineWidth {
            get {
                if (this.GeometryNode == null)
                    return base.LineWidth;
                return (int)GeometryNode.LineWidth;
            }
            set {
                bool notify = LineWidth != value && this.LineWidthHasChangedEventIsSet;
                
                if (this.GeometryNode == null)
                    base.LineWidth = value;
                else
                    GeometryNode.LineWidth = value;

                if (notify)
                    RaiseLineWidthHasChangedEvent(this, null);       
            }
        }

        Microsoft.Msagl.Node geometryNode;
        /// <summary>
        /// the underlying GLEE node
        /// </summary>
        public Microsoft.Msagl.Node GeometryNode {
            get { return geometryNode; }
            set { geometryNode = value; }
        }
        
        double padding = Microsoft.Msagl.LayoutAlgorithmSettings.PointSize*2;  

        /// <summary>
        /// Splines should avoid being closer to the node than Padding
        /// </summary>
        public double Padding {
            get { return padding; }
            set { padding = Math.Max(0,value); }
        }

        double xRad;

        /// <summary>
        ///x radius of the rectangle box 
        /// </summary>
        public double XRadius {
            get { return xRad; }
            set { xRad = value; }
        }

        double yRad;
        /// <summary>
        /// y radius of the rectangle box 
        /// </summary>
        public double YRadius {
            get { return yRad; }
            set { yRad = value; }
        }
       

        /// <summary>
        /// Node position.
        /// </summary>
        [Description("Center of node")]
        public P2 Pos {
            get { return GeometryNode.Center; }
        }


        /// <summary>
        /// 
        /// </summary>
        public NodeAttr() {
        }



        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString() {

            return Utils.ConcatWithComma(StylesToString(","), 
                                   Utils.ColorToString("color=", base.Color.ToString()),
                                   Utils.ShapeToString("shape=", this.shape),
                                   Utils.ColorToString("fillcolor=", fillcolor.ToString()),
                                   IdToString()
                                   );

        }

        /// <summary>
        /// Clones the node attribute
        /// </summary>
        /// <returns></returns>
        public NodeAttr Clone() {
            NodeAttr r = this.MemberwiseClone() as NodeAttr;
            return r;
        }

        static Color defaultFillColor = Color.LightGray;
        /// <summary>
        /// the default fill color
        /// </summary>
        static public Color DefaultFillColor {
            get { return defaultFillColor; }
            set { defaultFillColor = value; }
        }

     
        internal Color fillcolor = Color.Transparent;
        ///<summary>
        ///Node fill color.
        ///</summary>
        public Color FillColor {

            get {
                return fillcolor;
            }
            set {
                fillcolor = value;
            }
        }

        

        //void AddFilledStyle(){
        //    if(Array.IndexOf(styles,Style.filled)==-1){
        //        Style []st=new Style[styles.Length+1];
        //        st[0]=Style.filled;
        //        styles.CopyTo(st,1);
        //        styles=st;
        //    }
        //}

        //void RemoveFilledStyle()
        //{

        //  int index;
        //  if ((index = Array.IndexOf(styles, Style.filled)) != -1)
        //  {
        //    Style[] st = new Style[styles.Length - 1];

        //    int count = 0;
        //    for (int j = 0; j < styles.Length; j++)
        //    {
        //      if (j != index)
        //        st[count++] = styles[j];
        //    }
        //    styles = st;
        //  }
        //}

        ///<summary>
        ///Height .
        ///</summary>
        public double Height {
            get { return this.geometryNode.Height; }
        }

        internal Shape shape = Shape.Box;


        /// <summary>
        /// Node shape.
        /// </summary>
        public Shape Shape {
            get { return shape; }
            set { shape = value; }
        }        

        /// <summary>
        /// the node width
        /// </summary>
        public double Width {
            get { return geometryNode.Width; }
        }
  
        int labelMargin = 1;
        /// <summary>
        /// the node label margin
        /// </summary>
        public int LabelMargin {
            get { return labelMargin; }
            set { labelMargin = value; }
        }
    }
}
