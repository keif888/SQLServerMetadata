using System.Collections.Generic;
using Microsoft.Msagl.Splines;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// the interface for undo objects
    /// </summary>
    public class UndoRedoAction {
        
        Set<object> affectedObjects;
        /// <summary>
        /// the set of the objects that the viewer has to invalidate
        /// </summary>
        public Set<object> AffectedObjects {
            get { return affectedObjects; }
            internal set { affectedObjects = value; }
        }
        internal UndoRedoAction(GeometryGraph graphPar) {
            this.Graph = graphPar;
            this.graphBoundingBoxBefore = this.Graph.BoundingBox;
        }

        GeometryGraph graph;
        /// <summary>
        /// the graph being edited
        /// </summary>
        public GeometryGraph Graph {
            get { return graph; }
            set { graph = value; }
        }      
        UndoRedoAction next;
        UndoRedoAction prev;
        /// <summary>
        /// Undoes the action
        /// </summary>
        virtual public void Undo(){
            if(GraphBoundingBoxHasChanged)
                this.Graph.BoundingBox=GraphBoundingBoxBefore;
        }
        /// <summary>
        /// Redoes the action
        /// </summary>
        virtual public void Redo(){
            if(GraphBoundingBoxHasChanged)
                this.Graph.BoundingBox=GraphBoundingBoxAfter;
        }
        /// <summary>
        /// The pointer to the next undo object
        /// </summary>
        public UndoRedoAction Next { get { return next; } set { next = value; } }
        /// <summary>
        /// The pointer to the previous undo object
        /// </summary>
        public UndoRedoAction Previous { get { return prev; } set { prev = value; } }

        Dictionary<GeometryObject, RestoreData> restoreDataDictionary =
            new Dictionary<GeometryObject, RestoreData>();

        internal void AddRestoreData(GeometryObject msaglObject, RestoreData restoreData) {
            restoreDataDictionary[msaglObject] = restoreData;
        }

        internal static GeometryGraph GetParentGraph(GeometryObject geomObj) {
            do {
                GeometryGraph graph = geomObj.Parent as GeometryGraph;
                if (graph != null)
                    return graph;
                geomObj = geomObj.Parent;
            }
            while (true);
        }

        internal RestoreData GetRestoreData(GeometryObject msaglObject) {
            return restoreDataDictionary[msaglObject];
        }
        
        /// <summary>
        /// enumerates over all edited objects
        /// </summary>
        public  IEnumerator<GeometryObject> EditedObjects {
            get{ return restoreDataDictionary.Keys.GetEnumerator();}
        }

      
        Rectangle graphBoundingBoxBefore=new Rectangle();

        /// <summary>
        /// the graph bounding box before the change
        /// </summary>
        public Rectangle GraphBoundingBoxBefore {
            get { return graphBoundingBoxBefore; }
            set { graphBoundingBoxBefore = value; }
        }

        Rectangle graphBoundingBoxAfter=new Rectangle();

        /// <summary>
        /// the graph bounding box after the change
        /// </summary>
        public Rectangle GraphBoundingBoxAfter {
            get { return graphBoundingBoxAfter; }
            set { graphBoundingBoxAfter = value; }
        }

        /// <summary>
        /// returns true if the was a change in the bounding box of the graph
        /// </summary>
        public bool GraphBoundingBoxHasChanged {
            get{
                return graphBoundingBoxAfter!=graphBoundingBoxBefore;
            }
        }

        

    }
}
