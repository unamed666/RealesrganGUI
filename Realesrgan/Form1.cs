using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Realesrgan
{
    public partial class Form1 : Form
    {

        //data isian
        string currentdir = Directory.GetCurrentDirectory();
        string imgoutputfolder = string.Empty;
        string vidoutputfolder = string.Empty;
        string imgfilePath = string.Empty;
        string imgfileName = string.Empty;
        string imgfileExt = string.Empty;
        string imgdirPath = string.Empty;
        string vidfileName = string.Empty;
        string vidfileExt = string.Empty;
        string vidfilePath = string.Empty;
        string viddirPath = string.Empty;
        string dataimg = string.Empty;
        string dataimg2 = string.Empty;
        string datavid = string.Empty;
        string datavid2 = string.Empty;
        string imgscale = string.Empty;
        string vidscale = string.Empty;
        string imgfileOut = string.Empty;
        string vidfileOut = string.Empty;
        string imgoutname = string.Empty;
        string vidoutname = string.Empty;


        public Form1()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            radimgA.Checked = true;
            radimgX4.Checked = true;
            radimgOutloc1.Checked = true;
            string imgtwoLevelsUp = Directory.GetParent(Directory.GetParent(currentdir).FullName).FullName;
            string imgoutputFolderPath = Path.Combine(imgtwoLevelsUp, "Output", "Images");
            imgoutputfolder = imgoutputFolderPath;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkimgOutname.CheckState == CheckState.Checked) 
            {
                txtimgOutname.Enabled = true;
            }
            else if (checkimgOutname.CheckState == CheckState.Unchecked)
            {
                txtimgOutname.Enabled = false;
                imgoutname = imgfileName;
                txtimgOutname.Text = imgfileName;
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
        }

        private void button9_Click(object sender, EventArgs e)
        {
           

            if (radimgOutloc1.Checked)
            {
                Process.Start("explorer.exe", imgdirPath);
            }
            if (radimgOutloc2.Checked)
            {
                Process.Start("explorer.exe", imgoutputfolder);
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radimgOutloc1.Checked)
            {
                imgfileOut = imgdirPath;
            }
            if (radimgOutloc2.Checked)
            {
                imgfileOut = imgoutputfolder;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            
            using (OpenFileDialog ofd = new OpenFileDialog())
            {                
                ofd.Filter = "All files (*.*)|*.*|Image Files|*.jpg;*.jpeg;*.png;*.webp";
                ofd.FilterIndex = 2;
                //if (imgdirPath == String.Empty)
                //{
                //    ofd.InitialDirectory = Directory.GetCurrentDirectory();
                //}
                //if (imgdirPath != String.Empty)
                //{
                    ofd.RestoreDirectory = true;
                //}
                

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    imgfilePath = '\u0022' +  ofd.FileName + '\u0022';
                    txtimgPath.Text = ofd.FileName;
                    imgdirPath = Path.GetDirectoryName(txtimgPath.Text);
                    imgfileOut = imgdirPath;
                    imgfileName = ofd.SafeFileName;
                    imgoutname = Path.GetFileNameWithoutExtension(imgfileName);
                    txtimgOutname.Text = imgfileName;
                    imgfileExt = Path.GetExtension(txtimgPath.Text);

                }
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void submit_Click(object sender, EventArgs e)
        {
            string twoLevelsUp = Directory.GetParent(Directory.GetParent(currentdir).FullName).FullName;

            string datasubmit = " -i "+ imgfilePath +" -n "+ dataimg 
                +" -s "+ imgscale +" -o " + '\u0022' + imgfileOut + "\\"+ imgoutname + "-X"+ imgscale + dataimg2 + imgfileExt + '\u0022' ;


            string exeFilePath = Path.Combine(twoLevelsUp, "Realesrgan", "realesrgan-ncnn-vulkan.exe");

            ProcessStartInfo ps = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WindowStyle = ProcessWindowStyle.Normal,
                Arguments =$"/c \" \u0022{exeFilePath}\u0022 {datasubmit}\" ",
                CreateNoWindow = true, // Show the window
            };
            Process.Start(ps);
            label10.Enabled = true;
           
        }

        private void radioButton12_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidFix.Enabled == true && radvidFix.Checked == true )
            {
                numvidX.Enabled = true;
                numvidY.Enabled = true;
                txtvidCus.Enabled = true;
            }
            else if (radvidFix.Enabled == false || radvidFix.Checked == false)
            {
                numvidX.Enabled = false;
                numvidY.Enabled = false;
            }
            if (radvidCus.Enabled == true && radvidCus.Checked == true)
            {
                txtvidCus.Enabled = true;
            }
            else if (radvidCus.Enabled == false || radvidCus.Checked ==false)
            {
                txtvidCus.Enabled = false;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkvidCmd.Checked == true)
            {   radvidFix.Enabled = true; 
                radvidFix.Checked = true;
                radvidCus.Enabled = true;
            }
            else if (checkvidCmd.Checked == false)
            {
                radvidFix.Enabled = false;
                radvidFix.Checked = false;
                radvidCus.Enabled = false;
                radvidCus.Checked = false;
               
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void txtoutputname_TextChanged(object sender, EventArgs e)
        {
            if (txtimgOutname.Enabled == true)
            {
                imgoutname = txtimgOutname.Text;
            }

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }

        private void radimgA_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidA.Checked == true)
            {
                radvidX2.Enabled = true;
                radvidX3.Enabled = true;
                datavid = "realesr-animevideov3";
                datavid2 = "A";

            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidOutloc1.Checked)
            {
                vidfileOut = viddirPath;
            }
            if (radvidOutloc2.Checked)
            {
                vidfileOut = vidoutputfolder;
            }
        }

        private void txtvidCus_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtvidLengthX_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkvidOutname_CheckedChanged(object sender, EventArgs e)
        {
            if (checkvidOutname.CheckState == CheckState.Checked)
            {
                txtvidOutname.Enabled = true;
            }
            else if (checkvidOutname.CheckState == CheckState.Unchecked)
            {
                txtvidOutname.Enabled = false;
                vidoutname = vidfileName;
                txtvidOutname.Text = vidfileName;
            }
        }

        private void radimgA_CheckedChanged_1(object sender, EventArgs e)
        {
            
            if (radimgA.Checked == true)
            {
                radimgX2.Enabled = true;
                radimgX3.Enabled = true;
                dataimg = "realesr-animevideov3";
                dataimg2 = "A";
            }
        }

        private void radimgN_CheckedChanged(object sender, EventArgs e)
        {
            if (radimgN.Checked == true)
            {
                radimgX2.Enabled = false;
                radimgX3.Enabled = false;
                radimgX4.Checked = true;
                dataimg = "realesrgan-x4plus";
                dataimg2 = "N";
            }
        }

        private void radvidN_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidN.Checked == true)
            {
                radvidX2.Enabled = false;
                radvidX3.Enabled = false;
                radvidX4.Checked = true;
                datavid = "realesrgan-x4plus";
                datavid2 = "N";
            }
        }

        private void txtimgPath_TextChanged(object sender, EventArgs e)
        {
            imgfilePath = '\u0022' + txtimgPath.Text + '\u0022';
        }

        private void btnvidFind_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {                
                ofd.Filter = "All files (*.*)|*.*|Video Files|*.mp4;*.mkv;*.3gp;*.gif;*.wav";
                ofd.FilterIndex = 2;
                // if (viddirPath == String.Empty)
                //{
                //   ofd.InitialDirectory = Directory.GetCurrentDirectory();
                //}
                //if (viddirPath != String.Empty)
                //{
                    ofd.RestoreDirectory = true;
                //}


                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    vidfilePath = '\u0022' + ofd.FileName + '\u0022';
                    txtvidPath.Text = ofd.FileName;
                    viddirPath = Path.GetDirectoryName(txtvidPath.Text);
                    vidfileOut = viddirPath;
                    vidfileName = ofd.SafeFileName;
                    vidoutname = Path.GetFileNameWithoutExtension(vidfileName);
                    txtvidOutname.Text = vidfileName;
                    vidfileExt = Path.GetExtension(txtvidPath.Text);

                }
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void radimgX2_CheckedChanged(object sender, EventArgs e)
        {
            if (radimgX2.Checked == true)
            {
                imgscale = "2";
            }
        }

        private void radimgX3_CheckedChanged(object sender, EventArgs e)
        {
            if (radimgX3.Checked == true)
            {
                imgscale = "3";
            }
        }

        private void radimgX4_CheckedChanged(object sender, EventArgs e)
        {
            if (radimgX4.Checked == true)
            {
                imgscale = "4";
            }
        }
        private void radvidX2_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidX2.Checked == true)
            {
                vidscale = "2";
            }
        }

        private void radvidX3_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidX3.Checked == true)
            {
                vidscale = "3";
            }
        }

        private void radvidX4_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidX4.Checked == true)
            {
                vidscale = "4";
            }
        }

        private void label10_Click_1(object sender, EventArgs e)
        {

        }

        private void txtvidPath_TextChanged(object sender, EventArgs e)
        {
            vidfilePath = '\u0022' + txtvidPath.Text + '\u0022';
        }

        private void txtvidOutname_TextChanged(object sender, EventArgs e)
        {
            if (txtvidOutname.Enabled == true)
            {
                vidoutname = txtvidOutname.Text;
            }
        }

        private void radvidOutloc1_CheckedChanged(object sender, EventArgs e)
        {
            if (radvidOutloc1.Checked)
            {
                vidfileOut = viddirPath;
            }
            if (radvidOutloc2.Checked)
            {
                vidfileOut = vidoutputfolder;
            }
        }

        private void btnvidOutdir_Click(object sender, EventArgs e)
        {
            if (radvidOutloc1.Checked)
            {
                Process.Start("explorer.exe", viddirPath);
            }
            if (radvidOutloc2.Checked)
            {
                Process.Start("explorer.exe", vidoutputfolder);
            }
        }

        private void label12_Click_1(object sender, EventArgs e)
        {

        }
    }
}
