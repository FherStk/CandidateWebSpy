using System.Windows.Forms;

namespace CandidateWebSpy
{
    partial class WebSpy
    {        
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        protected WebBrowser wb = null;
        protected ProgressBar pb = null;

        protected Timer timer = null;

        protected Label label = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            //this.ClientSize = new System.Drawing.Size(800, 450);
            this.ClientSize = new System.Drawing.Size(400, 140);
            this.MaximumSize = this.ClientSize;
            this.Text = "Applicant's web spy - Alpha 2 (v0.0.2.0)";                  

            //WEB BROWSER
            this.wb = new WebBrowser();                  
            this.wb.Visible = false;            
            this.wb.DocumentCompleted += WebBrowserDocumentCompleted;
            this.wb.ScrollBarsEnabled = false;
            this.wb.ScriptErrorsSuppressed = true;     
            this.Controls.Add(wb);

            //LABEL
            this.label = new Label();
            this.label.Visible = true;            
            this.label.Width = this.ClientSize.Width - 40;  
            this.label.Top = 20;         
            this.label.Left = 20;
            this.label.Height = 20;
            this.label.Text = "Loading...";
            this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Controls.Add(label);

            //PROGRESS BAR
            this.pb = new ProgressBar();
            this.pb.Visible = true;            
            this.pb.Width = this.label.Width;   
            this.pb.Top = this.label.Top + this.label.Height + 20;       
            this.pb.Left = this.label.Left;
            this.pb.Step = 1;
            this.pb.Minimum = 0;
            this.pb.Value = 0;
            this.Controls.Add(pb);

            //TIMER
            this.timer = new Timer();
            this.timer.Interval = 1000;
            timer.Tick += TimerTick;
        }

        #endregion
    }
}

