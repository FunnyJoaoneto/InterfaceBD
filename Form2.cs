﻿using System;
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
        private string nome;
        public Form2(string texto, int id1, int id2, int mid)
        {
            nome = texto;
            InitializeComponent();
            this.label1.Text = nome;
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}