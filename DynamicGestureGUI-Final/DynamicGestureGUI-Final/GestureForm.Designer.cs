namespace DynamicGestureGUI
{
    partial class GestureForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && (components != null))
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
            this.buttonTrain = new System.Windows.Forms.Button();
            this.buttonRecog = new System.Windows.Forms.Button();
            this.labelMode = new System.Windows.Forms.Label();
            this.comboTrain = new System.Windows.Forms.ComboBox();
            this.labelGesture = new System.Windows.Forms.Label();
            this.buttonLearnHMM = new System.Windows.Forms.Button();
            this.buttonLoadFromFile = new System.Windows.Forms.Button();
            this.buttonSaveToFile = new System.Windows.Forms.Button();
            this.buttonKinect = new System.Windows.Forms.Button();
            this.labelPointing = new System.Windows.Forms.Label();
            this.comboPointing = new System.Windows.Forms.ComboBox();
            this.labelRobot = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonTrain
            // 
            this.buttonTrain.Location = new System.Drawing.Point(25, 45);
            this.buttonTrain.Name = "buttonTrain";
            this.buttonTrain.Size = new System.Drawing.Size(75, 53);
            this.buttonTrain.TabIndex = 0;
            this.buttonTrain.Text = "Change to Training Mode";
            this.buttonTrain.UseVisualStyleBackColor = true;
            this.buttonTrain.Click += new System.EventHandler(this.buttonTrain_Click);
            // 
            // buttonRecog
            // 
            this.buttonRecog.Location = new System.Drawing.Point(359, 45);
            this.buttonRecog.Name = "buttonRecog";
            this.buttonRecog.Size = new System.Drawing.Size(75, 53);
            this.buttonRecog.TabIndex = 1;
            this.buttonRecog.Text = "Change To Recognition Mode";
            this.buttonRecog.UseVisualStyleBackColor = true;
            this.buttonRecog.Click += new System.EventHandler(this.buttonRecog_Click);
            // 
            // labelMode
            // 
            this.labelMode.AutoSize = true;
            this.labelMode.Location = new System.Drawing.Point(197, 65);
            this.labelMode.Name = "labelMode";
            this.labelMode.Size = new System.Drawing.Size(94, 13);
            this.labelMode.TabIndex = 2;
            this.labelMode.Text = "Recognition Mode";
            // 
            // comboTrain
            // 
            this.comboTrain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTrain.FormattingEnabled = true;
            this.comboTrain.Items.AddRange(new object[] {
            "Forward",
            "Backward",
            "Speed Up",
            "Speed Down",
            "Return"});
            this.comboTrain.Location = new System.Drawing.Point(25, 115);
            this.comboTrain.Name = "comboTrain";
            this.comboTrain.Size = new System.Drawing.Size(84, 21);
            this.comboTrain.TabIndex = 3;
            this.comboTrain.Visible = false;
            // 
            // labelGesture
            // 
            this.labelGesture.AutoSize = true;
            this.labelGesture.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGesture.Location = new System.Drawing.Point(168, 237);
            this.labelGesture.Name = "labelGesture";
            this.labelGesture.Size = new System.Drawing.Size(65, 25);
            this.labelGesture.TabIndex = 4;
            this.labelGesture.Text = "Info: ";
            // 
            // buttonLearnHMM
            // 
            this.buttonLearnHMM.Location = new System.Drawing.Point(49, 323);
            this.buttonLearnHMM.Name = "buttonLearnHMM";
            this.buttonLearnHMM.Size = new System.Drawing.Size(75, 47);
            this.buttonLearnHMM.TabIndex = 5;
            this.buttonLearnHMM.Text = "Learn a new HMM";
            this.buttonLearnHMM.UseVisualStyleBackColor = true;
            this.buttonLearnHMM.Click += new System.EventHandler(this.buttonLearnHMM_Click);
            // 
            // buttonLoadFromFile
            // 
            this.buttonLoadFromFile.Location = new System.Drawing.Point(200, 323);
            this.buttonLoadFromFile.Name = "buttonLoadFromFile";
            this.buttonLoadFromFile.Size = new System.Drawing.Size(75, 47);
            this.buttonLoadFromFile.TabIndex = 6;
            this.buttonLoadFromFile.Text = "Load Training Data";
            this.buttonLoadFromFile.UseVisualStyleBackColor = true;
            this.buttonLoadFromFile.Click += new System.EventHandler(this.buttonLoadFromFile_Click);
            // 
            // buttonSaveToFile
            // 
            this.buttonSaveToFile.Location = new System.Drawing.Point(358, 323);
            this.buttonSaveToFile.Name = "buttonSaveToFile";
            this.buttonSaveToFile.Size = new System.Drawing.Size(75, 47);
            this.buttonSaveToFile.TabIndex = 7;
            this.buttonSaveToFile.Text = "Save Training Data";
            this.buttonSaveToFile.UseVisualStyleBackColor = true;
            this.buttonSaveToFile.Click += new System.EventHandler(this.buttonSaveToFile_Click);
            // 
            // buttonKinect
            // 
            this.buttonKinect.Location = new System.Drawing.Point(200, 12);
            this.buttonKinect.Name = "buttonKinect";
            this.buttonKinect.Size = new System.Drawing.Size(75, 23);
            this.buttonKinect.TabIndex = 8;
            this.buttonKinect.Text = "Start Kinect";
            this.buttonKinect.UseVisualStyleBackColor = true;
            this.buttonKinect.Click += new System.EventHandler(this.buttonKinect_Click);
            // 
            // labelPointing
            // 
            this.labelPointing.AutoSize = true;
            this.labelPointing.Location = new System.Drawing.Point(46, 423);
            this.labelPointing.Name = "labelPointing";
            this.labelPointing.Size = new System.Drawing.Size(187, 13);
            this.labelPointing.TabIndex = 9;
            this.labelPointing.Text = "Pointing Gesture Recognition Method:";
            // 
            // comboPointing
            // 
            this.comboPointing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboPointing.FormattingEnabled = true;
            this.comboPointing.Items.AddRange(new object[] {
            "Method 1",
            "Method 2"});
            this.comboPointing.Location = new System.Drawing.Point(239, 420);
            this.comboPointing.Name = "comboPointing";
            this.comboPointing.Size = new System.Drawing.Size(121, 21);
            this.comboPointing.TabIndex = 10;
            // 
            // labelRobot
            // 
            this.labelRobot.AutoSize = true;
            this.labelRobot.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold);
            this.labelRobot.Location = new System.Drawing.Point(120, 186);
            this.labelRobot.Name = "labelRobot";
            this.labelRobot.Size = new System.Drawing.Size(155, 25);
            this.labelRobot.TabIndex = 11;
            this.labelRobot.Text = "Robot Status:";
            // 
            // GestureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(478, 505);
            this.Controls.Add(this.labelRobot);
            this.Controls.Add(this.comboPointing);
            this.Controls.Add(this.labelPointing);
            this.Controls.Add(this.buttonKinect);
            this.Controls.Add(this.buttonSaveToFile);
            this.Controls.Add(this.buttonLoadFromFile);
            this.Controls.Add(this.buttonLearnHMM);
            this.Controls.Add(this.labelGesture);
            this.Controls.Add(this.comboTrain);
            this.Controls.Add(this.labelMode);
            this.Controls.Add(this.buttonRecog);
            this.Controls.Add(this.buttonTrain);
            this.Name = "GestureForm";
            this.Text = "Gesture";
            this.Load += new System.EventHandler(this.GestureForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonTrain;
        private System.Windows.Forms.Button buttonRecog;
        private System.Windows.Forms.Label labelMode;
        private System.Windows.Forms.ComboBox comboTrain;
        private System.Windows.Forms.Label labelGesture;
        private System.Windows.Forms.Button buttonLearnHMM;
        private System.Windows.Forms.Button buttonLoadFromFile;
        private System.Windows.Forms.Button buttonSaveToFile;
        private System.Windows.Forms.Button buttonKinect;
        private System.Windows.Forms.Label labelPointing;
        private System.Windows.Forms.ComboBox comboPointing;
        private System.Windows.Forms.Label labelRobot;
    }
}

