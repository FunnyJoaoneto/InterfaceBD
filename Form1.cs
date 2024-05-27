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
                string playerInfo = $"{row["PlayerID"]}: Name: {row["PlayerName"]} Nickname: {row["Nickname"]}";
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
            MessageBox.Show("Operation Canceled");
            textBox11.Clear();
            AbleEverything();
            groupBox2.Enabled = false;
        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Team Removed!");
            AbleEverything();
            groupBox2.Enabled = false;
            //não esquecer de dar clear
            //textBox11.Clear();
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
            MessageBox.Show("Add Match");
            groupBox3.Enabled = true;
            DisableEverything4();
        }
        private void DisableEverything4()
        {
            button10.Enabled = false;
            button17.Enabled = false;
            button9.Enabled = false;
            comboBox1.Enabled = false;
        }
        private void AbleEverything4()
        {
            button10.Enabled = true;
            button17.Enabled = true;
            button9.Enabled = true;
            comboBox1.Enabled = true;
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
            MessageBox.Show("Alter a Player");
            groupBox6.Visible = false;
            groupBox5.Visible = false;
            label50.Visible = false;
            label49.Visible = false;
            button5.Visible = true;
            DisableEverything2();
            AbleAlterations();
        }
        private void AbleAlterations()
        {
            textBox28.Enabled = true;
            textBox15.Enabled = true;
            textBox16.Enabled = true;
            textBox17.Enabled = true;
            DisableEverything2();
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
            Form2 form2 = new Form2(1, 2, 3, this, false); //id team 1, id team 2, match id, (form, sempre "this"), se false não vai dar load ás stats)
            groupBox3.Enabled = false;
            AbleEverything4();
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
            MessageBox.Show("Operation Canceled");
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

        private void label66_Click(object sender, EventArgs e)
        {

        }

        private void button14_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Add a Player!");
            DisableEverything2();
            groupBox6.Enabled = true;
        }
        private void DisableEverything2()
        {
            textBox13.Enabled = false;
            button12.Enabled = false;
            button13.Enabled = false;
            button14.Enabled = false;
            button22.Enabled = false;
        }
        private void AbleEverything2()
        {
            textBox13.Enabled = true;
            button12.Enabled = true;
            button13.Enabled = true;
            button14.Enabled = true;
            button22.Enabled = true;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Operation Canceled");
            textBox26.Clear();
            textBox27.Clear();
            textBox31.Clear();
            textBox32.Clear();
            textBox30.Clear();
            AbleEverything2();
            groupBox6.Enabled = false;
        }

        private void button21_Click_1(object sender, EventArgs e)
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
            MessageBox.Show("Player added");
            AbleEverything2();
            groupBox6.Enabled = false;
            textBox26.Clear();
            textBox27.Clear();
            textBox31.Clear();
            textBox32.Clear();
            textBox30.Clear();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Remove a Player!");
            DisableEverything2();
            groupBox5.Enabled = true;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Operation Canceled");
            textBox25.Clear();
            AbleEverything2();
            groupBox5.Enabled = false;
        }

        private void button19_Click(object sender, EventArgs e)
        {
            if (textBox25.Text == null)
            {
                MessageBox.Show("Please write the player id you want to remove");
                return;
            }

            string playerIDString = textBox25.Text.ToString();
            if (int.TryParse(playerIDString, out int playerID))
            {
                RemovePlayerByID(playerID);
                // Refresh the player list
                LoadPlayers();
            }
            else
            {
                MessageBox.Show("Error parsing player ID.");
            }

            MessageBox.Show("Player removed");
            AbleEverything2();
            groupBox5.Enabled = false;
            textBox25.Clear();
        }

        private void checkedListBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            try
            {
                int ccNumber = int.Parse(textBox14.Text);
                string nickname = textBox28.Text;
                string name = textBox16.Text;
                int age = int.Parse(textBox15.Text);
                int teamID = int.Parse(textBox17.Text);

                UpdatePlayerDetails(ccNumber, nickname, name, age, teamID);
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid values for all fields.");
            }
            DisableAlterations();
            groupBox6.Visible = true;
            groupBox5.Visible = true;
            label50.Visible = true;
            label49.Visible = true;
            button5.Visible = false;
        }
        private void DisableAlterations()
        {
            MessageBox.Show("Player alterated");
            textBox28.Enabled = false;
            textBox15.Enabled = false;
            textBox16.Enabled = false;
            textBox17.Enabled = false;
            AbleEverything2();
        }

        private void listBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox8.SelectedItem == null)
                return;

            string selectedPlayer = listBox8.SelectedItem.ToString();
            // The format is now "{PlayerID}: Name: {PlayerName} Nickname: {Nickname}"
            string playerIDString = selectedPlayer.Split(':')[0].Trim();
            if (int.TryParse(playerIDString, out int ccNumber))
            {
                LoadPlayerStats(ccNumber);
            }
            else
            {
                MessageBox.Show("Error parsing player ID.");
            }
        }
        private void LoadPlayerStats(int ccNumber)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                // Load Player Global Stats
                string query = $"SELECT * FROM PlayerGlobalStats WHERE CC_Number = {ccNumber}";
                adapter = new SqlDataAdapter(query, cn);
                DataSet playerDataSet = new DataSet();
                adapter.Fill(playerDataSet, "PlayerStats");

                DataRow row = playerDataSet.Tables["PlayerStats"].Rows[0];

                textBox14.Text = row["CC_Number"].ToString();
                textBox16.Text = row["Name"].ToString();
                textBox15.Text = row["Age"].ToString();
                textBox28.Text = row["Nickname"].ToString();
                textBox17.Text = row["Team_ID"].ToString();
                textBox21.Text = row["TotalKills"].ToString();
                textBox20.Text = row["TotalDeaths"].ToString();
                textBox19.Text = row["TotalAssists"].ToString();
                textBox18.Text = row["AverageAVS"].ToString();
                textBox22.Text = row["AverageRating"].ToString();
                textBox23.Text = row["TotalFirstKills"].ToString();

                // Load Player Roles
                query = $"SELECT ROLE_Role_Name FROM PlayerRoles WHERE PERSON_CC_Number = {ccNumber}";
                adapter = new SqlDataAdapter(query, cn);
                playerDataSet = new DataSet();
                adapter.Fill(playerDataSet, "PlayerRoles");

                listBox2.Items.Clear();
                foreach (DataRow roleRow in playerDataSet.Tables["PlayerRoles"].Rows)
                {
                    listBox2.Items.Add(roleRow["ROLE_Role_Name"].ToString());
                }

                // Load Player Agents
                query = $"SELECT AGENT_Agent_Name, GamesPlayed FROM PlayerAgents WHERE PLAYER_CC_Number = {ccNumber}";
                adapter = new SqlDataAdapter(query, cn);
                playerDataSet = new DataSet();
                adapter.Fill(playerDataSet, "PlayerAgents");

                listBox7.Items.Clear();
                foreach (DataRow agentRow in playerDataSet.Tables["PlayerAgents"].Rows)
                {
                    string agentInfo = $"{agentRow["AGENT_Agent_Name"]}: {agentRow["GamesPlayed"]}";
                    listBox7.Items.Add(agentInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading player stats: " + ex.Message);
            }
        }

        private void button11_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("Filtering teams by name");
            string teamNamefilter = textBox12.Text;
            FilterTeamsByName(teamNamefilter);
        }

        private void button31_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Add Coach!");
            groupBox9.Enabled = true;
            DisableEverything3();
        }
        private void DisableEverything3()
        {
            button30.Enabled = false;
            button31.Enabled = false;
            button25.Enabled = false;
            textBox50.Enabled = false;
            button32.Enabled = false;
        }

        private void button28_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Operation Canceled");
            groupBox9.Enabled = false;
            AbleEverything3();
            textBox35.Clear();
            textBox36.Clear();
            textBox38.Clear();
            textBox39.Clear();
        }
        private void AbleEverything3()
        {
            button30.Enabled = true;
            button31.Enabled = true;
            button25.Enabled = true;
            textBox50.Enabled = true;
            button32.Enabled = true;
        }

        private void button29_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Coach Added");
            groupBox9.Enabled = false;
            AbleEverything3();
            //nao esquecer de dar clear depois
            //textBox35.Clear();
            //textBox36.Clear();
            //textBox38.Clear();
            //textBox39.Clear();
        }

        private void button30_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Remove Coach!");
            groupBox8.Enabled = true;
            DisableEverything3();
        }

        private void button26_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Operation Canceled");
            groupBox8.Enabled = false;
            AbleEverything3();
            textBox34.Clear();
        }

        private void button27_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Coach Removed");
            groupBox8.Enabled = false;
            AbleEverything3();
            //nao esquecer de dar clear depois
            //textBox34.Clear();
        }

        private void button25_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Alter Coach");
            groupBox8.Visible = false;
            groupBox9.Visible = false;
            label65.Visible = false;
            label64.Visible = false;
            button6.Visible = true;
            DisableEverything3();
            AbleAlterationsCoach();
        }

        private void AbleAlterationsCoach()
        {
            textBox40.Enabled = true;
            textBox41.Enabled = true;
            textBox42.Enabled = true;
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("Coach Altered");
            groupBox8.Visible = true;
            groupBox9.Visible = true;
            label65.Visible = true;
            label64.Visible = true;
            button6.Visible = false;
            AbleEverything3();
            DisableAlterationsCoach();
        }

        private void DisableAlterationsCoach()
        {
            textBox40.Enabled = false;
            textBox41.Enabled = false;
            textBox42.Enabled = false;
        }

        private void label73_Click(object sender, EventArgs e)
        {

        }

        private void UpdatePlayerDetails(int ccNumber, string nickname, string name, int age, int teamID)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand("UpdatePlayerDetails", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CC_Number", ccNumber);
                    cmd.Parameters.AddWithValue("@Nickname", nickname);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Age", age);
                    cmd.Parameters.AddWithValue("@TeamID", teamID);

                    // Ensure the connection is open before executing the command
                    if (cn.State != ConnectionState.Open)
                    {
                        cn.Open();
                    }

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Player details updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating player details: " + ex.Message);
            }
            finally
            {
                // Ensure the connection is closed even if an error occurs
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
                LoadPlayers();
            }
        }
        private void FilterPlayersByName(string playerName)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand("FilterPlayersByName", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PlayerName", string.IsNullOrEmpty(playerName) ? DBNull.Value : (object)playerName);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet, "Players");

                        PopulatePlayersListBox(dataSet);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching for player: " + ex.Message);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
            }
        }
        private void PopulatePlayersListBox(DataSet dataSet)
        {
            listBox8.Items.Clear();

            foreach (DataRow row in dataSet.Tables["Players"].Rows)
            {
                string playerInfo = $"{row["PlayerID"]}: Name: {row["PlayerName"]} Nickname: {row["Nickname"]}";
                listBox8.Items.Add(playerInfo);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string playerName = textBox13.Text;

            // Call the search function, it will handle empty strings appropriately
            FilterPlayersByName(playerName);
        }
        private void RemovePlayerByID(int playerID)
        {
            try
            {
                if (!verifySGBDConnection())
                    return;

                using (SqlCommand cmd = new SqlCommand("RemovePlayerByID", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PlayerID", playerID);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Player removed successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error removing player: " + ex.Message);
            }
            finally
            {
                if (cn != null && cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {

        }

        private void button17_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Remove a match");
            groupBox7.Enabled = true;
            DisableEverything4();
        }

        private void button23_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Operation Canceled!");
            textBox24.Clear();
            groupBox7.Enabled=false;
            AbleEverything4();
        }

        private void button24_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Match removed!");
            groupBox7.Enabled = false;
            AbleEverything4();
            //nao esquecer dar clear
            //textBox24.Clear();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Alter Match!");
            groupBox10.Enabled = true;
            DisableEverything4();

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Form2 form2 = new Form2(1, 2, 3, this, true); //id team 1, id team 2, match id, (form, sempre "this"), se true vai dar load ás stats)
            groupBox10.Enabled = false;
            AbleEverything4();
            form2.Show();
            this.Hide();
        }
    }
}
