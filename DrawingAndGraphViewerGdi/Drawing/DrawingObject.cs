using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// Base class for graph objects  
    /// </summary>
    [Serializable]
    public abstract class DrawingObject {
        object userData;
        /// <summary>
        /// This field can be used as a backpointer to the user data associated with the object
        /// </summary>
        public object UserData {
            get { return userData; }
            set { userData = value; }
        }


        /// <summary>
        /// gets the bounding box of the object
        /// </summary>
        abstract public Microsoft.Msagl.Splines.Rectangle BoundingBox { get;}
        /// <summary>
        /// gets the geometry object corresponding to the drawing object
        /// </summary>
        public abstract GeometryObject GeometryObject { get;}
    }
}
