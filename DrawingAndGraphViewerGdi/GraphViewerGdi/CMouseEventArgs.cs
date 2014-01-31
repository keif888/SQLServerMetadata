using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.Msagl.GraphViewerGdi {
    /// <summary>
    /// Implements the IMouseEventArgs
    /// </summary>
    public class ViewerMouseEventArgs : Microsoft.Msagl.Drawing.MsaglMouseEventArgs {

        MouseEventArgs args;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="argsP"></param>
        public ViewerMouseEventArgs(MouseEventArgs argsP) { args = argsP; }


        /// <summary>
        /// true is the left mouse button is pressed
        /// </summary>
        override public bool LeftButtonIsPressed {
            get { return (args.Button & MouseButtons.Left) == MouseButtons.Left; }
        }
        /// <summary>
        /// true is the middle mouse button is pressed
        /// </summary>
        override public bool MiddleButtonIsPressed {
            get { return (args.Button & MouseButtons.Middle) == MouseButtons.Middle; }
        }
        /// <summary>
        /// true is the right button is pressed
        /// </summary>
        override public bool RightButtonIsPressed {
            get { return (args.Button & MouseButtons.Right) == MouseButtons.Right; }
        }

        bool handled;
        /// <summary>
        /// the controls should ignore the event if handled is set to true
        /// </summary>
        override public bool Handled {
            get { return handled; }
            set { handled = value; }
        }

        /// <summary>
        /// return x position
        /// </summary>
        override public int X {
            get { return args.X; }
        }
        /// <summary>
        /// return y position
        /// </summary>
        override public int Y {
            get { return args.Y; }
        }
        /// <summary>
        /// returns null always
        /// </summary>
        public override object Source {
            get { return null; }
        }
        /// <summary>
        /// gets the number of clicks of the button
        /// </summary>
        public override int Clicks {
            get { return this.args.Clicks; }
        }
    }
}
