using Microsoft.Msagl.Splines;
using System.Collections.Generic;
using GeomNode = Microsoft.Msagl.Node;
using GeomEdge = Microsoft.Msagl.Edge;
using GeomLabel = Microsoft.Msagl.Label;
namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// Undoes/redoes the node dragging operation. Works for multiple nodes.
    /// </summary>
    public class ObjectDragUndoRedoAction : UndoRedoDragAction {
        Set<GeometryObject> movedObjects;

        bool boundingBoxChanges;
        /// <summary>
        /// returns true if the bounding box changes
        /// </summary>
        public bool BoundingBoxChanges {
            get { return boundingBoxChanges; }
            set { boundingBoxChanges = value; }
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="movedNodesParameter"></param>
        public ObjectDragUndoRedoAction(Set<GeometryObject> movedNodesParameter)
            : base(GetGeomGraph(movedNodesParameter)) {
            movedObjects = movedNodesParameter.Clone();
        }

        private static GeometryGraph GetGeomGraph(Set<GeometryObject> movedObjsParam) {
            foreach (GeometryObject obj in movedObjsParam)
                return GetParentGraph(obj);

            throw new System.InvalidOperationException();
        }


        /// <summary>
        /// Undoes the editing
        /// </summary>
        public override void Undo() {
            base.Undo();
            Point point = new Point();
            foreach (GeometryObject geomObj in movedObjects) {
                GeomNode node = geomObj as GeomNode;
                if (node != null) {
                    GeometryGraphEditor.DragNode(node, point, GetRestoreData(node) as NodeRestoreData);
                    foreach (GeomEdge edge in node.Edges) {
                        RestoreEdge(edge);
                    }
                } else {
                    GeomEdge edge = geomObj as GeomEdge;
                    if (edge != null) {
                        GeometryGraphEditor.DragEdge(point, edge, GetRestoreData(edge) as EdgeRestoreData, movedObjects);
                    } else {
                        GeomLabel label = geomObj as GeomLabel;
                        if (label != null)
                           GeometryGraphEditor.DragLabel(label, point, (GetRestoreData(label) as LabelRestoreData).Center);
                        else
                            throw new System.NotImplementedException();
                    }
                }
            }
        }

        private void RestoreEdge(GeomEdge edge) {
            EdgeRestoreData edgeRestoreData = GetRestoreData(edge) as EdgeRestoreData;
            Site liveSite = edge.UnderlyingPolyline.HeadSite;
            Site restoreSite = edgeRestoreData.Polyline.HeadSite;
            while (liveSite != null) {
                liveSite.Point = restoreSite.Point;
                liveSite = liveSite.Next;
                restoreSite = restoreSite.Next;
            }
            edge.Curve = edgeRestoreData.Curve.Clone();
            edge.ArrowheadAtSourcePosition = edgeRestoreData.ArrowheadAtSourcePosition;
            edge.ArrowheadAtTargetPosition = edgeRestoreData.ArrowheadAtTargetPosition;
            if (edge.Label != null)
                edge.Label.Center = edgeRestoreData.LabelCenter;
        }

        /// <summary>
        /// redoes the editing
        /// </summary>
        public override void Redo() {
            base.Redo();
            foreach (GeometryObject geomObj in movedObjects) {
                GeomNode node = geomObj as GeomNode;
                if (node != null) {
                    GeometryGraphEditor.DragNode(node, Delta, GetRestoreData(node) as NodeRestoreData);
                    foreach (GeomEdge edge in node.OutEdges)
                        GeometryGraphEditor.DragEdgeWithSource(Delta, edge, GetRestoreData(edge) as EdgeRestoreData);
                    foreach (GeomEdge edge in node.InEdges)                        
                        GeometryGraphEditor.DragEdgeWithTarget(Delta, edge, GetRestoreData(edge) as EdgeRestoreData);
                    foreach (GeomEdge edge in node.SelfEdges)
                        GeometryGraphEditor.DragEdge(Delta, edge, GetRestoreData(edge) as EdgeRestoreData, movedObjects);
                } else {
                    GeomEdge edge = geomObj as GeomEdge;
                    if (edge != null) {
                        GeometryGraphEditor.DragEdge(Delta, edge, GetRestoreData(edge) as EdgeRestoreData, movedObjects);
                    } else {
                        GeomLabel label = geomObj as GeomLabel;
                        if (label != null)
                            GeometryGraphEditor.DragLabel(label, Delta, (GetRestoreData(label) as LabelRestoreData).Center);
                        else
                            throw new System.NotImplementedException();
                    }
                }
            }
        }
    }
}