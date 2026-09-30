using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DVLD.QuestionBank
{
    public partial class frmShowQuestionInfo : Form
    {
        public frmShowQuestionInfo(int QuestionID)
        {
            InitializeComponent();
            ctrlQuestionInfo1.LoadQuestionInfo(QuestionID);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ctrlQuestionInfo1_Load(object sender, EventArgs e)
        {

        }
    }
}
