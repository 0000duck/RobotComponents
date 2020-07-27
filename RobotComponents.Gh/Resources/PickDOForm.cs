﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

using RobotComponents.Gh.Components.ControllerUtility;

namespace RobotComponents.Gh.Resources
{
    public partial class PickDOForm : Form
    {
        public static int stationIndex = 0;

        public PickDOForm()
        {
            InitializeComponent();
        }

        public PickDOForm(List<string> items)
        {
            InitializeComponent();
            for (int i = 0; i < items.Count; i++)
            {
                comboBox1.Items.Add(items[i]);
            }
        }

        private void PicController_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.labelNameInfo.Text = GetDigitalOutputComponent.SignalGooList[comboBox1.SelectedIndex].Value.Name.ToString();
            this.labelValueInfo.Text = GetDigitalOutputComponent.SignalGooList[comboBox1.SelectedIndex].Value.Value.ToString();
            this.labelTypeInfo.Text = GetDigitalOutputComponent.SignalGooList[comboBox1.SelectedIndex].Value.Type.ToString();
            this.labelMinValueInfo.Text = GetDigitalOutputComponent.SignalGooList[comboBox1.SelectedIndex].Value.MinValue.ToString();
            this.labelMaxValueInfo.Text = GetDigitalOutputComponent.SignalGooList[comboBox1.SelectedIndex].Value.MaxValue.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            stationIndex = comboBox1.SelectedIndex;
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void labelTypeInfo_Click(object sender, EventArgs e)
        {

        }
    }
}