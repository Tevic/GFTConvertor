using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GFTConvertor
{
    public partial class MainForm : Form
    {
        private int FileCount = 0;
        private int CompletedCount = 0;
        private Array files;
        private BackgroundWorker bgWorker = new BackgroundWorker();

        public MainForm()
        {
            InitializeComponent();
            InitBackGroundWorker();
            if (Environment.OSVersion.Version.Major.ToString() == "6")
            {
                AeroForm.AeroEffect(this);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            label2.Text += Environment.OSVersion.ToString();
            
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            label1.Text = "Drop File Or Directory Here";
            label1.ForeColor = Color.Black;
            progressBar1.Value = 0;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            files = (Array)e.Data.GetData(DataFormats.FileDrop);

            foreach (string f in files)
            {
                string ext = Path.GetExtension(f);
                if (f.EndsWith(".gft", StringComparison.InvariantCultureIgnoreCase))
                {
                    FileCount++;
                }
                else if (Directory.Exists(f))
                {
                    DirectoryInfo folder = new DirectoryInfo(f);
                    FileInfo[] chldFiles = folder.GetFiles("*.gft", SearchOption.AllDirectories);
                    FileCount += chldFiles.Count();
                }
            }

            progressBar1.Step = 1;
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = FileCount;

            //如果后台进程未开始运行，则开始运行线程
            if (bgWorker.IsBusy != true)
            {
                bgWorker.RunWorkerAsync();
                this.AllowDrop = false;
            }
            

        }

        private void ConvertDir(string dir)
        {
            DirectoryInfo folder = new DirectoryInfo(dir);
            FileInfo[] chldFiles = folder.GetFiles("*.gft", SearchOption.AllDirectories);

            foreach (FileInfo chlFile in chldFiles)
            {
                string fileName = chlFile.FullName;
                ConvertFile(fileName);
            }
        }

        /// <summary>
        /// gft文件路径
        /// </summary>
        /// <param name="fileName"></param>
        private void ConvertFile(string fileName)
        {
            string ext = string.Empty;

            FileStream fs = File.OpenRead(fileName);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();

            int key = data[16];//第16个字指定了图片的起始地址。

            byte[] data2 = data.Skip(key).ToArray();
            if (data2[0].ToString("x") == "42") //42 4D
            {
                ext = "bmp";
            }
            else if (data2[0].ToString("x") == "89") //80 50
            {
                ext = "png";
            }

            string newFilename = Path.ChangeExtension(fileName, ext);

            FileStream fs2 = new FileStream(newFilename, FileMode.Create);
            fs2.Write(data2, 0, data2.Length);
            fs2.Close();

            CompletedCount++;
            bgWorker.ReportProgress(CompletedCount);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
                MessageBox.Show(
                    "This Program Is Designed For Converting GFT Files To PNG Or BMP Files!\nWritten By Tevic.TT\nE-Mail:Tevic.TT@Gmail.Com",
                    "About This", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
        }


        //初始化BackgroundWorker的相关属性和加载事件
        public void InitBackGroundWorker()
        {
            //BackgroundWorker是否支持报告执行进度
            bgWorker.WorkerReportsProgress = true;
            //BackgroundWorker是否支持取消运行异步操作
            bgWorker.WorkerSupportsCancellation = true;
            //加载DoWork、ProgressChanged、RunWorkerCompleted事件
            bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgressChanged);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
        }

        //后台异步执行操作
        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (string f in files)
            {
                string ext = Path.GetExtension(f);
                while (CompletedCount != FileCount)
                {
                    if (f.EndsWith(".gft", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ConvertFile(f);
                    }
                    else if (Directory.Exists(f))
                    {
                        ConvertDir(f);
                    }
                }
            }
        }

        //进度改变时触发的事件执行函数
        void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //设置当前进度条的值为事件执行完成程度值
            this.progressBar1.Value = e.ProgressPercentage;
            label1.Text = (progressBar1.Value * 100 / FileCount).ToString()+"%";
            
        }

        //异步操作执行完毕之后的处理
        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            label1.Text = "Conversion Completed";
            label1.ForeColor = Color.Blue;
            label1.TextAlign = label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            CompletedCount = 0;
            FileCount = 0;
            this.AllowDrop = true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bgWorker.IsBusy)
            {
                if (MessageBox.Show("Do You Really Stop The Conversion And Quit?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    bgWorker.CancelAsync();
                    e.Cancel = false;
                }
                else
                    e.Cancel=true;
            }
        }
    }
}
