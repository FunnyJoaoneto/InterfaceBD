using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ValoLeague
{
    public partial class Form1 : Form
    {
        private SqlConnection cn;
        private SqlDataAdapter adapter;
        private DataSet dataSet;

        public Form1()
        {
            InitializeComponent();
            verifySGBDConnection();
            LoadTeams();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private SqlConnection getSGBDConnection()
        {
            return new SqlConnection("data source = tcp:mednat.ieeta.pt\\SQLSERVER, 8101; Initial Catalog = p8g7; uid = p8g7; password = rumoao20.");
        }

        private bool verifySGBDConnection()
        {
            if (cn == null)
                cn = getSGBDConnection();

            if (cn.State != ConnectionState.Open)
                cn.Open();

            return cn.State == ConnectionState.Open;
        }

        private void LoadTeams()
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = "SELECT * FROM ListTeamsView";

                adapter = new SqlDataAdapter(query, cn);
                dataSet = new DataSet();
                adapter.Fill(dataSet, "Teams");

                PopulateListBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void PopulateListBoxes()
        {
            listBox1.Items.Clear();

            foreach (DataRow row in dataSet.Tables["Teams"].Rows)
            {
                string teamInfo = $"{row["Nome"]} (ID: {row["Team_ID"]})";
                listBox1.Items.Add(teamInfo);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Add a team!"); // To confirm the button click event is fired.
            groupBox1.Enabled = true;
            groupBox2.Enabled = false;
            DisableEverything();
        }

        private void DisableEverything()
        {
            listBox1.Enabled = false;
            listBox3.Enabled = false;
            textBox29.Enabled = false;
            TRem.Enabled = false;
            TAlt.Enabled = false;
            TAdd.Enabled = false;
            textBox12.Enabled = false;
            button11.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
            textBox5.Enabled = false;
            textBox6.Enabled = false;
            textBox7.Enabled = false;
            textBox8.Enabled = false;
        }

        private void AbleEverything()
        {
            listBox1.Enabled = true;
            TRem.Enabled = true;
            TAlt.Enabled = true;
            button11.Enabled = true;
            TAdd.Enabled = true;
            textBox12.Enabled = true;
        }




        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void label23_Click(object sender, EventArgs e)
        {

        }

        private void listBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void label29_Click(object sender, EventArgs e)
        {

        }

        private void label34_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void label40_Click(object sender, EventArgs e)
        {

        }

        private void label41_Click(object sender, EventArgs e)
        {

        }

        private void label43_Click(object sender, EventArgs e)
        {

        }

        private void label46_Click(object sender, EventArgs e)
        {

        }

        private void label33_Click(object sender, EventArgs e)
        {

        }

        private void listBox6_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button22_Click(object sender, EventArgs e)
        {

        }

        private void label36_Click(object sender, EventArgs e)
        {

        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label57_Click(object sender, EventArgs e)
        {

        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void label64_Click(object sender, EventArgs e)
        {

        }

        private void groupBox8_Enter(object sender, EventArgs e)
        {

        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            Form2 form2 = new Form2("lll", 1, 2, 3);
            form2.Show();
            this.Hide();
        }

        private void label18_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string teamName = textBox9.Text;
            DateTime foundationDate;

            if (string.IsNullOrEmpty(teamName))
            {
                MessageBox.Show("Please enter a team name.");
                return;
            }

            if (!DateTime.TryParse(textBox10.Text, out foundationDate))
            {
                MessageBox.Show("Please enter a valid foundation date.");
                return;
            }

            if (foundationDate <= new DateTime(2020, 6, 2))
            {
                MessageBox.Show("Foundation date must be greater than June 2, 2020.");
                return;
            }

            try
            {
                MessageBox.Show("Attempting to add team..."); // To confirm we're proceeding with the team addition.

                if (!verifySGBDConnection())
                    return;

                // Use the established connection to execute the stored procedure
                using (SqlCommand cmd = new SqlCommand("AddTeam", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Nome", teamName);
                    cmd.Parameters.AddWithValue("@Foundation_Date", foundationDate);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Team added successfully!");

                // Reload the teams to update the ListBox
                LoadTeams();
                textBox9.Clear();
                textBox10.Clear();
                AbleEverything();
                groupBox1.Enabled = false;

            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error adding team: " + ex.Message);
            }

        }

        private void TAddCan_Click(object sender, EventArgs e)
        {
            textBox9.Clear();
            textBox10.Clear();
            AbleEverything();
            groupBox1.Enabled = false;
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }
    }
}
