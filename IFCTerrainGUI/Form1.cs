﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using IFCTerrainGUI.Properties;
using IFCTerrain.Model;
using IFCTerrain.Model.Read;
using BimGisCad.Collections;
using IxMilia.Dxf;
using Newtonsoft.Json;
using System.Xml;

namespace IFCTerrainGUI
{
    public partial class Form1 : Form
    {
        //private Mesh Mesh { get; set; } = null;
        //private ReadInput Input { get;  set; } = new ReadInput();
        private DxfFile dxfFile = null;
        private RebDaData rebData = null;
        public JsonSettings jSettings { get; set; } = new JsonSettings();
        
        private string[] fileNames = new string[1];
        
        
        //Action<string> logText;
        ProgressBar progressBar;

        #region Empty Labels
        public Form1()
        {
            InitializeComponent();
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void label8_Click(object sender, EventArgs e)
        {

        }
        private void lblType_Click(object sender, EventArgs e)
        {

        }
        private void label4_Click(object sender, EventArgs e)
        {

        }
        #endregion

        #region Read XML/GML

        private void btnReadXml_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "LandXML *.xml|*.xml|CityGML *.gml|*.gml"
            };
            ofd.FilterIndex = Properties.Settings.Default.readTinFilterIndex;
            ofd.InitialDirectory = Properties.Settings.Default.initialDirectory;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.readTinFilterIndex = ofd.FilterIndex;
                Properties.Settings.Default.initialDirectory = Path.GetDirectoryName(ofd.FileName);
                switch (ofd.FilterIndex)
                {
                    case 1:
                        //this.Input.InputType = InputType.LandXML;
                        this.jSettings.fileType = "LandXML";
                        tbType.Text = "LandXML";
                        //this.logText(string.Format(Properties.Resources.msgReadFile, InputType.LandXML.ToString(), Path.GetFileName(ofd.FileName)));
                        break;
                    case 2:
                        //this.Input.InputType = InputType.CityGML;
                        this.jSettings.fileType = "CityGML";
                        tbType.Text = "CityGML";
                        //this.logText(string.Format(Properties.Resources.msgReadFile, InputType.CityGML.ToString(), Path.GetFileName(ofd.FileName)));
                        break;
                }
                this.jSettings.fileName = ofd.FileName;
                tbFile.Text = ofd.FileName;
                //this.Input.FileNames = new[] { ofd.FileName };
                //this.Enabled = false;
                //this.progressBar.Show();
                //this.backgroundWorkerXML.RunWorkerAsync();
            }
        }

        #endregion

        //private void setData()
        //{
        //    if (this.Mesh != null)
        //    {
        //        this.tbFile.Text = Path.GetFileName(this.Input.FileNames[0]);
        //        this.tbType.Text = this.Input.InputType.ToString();
        //        //this.tbExtent.Text = string.Format("dX = {0:f3}   dY = {1:f3}   dZ = {2:f3}",
        //        //    this.Mesh.MaxX - this.Mesh.MinX,
        //        //    this.Mesh.MaxY - this.Mesh.MinY,
        //        //    this.Mesh.MaxZ - this.Mesh.MinZ);
        //        //this.tbCount.Text = string.Format(Properties.Resources.msgCount, this.Mesh.Points.Count, this.Mesh.FixedEdges.Count, this.Mesh.FaceEdges.Count);
        //    }
        //    else
        //    {
        //        this.tbFile.Text = "";
        //        this.tbType.Text = "";
        //        this.tbLayHor.Text = "";
        //        //this.tbCount.Text = "";
        //    }
        //}

        #region Read DXF

        private void btnReadDXF_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "DXF Files *.dxf, *.dxb|*.dxf;*.dxb"
            };
            ofd.InitialDirectory = Properties.Settings.Default.initialDirectory;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //Properties.Settings.Default.initialDirectory = Path.GetDirectoryName(ofd.FileName);
                this.jSettings.fileType = "DXF";
                tbType.Text = "DXF";
                this.jSettings.fileName = ofd.FileName;
                tbFile.Text = ofd.FileName;
                //this.Input.FileNames = new[] { ofd.FileName };
                //this.Input.InputType = InputType.DXF;
                //this.logText(string.Format(Properties.Resources.msgReadFile, "DXF", Path.GetFileName(ofd.FileName)));
                this.Enabled = false;
                //this.progressBar.Show();
                this.backgroundWorkerDXF.RunWorkerAsync(ofd.FileName);
            }
        }
        
        private void backgroundWorkerDXF_DoWork_1(object sender, DoWorkEventArgs e)
        {
            e.Result = DXF.ReadFile((string)e.Argument, out this.dxfFile) ? (string)e.Argument : "";
        }

        private void backgroundWorkerDXF_RunWorkerCompleted_1(object sender, RunWorkerCompletedEventArgs e)
        {
            string name = (string)e.Result;
            this.lbLayer.SuspendLayout();
            this.lbLayer.Items.Clear();
            this.btnProcess.Enabled = false;
            if (string.IsNullOrEmpty(name))
            {
                this.dxfFile = null;
                //this.logText(string.Format(Properties.Resources.errFileNotReadable, Path.GetFileName(name)));
            }
            else
            {
                //this.logText(string.Format(Properties.Resources.msgSuccessRead, "DXF", Path.GetFileName(name)));
                foreach (var l in this.dxfFile.Layers)
                {
                    this.lbLayer.Items.Add(l.Name);
                }
            }
            this.lbLayer.ResumeLayout();
            //this.progressBar.Hide();
            this.Enabled = true;
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            if (this.lbLayer.SelectedIndex >= 0)
            {
                string layer = (string)this.lbLayer.SelectedItem;
                this.jSettings.layer = layer;
                tbLayHor.Text = layer;
                this.jSettings.isTin = this.rbFaces.Checked;
            }
        }

        private void lbLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.btnProcess.Enabled = this.lbLayer.SelectedItem is string;
        }

        #endregion

        #region Read REB

        private void btnReadReb_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "REB Dateien *.REB, *.D45, *.D49, *.D58|*.reb;*.d45;*.d49;*.d58|Alle Dateitypen|*.*"
            };
            ofd.FilterIndex = Properties.Settings.Default.readTinFilterIndex;
            ofd.InitialDirectory = Properties.Settings.Default.initialDirectory;
            //ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.jSettings.fileName = ofd.FileName;
                tbFile.Text = ofd.FileName;
                this.jSettings.fileType = "REB";
                tbType.Text = "REB";
                fileNames[0] = ofd.FileName;
                //Properties.Settings.Default.initialDirectory = Path.GetDirectoryName(ofd.FileName);
                //this.Input.InputType = InputType.REB;
                //this.logText(string.Format(Properties.Resources.msgReadFile, InputType.LandXML.ToString(), Path.GetFileNameWithoutExtension(ofd.FileNames[0])));
                //this.Input.FileNames = ofd.FileNames;
                this.Enabled = false;
                //this.progressBar.Show();
                this.backgroundWorkerREB.RunWorkerAsync(fileNames);
            }
        }

        private void backgroundWorkerREB_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = RebDa.ReadREB(fileNames);
        }

        private void backgroundWorkerREB_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.lbHorizon.SuspendLayout();
            this.lbHorizon.Items.Clear();
            this.btnProcessReb.Enabled = false;
            if (e.Result is RebDaData rdat)
            {
                this.rebData = rdat;
                //this.logText(string.Format(Properties.Resources.msgSuccessRead, "REB", Path.GetFileNameWithoutExtension(this.Input.FileNames[0])));
                foreach (var h in rdat.GetHorizons())
                {
                    this.lbHorizon.Items.Add(h);
                }
            }
            else
            {
                this.rebData = null;
                //this.logText(string.Format(Properties.Resources.errFileNotReadable, Path.GetFileNameWithoutExtension(this.Input.FileNames[0])));
            }
            this.lbHorizon.ResumeLayout();
            //this.progressBar.Hide();
            this.Enabled = true;
        }

        private void lbHorizon_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.btnProcessReb.Enabled = this.lbHorizon.SelectedIndex > -1;
        }

        private void btnProcessReb_Click(object sender, EventArgs e)
        {
            if (this.lbHorizon.SelectedIndex >= 0)
            {
                int horizon = (int)this.lbHorizon.SelectedItem;
                this.jSettings.horizon = horizon;
                tbLayHor.Text = horizon.ToString();
            }
        }

        #endregion


    //    if(double.TryParse(this.tbX.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double x)
    //                    && double.TryParse(this.tbY.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
    //                {
    //                    double z = 0.0;
    //                    if(!this.chkZ.Checked && !double.TryParse(this.tbZ.Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out z))
    //                    { z = 0.0; }

    //sitePlacement.Location = Vector3.Create(x, y, z);
    //                }


        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Industry Foundation Classes | *.ifc";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbTarDir.Text = sfd.FileName;
                this.jSettings.destFileName = sfd.FileName;
            }
        }

        #region Start
        private void btnStart_Click(object sender, EventArgs e)
        {
            this.jSettings.geoElement = chkGeo.Checked;
            //Ignore Z-Value Checkbox einbauen?
            this.jSettings.is3D = true;
            this.jSettings.minDist = 1.0;
            if (tbDist.Text != null)
            {
                this.jSettings.minDist = Convert.ToDouble(tbDist.Text);
            }
            this.jSettings.outIFCType = rb4.Checked ? "IFC4": "IFC2x3";
            this.jSettings.surfaceType = "TFS";
            this.jSettings.outFileType = "Step";
            if (chkXML.Checked)
            {
                this.jSettings.outFileType = "XML";
            }
            if (rbGCS.Checked)
            {
                this.jSettings.surfaceType = "GCS";
            }
            else if (rbSSM.Checked)
            {
                this.jSettings.surfaceType = "SBSM";
            }

            this.jSettings.customOrigin = false;
            if(rbCoCus.Checked)
            {
                this.jSettings.customOrigin = true;
                this.jSettings.xOrigin = Convert.ToDouble(tbCoX.Text);
                this.jSettings.yOrigin = Convert.ToDouble(tbCoY.Text);
                this.jSettings.zOrigin = Convert.ToDouble(tbCoZ.Text);
            }

            string path = Path.GetDirectoryName(this.jSettings.destFileName);

            //Werte aus UserSettings übernehmen bzw. Standardwerte
            //if (System.Configuration.ConfigurationManager.AppSettings["LogFilePath"] == null)
            //{
            //    this.jSettings.logFilePath = path + @"\log.txt";            }
            //else
            //{
            //    this.jSettings.logFilePath = System.Configuration.ConfigurationManager.AppSettings["LogFilePath"];
            //}
            
            //if (System.Configuration.ConfigurationManager.AppSettings["VerbosityLevel"] == null)
            //{
            //    this.jSettings.verbosityLevel = "Information";
            //}
            //else
            //{
            //    switch (System.Configuration.ConfigurationManager.AppSettings["VerbosityLevel"])
            //    {
            //        case "Error":
            //            this.jSettings.verbosityLevel = "Error";
            //            break;
            //        case "Debug":
            //            this.jSettings.verbosityLevel = "Debug";
            //            break;
            //        default:
            //            this.jSettings.verbosityLevel = "Information";
            //            break;
            //    }       
            //}

            if (jSettings.editorsOrganisationName == null)
            {
                this.jSettings.editorsOrganisationName = "Organisation";
            }

            if (jSettings.editorsGivenName == null)
            {
                this.jSettings.editorsGivenName = "GivenName";
            }

            if (jSettings.editorsFamilyName == null)
            {
                this.jSettings.editorsFamilyName = "FamilyName";
            }

            
            // Serialisieren von Json-Datei
            try
            {
                string jExportText = JsonConvert.SerializeObject(this.jSettings);
                System.IO.File.WriteAllText(path + @"\config.json", jExportText);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message.ToString() + Environment.NewLine);
            }
            
            MessageBox.Show("Transformation gestartet");
            //progressBarIFC.Visible = true;
            this.Enabled = false;
            this.backgroundWorkerIFC.RunWorkerAsync();
        }

        private void backgroundWorkerIFC_DoWork(object sender, DoWorkEventArgs e)
        {
            //BackgroundWorker workerSender = sender as BackgroundWorker;

            //// get a node list from agrument passed to RunWorkerAsync
            //XmlNodeList node = e.Argument as XmlNodeList;

            //for (int i = 0; i < node.Count; i++)
            //{
            //    textBox2.Text = node[i].InnerText;
            //    workerSender.ReportProgress(node.Count / i);
            //}

            ConnectionInterface conInt = new ConnectionInterface();
            conInt.mapProcess(this.jSettings);
        }

        private void backgroundWorkerIFC_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("IFC Datei erfolgreich erstellt");

            this.Enabled = true;
        }
        #endregion

        private void btnSettings_Click(object sender, EventArgs e)
        {
            UserSettings settingsForm = new UserSettings(this);
            settingsForm.Show();
        }

        private void backgroundWorkerIFC_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //progressBarIFC.Value = e.ProgressPercentage;
        }

        

        private void rb2_CheckedChanged(object sender, EventArgs e)
        {
            if (rb2.Checked)
            {
                chkGeo.Visible = false;
            }
        }

        private void rb4_CheckedChanged(object sender, EventArgs e)
        {
            if (rb4.Checked)
            {
                chkGeo.Visible = true;
            }
        }

        private void btnReadGrid_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Textfile *.txt|*.txt|XYZ *.xyz|*.xyz"
            };
            ofd.FilterIndex = Properties.Settings.Default.readTinFilterIndex;
            ofd.InitialDirectory = Properties.Settings.Default.initialDirectory;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.readTinFilterIndex = ofd.FilterIndex;
                Properties.Settings.Default.initialDirectory = Path.GetDirectoryName(ofd.FileName);
                
                this.jSettings.fileType = "Grid";
                tbType.Text = "Grid";
                this.jSettings.fileName = ofd.FileName;
                tbFile.Text = ofd.FileName;
                int gridSize = Convert.ToInt32(tbGsSet.Text);
                this.jSettings.gridSize = gridSize;
                tbGridSize.Text = gridSize.ToString();
            }
        }

        

        private void btnGridSize_Click(object sender, EventArgs e)
        {
            int gridSize = Convert.ToInt32(tbGsSet.Text);
            
            this.jSettings.gridSize = gridSize;
            this.jSettings.bBox = cb_BBox.Checked;
            if(cb_BBox.Checked)
            {
                this.jSettings.bbNorth = Convert.ToDouble(tb_bbNorth.Text);
                this.jSettings.bbEast = Convert.ToDouble(tb_bbEast.Text);
                this.jSettings.bbWest = Convert.ToDouble(tb_bbWest.Text);
                this.jSettings.bbSouth = Convert.ToDouble(tb_bbSouth.Text);
            }
            else
            {
                this.jSettings.bbNorth = 0.0;
                this.jSettings.bbEast = 0.0;
                this.jSettings.bbWest = 0.0;
                this.jSettings.bbSouth = 0.0;
            }
            

            tbGridSize.Text = gridSize.ToString();
        }

        

        private void rbCoCus_CheckedChanged(object sender, EventArgs e)
        {
            if(rbCoCus.Checked)
            {
                tbCoX.ReadOnly = false;
                tbCoY.ReadOnly = false;
                tbCoZ.ReadOnly = false;
            }
            else
            {
                tbCoX.ReadOnly = true;
                tbCoY.ReadOnly = true;
                tbCoZ.ReadOnly = true;
            }
        }

        private void rbCoDef_CheckedChanged(object sender, EventArgs e)
        {
            if (rbCoCus.Checked)
            {
                tbCoX.ReadOnly = false;
                tbCoY.ReadOnly = false;
                tbCoZ.ReadOnly = false;
            }
            else
            {
                tbCoX.ReadOnly = true;
                tbCoY.ReadOnly = true;
                tbCoZ.ReadOnly = true;
            }
        }

        private void btn_docu_Click(object sender, EventArgs e)
        {
            try
            {
                string mainDirec = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName).FullName).FullName).FullName;
                string docuPath = Path.Combine(mainDirec, "Documentation.html");
                System.Diagnostics.Process.Start(docuPath);
                //System.Diagnostics.Process.Start(@"Documentation.html");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Dokumentation konnte nicht geöffnet werden:" + ex);
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void cb_BBox_CheckedChanged(object sender, EventArgs e)
        {
            if(cb_BBox.Checked)
            {
                tb_bbNorth.Enabled = true;
                tb_bbEast.Enabled = true;
                tb_bbWest.Enabled = true;
                tb_bbSouth.Enabled = true;
            }
            else
            {
                tb_bbNorth.Enabled = false;
                tb_bbEast.Enabled = false;
                tb_bbWest.Enabled = false;
                tb_bbSouth.Enabled = false;
            }
        }
    }
}
