using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Numbering
{
    public partial class StartNumber : Form
    {                      
        public StartNumber()
        {
            InitializeComponent();
        }
        
        private void TextBox1_Click(object sender, System.EventArgs e)
        {
            textBox1.Text = "";
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //allows backspace key
            if (e.KeyChar != '\b')
            {
                //allows just number keys
                e.Handled = !char.IsNumber(e.KeyChar);
            }
            if (e.KeyChar == 13)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            if (e.KeyChar == 27)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }            
        }
        
        public bool CheckStartNumber()
        {
            int trial =-1;

            try
            {
                trial=int.Parse(textBox1.Text.Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public int ReturnStartNumber()
        {
            return Convert.ToInt32(textBox1.Text.Trim());            
        }

        private void Form_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }
           
    }
}
