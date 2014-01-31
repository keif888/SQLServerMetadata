using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Msagl.Splines;


namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// This class keeps information about edge geometry;
    /// Bezier curve and arrows settings: it is mostly for DOT support
    /// </summary>
    [Serializable]
    public class PosData {
/// <summary>
/// the edge start point
/// </summary>
        public Point StartPoint {
            get { return controlPoints != null ? controlPoints[0] : this.edgeCurve.Start; }
        }

        /// <summary>
        /// the edge end point
        /// </summary>
        public Point Endpoint {
            get { return controlPoints != null ? controlPoints[controlPoints.Count-1] : this.edgeCurve.End; }
        }

        /// <summary>
        /// Control points of consequent Bezier segments.
        /// </summary>
        private List<Point> controlPoints; //array of Points

        ICurve edgeCurve; //one of two is zero
/// <summary>
/// gets or sets the edge curve
/// </summary>
        public ICurve EdgeCurve {
            get { return edgeCurve; }
            set { edgeCurve = value; }
        }

        /// <summary>
        /// enumerates over the edge control points
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
       
        public IEnumerable<Point> ControlPoints {
            get {
                if (this.edgeCurve == null) {
                    if (this.controlPoints == null)
                        controlPoints = new List<Point>();
                    return controlPoints;
                }

                return EnumerateCurvePoints();
            }
        }

        IEnumerable<Point> EnumerateCurvePoints() {

              Curve curve = EdgeCurve as Curve;
              if (curve == null) {
                  //maybe it is a line
                  LineSegment lineSeg = this.EdgeCurve as LineSegment;
                  if (lineSeg == null)
                      throw new System.InvalidOperationException("unexpected curve type");
                  Point a = lineSeg.Start;
                  Point b = lineSeg.End;
                   yield return  a;
                  yield return 2.0 / 3.0 * a + b / 3.0;
                  yield return a / 3.0 + 2.0 * b / 3.0;
                  yield return b;
              } else {

                  yield return (curve.Segments[0] as CubicBezierSegment).B(0);
                  foreach (CubicBezierSegment bez in curve.Segments) {
                      yield return bez.B(1);
                      yield return bez.B(2);
                      yield return bez.B(3);
                  }
              }
             
      }


        /// <summary>
        /// Signals if an arrow should be drawn at the end.
        /// </summary>
        private bool arrowAtTarget;
/// <summary>
/// gets ro sets if an arrow head at target is needed
/// </summary>
        public bool ArrowAtTarget {
            get { return arrowAtTarget; }
            set { arrowAtTarget = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private Point arrowAtTargetPosition;
/// <summary>
/// gets or sets the position of the arrow head at the target node
/// </summary>
        public Point ArrowAtTargetPosition {
            get { return arrowAtTargetPosition; }
            set { arrowAtTargetPosition = value; }
        }
        /// <summary>
        /// Signals if an arrow should be drawn at the beginning.
        /// </summary>
        private bool arrowAtSource;

        /// <summary>
        /// if set to true then we need to draw an arrow head at source
        /// </summary>
        public bool ArrowAtSource {
            get { return arrowAtSource; }
            set { arrowAtSource = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        private Point arrowAtSourcePosition;
        /// <summary>
        /// the position of the arrow head at the source node
        /// </summary>
        public Point ArrowAtSourcePosition {
            get { return arrowAtSourcePosition; }
            set { arrowAtSourcePosition = value; }
        }
    }
}
								
