using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace minesweeper_ui
{
    public partial class CustomInputs : Form
    {
        public CustomInputs()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int dim1 = Convert.ToInt32(textBox1.Text);
            int dim2 = Convert.ToInt32(textBox2.Text);
            int mines = Convert.ToInt32(textBox3.Text);
            Global.GlobalInts = [dim1, dim2, mines]; //dimensions and mines to global array, which other programs pull from
            this.Close();
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }
    }
}
