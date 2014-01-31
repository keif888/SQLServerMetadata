using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using P2=Microsoft.Msagl.Point;
using Microsoft.Msagl.Drawing;



namespace Microsoft.Msagl.Drawing {

    /// <summary>
    /// Base class for attribute hierarchy.
    /// Some of the attributes are present just for DOT compatibility and not fully supported.  
    /// </summary>
    [Serializable]
    abstract public class BaseAttr {
        /// <summary>
        /// I don't know what to do with this method at the moment
        /// </summary>
        /// <returns></returns>
        public string FontsToString(string separator) { return ""; }

        private static double pointSize = 1;
        /// <summary>
        /// Sets the size of the point. The value of this property can scale all dimensions up or down, the default is 1.
        /// </summary>
        public static double PointSize {
            get { return BaseAttr.pointSize; }
            set { BaseAttr.pointSize = value; }
        }

        static CultureInfo uSCultureInfo = new CultureInfo("en-US");

        /// <summary>
        /// The current culture. Not tested with another culture.
        /// </summary>
        public static CultureInfo USCultureInfo {
            get { return BaseAttr.uSCultureInfo; }
            set { BaseAttr.uSCultureInfo = value; }
        }


        /// <summary>
        /// a default constructor
        /// </summary>
        protected BaseAttr() {
            color = new Color(0, 0, 0);//black
        }


/// <summary>
/// 
/// </summary>
        public event EventHandler StylesChanged;

   /// <summary>
   /// 
   /// </summary>
   /// <param name="style"></param>
        public void AddStyle(Style style) {
            this.styles.Add(style);
            RaiseStyleChangedEvent();
        }

        private void RaiseStyleChangedEvent() {
            if (StylesChanged != null)
                StylesChanged(this, null);

        }
/// <summary>
/// 
/// </summary>
/// <param name="style"></param>
        public void RemoveStyle(Style style) {
            this.styles.Remove(style);
            RaiseStyleChangedEvent();
        }
/// <summary>
/// 
/// </summary>
        public void ClearStyles() {
            this.styles.Clear();
            RaiseStyleChangedEvent();
        }


        Color color;
        /// <summary>
        /// A color of the object.
        /// </summary>
        public Color Color {
            get {
                return color;
            }
            set {
                color = value;
            }
        }

        internal List<Style> styles = new List<Style>();

        /// <summary>
        /// An array of attribute styles.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public IEnumerable<Style> Styles {
            get { return styles; }
        }
        /// <summary>
        /// The width of a node border or an edge.
        /// </summary>
        internal int lineWidth = 1;

        /// <summary>
        /// An id of the entity.
        /// </summary>
        string id;
        /// <summary>
        /// the ID of the entity
        /// </summary>
        public string Id {
            get { return id; }
            set { id = value; }
        }

        ///<summary>
        ///Influences border width of clusters, border width of nodes
        /// and edge thickness.
        ///</summary>
        virtual public int LineWidth {
            get { return lineWidth; }
            set {
                bool notify = lineWidth != value && LineWidthHasChanged != null;
                lineWidth = value;
                if (notify)
                    LineWidthHasChanged(this, null);
            }
        }

        internal bool LineWidthHasChangedEventIsSet {
            get { return LineWidthHasChanged != null; }
        }

        internal void RaiseLineWidthHasChangedEvent(object sender, EventArgs args) {
            if (LineWidthHasChanged != null)
                LineWidthHasChanged(sender, args);
        }

        internal void SubscribeToLineWidthHasChangedEvent(EventHandler eventHandler) {
            this.LineWidthHasChanged += eventHandler;
        }

        internal void UnsubscribeToLineWidthHasChangedEvent(EventHandler eventHandler) {
            this.LineWidthHasChanged -= eventHandler;
        }

        /// <summary>
        /// an event notifying that the line width has changed
        /// </summary>
        public event EventHandler LineWidthHasChanged;

        internal string IdToString() {
            if (String.IsNullOrEmpty(Id))
                return "";

            return "id=" + Utils.Quote(this.Id);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString")]
        internal string StylesToString(string delimeter) {
            ArrayList al = new ArrayList();

            if (lineWidth != -1)
                al.Add("style=\"setlinewidth(" + lineWidth.ToString() + ")\"");




            if (styles != null) {
                foreach (Style style in styles)
                    al.Add("style=" + Utils.Quote(style.ToString()));
            }

            string[] s = al.ToArray(Type.GetType("System.String")) as String[];

            string ret = Utils.ConcatWithDelimeter(delimeter, s);

            return ret;

        }


    }
        
}
