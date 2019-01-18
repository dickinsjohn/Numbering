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
    public partial class NumberAndAlignment : Form
    {
        public NumberAndAlignment()
        {
            InitializeComponent();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void textBox2_Click(object sender, EventArgs e)
        {
            this.textBox2.Text = "";
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
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
            if (e.KeyChar != 27)
                return;
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = !char.IsNumber(e.KeyChar);
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
            int num1 = -1;
            float num2 = -1f;
            try
            {
                num1 = int.Parse(this.textBox1.Text.Trim());
                num2 = float.Parse(this.textBox2.Text.Trim());
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

        public float ReturnTolerance()
        {
            return float.Parse(this.textBox2.Text.Trim());
        }


        private void NumberAlignment_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }

        public int ReturnNumberingChoice()
        {
            if(radioButton1.Checked==true)
            {
                return 1;
            }
            else if(radioButton2.Checked==true)
            {
                return 2;
            }
            return -1;
        }

        private void button1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
