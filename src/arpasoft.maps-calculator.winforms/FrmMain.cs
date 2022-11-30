using arpasoft.maps_calculator.core.Services;
using arpasoft.maps_calculator.winforms.Utils;
using System.Text;

namespace arpasoft.maps_calculator.winforms
{
    public partial class FrmMain : Form
    {
        #region Config
        private const int FIX_POSITION_X = 16;
        private const int FIX_POSITION_Y = 44;
        private const int RADIUS = 10;
        /// ERROR
        private const int ERROR_LINE_X1 = 5;
        private const int ERROR_LINE_Y1 = 5;
        private const int ERROR_LINE_X2 = 5;
        private const int ERROR_LINE_Y2 = 5;
        #endregion

        #region Map
        private IMapService<Coordinate> _mapService;
        #endregion

        #region Form-Mode
        private Graphics? _myGraphics;
        private bool _dirty = false;
        private FormMode _formMode = FormMode.ReadOnly;
        private AddingEdgeType _addingEdgeType = AddingEdgeType.Single;
        private Coordinate? _lastNodeMatched = null;
        #endregion

        #region Constructor
        public FrmMain(IMapService<Coordinate> mapService)
        {
            InitializeComponent();
            _mapService = mapService;
        }
        #endregion

        #region Events
        private void FrmMain_Load(object sender, EventArgs e)
        {
            /// Load Controls
            _myGraphics = picMap.CreateGraphics();
            rbtSingleEdge.Visible = rbtDoubleEdge.Visible = (_formMode == FormMode.AddingEdges);
            btnAddNodes.Enabled = btnAddEdges.Enabled = false;
        }

        private void picMap_Click(object sender, EventArgs e)
        {
            switch (_formMode)
            {
                case FormMode.AddingNodes:
                    AddNodeAndDrawMap();
                    break;
                case FormMode.AddingEdges:
                    DrawMapEdge();
                    break;
                case FormMode.ReadOnly:
                    if (!_dirty)
                    {
                        btnAddNodes.Enabled = btnAddEdges.Enabled = true;
                        LoadAndPrintNodes();
                        _dirty = true;
                    }
                    break;
            }
        }

        private void btnAddNodes_Click(object sender, EventArgs e)
        {
            if (AbortAddingNodeAction())
                return;

            switch (_formMode)
            {
                case FormMode.AddingNodes:
                    btnAddNodes.Text = "Add Nodes";
                    btnAddEdges.Enabled = true;
                    picMap.Cursor = Cursors.Arrow;
                    SaveNodes();
                    _formMode = FormMode.ReadOnly;
                    break;
                case FormMode.ReadOnly:
                    btnAddNodes.Text = "Exit";
                    btnAddEdges.Enabled = false;
                    picMap.Cursor = Cursors.Cross;
                    _formMode = FormMode.AddingNodes;
                    break;
            }
        }

        private void btnAddEdges_Click(object sender, EventArgs e)
        {
            if (AbortAddingEdgeAction())
                return;

            btnAddEdges.Text = _formMode == FormMode.AddingEdges ? "Add Edges" : "Exit";
            btnAddNodes.Enabled = _formMode == FormMode.AddingEdges;
            picMap.Cursor = _formMode == FormMode.AddingEdges ? Cursors.Arrow : Cursors.Hand;
            rbtSingleEdge.Visible = rbtDoubleEdge.Visible = (_formMode == FormMode.ReadOnly);

            if (_formMode == FormMode.AddingEdges)
            {
                _lastNodeMatched = null; // Release last node matched
            }

            rbtSingleEdge.Checked = (_addingEdgeType == AddingEdgeType.Single);
            rbtDoubleEdge.Checked = (_addingEdgeType == AddingEdgeType.Double);

            _formMode = _formMode == FormMode.ReadOnly ? FormMode.AddingEdges : FormMode.ReadOnly;
        }

        private void rbtSingleEdge_CheckedChanged(object sender, EventArgs e)
        {
            _addingEdgeType = AddingEdgeType.Single;
        }

        private void rbtDoubleEdge_CheckedChanged(object sender, EventArgs e)
        {
            _addingEdgeType = AddingEdgeType.Double;
        }
        #endregion

        #region DataIO
        private void LoadAndPrintNodes()
        {
            try
            {
                /// 1. Read file
                var path = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\Data\\");
                var nodesCsv = File.ReadAllText(Path.Combine(path, "Nodes.csv"));
                var nodeLines = nodesCsv.Split('\n');

                /// 2. Load and print nodes
                foreach (var nodeLine in nodeLines)
                {
                    try
                    {
                        var fields = nodeLine.Split(',');
                        var nodesCount = _mapService.GetNodesCount();
                        var newNode = new Coordinate(nodesCount + 1)
                        {
                            X = int.Parse(fields[0]),
                            Y = int.Parse(fields[1]),
                        };
                        AddNodeAndDrawMap(newNode);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SaveNodes()
        {
            try
            {
                /// 1. Get file and structure
                var path = Path.Combine(Environment.CurrentDirectory, "..\\..\\..\\Data\\");
                var strBuilder = new StringBuilder();
                var nodes = _mapService.GetAllNodes()?.ToList();

                if (nodes == null)
                    return;

                /// 2. Build content
                foreach (var node in nodes)
                {
                    strBuilder.AppendLine($"{node.X},{node.Y}");
                }

                /// 3. Write content
                File.WriteAllText(Path.Combine(path, "Nodes.csv"), strBuilder.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Drawing
        private void AddNodeAndDrawMap(Coordinate? coordinate = null)
        {
            Coordinate coordinateTarget = coordinate ?? GetCoordinate();
            _mapService.AddNode(coordinateTarget);
            _myGraphics!.DrawEllipse(new Pen(Color.Red, 2), coordinateTarget.X, coordinateTarget.Y, RADIUS, RADIUS);
        }

        private void DrawMapEdge()
        {
            var coordinate = GetCoordinate();
            var matchedNode = _mapService.GetNodeByValue(coordinate);

            if (matchedNode != null)
            {
                if (_lastNodeMatched != null)
                {
                    var edgeAdded = _mapService.AddEdge(_lastNodeMatched.ID, matchedNode.ID);
                    if (edgeAdded)
                    {
                        var color = _addingEdgeType == AddingEdgeType.Single ? Color.Red : Color.Green;
                        _myGraphics!.DrawLine(new Pen(color, 2),
                            _lastNodeMatched!.X + ERROR_LINE_X1, _lastNodeMatched!.Y + ERROR_LINE_Y1,
                            matchedNode.X + ERROR_LINE_X2, matchedNode.Y + ERROR_LINE_Y2);
                    }
                }

                _lastNodeMatched = matchedNode;
            }
        }
        #endregion

        #region Utils
        private Coordinate GetCoordinate()
        {
            var mousePositionX = MousePosition.X;
            var mousePositionY = MousePosition.Y;
            var locationX = Location.X;
            var locationY = Location.Y;
            var mapLocationX = picMap.Location.X;
            var mapLocationY = picMap.Location.Y;

            var x = mousePositionX - locationX - mapLocationX - FIX_POSITION_X;
            var y = mousePositionY - locationY - mapLocationY - FIX_POSITION_Y;

            var nodesCount = _mapService.GetNodesCount();
            var coordinate = new Coordinate(nodesCount + 1) { X = x, Y = y };
            return coordinate;
        }

        private bool AbortAddingNodeAction()
        {
            return
                _formMode == FormMode.ReadOnly &&
                MessageBox.Show(
                    this,
                    "Esta opci�n no es reversible, �Est� seguro que desea agregar nodos?",
                    "Adding Nodes",
                    MessageBoxButtons.YesNo) == DialogResult.No;
        }

        private bool AbortAddingEdgeAction()
        {
            return
                _formMode == FormMode.ReadOnly &&
                MessageBox.Show(
                    this,
                    "Esta opci�n no es reversible, �Est� seguro que desea agregar aristas?",
                    "Adding Edges",
                    MessageBoxButtons.YesNo) == DialogResult.No;
        }
        #endregion
    }
}