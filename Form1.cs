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
    public partial class Form1 : Form
    {
        private SqlConnection cn;
        private int totalItems;
        private String comandoConfirmar;
        private String comandoConfirmarBilhetes;
        private String guardarNumber;
        private String guardarTeamID;
        private String guardarTeamID1;
        private String guardarCCNumber;
        private String guardarteamID2;
        private String guardarGameID;

        public Form1()
        {
            InitializeComponent();
            verifySGBDConnection();
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
    }
}
