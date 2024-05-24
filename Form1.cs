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
                string query = @"
                    SELECT 
                        t.Team_ID, t.Nome, t.Foundation_Date, 
                        (SELECT COUNT(*) FROM VALO_PLAYER p WHERE p.Team_ID = t.Team_ID) AS PlayerCount,
                        (SELECT COUNT(*) FROM VALO_COACH c WHERE c.Team_ID = t.Team_ID) AS CoachCount
                    FROM VALO_TEAM t";

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
            listBox2.Items.Clear();

            foreach (DataRow row in dataSet.Tables["Teams"].Rows)
            {
                string teamInfo = $"{row["Nome"]} (ID: {row["Team_ID"]})";
                int playerCount = (int)row["PlayerCount"];
                int coachCount = (int)row["CoachCount"];

                if (playerCount >= 5 && coachCount > 0)
                {
                    listBox1.Items.Add(teamInfo);
                }
                else
                {
                    listBox2.Items.Add(teamInfo);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            groupBox1.Enabled = true;
            groupBox2.Enabled = false;
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
    }
}
