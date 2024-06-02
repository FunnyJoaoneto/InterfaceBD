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
using System.Security.Cryptography;
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
        private bool tabsLocked;
        private String UserRole;

        public Form1()
        {
            InitializeComponent();
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            listBox4.SelectedIndexChanged += new EventHandler(listBox4_SelectedIndexChanged);
            listBox5.SelectedIndexChanged += new EventHandler(listBox5_SelectedIndexChanged);
            verifySGBDConnection();
            LoadTeams();
            LoadPlayers();
            LoadCoaches();
            LoadMatches();

            comboBox1.Items.Add("Admin");
            comboBox1.Items.Add("User");
            textBox51.PasswordChar = '*';

            tabsLocked = true;
            tabControl1.Selecting += new TabControlCancelEventHandler(tabControl1_Selecting);
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






        //CODE: Login






        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            // Check if tabs are locked
            if (tabsLocked)
            {
                e.Cancel = true; // Cancel the tab change
                MessageBox.Show("Please Log in");
            }
        }

        private void button16_Click_1(object sender, EventArgs e)
        {
            string username = comboBox1.Text;
            string password = textBox51.Text;

            if (Login(username, password))
            {
                MessageBox.Show("Login successful!");
                tabsLocked = false;
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        private bool Login(string username, string password)
        {
            string hashedPassword = HashingHelper.HashPassword(password);

            using (SqlConnection conn = new SqlConnection(getSGBDConnection().ConnectionString))
            {
                conn.Open();
                string query = "SELECT role FROM VALO_LOGIN WHERE username = @username AND passwordhash = @passwordhash";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@passwordhash", hashedPassword);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        UserRole = result.ToString();
                        tabsLocked = false; // Unlock tabs
                        tabControl1.Enabled = true; // Ensure the TabControl is enabled

                        if (UserRole == "User")
                        {
                            HideAdminButtons(); // Call the method to hide admin buttons
                        }
                        else
                        {
                            ShowAdminButtons();
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private void HideAdminButtons()
        {
            // Placeholder code to hide buttons for User role
            // Replace 'button1', 'button2', etc. with actual button names
            TRem.Visible = false;
            TAdd.Visible = false;
            TAlt.Visible = false;
            button13.Visible = false;
            button14.Visible = false;
            button22.Visible = false;
            button30.Visible = false;
            button25.Visible = false;
            button31.Visible = false;
            button17.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            groupBox1.Visible = false;
            groupBox2.Visible = false;
            label14.Visible = false;
            label15.Visible = false;
            label50.Visible = false;
            label49.Visible = false;
            groupBox5.Visible = false;
            groupBox6.Visible = false;
            label65.Visible = false;
            label64.Visible = false;
            groupBox8.Visible = false;
            groupBox9.Visible = false;
            label29.Visible = false;
            label60.Visible = false;
            label26.Visible = false;
            groupBox3.Visible = false;
            groupBox7.Visible = false;
            groupBox10.Visible = false;
            // Add more buttons as necessary
        }
        private void ShowAdminButtons()
        {
            // Placeholder code to hide buttons for User role
            // Replace 'button1', 'button2', etc. with actual button names
            TRem.Visible = true;
            TAdd.Visible = true;
            TAlt.Visible = true;
            button13.Visible = true;
            button14.Visible = true;
            button22.Visible = true;
            button30.Visible = true;
            button25.Visible = true;
            button31.Visible = true;
            button17.Visible = true;
            button9.Visible = true;
            button10.Visible = true;
            groupBox1.Visible = true;
            groupBox2.Visible = true;
            label14.Visible = true;
            label15.Visible = true;
            label50.Visible = true;
            label49.Visible = true;
            groupBox5.Visible = true;
            groupBox6.Visible = true;
            label65.Visible = true;
            label64.Visible = true;
            groupBox8.Visible = true;
            groupBox9.Visible = true;
            label29.Visible = true;
            label60.Visible = true;
            label26.Visible = true;
            groupBox3.Visible = true;
            groupBox7.Visible = true;
            groupBox10.Visible = true;
            // Add more buttons as necessary
        }



        public static class HashingHelper
        {
            public static string HashPassword(string password)
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                    StringBuilder builder = new StringBuilder();
                    foreach (byte b in bytes)
                    {
                        builder.Append(b.ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
        }





        //CODE: TEAMS





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

        private void button11_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("Filtering teams by name");
            string teamNamefilter = textBox12.Text;
            FilterTeamsByName(teamNamefilter);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Remove a team!"); // To confirm the button click event is fired.
            groupBox2.Enabled = true;
            groupBox1.Enabled = false;
            DisableEverything();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Add a team!"); // To confirm the button click event is fired.
            groupBox1.Enabled = true;
            groupBox2.Enabled = false;
            DisableEverything();
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
        private void RemoveTeam(int teamID)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand("RemoveTeam", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TeamID", teamID);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Team removed successfully.");
                }

                // Recalculate HAS table
                RecalculateHAS();

                // Refresh the UI to reflect changes
                LoadTeams();
                LoadPlayers();
                LoadCoaches();
                LoadMatches();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error removing team: " + ex.Message);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
            }
        }
        private void RecalculateHAS()
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("RecalculateHAS", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("HAS table recalculated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error recalculating HAS table: " + ex.Message);
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

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (int.TryParse(textBox11.Text, out int teamID))
                {
                    var result = MessageBox.Show("Are you sure you want to remove this team? This action cannot be undone.", "Confirm Removal", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        RemoveTeam(teamID);
                    }
                    MessageBox.Show("Team Removed!");
                }
                else
                {
                    MessageBox.Show("Please enter a valid team ID.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            AbleEverything();
            groupBox2.Enabled = false;
            textBox11.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Operation Canceled");
            textBox11.Clear();
            AbleEverything();
            groupBox2.Enabled = false;
        }




        //CODE: PLAYERS




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

        private void AddPlayer(int ccNumber, string name, int age, string nickname, int? teamID)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand("AddPlayer", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CC_Number", ccNumber);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Age", age);
                    cmd.Parameters.AddWithValue("@Nickname", nickname);
                    cmd.Parameters.AddWithValue("@Team_ID", teamID.HasValue ? (object)teamID.Value : DBNull.Value);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Player added successfully!");
                    LoadPlayers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding player: " + ex.Message);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
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

        private void label16_Click(object sender, EventArgs e)
        {

        }


        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void label23_Click(object sender, EventArgs e)
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


        private void label18_Click(object sender, EventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

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
                int? teamID = string.IsNullOrEmpty(textBox32.Text) ? (int?)null : int.Parse(textBox32.Text);

                AddPlayer(ccNumber, name, age, nickname, teamID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid input: " + ex.Message);
            }
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
                int? teamID = string.IsNullOrEmpty(textBox17.Text) ? (int?)null : int.Parse(textBox17.Text);

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

            DataSet playerDataSet = new DataSet();

            try
            {
                // Load basic player information
                string basicInfoQuery = $"SELECT * FROM PlayerBasicInfo WHERE CC_Number = {ccNumber}";
                adapter = new SqlDataAdapter(basicInfoQuery, cn);
                adapter.Fill(playerDataSet, "PlayerBasicInfo");

                if (playerDataSet.Tables["PlayerBasicInfo"].Rows.Count > 0)
                {
                    DataRow row = playerDataSet.Tables["PlayerBasicInfo"].Rows[0];

                    textBox14.Text = row["CC_Number"].ToString();
                    textBox16.Text = row["Name"].ToString();
                    textBox15.Text = row["Age"].ToString();
                    textBox28.Text = row["Nickname"].ToString();
                    textBox17.Text = row["Team_ID"].ToString();
                    textBox21.Text = "";
                    textBox20.Text = "";
                    textBox19.Text = "";
                    textBox18.Text = "";
                    textBox22.Text = "";
                    textBox23.Text = "";
                }
                else
                {
                    MessageBox.Show("Basic player information not found.");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading basic player information: " + ex.Message);
                return;
            }

            try
            {
                // Load Player Global Stats
                string statsQuery = $"SELECT * FROM PlayerGlobalStats WHERE CC_Number = {ccNumber}";
                adapter = new SqlDataAdapter(statsQuery, cn);
                adapter.Fill(playerDataSet, "PlayerStats");

                if (playerDataSet.Tables["PlayerStats"].Rows.Count > 0)
                {
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
                }
                else
                {
                    MessageBox.Show("This player has no games");
                }

                // Load Player Roles
                string rolesQuery = $"SELECT ROLE_Role_Name FROM PlayerRoles WHERE PERSON_CC_Number = {ccNumber}";
                adapter = new SqlDataAdapter(rolesQuery, cn);
                playerDataSet = new DataSet();
                adapter.Fill(playerDataSet, "PlayerRoles");

                listBox2.Items.Clear();
                foreach (DataRow roleRow in playerDataSet.Tables["PlayerRoles"].Rows)
                {
                    listBox2.Items.Add(roleRow["ROLE_Role_Name"].ToString());
                }

                // Load Player Agents
                string agentsQuery = $"SELECT AGENT_Agent_Name, GamesPlayed FROM PlayerAgents WHERE PLAYER_CC_Number = {ccNumber}";
                adapter = new SqlDataAdapter(agentsQuery, cn);
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

        private void label73_Click(object sender, EventArgs e)
        {

        }

        private void UpdatePlayerDetails(int ccNumber, string nickname, string name, int age, int? teamID)
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
                    cmd.Parameters.AddWithValue("@TeamID", teamID.HasValue ? (object)teamID.Value : DBNull.Value);

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

        private void FilterPlayersByTeamID(int? teamID = null)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.FilterPlayersByTeamID(@TeamID)", cn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@TeamID", teamID.HasValue ? (object)teamID.Value : DBNull.Value);

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
                MessageBox.Show("Error loading players: " + ex.Message);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox48.Text, out int teamID))
            {
                FilterPlayersByTeamID(teamID);
            }
            else
            {
                FilterPlayersByTeamID(); // Load all players if no valid team ID is provided
            }
        }

        private void button3_Click_2(object sender, EventArgs e)
        {
            if (int.TryParse(textBox48.Text, out int teamID))
            {
                FilterPlayersByTeamID(teamID);
            }
            else
            {
                FilterPlayersByTeamID(); // Load all players if no valid team ID is provided
            }
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

        private void LoadUnemployedPlayers()
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = "SELECT * FROM dbo.ListUnemployedPlayers()";
                adapter = new SqlDataAdapter(query, cn);
                dataSet = new DataSet();
                adapter.Fill(dataSet, "Players");

                PopulatePlayersListBox(dataSet);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading unemployed players: " + ex.Message);
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            LoadUnemployedPlayers();
        }






        //CODE : COACH






        private void LoadCoaches()
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = "SELECT * FROM ListCoachesView";
                adapter = new SqlDataAdapter(query, cn);
                dataSet = new DataSet();
                adapter.Fill(dataSet, "Coaches");

                PopulateCoachesListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }
        private void PopulateCoachesListBox()
        {
            listBox9.Items.Clear();

            foreach (DataRow row in dataSet.Tables["Coaches"].Rows)
            {
                string coachInfo = $"ID: {row["CoachID"]} Name: {row["CoachName"]} Team ID: {row["Team_ID"]}";
                listBox9.Items.Add(coachInfo);
            }
        }

        private void listBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox9.SelectedItem == null)
                return;

            string selectedCoach = listBox9.SelectedItem.ToString();
            // The format is now "ID: {CoachID} Name: {CoachName} Team ID: {Team_ID}"
            string coachIDString = selectedCoach.Split(':')[1].Split(' ')[1].Trim();
            if (int.TryParse(coachIDString, out int coachID))
            {
                DisplayCoachDetails(coachID);
            }
            else
            {
                MessageBox.Show("Error parsing coach ID.");
            }
        }

        private void DisplayCoachDetails(int coachID)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = $"SELECT * FROM ListCoachesView WHERE CoachID = {coachID}";
                adapter = new SqlDataAdapter(query, cn);
                DataSet coachDataSet = new DataSet();
                adapter.Fill(coachDataSet, "CoachDetails");

                if (coachDataSet.Tables["CoachDetails"].Rows.Count > 0)
                {
                    DataRow row = coachDataSet.Tables["CoachDetails"].Rows[0];

                    textBox43.Text = row["CoachID"].ToString();
                    textBox41.Text = row["CoachName"].ToString();
                    textBox42.Text = row["Age"].ToString();
                    textBox40.Text = row["Team_ID"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading coach details: " + ex.Message);
            }
        }

        private void AddCoach(int ccNumber, string name, int age, int? teamID)
        {
            try
            {
                using (SqlConnection cn = getSGBDConnection())
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand("AddCoach", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@CC_Number", ccNumber);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Age", age);
                        cmd.Parameters.AddWithValue("@TeamID", teamID.HasValue ? (object)teamID.Value : DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Coach added successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding coach: " + ex.Message);
            }
        }
        private void button29_Click(object sender, EventArgs e)
        {
            try
            {
                int ccNumber = int.Parse(textBox39.Text);
                string name = textBox36.Text;
                int age = int.Parse(textBox38.Text);
                int? teamID = string.IsNullOrEmpty(textBox35.Text) ? (int?)null : int.Parse(textBox35.Text);

                AddCoach(ccNumber, name, age, teamID);
                // Refresh the coach list
                LoadCoaches();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid values for all fields.");
            }
            groupBox9.Enabled = false;
            AbleEverything3();

            textBox35.Clear();
            textBox36.Clear();
            textBox38.Clear();
            textBox39.Clear();
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

        private void DisableAlterationsCoach()
        {
            textBox40.Enabled = false;
            textBox41.Enabled = false;
            textBox42.Enabled = false;
        }

        private void RemoveCoach(int ccNumber)
        {
            try
            {
                using (SqlConnection cn = getSGBDConnection())
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand("RemoveCoach", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CC_Number", ccNumber);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Coach removed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error removing coach: " + ex.Message);
            }
        }
        private void button27_Click(object sender, EventArgs e)
        {
            try
            {
                int ccNumber = int.Parse(textBox34.Text);
                RemoveCoach(ccNumber);
                // Refresh the coach list
                LoadCoaches();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid CC Number.");
            }
            MessageBox.Show("Coach Removed");
            groupBox8.Enabled = false;
            AbleEverything3();

            textBox34.Clear();
        }
        private void SearchCoachByName(string coachName)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand("SearchCoachByName", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CoachName", string.IsNullOrEmpty(coachName) ? DBNull.Value : (object)coachName);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet, "Coaches");

                        PopulateCoachesListBox(dataSet);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching for coach: " + ex.Message);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
            }
        }
        private void PopulateCoachesListBox(DataSet dataSet)
        {
            listBox9.Items.Clear();

            foreach (DataRow row in dataSet.Tables["Coaches"].Rows)
            {
                string coachInfo = $"ID: {row["CoachID"]} Name: {row["CoachName"]} Team ID: {row["Team_ID"]}";
                listBox9.Items.Add(coachInfo);
            }
        }

        private void FilterCoachesByTeamID(int? teamID = null)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.FilterCoachesByTeamID(@TeamID)", cn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@TeamID", teamID.HasValue ? (object)teamID.Value : DBNull.Value);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet, "Coaches");

                        PopulateCoachesListBox(dataSet);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading coaches: " + ex.Message);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                {
                    cn.Close();
                }
            }
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            if (int.TryParse(textBox49.Text, out int teamID))
            {
                FilterCoachesByTeamID(teamID);
            }
            else
            {
                FilterCoachesByTeamID(); // Load all coaches if no valid team ID is provided
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox49.Text, out int teamID))
            {
                FilterCoachesByTeamID(teamID);
            }
            else
            {
                FilterCoachesByTeamID(); // Load all coaches if no valid team ID is provided
            }
        }

        private void button32_Click(object sender, EventArgs e)
        {
            string coachName = textBox50.Text;

            // Call the search function, it will handle empty strings appropriately
            SearchCoachByName(coachName);
        }
        private void UpdateCoachDetails(int ccNumber, string name, int age, int? teamID)
        {
            try
            {
                using (SqlConnection cn = getSGBDConnection())
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand("UpdateCoachDetails", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@CC_Number", ccNumber);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Age", age);
                        cmd.Parameters.AddWithValue("@TeamID", teamID.HasValue ? (object)teamID.Value : DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Coach details updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating coach details: " + ex.Message);
            }
        }
        private void button6_Click_1(object sender, EventArgs e)
        {
            try
            {
                int ccNumber = int.Parse(textBox43.Text);
                int age = int.Parse(textBox42.Text);
                string name = textBox41.Text;
                int? teamID = string.IsNullOrEmpty(textBox40.Text) ? (int?)null : int.Parse(textBox40.Text);

                UpdateCoachDetails(ccNumber, name, age, teamID);
                // Refresh the coach list
                LoadCoaches();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid values for all fields.");
            }

            MessageBox.Show("Coach Altered");
            groupBox8.Visible = true;
            groupBox9.Visible = true;
            label65.Visible = true;
            label64.Visible = true;
            button6.Visible = false;
            AbleEverything3();
            DisableAlterationsCoach();

            textBox40.Clear();
            textBox41.Clear();
            textBox42.Clear();
            textBox43.Clear();
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }






        //CODE: MATCH





        public void LoadMatches()
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = "SELECT * FROM ListMatchesView";
                adapter = new SqlDataAdapter(query, cn);
                dataSet = new DataSet();
                adapter.Fill(dataSet, "Matches");

                PopulateMatchesListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading matches: " + ex.Message);
            }
        }

        private void LoadGames(int matchID)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                // Clear previous games data
                if (dataSet.Tables["Games"] != null)
                    dataSet.Tables["Games"].Clear();

                string query = $"SELECT * FROM ListGamesView WHERE MATCH_Match_ID = {matchID}";
                adapter = new SqlDataAdapter(query, cn);
                adapter.Fill(dataSet, "Games");

                PopulateGamesListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading games: " + ex.Message);
            }
        }

        private void PopulateGamesListBox()
        {
            listBox5.Items.Clear();

            foreach (DataRow row in dataSet.Tables["Games"].Rows)
            {
                string gameInfo = $"Match_ID: {row["MATCH_Match_ID"]} Game: {row["Game_ID"]} {row["Team1_Name"]} vs {row["Team2_Name"]} {row["Rounds_Won_Team_1"]} - {row["Rounds_Won_Team_2"]}";
                listBox5.Items.Add(gameInfo);
            }
        }

        private void PopulateMatchesListBox()
        {
            listBox4.Items.Clear();

            foreach (DataRow row in dataSet.Tables["Matches"].Rows)
            {
                string matchInfo = $"ID: {row["Match_ID"]}, {row["Team1_Name"]} vs {row["Team2_Name"]}";
                listBox4.Items.Add(matchInfo);
            }
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
            textBox44.Enabled = false;
            button2.Enabled = false;
        }
        private void AbleEverything4()
        {
            button10.Enabled = true;
            button17.Enabled = true;
            button9.Enabled = true;
            textBox44.Enabled = true;
            button2.Enabled = true;
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            if (int.TryParse(textBox45.Text, out int team1ID) && int.TryParse(textBox46.Text, out int team2ID))
            {
                int matchID = GetNextAvailableMatchID();
                MessageBox.Show(matchID.ToString());
                if (matchID == -1)
                {
                    MessageBox.Show("Error generating match ID.");
                    return;
                }

                Form2 form2 = new Form2(team1ID, team2ID, matchID, this, false);
                groupBox3.Enabled = false;
                AbleEverything4();
                form2.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Please enter valid team IDs.");
            }
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
            groupBox7.Enabled = false;
            AbleEverything4();
        }

        private void button24_Click(object sender, EventArgs e)
        {
            if ( textBox24.Text == null)
                return;

            string matchIDString = textBox24.Text.ToString();
            if (int.TryParse(matchIDString, out int matchID))
            {
                RemoveMatch(matchID);
                LoadMatches();
                MessageBox.Show("Match removed!");
            }
            else
            {
                MessageBox.Show("Error parsing match ID.");
            }
            groupBox7.Enabled = false;
            AbleEverything4();

            textBox24.Clear();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Alter Match!");
            groupBox10.Enabled = true;
            DisableEverything4();

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (int.TryParse(textBox47.Text, out int matchID))
            {
                Form2 form2 = new Form2(-1, -1, matchID, this, true);
                groupBox10.Enabled = false;
                AbleEverything4();
                form2.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Please enter a valid match ID.");
            }
        }

        private void label31_Click(object sender, EventArgs e)
        {

        }

        private void label32_Click(object sender, EventArgs e)
        {

        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox4.SelectedItem == null)
                return;

            string selectedMatch = listBox4.SelectedItem.ToString();
            string matchIDString = selectedMatch.Split(':')[1].Split(',')[0].Trim();
            if (int.TryParse(matchIDString, out int matchID))
            {
                LoadGames(matchID);
                string te = $"Games From Match (ID: {matchID})";
                label22.Text = te;
            }
            else
            {
                MessageBox.Show("Error parsing match ID.");
            }
        }
        private void LoadGameDetails(int matchID, int gameID)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = $"SELECT * FROM ListPlayerStatsView WHERE MATCH_Match_ID = {matchID} AND Game_ID = {gameID}";
                adapter = new SqlDataAdapter(query, cn);
                DataSet gameDataSet = new DataSet();
                adapter.Fill(gameDataSet, "GameDetails");

                if (gameDataSet.Tables["GameDetails"].Rows.Count > 0)
                {
                    DataRow row = gameDataSet.Tables["GameDetails"].Rows[0];

                    T1.Text = row["Team1_Name"].ToString();
                    T2.Text = row["Team2_Name"].ToString();
                    textBox33.Text = row["Rounds_Won_Team_1"].ToString();
                    textBox37.Text = row["Rounds_Won_Team_2"].ToString();
                    string te = $"LeaderBoard from (Match id: {matchID}) (Game id: {gameID}): ";
                    label25.Text = te;


                    // Load player stats in default order
                    LoadPlayerStats(matchID, gameID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading game details: " + ex.Message);
            }
        }
        private void LoadPlayerStats(int matchID, int gameID)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = $@"
    SELECT * 
    FROM PlayerStatsWithDefaultOrder
    WHERE MATCH_Match_ID = {matchID} AND Game_ID = {gameID}
    ORDER BY TeamOrder, AVS DESC";

                adapter = new SqlDataAdapter(query, cn);
                DataSet playerStatsDataSet = new DataSet();
                adapter.Fill(playerStatsDataSet, "PlayerStats");

                PopulatePlayerStatsListBox(playerStatsDataSet);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading player stats: " + ex.Message);
            }
        }
        private void PopulatePlayerStatsListBox(DataSet playerStatsDataSet)
        {
            listBox6.Items.Clear();

            var playerStats = playerStatsDataSet.Tables["PlayerStats"].AsEnumerable();
            string topic = "PlayerID  \tPlayerName\tTeamName\t (  K  /  D  /  A  )\tAVS\t Rating\t       FK\t Agent\t";
            listBox6.Items.Add(topic);

            foreach (var row in playerStats)
            {
                string playerID = row["Player_ID"].ToString().PadRight(10);
                string playerName = row["Player_Name"].ToString().PadRight(20);
                string teamName = row["Team_Name"].ToString().PadRight(15);
                string kills = row["Kills"].ToString().PadLeft(5);
                string deaths = row["Deaths"].ToString().PadLeft(5);
                string assists = row["Assists"].ToString().PadLeft(5);
                string avs = row["AVS"].ToString().PadLeft(6);
                string rating = row["Rating"].ToString().PadLeft(6);
                string firstKills = row["First_kills"].ToString().PadLeft(11);
                string agentName = row["AGENT_Agent_Name"].ToString().PadRight(15);

                string playerInfo = $"{playerID}\t{playerName}\t{teamName}\t({kills}/{deaths}/{assists})\t{avs}\t{rating}\t{firstKills}\t{agentName}";
                listBox6.Items.Add(playerInfo);
            }
        }
        private void listBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox5.SelectedItem == null)
                return;

            string selectedGame = listBox5.SelectedItem.ToString();
            // Match the pattern "Match_ID: 1 Game: 1 Team Alpha vs Team Kappa 13 - 11"
            string pattern = @"Match_ID: (\d+) Game: (\d+)";
            var match = System.Text.RegularExpressions.Regex.Match(selectedGame, pattern);

            if (match.Success && int.TryParse(match.Groups[1].Value, out int matchID) && int.TryParse(match.Groups[2].Value, out int gameID))
            {
                LoadGameDetails(matchID, gameID);
            }
            else
            {
                MessageBox.Show("Error parsing match or game ID.");
            }
        }
        private void LoadFilteredMatches(string teamName)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = "FilterMatchesByTeamName"; // Name of the stored procedure
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TeamName", teamName);

                adapter = new SqlDataAdapter(cmd);
                dataSet = new DataSet();
                adapter.Fill(dataSet, "Matches");

                PopulateMatchesListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading matches: " + ex.Message);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            string teamName = textBox44.Text.Trim();
            LoadFilteredMatches(teamName);
        }

        private void RemoveMatch(int matchID)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = "RemoveMatch"; // Name of the stored procedure
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MatchID", matchID);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Match removed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error removing match: " + ex.Message);
            }
        }
        private int GetNextAvailableMatchID()
        {
            if (!verifySGBDConnection())
                return -1;

            try
            {
                string query = "SELECT ISNULL(MAX(Match_ID), 0) + 1 FROM VALO_MATCH";
                SqlCommand cmd = new SqlCommand(query, cn);
                return (int)cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving next match ID: " + ex.Message);
                return -1;
            }
        }

        private void groupBox10_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

}
