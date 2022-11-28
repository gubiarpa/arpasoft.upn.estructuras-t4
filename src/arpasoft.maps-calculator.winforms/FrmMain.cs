using arpasoft.maps_calculator.winforms.Utils;

namespace arpasoft.maps_calculator.winforms
{
    public partial class FrmMain : Form
    {
        private const int FIX_POSITION_X = 16;
        private const int FIX_POSITION_Y = 44;

        #region Form-Mode
        private Graphics? _myGraphics;
        private FormMode _formMode = FormMode.ReadOnly;
        #endregion

        #region Constructor
        public FrmMain()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        private void FrmMain_Load(object sender, EventArgs e)
        {
            _myGraphics = picMap.CreateGraphics();
        }

        private void picMap_Click(object sender, EventArgs e)
        {
            if (_formMode == FormMode.ReadOnly)
                return;

            DrawMapNode();
        }

        private void btnAddNodes_Click(object sender, EventArgs e)
        {
            btnAddNodes.Text = _formMode == FormMode.AddingNodes ? "Add Nodes" : "Exit";
            btnAddEdges.Enabled = _formMode == FormMode.AddingNodes;
            picMap.Cursor = _formMode == FormMode.AddingNodes ? Cursors.Arrow : Cursors.Cross;

            _formMode =  _formMode == FormMode.ReadOnly ? FormMode.AddingNodes : FormMode.ReadOnly;
        }

        private void btnAddEdges_Click(object sender, EventArgs e)
        {
        }
        #endregion


        #region Drawing
        private void DrawMapNode()
        {
            var mousePositionX = MousePosition.X;
            var mousePositionY = MousePosition.Y;
            var locationX = Location.X;
            var locationY = Location.Y;
            var mapLocationX = picMap.Location.X;
            var mapLocationY = picMap.Location.Y;

            var x = mousePositionX - locationX - mapLocationX - FIX_POSITION_X;
            var y = mousePositionY - locationY - mapLocationY - FIX_POSITION_Y;

            _myGraphics!.DrawEllipse(new Pen(Color.Red, 2), x, y, 10, 10);
        }
        #endregion

        #region Utils
        private bool ContinueAddingNodeAction()
        {
            return
                _formMode == FormMode.ReadOnly &&
                MessageBox.Show(
                    this,
                    "Esta opci�n no es reversible, �Est� seguro que desea agregar nodos?",
                    "Adding Nodes",
                    MessageBoxButtons.YesNo) == DialogResult.No;
        }

        private bool ContinueAddingEdgeAction()
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