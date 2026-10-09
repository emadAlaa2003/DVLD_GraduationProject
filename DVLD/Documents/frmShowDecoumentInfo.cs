using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DVLD.Documents
{
    public partial class frmShowDecoumentInfo : Form
    {
        public frmShowDecoumentInfo(int DecoumentID)
        {
            InitializeComponent();
            ctrlDocumentInfo1.LoadDocumentInfo(DecoumentID);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
