///
/// Microsoft SQL Server 2005 Business Intelligence Metadata Reporting Samples
/// Dependency Viewer Sample
/// 
/// Copyright (c) Microsoft Corporation.  All rights reserved.
///
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DependencyKey = System.Collections.Generic.KeyValuePair<int, int>;

namespace DependencyViewer
{
    public class DependencyGraph
    {
        private Dictionary<int, List<int>> fromToListMap = new Dictionary<int,List<int>>(); 
        private Dictionary<int,List<int>> toFromListMap = new Dictionary<int,List<int>>();
        private Dictionary<int, List<int>> ownershipMap = new Dictionary<int, List<int>>();
        private Dictionary<DependencyKey, string> dependencyTypeMap = new Dictionary<DependencyKey, string>();

        public DependencyGraph()
        {
        }

        /// <summary>
        /// Add an containment information between two nodes
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        public void AddContainment(int parent, int child)
        {
            List<int> childList = null;
            if (ownershipMap.TryGetValue(parent, out childList) == false)
            {
                // create the list
                childList = new List<int>();
                ownershipMap.Add(parent, childList);
            }
            Debug.Assert(childList != null);

            if (childList.Contains(child) == false)
            {
                childList.Add(child);
            }

            DependencyKey dk = new DependencyKey(parent, child);
            Debug.Assert(!dependencyTypeMap.ContainsKey(dk));
            dependencyTypeMap[dk] = "Containment";
        }

        /// <summary>
        /// Adds a dependency between two nodes
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void AddDependency(int from, int to, string depType)
        {
            List<int> toList = null;
            List<int> fromList = null;
            if (fromToListMap.ContainsKey(from) == false)
            {
                // create the list
                toList = new List<int>();
                fromToListMap.Add(from, toList);
            }
            else
            {
                toList = fromToListMap[from];
            }

            if (toFromListMap.ContainsKey(to) == false)
            {
                fromList = new List<int>();
                toFromListMap.Add(to, fromList);
            }
            else
            {
                fromList = toFromListMap[to];
            }

            Debug.Assert(toList != null);
            Debug.Assert(fromList != null);

            toList.Add(to);

            fromList.Add(from);

            DependencyKey dk = new DependencyKey(from, to);
            Debug.Assert(!dependencyTypeMap.ContainsKey(dk));
            dependencyTypeMap[dk] = depType;
        }

        /// <summary>
        ///  returns a list of all objects that are visited going out from the specified object
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public IDictionary<int, List<int>> GetDependencies(int from, int limit)
        {
            return TraverseGraph(from, fromToListMap, true, false, limit);
        }

        public IDictionary<int, List<int>> GetLineage(int to, int limit)
        {
            return TraverseGraph(to, toFromListMap, true, false, limit);
        }

        public string GetDependencyType(int from, int to)
        {
            DependencyKey dk = new DependencyKey(from, to);
            Debug.Assert(this.dependencyTypeMap.ContainsKey(dk));
            return this.dependencyTypeMap[dk];
        }

        /// <summary>
        ///  returns a list of nodes encountered when traversing the graph
        /// </summary>
        /// <param name="start"></param>
        /// <param name="graph"></param>
        /// <returns></returns>
        private IDictionary<int, List<int>> TraverseGraph(int start, Dictionary<int, List<int>> graph, bool includeChildren, bool childDep, int limit)
        {
            Queue<int> traversalQueue = new Queue<int>();
            Queue<int> traversalDepth = new Queue<int>();
            Dictionary<int, List<int>> nodesVisited = new Dictionary<int, List<int>>();
            int depth = 0;

            traversalQueue.Enqueue(start);
            traversalDepth.Enqueue(depth);

            IDictionary<int, List<int>> children = null;
            if (includeChildren && limit > 0)
            {
                children = TraverseGraph(start, this.ownershipMap, false, false, limit);
                if (children.Count > 0)
                    depth++;
                foreach (KeyValuePair<int, List<int>> child in children)
                {
                    traversalQueue.Enqueue(child.Key);
                    traversalDepth.Enqueue(depth);
                }
            }

            do
            {
                int currentStart = traversalQueue.Dequeue();
                depth = traversalDepth.Dequeue();
                if ((nodesVisited.ContainsKey(currentStart) == false) && (depth <= limit))
                {
                    List<int> directDependencies;

                    if (graph.ContainsKey(currentStart))
                    {
                        // get a list of all nodes connected to this one
                        directDependencies = graph[currentStart];
                    }
                    else
                    {
                        // current node not connected to anything
                        directDependencies = new List<int>();
                    }

                    // visiting this node the first time
                    nodesVisited.Add(currentStart, directDependencies);
                    if (directDependencies.Count > 0)
                        depth++;
                    foreach (int directDependency in directDependencies)
                    {
                        // add it to the list of nodes visited and to pursue further                        
                        traversalQueue.Enqueue(directDependency);
                        traversalDepth.Enqueue(depth);
                    }
                }

                // add all of the unvisited nodes to our list of dependencies
            } while (traversalQueue.Count > 0);

            // now add parent->child relationships
            if (childDep)
            {
                foreach (KeyValuePair<int, List<int>> child in children)
                {
                    nodesVisited[child.Key].AddRange(child.Value);
                }
            }

            return nodesVisited;
        }

        internal void Clear()
        {
            fromToListMap.Clear();
            toFromListMap.Clear();
            ownershipMap.Clear();
            dependencyTypeMap.Clear();
        }
    }
}
