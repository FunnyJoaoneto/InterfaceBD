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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

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
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            verifySGBDConnection();
            LoadTeams();
            LoadPlayers();
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


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedTeam = listBox1.SelectedItem.ToString();
                int teamID = ExtractTeamID(selectedTeam); // A function to extract team ID from the selected item string

                LoadTeamStats(teamID);
                LoadTeamPlayers(teamID);
            }
        }

        private void LoadTeamStats(int teamID)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = "SELECT * FROM TeamStats WHERE Team_ID = @TeamID";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@TeamID", teamID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        textBox29.Text = reader["TeamName"].ToString();
                        textBox8.Text = DateTime.Parse(reader["Foundation_Date"].ToString()).ToString("yyyy-MM-dd");
                        textBox7.Text = $"{reader["CoachID"]} {reader["CoachName"]}";
                        textBox1.Text = reader["GamesWon"].ToString();
                        textBox2.Text = reader["GamesLost"].ToString();
                        textBox6.Text = reader["RoundsWon"].ToString();
                        textBox5.Text = reader["RoundsLost"].ToString();
                        textBox4.Text = reader["MatchesWon"].ToString();
                        textBox3.Text = reader["MatchesLost"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading team stats: " + ex.Message);
            }
        }

        private void LoadTeamPlayers(int teamID)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = "SELECT * FROM TeamPlayers WHERE Team_ID = @TeamID";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@TeamID", teamID);

                listBox3.Items.Clear();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string playerInfo = $"ID: {reader["PlayerID"]}, Nickname: {reader["Nickname"]}";
                        listBox3.Items.Add(playerInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading team players: " + ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select a team first.");
                return;
            }

            string newName = textBox29.Text;
            DateTime newFoundationDate;

            if (!DateTime.TryParse(textBox8.Text, out newFoundationDate))
            {
                MessageBox.Show("Please enter a valid date.");
                return;
            }

            string selectedTeam = listBox1.SelectedItem.ToString();
            int teamID = ExtractTeamID(selectedTeam); // Ensure this function exists to extract the team ID from the selected item

            UpdateTeamDetails(teamID, newName, newFoundationDate);
            textBox29.Enabled = false;
            textBox8.Enabled = false;
            label14.Visible = true;
            label15.Visible = true;
            groupBox1.Visible = true;
            groupBox2.Visible = true;
            button4.Visible = false;
        }

        private void UpdateTeamDetails(int teamID, string newName, DateTime newFoundationDate)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand("UpdateTeamDetails", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TeamID", teamID);
                    cmd.Parameters.AddWithValue("@NewName", newName);
                    cmd.Parameters.AddWithValue("@NewFoundationDate", newFoundationDate);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Team details updated successfully.");

                // Optionally, refresh the team list and details displayed
                LoadTeams();
                LoadTeamStats(teamID);
                LoadTeamPlayers(teamID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating team details: " + ex.Message);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            MessageBox.Show("asdasdasd");
            string teamNameFilter = textBox12.Text;
            FilterTeamsByName(teamNameFilter);
        }

        private void FilterTeamsByName(string teamName)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = "FilterTeamsByName";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TeamName", teamName);

                adapter = new SqlDataAdapter(cmd);
                dataSet = new DataSet();
                adapter.Fill(dataSet, "FilteredTeams");

                PopulateFilteredTeamsListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering teams: " + ex.Message);
            }
        }

        private void PopulateFilteredTeamsListBox()
        {
            listBox1.Items.Clear();

            foreach (DataRow row in dataSet.Tables["FilteredTeams"].Rows)
            {
                string teamInfo = $"{row["Nome"]} (ID: {row["Team_ID"]})";
                listBox1.Items.Add(teamInfo);
            }
        }

        private int ExtractTeamID(string selectedTeam)
        {
            var match = Regex.Match(selectedTeam, @"\(ID: (\d+)\)");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }




        //players




        private void LoadPlayers()
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = "SELECT * FROM ListPlayersView";

                adapter = new SqlDataAdapter(query, cn);
                dataSet = new DataSet();
                adapter.Fill(dataSet, "Players");

                PopulatePlayersListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void PopulatePlayersListBox()
        {
            listBox8.Items.Clear();

            foreach (DataRow row in dataSet.Tables["Players"].Rows)
            {
                string playerInfo = $"Name: {row["PlayerName"]} Nickname: {row["Nickname"]}";
                listBox8.Items.Add(playerInfo);
            }
        }

        private void AddPlayer(int ccNumber, string name, int age, string nickname, int teamID)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                SqlCommand cmd = new SqlCommand("AddPlayer", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CC_Number", ccNumber);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Age", age);
                cmd.Parameters.AddWithValue("@Nickname", nickname);
                cmd.Parameters.AddWithValue("@Team_ID", teamID);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Player added successfully!");
                LoadPlayers();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding player: " + ex.Message);
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            try
            {
                int ccNumber = int.Parse(textBox27.Text);
                string name = textBox31.Text;
                int age = int.Parse(textBox30.Text);
                string nickname = textBox26.Text;
                int teamID = int.Parse(textBox32.Text);

                AddPlayer(ccNumber, name, age, nickname, teamID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid input: " + ex.Message);
            }
        }


        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Remove a team!"); // To confirm the button click event is fired.
            groupBox2.Enabled = true;
            groupBox1.Enabled = false;
            DisableEverything();
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
            textBox11.Clear();
            AbleEverything();
            groupBox2.Enabled = false;
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

        private void TAlt_Click(object sender, EventArgs e)
        {
            textBox29.Enabled = true;
            textBox8.Enabled = true;
            label14.Visible = false;
            label15.Visible = false;
            groupBox1.Visible = false;
            groupBox2.Visible = false;
            button4.Visible = true;
        }

        private void label58_Click(object sender, EventArgs e)
        {

        }

        private void listBox7_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
