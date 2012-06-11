using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Drawing;
using CurveFactory = Microsoft.Msagl.Splines.CurveFactory;
using P2 = Microsoft.Msagl.Point;
using GeomNode = Microsoft.Msagl.Node;
using GeomEdge = Microsoft.Msagl.Edge;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;
using DrawingNode = Microsoft.Msagl.Drawing.Node;


namespace DependencyViewer
{
    public partial class MainFrom : Form
    {
        private string connectionString = "server=localhost;database=SSIS_META;Integrated Security=SSPI;";
        private SqlConnection sqlConnection;
        private DependencyGraph dependencyGraph = new DependencyGraph();
        private Dictionary<int, string> objectIDToNameMap = new Dictionary<int, string>();
        private Dictionary<int, string> objectIDToTypeMap = new Dictionary<int, string>();
        private string runIDsInList;
        private int numberInbound, numberOutbound;
        private Node oldNode;
        private Microsoft.Msagl.Drawing.Color oldFillColor;

        public MainFrom()
        {
            InitializeComponent();
            oldNode = null;
        }

        public MainFrom(string connection)
            : this()
        {
            if ( !string.IsNullOrEmpty(connection) )
            {
                this.connectionString = connection;
            }
            oldNode = null;
            oldFillColor = Microsoft.Msagl.Drawing.Color.White;
        }


        private void btnLoad_Click(object sender, EventArgs e)
        {
            Dictionary<int, string> runIDs = new Dictionary<int, string>();
            Dictionary<int, string> selectedRunIDs = new Dictionary<int, string>();
            RunSelection dlgSelectRun = new RunSelection();

            sqlConnection = new SqlConnection(this.connectionString);
            sqlConnection.Open();

            graphViewer.Enabled = false;
            graphViewer.Graph = null;
            objectIDToNameMap.Clear();
            objectIDToTypeMap.Clear();
            dependencyGraph.Clear();

            numberInbound = (int)nbBefore.Value;
            numberOutbound = (int)nbAfter.Value;
            // Get the list of Run's
            using (SqlCommand runsCommand = new SqlCommand("EXEC [dbo].[usp_RetrieveRunIDs];", sqlConnection))
            {
                using (SqlDataReader sqlReader = runsCommand.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        runIDs.Add(sqlReader.GetInt32(0), sqlReader.GetString(1));
                    }
                }
            }

            dlgSelectRun.LoadItems(runIDs);

            if (dlgSelectRun.ShowDialog() == DialogResult.OK)
            {
                selectedRunIDs = dlgSelectRun.GetItems();

                runIDsInList = "";
                foreach (KeyValuePair<int, string> runText in selectedRunIDs)
                {
                    if (runIDsInList == "")
                    {
                        runIDsInList = runText.Key.ToString();
                    }
                    else
                    {
                        runIDsInList += ", " + runText.Key.ToString();
                    }
                }

                if (runIDsInList != "")
                {
                    graphViewer.Enabled = true;
                    // Initialize list of objects
                    InitializeObjectsMap();

                    // initialize the lineage map graph
                    InitializeLineageMap();

                    PopulateObjectList();
                }
            }
        }


        /// <summary>
        /// Loads all the objects into the map tables.
        /// </summary>
        private void InitializeObjectsMap()
        {
            objectIDToNameMap.Clear();
            objectIDToTypeMap.Clear();

            using (SqlCommand objectsCommand = new SqlCommand("EXEC [dbo].[usp_RetrieveObjects] @RunList", sqlConnection))
            {
                objectsCommand.Parameters.Add("@RunList", SqlDbType.NVarChar, runIDsInList.Length).Value = runIDsInList;
                using (SqlDataReader objectsList = objectsCommand.ExecuteReader())
                {
                    while (objectsList.Read())
                    {
                        int objID = objectsList.GetInt32(0);
                        string objName = objectsList.GetString(1);
                        string objType = objectsList.IsDBNull(3) ? objectsList.GetString(2) : objectsList.GetString(3);
                        objectIDToNameMap.Add(objID, objName);
                        objectIDToTypeMap.Add(objID, objType);
                    }
                }
            }
        }

        /// <summary>
        ///  creates a map of dependencies in memory
        ///  This loads ALL the dependencies...
        /// </summary>
        private void InitializeLineageMap()
        {
            using (SqlCommand lineageMapCommand = new SqlCommand("EXEC [dbo].[usp_RetrieveLineageMap] @RunList", sqlConnection))
            {
                lineageMapCommand.Parameters.Add("@RunList", SqlDbType.NVarChar, runIDsInList.Length).Value = runIDsInList;
                using (SqlDataReader lineageMapReader = lineageMapCommand.ExecuteReader())
                {
                    while (lineageMapReader.Read())
                    {
                        int from = lineageMapReader.GetInt32(0);
                        int to = lineageMapReader.GetInt32(1);
                        string depType = lineageMapReader.GetString(2);
                        if (depType == "Map" || depType == "Use")
                        {
                            dependencyGraph.AddDependency(from, to, depType);
                        }
                        else if (depType == "Containment")
                        {
                            dependencyGraph.AddContainment(from, to);
                        }
                    }
                }
            }
        }

        #region treeview methods
        /// <summary>
        /// Loads the data into the Tree View.
        /// </summary>
        private void PopulateObjectList()
        {
            try
            {
                tvObjectList.BeginUpdate();

                // remove all nodes
                tvObjectList.Nodes.Clear();

                // Show the AS objects
                ShowAnalysisServicesObjects();

                // create the package root
                ShowPackages();

                // create the connections root
                ShowConnections();

                // create the file servers root
                ShowFileServers();

                // create the Reporting Server root
                ShowReports();
            }
            finally
            {
                tvObjectList.EndUpdate();
            }
        }

        /// <summary>
        /// Loads all analysis services connections
        /// </summary>
        private void ShowAnalysisServicesObjects()
        {
            TreeNode analysisServicesRoot = tvObjectList.Nodes.Add("Analysis Services");

            using (SqlCommand analysisServersCommand = new SqlCommand("EXEC [dbo].[usp_RetrieveSSASObjects] @RunList", sqlConnection))
            {
                analysisServersCommand.Parameters.Add("@RunList", SqlDbType.NVarChar, runIDsInList.Length).Value = runIDsInList;
                using (SqlDataReader sqlReader = analysisServersCommand.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        TreeNode objectNode = analysisServicesRoot.Nodes.Add(sqlReader.GetInt32(0).ToString(), sqlReader.GetString(1));

                        // for each node add a dummy child node so that we get a '+' and can dig in deeper
                        objectNode.Nodes.Add(new DummyTreeNode());
                    }
                }
            }
        }

        /// <summary>
        /// Loads all Database connections
        /// </summary>
        private void ShowConnections()
        {
            TreeNode connectionsRootNode = tvObjectList.Nodes.Add("Relational Database");
            using (SqlCommand connectionsCommand = new SqlCommand("EXEC [dbo].[usp_RetrieveSQLSObjects] @RunList", sqlConnection))
            {
                connectionsCommand.Parameters.Add("@RunList", SqlDbType.NVarChar, runIDsInList.Length).Value = runIDsInList;
                using (SqlDataReader sqlReader = connectionsCommand.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        TreeNode objectNode = connectionsRootNode.Nodes.Add(sqlReader.GetInt32(0).ToString(), sqlReader.GetString(1));

                        // for each node add a dummy child node so that we get a '+' and can dig in deeper
                        objectNode.Nodes.Add(new DummyTreeNode());
                    }
                }
            }
        }

        /// <summary>
        /// Loads all reporting servers
        /// </summary>
        private void ShowReports()
        {
            TreeNode reportServersRootNode = tvObjectList.Nodes.Add("Reporting Servers");
            using (SqlCommand reportServersCommand = new SqlCommand("EXEC [dbo].[usp_RetrieveSSRSObjects] @RunList", sqlConnection))
            {
                reportServersCommand.Parameters.Add("@RunList", SqlDbType.NVarChar, runIDsInList.Length).Value = runIDsInList;
                using (SqlDataReader sqlReader = reportServersCommand.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        TreeNode objectNode = reportServersRootNode.Nodes.Add(sqlReader.GetInt32(0).ToString(), sqlReader.GetString(1));

                        // for each node add a dummy child node so that we get a '+' and can dig in deeper
                        objectNode.Nodes.Add(new DummyTreeNode());
                    }
                }
            }
        }


        /// <summary>
        /// Loads all File connections
        /// </summary>
        private void ShowFileServers()
        {
            TreeNode filesRootNode = tvObjectList.Nodes.Add("Files");
            using (SqlCommand fileServersCommand = new SqlCommand("EXEC [dbo].[usp_RetrieveFileObjects] @RunList", sqlConnection))
            {
                fileServersCommand.Parameters.Add("@RunList", SqlDbType.NVarChar, runIDsInList.Length).Value = runIDsInList;
                using (SqlDataReader sqlReader = fileServersCommand.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        TreeNode objectNode = filesRootNode.Nodes.Add(sqlReader.GetInt32(0).ToString(), sqlReader.GetString(1));

                        // for each node add a dummy child node so that we get a '+' and can dig in deeper
                        objectNode.Nodes.Add(new DummyTreeNode());
                    }
                }
            }
        }

        /// <summary>
        /// Loads all SSIS packages
        /// </summary>
        private void ShowPackages()
        {
            TreeNode packageRootNode = tvObjectList.Nodes.Add("Integration Services");

            using (SqlCommand packageCommand = new SqlCommand("EXEC [dbo].[usp_RetrieveSSISObjects] @RunList", this.sqlConnection))
            {
                packageCommand.Parameters.Add("@RunList", SqlDbType.NVarChar, runIDsInList.Length).Value = runIDsInList;
                using (SqlDataReader sqlReader = packageCommand.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        TreeNode objectNode = packageRootNode.Nodes.Add(sqlReader.GetInt32(0).ToString(), sqlReader.GetString(1));

                        // for each node add a dummy child node so that we get a '+' and can dig in deeper
                        objectNode.Nodes.Add(new DummyTreeNode());
                    }
                }
            }
        }

        /// <summary>
        /// Fires before a node is expanded to load the missing data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvObjectList_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            // if the node we have is a package and the child is a dummy, remove that node and add the next level dependencies
            if (e.Action == TreeViewAction.Expand)
            {
                TreeNodeCollection nodeCollection = e.Node.Nodes;
                if ((nodeCollection.Count == 1) && (nodeCollection[0] is DummyTreeNode))
                {
                    // remove the dummy tree node
                    nodeCollection.Clear();

                    // get the new list of dependencies
                    AddChildren(int.Parse(e.Node.Name), e.Node);
                }
            }
        }

        /// <summary>
        /// Adds the child nodes into the tree view.
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentNode"></param>
        private void AddChildren(int parentID, TreeNode parentNode)
        {
            using (SqlCommand childrenCommand = new SqlCommand("EXEC [dbo].[usp_RetrieveContained] @SrcObjectKey", sqlConnection))
            {
                childrenCommand.Parameters.Add("@SrcObjectKey", SqlDbType.Int).Value = parentID;
                using (SqlDataReader sqlReader = childrenCommand.ExecuteReader())
                {
                    string lastCategory = null;
                    TreeNode categoryNode = null;
                    while (sqlReader.Read())
                    {
                        // get the type category
                        string currentCategory = sqlReader.GetString(2);

                        if (lastCategory != currentCategory)
                        {
                            lastCategory = currentCategory;
                            categoryNode = parentNode.Nodes.Add(currentCategory);
                        }

                        TreeNode objectNode = categoryNode.Nodes.Add(sqlReader.GetInt32(0).ToString(), sqlReader.GetString(1));

                        objectNode.Nodes.Add(new DummyTreeNode());
                    }
                }
            }
        }

        private void tvObjectList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                this.UseWaitCursor = true;
                graphViewer.Enabled = true;

                // do the impact analysis and lineage if that makes sense
                // get the ID of the selected node
                string objectName = e.Node.Name;
                int objectID = 0;
                if (int.TryParse(objectName, out objectID))
                {
                    //create the normal graph.
                    Graph newGraph = new Graph(objectName, objectID.ToString());
                    newGraph.Attr.LayerDirection = GetDirection();
                    //create the geom graph
                    //Microsoft.Msagl.GeometryGraph geomGraph = new Microsoft.Msagl.GeometryGraph();
                    newGraph.CreateGeometryGraph();
                    Microsoft.Msagl.GeometryGraph geomGraph = newGraph.GeometryGraph;

                    Dictionary<int, Node> idToNodeMap = new Dictionary<int, Node>();
                    // get a list of nodes that depend on us
                    IDictionary<int, List<int>> dependencies = dependencyGraph.GetDependencies(objectID, numberOutbound);

                    foreach (int dependency in dependencies.Keys)
                    {
                        if (idToNodeMap.ContainsKey(dependency) == false)
                        {
                            Node node = newGraph.AddNode(dependency.ToString());
                            if (dependency == objectID)
                            {
                                node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightGreen;
                            }
                            else
                            {
                                node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.White;
                            }

                            node.Attr.Color = Microsoft.Msagl.Drawing.Color.Blue;

                            node.DrawNodeDelegate = new DelegateToOverrideNodeRendering(this.DrawNode);
                            float width = 0, height = 0;
                            System.Drawing.Size textSize = TextRenderer.MeasureText(objectIDToNameMap[dependency], new Font("Arial", 14, FontStyle.Regular));
                            width = textSize.Width;
                            height = textSize.Height;
                            textSize = TextRenderer.MeasureText(objectIDToTypeMap[dependency], new Font("Arial", 10, FontStyle.Regular));
                            if (textSize.Width > width)
                                width = textSize.Width;
                            height += textSize.Height;
                            height += 10;  // 10 extra pixels...
                            GeomNode geomNode = new Microsoft.Msagl.Node(dependency.ToString(), CurveFactory.CreateBox(width, height, new P2()));
                            geomGraph.AddNode(geomNode);
                            node.Attr.GeometryNode = geomNode;
                            idToNodeMap.Add(dependency, node);
                        }
                    }

                    // add the edges
                    foreach (int currentNode in dependencies.Keys)
                    {
                        Node fromNode = idToNodeMap[currentNode];

                        List<int> directDependencies = dependencies[currentNode];

                        foreach (int directDependency in directDependencies)
                        {
                            if (idToNodeMap.ContainsKey(directDependency))
                            {
                                Node toNode = idToNodeMap[directDependency];

                                // add an edge
                                Edge edge = newGraph.AddEdge(fromNode.Id, toNode.Id);
                                GeomEdge geomEdge = new GeomEdge(geomGraph.FindNode(currentNode.ToString()), geomGraph.FindNode(directDependency.ToString()));
                                // Arrow?
                                geomEdge.ArrowheadLength = 20;
                                geomGraph.AddEdge(geomEdge);
                                edge.Attr.GeometryEdge = geomEdge;
                            }
                        }
                    }

                    IDictionary<int, List<int>> lineages = dependencyGraph.GetLineage(objectID, numberInbound);

                    foreach (int lineage in lineages.Keys)
                    {
                        if (idToNodeMap.ContainsKey(lineage) == false)
                        {
                            Node node = newGraph.AddNode(lineage.ToString());
                            if (lineage == objectID)
                            {
                                node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightGreen;
                            }
                            else
                            {
                                node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.White;
                            }
                            node.Attr.Color = Microsoft.Msagl.Drawing.Color.Blue;

                            node.DrawNodeDelegate = new DelegateToOverrideNodeRendering(this.DrawNode);
                            float width = 0, height = 0;
                            System.Drawing.Size textSize = TextRenderer.MeasureText(objectIDToNameMap[lineage], new Font("Arial", 14, FontStyle.Regular));
                            width = textSize.Width;
                            height = textSize.Height;
                            textSize = TextRenderer.MeasureText(objectIDToTypeMap[lineage], new Font("Arial", 10, FontStyle.Regular));
                            if (textSize.Width > width)
                                width = textSize.Width;
                            height += textSize.Height;
                            height += 10;  // 10 extra pixels...
                            GeomNode geomNode = new Microsoft.Msagl.Node(lineage.ToString(), CurveFactory.CreateBox(width, height, new P2()));
                            geomGraph.AddNode(geomNode);
                            node.Attr.GeometryNode = geomNode;
                            idToNodeMap.Add(lineage, node);
                        }
                    }

                    // add the edges
                    foreach (int currentNode in lineages.Keys)
                    {
                        Node toNode = idToNodeMap[currentNode];

                        List<int> directLineages = lineages[currentNode];

                        foreach (int directLineage in directLineages)
                        {
                            if (idToNodeMap.ContainsKey(directLineage))
                            {
                                Node fromNode = idToNodeMap[directLineage];

                                // add an edge
                                Edge edge = newGraph.AddEdge(fromNode.Id, toNode.Id);
                                GeomEdge geomEdge = new GeomEdge(geomGraph.FindNode(directLineage.ToString()), geomGraph.FindNode(currentNode.ToString()));
                                // Arrow?
                                geomEdge.ArrowheadLength = 20;
                                geomGraph.AddEdge(geomEdge);
                                edge.Attr.GeometryEdge = geomEdge;
                            }
                        }
                    }

                    //impactAnalysisGraphCtrl1.DrawMap(objectID, numberInbound, numberOutbound);
                    newGraph.GeometryGraph = geomGraph;
                    //geomGraph.LayoutAlgorithmSettings = new Microsoft.Msagl.LayoutAlgorithmSettings();
                    
                    geomGraph.CalculateLayout();
                    graphViewer.NeedToCalculateLayout = false;
                    graphViewer.Graph = newGraph;
                    graphViewer.LocalScale = 1.0;  // ToDo: Work out why scaling isn't doing what I expect.  Need to setup "Minimum Zoom" so that Very Large graphs don't crash.
                }
                else
                {
                    graphViewer.Graph = null;
                }
            }
            finally
            {
                this.UseWaitCursor = false;
            }
        }
        #endregion


        bool DrawNode(DrawingNode node, object graphics)
        {
            Graphics g = (Graphics)graphics;
            Pen p = new Pen(System.Drawing.Color.Blue, (float)1);

            if (node.Attr.Color != Microsoft.Msagl.Drawing.Color.Blue)
            {
                p.Color = System.Drawing.Color.DarkRed;
            }

            // Draw the bounding box
            if (node.Attr.FillColor == Microsoft.Msagl.Drawing.Color.LightGreen)
            {
                g.FillRectangle(Brushes.LightGreen, (float)node.Attr.GeometryNode.BoundingBox.LeftBottom.X, (float)node.Attr.GeometryNode.BoundingBox.LeftBottom.Y, (float)node.Attr.GeometryNode.BoundingBox.Width, (float)node.Attr.GeometryNode.BoundingBox.Height);
            }
            else if (node.Attr.FillColor == Microsoft.Msagl.Drawing.Color.LightSalmon)
            {
                g.FillRectangle(Brushes.LightSalmon, (float)node.Attr.GeometryNode.BoundingBox.LeftBottom.X, (float)node.Attr.GeometryNode.BoundingBox.LeftBottom.Y, (float)node.Attr.GeometryNode.BoundingBox.Width, (float)node.Attr.GeometryNode.BoundingBox.Height);
            }
            g.DrawRectangle(p, (float)node.Attr.GeometryNode.BoundingBox.LeftBottom.X, (float)node.Attr.GeometryNode.BoundingBox.LeftBottom.Y, (float)node.Attr.GeometryNode.BoundingBox.Width, (float)node.Attr.GeometryNode.BoundingBox.Height);

            using (System.Drawing.Drawing2D.Matrix m = g.Transform)
            {
                using (System.Drawing.Drawing2D.Matrix saveM = m.Clone())
                {
                    float c = (float)node.Attr.GeometryNode.Center.Y;

                    using (System.Drawing.Drawing2D.Matrix m2 = new System.Drawing.Drawing2D.Matrix(1, 0, 0, -1, 0, 2 * c))
                        m.Multiply(m2);

                    g.Transform = m;

                    string textToShow = objectIDToNameMap[Int32.Parse(node.Id)];
                    g.DrawString(textToShow, new Font("Arial", 14, FontStyle.Regular), Brushes.Blue, (float)node.Attr.GeometryNode.BoundingBox.LeftBottom.X, (float)node.Attr.GeometryNode.BoundingBox.LeftBottom.Y);
                    System.Drawing.Size textSize = TextRenderer.MeasureText(g, objectIDToTypeMap[Int32.Parse(node.Id)], new Font("Arial", 10, FontStyle.Regular));
                    g.DrawString(objectIDToTypeMap[Int32.Parse(node.Id)], new Font("Arial", 10, FontStyle.Regular), Brushes.Blue, (float)node.Attr.GeometryNode.BoundingBox.LeftBottom.X, (float)node.Attr.GeometryNode.BoundingBox.LeftBottom.Y + textSize.Height + 10);
                    g.Transform = saveM;
                }
            }

            return true;//returning false would enable the default rendering
        }


        static internal PointF PointF(P2 p) { return new PointF((float)p.X, (float)p.Y); }

        private void nbBefore_ValueChanged(object sender, EventArgs e)
        {
            numberInbound = (int)nbBefore.Value;
            tvObjectList_AfterSelect(sender, new TreeViewEventArgs(tvObjectList.SelectedNode));
        }

        private void nbAfter_ValueChanged(object sender, EventArgs e)
        {
            numberOutbound = (int)nbAfter.Value;
            tvObjectList_AfterSelect(sender, new TreeViewEventArgs(tvObjectList.SelectedNode));
        }

        private LayerDirection GetDirection()
        {
            switch ((string)cbLayoutDirection.SelectedItem)
            {
                case "Left to Right":
                    return LayerDirection.LR;
                case "Right to Left":
                    return LayerDirection.RL;
                case "Top Down":
                    return LayerDirection.TB;
                case "Bottom Up":
                    return LayerDirection.BT;
                default: return LayerDirection.None;
            }
        }

        private void btLayout_Click(object sender, EventArgs e)
        {
            tvObjectList_AfterSelect(sender, new TreeViewEventArgs(tvObjectList.SelectedNode));
        }

        private void cbLayoutDirection_SelectedValueChanged(object sender, EventArgs e)
        {
            tvObjectList_AfterSelect(sender, new TreeViewEventArgs(tvObjectList.SelectedNode));
        }

        void AddObjectDetails(int objectID)
        {
            string objectType, objectDesc;
            using (SqlCommand objectsCommand = new SqlCommand("EXEC [dbo].[usp_RetrieveObjectDetails] @RunList, @ObjectKey", sqlConnection))
            {
                objectsCommand.Parameters.Add("@RunList", SqlDbType.NVarChar, runIDsInList.Length).Value = runIDsInList;
                objectsCommand.Parameters.Add("@ObjectKey", SqlDbType.Int).Value = objectID;
                using (SqlDataReader attributes = objectsCommand.ExecuteReader())
                {
                    attributes.Read();
                    objectType = attributes["ObjectTypeString"] as string;
                    objectDesc = attributes["ObjectDesc"] as string;
                }
            }

            // try to convert object type name to readable name in ObjectTypes table
            using (SqlCommand objectsCommand = new SqlCommand("EXEC [dbo].[usp_RetrieveObjectTypes] @ObjectTypeKey", sqlConnection))
            {
                objectsCommand.Parameters.Add(new SqlParameter("@ObjectTypeKey", objectType));
                using (SqlDataReader attributes = objectsCommand.ExecuteReader())
                {
                    if (attributes.Read())
                    {
                        objectType = (string)attributes["ObjectTypeName"];
                    }
                }
            }

            string groupName = string.Format("{0} [{1}] [ID: {2}]", this.objectIDToNameMap[objectID], objectType, objectID);
            ListViewGroup group = new ListViewGroup(groupName);
            this.lvObjectProperties.Groups.Add(group);

            if (!System.Windows.Forms.VisualStyles.VisualStyleInformation.IsSupportedByOS)
            {
                // if OS does not support groups, we need to create a list view item, since groups are ignored
                ListViewItem grp = new ListViewItem(this.objectIDToNameMap[objectID]);
                grp.SubItems.Add(string.Format("[{0}] [ID: {1}]", objectType, objectID));
                grp.Font = new Font(this.lvObjectProperties.Font, FontStyle.Bold);
                this.lvObjectProperties.Items.Add(grp);
            }

            // we need at least one item in a group for the group to show up
            // thus show the description even if it is empty
            lvObjectProperties.Items.Add(CreateAttributeViewItem(group, "Description", objectDesc));

            using (SqlCommand objectsCommand = new SqlCommand("EXEC [dbo].[usp_RetrieveObjectAttributes] @RunList, @ObjectKey", sqlConnection))
            {
                objectsCommand.Parameters.Add("@RunList", SqlDbType.NVarChar, runIDsInList.Length).Value = runIDsInList;
                objectsCommand.Parameters.Add("@ObjectKey", SqlDbType.Int).Value = objectID;
                using (SqlDataReader attributes = objectsCommand.ExecuteReader())
                {
                    while (attributes.Read())
                    {
                        string attribName = (string)attributes["ObjectAttrName"];
                        string attribValue = (string)attributes["ObjectAttrValue"];

                        if (!string.IsNullOrEmpty(attribValue))
                        {
                            lvObjectProperties.Items.Add(CreateAttributeViewItem(group, attribName, attribValue));
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Create a list view item for an attribute, filtering the control characters
        /// </summary>
        private ListViewItem CreateAttributeViewItem(ListViewGroup group, string name, string value)
        {
            ListViewItem attrib = new ListViewItem(name, group);
            if (value != null)
            {
                value = value.Replace('\r', ' ');
                value = value.Replace('\n', ' ');
                attrib.SubItems.Add(value);
            }
            return attrib;
        }

        private bool GetParent(int child, out int parent)
        {
            using (SqlCommand containmentCommand = new SqlCommand("EXEC [dbo].[usp_RetrieveContainedTargetDependencies] @RunList, @TgtObjectKey", sqlConnection))
            {
                containmentCommand.Parameters.Add("@RunList", SqlDbType.NVarChar, runIDsInList.Length).Value = runIDsInList;
                containmentCommand.Parameters.Add("@TgtObjectKey", SqlDbType.Int).Value = child;
                object value = containmentCommand.ExecuteScalar();
                if (value != null)
                {
                    parent = (int)value;
                    return true;
                }
                else
                {
                    parent = -1;
                    return false;
                }
            }
        }


        private void graphViewer_SelectionChanged(object sender, EventArgs e)
        {
            if (graphViewer.SelectedObject != null)
            {
                object selectedObject = graphViewer.SelectedObject;
                if (selectedObject is Node)
                {
                    if (oldNode != null)
                    {
                        oldNode.Attr.Color = Microsoft.Msagl.Drawing.Color.Blue;
                        oldNode.Attr.FillColor = oldFillColor;
                    }
                    Node selectedNode = (Node)selectedObject;
                    oldFillColor = selectedNode.Attr.FillColor;
                    selectedNode.Attr.Color = Microsoft.Msagl.Drawing.Color.DarkRed;
                    selectedNode.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightSalmon;
                    int objID = Int32.Parse(selectedNode.Id);
                    try
                    {
                        lvObjectProperties.BeginUpdate();
                        lvObjectProperties.Items.Clear();
                        lvObjectProperties.Groups.Clear();
                        while (objID > 0)
                        {
                            AddObjectDetails(objID);
                            GetParent(objID, out objID);
                        }
                        lvObjectProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                        lvObjectProperties.Columns[0].Width += 10;
                    }
                    finally
                    {
                        this.lvObjectProperties.EndUpdate();
                    }
                    oldNode = selectedNode;
                    graphViewer.Invalidate();
                }
            }
        }

        private void locateObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (oldNode != null)
            {
                TreeNode[] foundNodes = this.tvObjectList.Nodes.Find(oldNode.Id, true);
                if (foundNodes.Length > 0)
                {
                    this.tvObjectList.SelectedNode = foundNodes[0];
                }
                else
                {
                    this.tvObjectList.ExpandAll();
                    this.tvObjectList.CollapseAll();
                    foundNodes = this.tvObjectList.Nodes.Find(oldNode.Id, true);
                    if (foundNodes.Length > 0)
                    {
                        this.tvObjectList.SelectedNode = foundNodes[0];
                    }
                }
            }
        }        

    }
}
