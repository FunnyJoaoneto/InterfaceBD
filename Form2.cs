using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ValoLeague
{
    public partial class Form2 : Form
    {
        private Form1 form1;
        public Form2(int id1, int id2, int mid, Form1 form, Boolean load)
        {
            InitializeComponent();
            this.form1 = form;
            if (load)
            {
                LoadStats();
            }
        }
        private void LoadStats()
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox50_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox23_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox16_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
            MessageBox.Show("Operation Cancelled");
            this.form1.Show();
        }

        private void End_Click(object sender, EventArgs e)
        {
            this.Close();
            MessageBox.Show("Match Added!");
            this.form1.Show();
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void Game3Show_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = false;
            label9.Visible = false;
            label11.Visible = false;
            groupBox2.Visible = false;
            groupBox3.Visible = true;
            label32.Visible = true;
            button1.Visible = true;
            Game3Show.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = true;
            label9.Visible = true;
            label11.Visible = true;
            groupBox2.Visible = true;
            groupBox3.Visible = false;
            label32.Visible = false;
            button1.Visible = false;
            Game3Show.Visible = true;
        }
    }
}
