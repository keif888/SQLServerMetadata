using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// a helper class to create a geometry graph
    /// </summary>
    public sealed class CreateLayoutGraph {

        CreateLayoutGraph() { }
        
        static internal Microsoft.Msagl.GeometryGraph Create(Graph graph) {
            
            Microsoft.Msagl.GeometryGraph msaglGraph = new Microsoft.Msagl.GeometryGraph();
            return FillGraph(graph, msaglGraph);
        }

        private static GeometryGraph FillGraph(Graph graph, Microsoft.Msagl.GeometryGraph msaglGraph) {
            ProcessGraphAttrs(graph, msaglGraph);
            ProcessNodes(graph, msaglGraph);
            ProcessEdges(graph, msaglGraph);

            return msaglGraph;
        }

        private static GeometryGraph FillPhyloTree(Graph graph, Microsoft.Msagl.GeometryGraph msaglGraph) {
            ProcessGraphAttrs(graph, msaglGraph);
            ProcessNodes(graph, msaglGraph);
            ProcessPhyloEdges(graph, msaglGraph);
            return msaglGraph;
        }

        private static void ProcessEdges(Graph graph, Microsoft.Msagl.GeometryGraph msaglGraph) {
            foreach (Edge drawingEdge in graph.Edges) {
                Microsoft.Msagl.Node sourceNode = msaglGraph.FindNode(drawingEdge.Source);
                Microsoft.Msagl.Node targetNode = msaglGraph.FindNode(drawingEdge.Target);

                if (sourceNode == null)
                    sourceNode = CreateGeometryNode(msaglGraph, graph.FindNode(drawingEdge.Source) as Node,Connection.Connected);
                if (targetNode == null)
                    targetNode = CreateGeometryNode(msaglGraph, graph.FindNode(drawingEdge.Target) as Node,Connection.Connected);

                Microsoft.Msagl.Edge msaglEdge = new Microsoft.Msagl.Edge(sourceNode, targetNode);
                if (drawingEdge.Label != null && graph.LayoutAlgorithmSettings is SugiyamaLayoutSettings) {
                    msaglEdge.Label = drawingEdge.Label.GeometryLabel;
                    msaglEdge.Label.Parent = msaglEdge;
                }
                msaglEdge.Weight = drawingEdge.Attr.Weight;
                msaglEdge.Length = drawingEdge.Attr.Length;
                msaglEdge.Separation = drawingEdge.Attr.Separation;
                msaglEdge.ArrowheadAtSource = drawingEdge.Attr.ArrowAtSource;
                msaglEdge.ArrowheadAtTarget = drawingEdge.Attr.ArrowAtTarget;
                msaglGraph.AddEdge(msaglEdge);
                msaglEdge.UserData = drawingEdge;
                msaglEdge.ArrowheadLength = drawingEdge.Attr.ArrowheadLength;
                msaglEdge.LineWidth = drawingEdge.Attr.LineWidth;
            }
        }

        private static void ProcessPhyloEdges(Graph graph, Microsoft.Msagl.GeometryGraph msaglGraph) {
            foreach (Edge e in graph.Edges) {
                Microsoft.Msagl.Node sourceNode = msaglGraph.FindNode(e.Source);
                Microsoft.Msagl.Node targetNode = msaglGraph.FindNode(e.Target);

                if (sourceNode == null)
                    sourceNode = CreateGeometryNode(msaglGraph, graph.FindNode(e.Source) as Node,Connection.Connected);
                if (targetNode == null)
                    targetNode = CreateGeometryNode(msaglGraph, graph.FindNode(e.Target) as Node,Connection.Connected);

                Microsoft.Msagl.Edge msaglEdge = new Microsoft.Msagl.PhyloEdge(sourceNode, targetNode);
                msaglEdge.Weight = e.Attr.Weight;
                msaglEdge.Separation = e.Attr.Separation;
                msaglEdge.ArrowheadAtSource = e.Attr.ArrowAtSource;
                msaglEdge.ArrowheadAtTarget = e.Attr.ArrowAtTarget;
                msaglGraph.AddEdge(msaglEdge);
                msaglEdge.UserData = e;
                msaglEdge.ArrowheadLength = e.Attr.ArrowheadLength;
                msaglEdge.LineWidth = e.Attr.LineWidth;
            }
        }


        private static void ProcessNodes(Graph graph, Microsoft.Msagl.GeometryGraph msaglGraph) {
            foreach (Node n in graph.NodeMap.Values) {
                Microsoft.Msagl.Node msaglNode = msaglGraph.FindNode(n.Id);
                if (msaglNode == null)
                    msaglNode = CreateGeometryNode(msaglGraph, n,Connection.Connected);
                else
                    msaglGraph.NodeMap[msaglNode.Id] = msaglNode;
            }
        }

        private static void ProcessGraphAttrs(Graph graph, Microsoft.Msagl.GeometryGraph msaglGraph) {
            msaglGraph.LayoutAlgorithmSettings = graph.LayoutAlgorithmSettings;
            
            msaglGraph.Margins = graph.Attr.Margin;
            msaglGraph.NodeSeparation = graph.Attr.NodeSeparation;
            msaglGraph.LayerSeparation = graph.Attr.LayerSeparation;
            msaglGraph.AspectRatio = graph.attr.AspectRatio;

            switch (graph.Attr.LayerDirection) {
                case LayerDirection.None:
                case LayerDirection.TB:
                    break;
                case LayerDirection.LR:
                    msaglGraph.Transformation = Microsoft.Msagl.Splines.PlaneTransformation.Rotation(Math.PI / 2);
                    if (msaglGraph.AspectRatio != 0)
                        msaglGraph.AspectRatio = 1 / msaglGraph.AspectRatio;
                    break;
                case LayerDirection.RL:
                    msaglGraph.Transformation = Microsoft.Msagl.Splines.PlaneTransformation.Rotation(-Math.PI / 2);
                    if (msaglGraph.AspectRatio != 0)
                        msaglGraph.AspectRatio = 1 / msaglGraph.AspectRatio;

                    break;

                case LayerDirection.BT:
                    msaglGraph.Transformation = Microsoft.Msagl.Splines.PlaneTransformation.Rotation(Math.PI);
                    break;

                default:
                    throw new InvalidOperationException();//"unexpected layout direction");
            }
        }

        /// <summary>
        /// a helper function creating a geometry node
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="node"></param>
        /// <param name="connection">controls if the node is connected to the graph</param>
        /// <returns></returns>
        public static Microsoft.Msagl.Node CreateGeometryNode(Microsoft.Msagl.GeometryGraph graph, Node node, Connection connection) {
            Microsoft.Msagl.Node geomNode = new Microsoft.Msagl.Node(node.Id, null);
            
            if(connection==Connection.Connected)
                graph.AddNode(geomNode);
            if (node.Label != null) {
                geomNode.Label = node.Label.GeometryLabel;
                if (geomNode.Label != null)
                    geomNode.Label.Parent = geomNode;
            }
            geomNode.UserData = node;
            geomNode.Padding = node.Attr.Padding;
            return geomNode;
        }

        internal static GeometryGraph CreatePhyloTree(PhylogeneticTree drawingTree) {
            Microsoft.Msagl.PhyloTree phyloTree = new Microsoft.Msagl.PhyloTree();
            FillPhyloTree(drawingTree, phyloTree);
            AssignLengthsToGeometryEdges(phyloTree);
            return phyloTree;
        }

        private static void AssignLengthsToGeometryEdges(Microsoft.Msagl.PhyloTree phyloTree) {
            foreach (Microsoft.Msagl.PhyloEdge msaglEdge in phyloTree.Edges) {
                PhyloEdge drawingEdge = msaglEdge.UserData as PhyloEdge;
                msaglEdge.Length = drawingEdge.Length;
            }
        }
    }
}
