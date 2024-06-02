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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ValoLeague
{
    public partial class Form2 : Form
    {
        private Form1 form1;
        private SqlConnection cn;
        private SqlDataAdapter adapter;
        private DataSet dataSet;
        private int team1ID;
        private int team2ID;
        private int matchID;
        private bool loadStats;
        public Form2(int id1, int id2, int mid, Form1 form, Boolean load)
        {
            InitializeComponent();
            this.form1 = form;
            this.team1ID = id1;
            this.team2ID = id2;
            this.matchID = mid;
            this.loadStats = load;
            System.Windows.Forms.TextBox[] playerTextBoxes = { P1, P2, P3, P4, P5, P6, P7, P8, P9, P10,
                                  P11, P12, P13, P14, P15, P16, P17, P18, P19, P20,
                                  P21, P22, P23, P24, P25, P26, P27, P28, P29, P30 };

            // Fill TextBoxes with player names
            for (int i = 0; i < playerTextBoxes.Length; i++)
            {
                playerTextBoxes[i].Enabled = false;
            }
            verifySGBDConnection();
            if (load)
            {
                getMatchDetails(mid);
            }
            else
            {
                LoadTeamDetails();
            }
            addRolesToComboBoxes();
            addMapsToComboBoxes();
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

        private void getMatchDetails(int matchID)
        {
            if (!verifySGBDConnection())
                return;

            DataSet matchDetailsDataSet = new DataSet();

            try
            {
                string query = $"SELECT * FROM dbo.getmatchdetails({matchID})";
                adapter = new SqlDataAdapter(query, cn);
                adapter.Fill(matchDetailsDataSet, "MatchDetails");

                if (matchDetailsDataSet.Tables["MatchDetails"].Rows.Count > 0)
                {
                    var matchDetails = matchDetailsDataSet.Tables["MatchDetails"].AsEnumerable();

                    var team1Details = matchDetails.First();
                    T1.Text = team1Details["Team_1_Name"].ToString();
                    T2.Text = team1Details["Team_2_Name"].ToString();

                    var games = matchDetails
                        .GroupBy(row => row.Field<int>("Game_ID"))
                        .ToList();

                    for (int i = 0; i < 3; i++) // Iterate through 3 possible games
                    {
                        var gameIndex = i + 1;

                        if (i < games.Count)
                        {
                            var game = games[i].First();

                            SetControlText($"M{gameIndex}", game["Map"].ToString());
                            SetControlText($"S{gameIndex}1", game["Rounds_Won_Team_1"].ToString());
                            SetControlText($"S{gameIndex}2", game["Rounds_Won_Team_2"].ToString());

                            for (int j = 0; j < 10; j++) // Iterate through 10 possible players per game
                            {
                                var playerIndex = (gameIndex - 1) * 10 + j + 1;

                                if (j < games[i].Count())
                                {
                                    var player = games[i].ElementAt(j);

                                    SetControlText($"P{playerIndex}", player["PLAYER_CC_Number"].ToString());
                                    SetControlText($"K{playerIndex}", player["Kills"].ToString());
                                    SetControlText($"D{playerIndex}", player["Deaths"].ToString());
                                    SetControlText($"A{playerIndex}", player["Assists"].ToString());
                                    SetControlText($"C{playerIndex}", player["AVS"].ToString());
                                    SetControlText($"F{playerIndex}", player["First_kills"].ToString());
                                    SetControlText($"E{playerIndex}", player["Rating"].ToString());
                                    SetComboBoxValue($"R{playerIndex}", player["ROLE_Role_Name"].ToString());
                                    SetComboBoxValue($"G{playerIndex}", player["AGENT_Agent_Name"].ToString());
                                }
                                else
                                {
                                    // Clear the controls if there are no more players for this game
                                    SetControlText($"P{playerIndex}", "");
                                    SetControlText($"K{playerIndex}", "");
                                    SetControlText($"D{playerIndex}", "");
                                    SetControlText($"A{playerIndex}", "");
                                    SetControlText($"C{playerIndex}", "");
                                    SetControlText($"F{playerIndex}", "");
                                    SetControlText($"E{playerIndex}", "");
                                    SetComboBoxValue($"R{playerIndex}", "");
                                    SetComboBoxValue($"G{playerIndex}", "");
                                }
                            }
                        }
                        else
                        {
                            // Clear the controls if there are no more games
                            SetControlText($"M{gameIndex}", "");
                            SetControlText($"S{gameIndex}1", "");
                            SetControlText($"S{gameIndex}2", "");

                            for (int j = 0; j < 10; j++)
                            {
                                var playerIndex = (gameIndex - 1) * 10 + j + 1;

                                SetControlText($"P{playerIndex}", "");
                                SetControlText($"K{playerIndex}", "");
                                SetControlText($"D{playerIndex}", "");
                                SetControlText($"A{playerIndex}", "");
                                SetControlText($"C{playerIndex}", "");
                                SetControlText($"F{playerIndex}", "");
                                SetControlText($"E{playerIndex}", "");
                                SetComboBoxValue($"R{playerIndex}", "");
                                SetComboBoxValue($"G{playerIndex}", "");
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No match details found.");
                }

                // Call getPlayersFromMatch to fill list boxes
                getPlayersFromMatch();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading match details: " + ex.Message);
            }
        }

        private void SetControlText(string controlName, string text)
        {
            var control = this.Controls.Find(controlName, true).FirstOrDefault();
            if (control != null)
            {
                control.Text = text;
            }
        }

        private void SetComboBoxValue(string controlName, string value)
        {
            var control = this.Controls.Find(controlName, true).FirstOrDefault() as System.Windows.Forms.ComboBox;
            if (control != null)
            {
                if (!control.Items.Contains(value))
                {
                    control.Items.Add(value);
                }
                control.SelectedItem = value;
            }
        }

        private void getPlayersFromMatch()
        {
            if (!verifySGBDConnection())
                return;

            listBox1.Items.Clear();
            listBox2.Items.Clear();

            try
            {
                // Define arrays for player ID textboxes
                string[] team1PlayerIds = { "P1", "P2", "P3", "P4", "P5" };
                string[] team2PlayerIds = { "P6", "P7", "P8", "P9", "P10" };

                // Fetch and add players from team 1
                foreach (string playerIdControl in team1PlayerIds)
                {
                    var playerIdTextBox = this.Controls.Find(playerIdControl, true).FirstOrDefault() as System.Windows.Forms.TextBox;
                    if (playerIdTextBox != null && int.TryParse(playerIdTextBox.Text, out int playerId))
                    {
                        var playerInfo = GetPlayerInfo(playerId);
                        if (playerInfo != null && !listBox1.Items.Contains(playerInfo))
                        {
                            listBox1.Items.Add(playerInfo);
                        }
                    }
                }

                // Fetch and add players from team 2
                foreach (string playerIdControl in team2PlayerIds)
                {
                    var playerIdTextBox = this.Controls.Find(playerIdControl, true).FirstOrDefault() as System.Windows.Forms.TextBox;
                    if (playerIdTextBox != null && int.TryParse(playerIdTextBox.Text, out int playerId))
                    {
                        var playerInfo = GetPlayerInfo(playerId);
                        if (playerInfo != null && !listBox2.Items.Contains(playerInfo))
                        {
                            listBox2.Items.Add(playerInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading player details: " + ex.Message);
            }
        }

        private string GetPlayerInfo(int playerId)
        {
            try
            {
                string query = $"SELECT Nickname FROM VALO_PLAYER WHERE PERSON_CC_Number = {playerId}";
                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    var nickname = cmd.ExecuteScalar()?.ToString();
                    if (!string.IsNullOrEmpty(nickname))
                    {
                        return $"ID: {playerId} Nickname: {nickname}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching player info: " + ex.Message);
            }
            return null;
        }

        private void LoadTeamDetails()
        {
            var team1Details = GetTeamDetails(team1ID);
            var team2Details = GetTeamDetails(team2ID);

            if (team1Details.TeamName != null && team2Details.TeamName != null)
            {
                // Pass team names to textboxes
                T1.Text = team1Details.TeamName;
                T2.Text = team2Details.TeamName;

                // Pass players to listboxes
                listBox1.Items.Clear();
                listBox1.Items.AddRange(team1Details.Players.ToArray());

                listBox2.Items.Clear();
                listBox2.Items.AddRange(team2Details.Players.ToArray());
            }
            else
            {
                MessageBox.Show("Error retrieving team details.");
            }
            var team1PlayersID = GetPlayersID(team1ID);
            var team2PlayersID = GetPlayersID(team2ID);

            System.Windows.Forms.TextBox[] playerst1 = { P1, P2, P3, P4, P5, P11, P12, P13, P14, P15, P21, P22, P23, P24, P25};
            System.Windows.Forms.TextBox[] playerst2 = { P6, P7, P8, P9, P10, P16, P17, P18, P19, P20, P26, P27, P28, P29, P30 };
            // Player stats for Game 1, Team 1
            for (int i = 1; i < playerst1.Length+1; i++)
            {
                if (team1PlayersID.Players != null)
                {
                    int id1 = i % 5;
                    if(id1 == 0)
                    {
                        id1 = 5;
                    }
                    if(team1PlayersID.Players.Count >= id1)
                    {
                        playerst1[i-1].Text = team1PlayersID.Players[id1-1];
                    }
                    else
                    {
                        playerst1[i-1].Text = "";
                    }
                }
                else
                {
                    playerst1[i-1].Text = "";
                }
            }
            for (int i = 1; i < playerst2.Length+1; i++)
            {
                if (team2PlayersID.Players != null)
                {
                    int id2 = i % 5;
                    if (id2 == 0)
                    {
                        id2 = 5;
                    }
                    if (team2PlayersID.Players.Count >= id2)
                    {
                        playerst2[i - 1].Text = team2PlayersID.Players[id2-1];
                    }
                    else
                    {
                        playerst2[i-1].Text = "";
                    }
                }
                else
                {
                    playerst2[i - 1].Text = "";
                }
            }
        }
        private (string TeamName, List<string> Players) GetTeamDetails(int teamID)
        {
            if (!verifySGBDConnection())
                return (null, null);

            try
            {
                string query = "GetTeamDetails"; // Name of the stored procedure
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TeamID", teamID);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);

                // Extract team name
                string teamName = dataSet.Tables[0].Rows[0]["Nome"].ToString();

                // Extract players
                List<string> players = new List<string>();
                foreach (DataRow row in dataSet.Tables[1].Rows)
                {
                    string playerInfo = $"ID:{row["PlayerID"]} NickName:{row["Nickname"]}";
                    players.Add(playerInfo);
                }

                return (teamName, players);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving team details: " + ex.Message);
                return (null, null);
            }
        }

        private (string TeamName, List<string> Players) GetPlayersID(int teamID)
        {
            if (!verifySGBDConnection())
                return (null, null);

            try
            {
                string query = "GetTeamDetails"; // Name of the stored procedure
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TeamID", teamID);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);

                // Extract team name
                string teamName = dataSet.Tables[0].Rows[0]["Nome"].ToString();

                // Extract players
                List<string> players = new List<string>();
                foreach (DataRow row in dataSet.Tables[1].Rows)
                {
                    string playerInfo = $"{row["PlayerID"]}";
                    players.Add(playerInfo);
                }

                return (teamName, players);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving team details: " + ex.Message);
                return (null, null);
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

        private void detectAndRemoveInvalidMatches()
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand("detectAndRemoveInvalidMatches", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cn.InfoMessage += (sender, e) =>
                    {
                        foreach (var info in e.Message.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            MessageBox.Show(info, "Invalid Match Removed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    };

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error detecting and removing invalid matches: " + ex.Message);
            }
        }

        private void End_Click(object sender, EventArgs e)
        {
            try
            {
                if (loadStats)
                {
                    UpdateMatch();
                    MessageBox.Show("Match Updated!");
                }
                else
                {
                    AddMatch();
                    MessageBox.Show("Match Added!");
                }

                detectAndRemoveInvalidMatches();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing match: " + ex.Message);
            }
            finally
            {
                this.Close();
                this.form1.Show();
                this.form1.LoadMatches();
            }
        }

        private int getNumGames(int matchid)
        {
            List<string> games = new List<string>();
            string query = "SELECT Game_ID FROM VALO_GAME WHERE MATCH_Match_ID = @MID";

            using (SqlCommand command = new SqlCommand(query, cn))
            {
                command.Parameters.AddWithValue("@MID", matchid);

                try
                {
                    if (cn.State == System.Data.ConnectionState.Closed)
                    {
                        cn.Open();
                    }

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        games.Add(reader["Game_ID"].ToString());
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., log the error)
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
            return games.Count();
        }

        private void UpdateMatch()
        {

            using (SqlConnection conn = getSGBDConnection())
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("UpdateMatch", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Match_ID", matchID);
                    cmd.ExecuteNonQuery();
                }

                UpdateGame(conn, 1, int.Parse(S11.Text), int.Parse(S12.Text), matchID, M1.Text);
                UpdateGame(conn, 2, int.Parse(S21.Text), int.Parse(S22.Text), matchID, M2.Text);

                int maxPlayers = 20;

                if(getNumGames(matchID) == 2 && !string.IsNullOrEmpty(M3.Text))
                {
                    AddGame(conn, 3, int.Parse(S31.Text), int.Parse(S32.Text), matchID, M3.Text);
                    maxPlayers = 30;
                    for (int i = 21; i <= maxPlayers; i++)
                    {
                        AddPlayerStats(conn, GetPlayerStatsParameters(i));
                    }
                }
                else if (getNumGames(matchID) == 3 && !string.IsNullOrEmpty(M3.Text))
                {
                    UpdateGame(conn, 3, int.Parse(S31.Text), int.Parse(S32.Text), matchID, M3.Text);
                    maxPlayers = 30; // If there is a third game, include players 21-30
                }
                else if (getNumGames(matchID) == 3 && string.IsNullOrEmpty(M3.Text))
                {
                    RemoveGame(conn, 3, matchID);
                }
                else
                {
                    MessageBox.Show("Isto não deve aparecer");
                }

                for (int i = 1; i <= maxPlayers; i++)
                {
                    UpdatePlayerStats(conn, GetPlayerStatsParameters(i));
                }
            }
        }

        private void RemoveGame(SqlConnection conn, int gameId, int matchId)
        {
            if (!verifySGBDConnection())
                return;

            try
            {
                string query = "RemoveGame"; // Name of the stored procedure
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MatchID", matchId);
                cmd.Parameters.AddWithValue("@GameID", gameId);

                //MessageBox.Show($"Executing RemoveGame with MatchID: {matchId}, GameID: {gameId}");

                cmd.ExecuteNonQuery();
                MessageBox.Show("Game removed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error removing game: " + ex.Message);
            }
        }

        private void UpdateGame(SqlConnection conn, int gameId, int roundsWonTeam1, int roundsWonTeam2, int matchId, string map)
        {
            using (SqlCommand cmd = new SqlCommand("UpdateGame", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Game_ID", gameId);
                cmd.Parameters.AddWithValue("@Rounds_Won_Team_1", roundsWonTeam1);
                cmd.Parameters.AddWithValue("@Rounds_Won_Team_2", roundsWonTeam2);
                cmd.Parameters.AddWithValue("@Match_ID", matchId);
                cmd.Parameters.AddWithValue("@Map", map);
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdatePlayerStats(SqlConnection conn, PlayerStatsParameters parameters)
        {
            using (SqlCommand cmd = new SqlCommand("UpdatePlayerStats", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PLAYER_CC_Number", parameters.PlayerId);
                cmd.Parameters.AddWithValue("@GAME_Match_ID", matchID);
                cmd.Parameters.AddWithValue("@GAME_Game_ID", parameters.GameId);
                cmd.Parameters.AddWithValue("@Kills", parameters.Kills);
                cmd.Parameters.AddWithValue("@Deaths", parameters.Deaths);
                cmd.Parameters.AddWithValue("@Assists", parameters.Assists);
                cmd.Parameters.AddWithValue("@AVS", parameters.AVS);
                cmd.Parameters.AddWithValue("@Rating", parameters.Rating);
                cmd.Parameters.AddWithValue("@First_kills", parameters.FirstKills);
                cmd.Parameters.AddWithValue("@AGENT_Agent_Name", parameters.AgentName);
                cmd.Parameters.AddWithValue("@ROLE_Role_Name", parameters.RoleName);
                cmd.ExecuteNonQuery();
            }
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

        private void label30_Click(object sender, EventArgs e)
        {

        }
        private void addRolesToComboBoxes()
        {
            List<string> roles = new List<string>();
            string query = "SELECT Role_Name FROM VALO_ROLE";

            using (SqlCommand command = new SqlCommand(query, cn))
            {
                try
                {
                    if (cn.State == System.Data.ConnectionState.Closed)
                    {
                        cn.Open();
                    }

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        roles.Add(reader["Role_Name"].ToString());
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., log the error)
                    MessageBox.Show("An error occurred while retrieving roles: " + ex.Message);
                }
            }
            for(int i = 1; i <= 30; i++)
    {
                System.Windows.Forms.ComboBox comboBoxR = this.Controls.Find($"R{i}", true).FirstOrDefault() as System.Windows.Forms.ComboBox;
                if (comboBoxR != null)
                {
                    comboBoxR.Items.AddRange(roles.ToArray());
                    comboBoxR.SelectedIndexChanged += R_SelectedIndexChanged;
                }
            }
        }

        private void R21_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void R_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox comboBoxR = sender as System.Windows.Forms.ComboBox;
            if (comboBoxR != null)
            {
                string selectedRole = comboBoxR.SelectedItem.ToString();
                string comboBoxRName = comboBoxR.Name;
                int index = int.Parse(comboBoxRName.Substring(1)); // Extract the index from the ComboBox name
                System.Windows.Forms.ComboBox comboBoxG = this.Controls.Find($"G{index}", true).FirstOrDefault() as System.Windows.Forms.ComboBox;
                if (comboBoxG != null)
                {
                    UpdateGItems(comboBoxG, selectedRole);
                }
            }
        }
        private void UpdateGItems(System.Windows.Forms.ComboBox comboBoxG, string role)
        {
            comboBoxG.Items.Clear();

            // Define the query to get agent names based on the role
            string query = "SELECT Agent_Name FROM VALO_AGENT WHERE ROLE_Role_Name = @Role";

            // Create a list to store the agent names
            List<string> agentNames = new List<string>();

            using (SqlCommand command = new SqlCommand(query, cn))
            {
                command.Parameters.AddWithValue("@Role", role);

                try
                {
                    if (cn.State == System.Data.ConnectionState.Closed)
                    {
                        cn.Open();
                    }

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        agentNames.Add(reader["Agent_Name"].ToString());
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., log the error)
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }

            // Add the retrieved agent names to the ComboBox
            comboBoxG.Items.AddRange(agentNames.ToArray());
        }
        private void addMapsToComboBoxes()
        {
            string[] maps = { "Bind", "Haven", "Split", "Ascent", "Icebox", "Breeze", "Fracture", "Pearl", "Lotus", "Sunset" };
            System.Windows.Forms.ComboBox[] mapComboBoxes = { M1, M2, M3 };

            foreach (System.Windows.Forms.ComboBox comboBox in mapComboBoxes)
            {
                comboBox.Items.Clear();
                comboBox.Items.AddRange(maps);
            }
        }

        private void AddMatch()
        {
            using (SqlConnection conn = getSGBDConnection())
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("AddMatch", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Match_ID", matchID);
                    cmd.Parameters.AddWithValue("@Team_1_ID", team1ID);
                    cmd.Parameters.AddWithValue("@Team_2_ID", team2ID);
                    cmd.ExecuteNonQuery();
                }

                AddGame(conn, 1, int.Parse(S11.Text), int.Parse(S12.Text), matchID, M1.Text);
                AddGame(conn, 2, int.Parse(S21.Text), int.Parse(S22.Text), matchID, M2.Text);

                int maxPlayers = 20;

                if (!string.IsNullOrEmpty(M3.Text))
                {
                    AddGame(conn, 3, int.Parse(S31.Text), int.Parse(S32.Text), matchID, M3.Text);
                    maxPlayers = 30; // If there is a third game, include players 21-30
                }

                for (int i = 1; i <= maxPlayers; i++)
                {
                    AddPlayerStats(conn, GetPlayerStatsParameters(i));
                }
            }
        }

        private void AddGame(SqlConnection conn, int gameId, int roundsWonTeam1, int roundsWonTeam2, int matchId, string map)
        {
            using (SqlCommand cmd = new SqlCommand("AddGame", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Game_ID", gameId);
                cmd.Parameters.AddWithValue("@Rounds_Won_Team_1", roundsWonTeam1);
                cmd.Parameters.AddWithValue("@Rounds_Won_Team_2", roundsWonTeam2);
                cmd.Parameters.AddWithValue("@Match_ID", matchId);
                cmd.Parameters.AddWithValue("@Map", map);
                cmd.ExecuteNonQuery();
            }
        }

        private void AddPlayerStats(SqlConnection conn, PlayerStatsParameters parameters)
        {
            using (SqlCommand cmd = new SqlCommand("AddPlayerStats", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PLAYER_CC_Number", parameters.PlayerId);
                cmd.Parameters.AddWithValue("@GAME_Match_ID", matchID);
                cmd.Parameters.AddWithValue("@GAME_Game_ID", parameters.GameId);
                cmd.Parameters.AddWithValue("@Kills", parameters.Kills);
                cmd.Parameters.AddWithValue("@Deaths", parameters.Deaths);
                cmd.Parameters.AddWithValue("@Assists", parameters.Assists);
                cmd.Parameters.AddWithValue("@AVS", parameters.AVS);
                cmd.Parameters.AddWithValue("@Rating", parameters.Rating);
                cmd.Parameters.AddWithValue("@First_kills", parameters.FirstKills);
                cmd.Parameters.AddWithValue("@AGENT_Agent_Name", parameters.AgentName);
                cmd.Parameters.AddWithValue("@ROLE_Role_Name", parameters.RoleName);
                cmd.ExecuteNonQuery();
            }
        }

        private PlayerStatsParameters GetPlayerStatsParameters(int index)
        {
            PlayerStatsParameters parameters = new PlayerStatsParameters();

            try
            {
                // Validate and parse PlayerId
                System.Windows.Forms.TextBox playerTextBox = this.Controls.Find($"P{index}", true).FirstOrDefault() as System.Windows.Forms.TextBox;
                if (playerTextBox != null && int.TryParse(playerTextBox.Text, out int playerId))
                {
                    parameters.PlayerId = playerId;
                }
                else
                {
                    throw new Exception($"Control P{index} contains an invalid integer value or was not found");
                }

                // Validate and parse Kills
                System.Windows.Forms.TextBox killsTextBox = this.Controls.Find($"K{index}", true).FirstOrDefault() as System.Windows.Forms.TextBox;
                if (killsTextBox != null && int.TryParse(killsTextBox.Text, out int kills))
                {
                    parameters.Kills = kills;
                }
                else
                {
                    throw new Exception($"Control K{index} contains an invalid integer value or was not found");
                }

                // Validate and parse Deaths
                System.Windows.Forms.TextBox deathsTextBox = this.Controls.Find($"D{index}", true).FirstOrDefault() as System.Windows.Forms.TextBox;
                if (deathsTextBox != null && int.TryParse(deathsTextBox.Text, out int deaths))
                {
                    parameters.Deaths = deaths;
                }
                else
                {
                    throw new Exception($"Control D{index} contains an invalid integer value or was not found");
                }

                // Validate and parse Assists
                System.Windows.Forms.TextBox assistsTextBox = this.Controls.Find($"A{index}", true).FirstOrDefault() as System.Windows.Forms.TextBox;
                if (assistsTextBox != null && int.TryParse(assistsTextBox.Text, out int assists))
                {
                    parameters.Assists = assists;
                }
                else
                {
                    throw new Exception($"Control A{index} contains an invalid integer value or was not found");
                }

                // Validate and parse AVS
                System.Windows.Forms.TextBox avsTextBox = this.Controls.Find($"C{index}", true).FirstOrDefault() as System.Windows.Forms.TextBox;
                if (avsTextBox != null && decimal.TryParse(avsTextBox.Text, out decimal avs))
                {
                    parameters.AVS = avs;
                }
                else
                {
                    throw new Exception($"Control C{index} contains an invalid decimal value or was not found");
                }

                // Validate and parse Rating
                System.Windows.Forms.TextBox ratingTextBox = this.Controls.Find($"E{index}", true).FirstOrDefault() as System.Windows.Forms.TextBox;
                if (ratingTextBox != null && int.TryParse(ratingTextBox.Text, out int rating))
                {
                    parameters.Rating = rating;
                }
                else
                {
                    throw new Exception($"Control E{index} contains an invalid integer value or was not found");
                }

                // Validate and parse FirstKills
                System.Windows.Forms.TextBox firstKillsTextBox = this.Controls.Find($"F{index}", true).FirstOrDefault() as System.Windows.Forms.TextBox;
                if (firstKillsTextBox != null && int.TryParse(firstKillsTextBox.Text, out int firstKills))
                {
                    parameters.FirstKills = firstKills;
                }
                else
                {
                    throw new Exception($"Control F{index} contains an invalid integer value or was not found");
                }

                // Validate RoleName
                System.Windows.Forms.ComboBox roleComboBox = this.Controls.Find($"R{index}", true).FirstOrDefault() as System.Windows.Forms.ComboBox;
                if (roleComboBox != null && !string.IsNullOrWhiteSpace(roleComboBox.Text))
                {
                    parameters.RoleName = roleComboBox.Text;
                }
                else
                {
                    throw new Exception($"Control R{index} contains an invalid value or was not found");
                }

                // Validate AgentName
                System.Windows.Forms.ComboBox agentComboBox = this.Controls.Find($"G{index}", true).FirstOrDefault() as System.Windows.Forms.ComboBox;
                if (agentComboBox != null && !string.IsNullOrWhiteSpace(agentComboBox.Text))
                {
                    parameters.AgentName = agentComboBox.Text;
                }
                else
                {
                    throw new Exception($"Control G{index} contains an invalid value or was not found");
                }

                parameters.GameId = GetGameId(index);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }

            return parameters;
        }


        private int GetGameId(int index)
        {
            if (index <= 10) return 1;
            if (index <= 20) return 2;
            return 3;
        }

        private void autofill_Click(object sender, EventArgs e)
        {
            // Automatically fill fields for debugging
            K1.Text = "10";
            D1.Text = "5";
            A1.Text = "2";
            C1.Text = "1,5";
            F1.Text = "1";
            E1.Text = "100";
            R1.Text = "Sentinel";
            G1.Text = "Sage";

            K2.Text = "12";
            D2.Text = "6";
            A2.Text = "4";
            C2.Text = "1,7";
            F2.Text = "1";
            E2.Text = "110";
            R2.Text = "Duelist";
            G2.Text = "Jett";

            K3.Text = "8";
            D3.Text = "7";
            A3.Text = "3";
            C3.Text = "1,2";
            F3.Text = "2";
            E3.Text = "90";
            R3.Text = "Initiator";
            G3.Text = "Sova";

            K4.Text = "11";
            D4.Text = "5";
            A4.Text = "3";
            C4.Text = "1,6";
            F4.Text = "2";
            E4.Text = "105";
            R4.Text = "Controller";
            G4.Text = "Omen";

            K5.Text = "9";
            D5.Text = "6";
            A5.Text = "3";
            C5.Text = "1,4";
            F5.Text = "1";
            E5.Text = "95";
            R5.Text = "Sentinel";
            G5.Text = "Cypher";

            // Player stats for Game 1, Team 2
            K6.Text = "8";
            D6.Text = "7";
            A6.Text = "2";
            C6.Text = "1,2";
            F6.Text = "1";
            E6.Text = "90";
            R6.Text = "Duelist";
            G6.Text = "Phoenix";

            K7.Text = "10";
            D7.Text = "5";
            A7.Text = "2";
            C7.Text = "1,5";
            F7.Text = "1";
            E7.Text = "100";
            R7.Text = "Initiator";
            G7.Text = "Breach";

            K8.Text = "11";
            D8.Text = "6";
            A8.Text = "3";
            C8.Text = "1,6";
            F8.Text = "2";
            E8.Text = "105";
            R8.Text = "Controller";
            G8.Text = "Viper";

            K9.Text = "12";
            D9.Text = "7";
            A9.Text = "4";
            C9.Text = "1,7";
            F9.Text = "1";
            E9.Text = "110";
            R9.Text = "Sentinel";
            G9.Text = "Killjoy";

            K10.Text = "7";
            D10.Text = "8";
            A10.Text = "2";
            C10.Text = "1,1";
            F10.Text = "0";
            E10.Text = "85";
            R10.Text = "Duelist";
            G10.Text = "Reyna";

            // Player stats for Game 2, Team 1
            K11.Text = "10";
            D11.Text = "5";
            A11.Text = "2";
            C11.Text = "1,5";
            F11.Text = "1";
            E11.Text = "100";
            R11.Text = "Sentinel";
            G11.Text = "Sage";

            K12.Text = "12";
            D12.Text = "6";
            A12.Text = "4";
            C12.Text = "1,7";
            F12.Text = "1";
            E12.Text = "110";
            R12.Text = "Duelist";
            G12.Text = "Jett";

            K13.Text = "8";
            D13.Text = "7";
            A13.Text = "3";
            C13.Text = "1,2";
            F13.Text = "2";
            E13.Text = "90";
            R13.Text = "Initiator";
            G13.Text = "Sova";

            K14.Text = "11";
            D14.Text = "5";
            A14.Text = "3";
            C14.Text = "1,6";
            F14.Text = "2";
            E14.Text = "105";
            R14.Text = "Controller";
            G14.Text = "Omen";

            K15.Text = "9";
            D15.Text = "6";
            A15.Text = "3";
            C15.Text = "1,4";
            F15.Text = "1";
            E15.Text = "95";
            R15.Text = "Sentinel";
            G15.Text = "Cypher";

            // Player stats for Game 2, Team 2
            K16.Text = "8";
            D16.Text = "7";
            A16.Text = "2";
            C16.Text = "1,2";
            F16.Text = "1";
            E16.Text = "90";
            R16.Text = "Duelist";
            G16.Text = "Phoenix";

            K17.Text = "10";
            D17.Text = "5";
            A17.Text = "2";
            C17.Text = "1,5";
            F17.Text = "1";
            E17.Text = "100";
            R17.Text = "Initiator";
            G17.Text = "Breach";

            K18.Text = "11";
            D18.Text = "6";
            A18.Text = "3";
            C18.Text = "1,6";
            F18.Text = "2";
            E18.Text = "105";
            R18.Text = "Controller";
            G18.Text = "Viper";

            K19.Text = "12";
            D19.Text = "7";
            A19.Text = "4";
            C19.Text = "1,7";
            F19.Text = "1";
            E19.Text = "110";
            R19.Text = "Sentinel";
            G19.Text = "Killjoy";

            K20.Text = "7";
            D20.Text = "8";
            A20.Text = "2";
            C20.Text = "1,1";
            F20.Text = "0";
            E20.Text = "85";
            R20.Text = "Duelist";
            G20.Text = "Reyna";

            // Player stats for Game 3, Team 1
            K21.Text = "10";
            D21.Text = "5";
            A21.Text = "2";
            C21.Text = "1,5";
            F21.Text = "1";
            E21.Text = "100";
            R21.Text = "Sentinel";
            G21.Text = "Sage";

            K22.Text = "12";
            D22.Text = "6";
            A22.Text = "4";
            C22.Text = "1,7";
            F22.Text = "1";
            E22.Text = "110";
            R22.Text = "Duelist";
            G22.Text = "Jett";

            K23.Text = "8";
            D23.Text = "7";
            A23.Text = "3";
            C23.Text = "1,2";
            F23.Text = "2";
            E23.Text = "90";
            R23.Text = "Initiator";
            G23.Text = "Sova";

            K24.Text = "11";
            D24.Text = "5";
            A24.Text = "3";
            C24.Text = "1,6";
            F24.Text = "2";
            E24.Text = "105";
            R24.Text = "Controller";
            G24.Text = "Omen";

            K25.Text = "9";
            D25.Text = "6";
            A25.Text = "3";
            C25.Text = "1,4";
            F25.Text = "1";
            E25.Text = "95";
            R25.Text = "Sentinel";
            G25.Text = "Cypher";

            // Player stats for Game 3, Team 2
            K26.Text = "8";
            D26.Text = "7";
            A26.Text = "2";
            C26.Text = "1,2";
            F26.Text = "1";
            E26.Text = "90";
            R26.Text = "Duelist";
            G26.Text = "Phoenix";

            K27.Text = "10";
            D27.Text = "5";
            A27.Text = "2";
            C27.Text = "1,5";
            F27.Text = "1";
            E27.Text = "100";
            R27.Text = "Initiator";
            G27.Text = "Breach";

            K28.Text = "11";
            D28.Text = "6";
            A28.Text = "3";
            C28.Text = "1,6";
            F28.Text = "2";
            E28.Text = "105";
            R28.Text = "Controller";
            G28.Text = "Viper";

            K29.Text = "12";
            D29.Text = "7";
            A29.Text = "4";
            C29.Text = "1,7";
            F29.Text = "1";
            E29.Text = "110";
            R29.Text = "Sentinel";
            G29.Text = "Killjoy";

            K30.Text = "7";
            D30.Text = "8";
            A30.Text = "2";
            C30.Text = "1,1";
            F30.Text = "0";
            E30.Text = "85";
            R30.Text = "Duelist";
            G30.Text = "Reyna";

            // Rounds won by teams
            S11.Text = "13";
            S12.Text = "7";
            S21.Text = "9";
            S22.Text = "13";
            S31.Text = "15";
            S32.Text = "13";

            // Maps for each game
            M1.Text = "Ascent";
            M2.Text = "Bind";
            M3.Text = "Haven";
        }
    }

    public class PlayerStatsParameters
    {
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public decimal AVS { get; set; }
        public int Rating { get; set; }
        public int FirstKills { get; set; }
        public string RoleName { get; set; }
        public string AgentName { get; set; }
    }


}
