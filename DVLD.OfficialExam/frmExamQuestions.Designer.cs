namespace DVLD.OfficialExam
{
    partial class frmExamQuestions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmExamQuestions));
            this.lblQuestionProgress = new System.Windows.Forms.Label();
            this.lblRemainingTime = new System.Windows.Forms.Label();
            this.lblQuestionText = new System.Windows.Forms.Label();
            this.grpAnswers = new System.Windows.Forms.GroupBox();
            this.rbOptionD = new System.Windows.Forms.RadioButton();
            this.rbOptionC = new System.Windows.Forms.RadioButton();
            this.rbOptionB = new System.Windows.Forms.RadioButton();
            this.rbOptionA = new System.Windows.Forms.RadioButton();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnSubmitExam = new System.Windows.Forms.Button();
            this.lblSaveStatus = new System.Windows.Forms.Label();
            this.grpAnswers.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblQuestionProgress
            // 
            this.lblQuestionProgress.AutoSize = true;
            this.lblQuestionProgress.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQuestionProgress.Location = new System.Drawing.Point(12, 32);
            this.lblQuestionProgress.Name = "lblQuestionProgress";
            this.lblQuestionProgress.Size = new System.Drawing.Size(145, 23);
            this.lblQuestionProgress.TabIndex = 0;
            this.lblQuestionProgress.Text = "السؤال 1 من 5";
            // 
            // lblRemainingTime
            // 
            this.lblRemainingTime.AutoSize = true;
            this.lblRemainingTime.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRemainingTime.ForeColor = System.Drawing.Color.DarkRed;
            this.lblRemainingTime.Location = new System.Drawing.Point(863, 32);
            this.lblRemainingTime.Name = "lblRemainingTime";
            this.lblRemainingTime.Size = new System.Drawing.Size(209, 23);
            this.lblRemainingTime.TabIndex = 1;
            this.lblRemainingTime.Text = "الوقت المتبقي: 15:00";
            // 
            // lblQuestionText
            // 
            this.lblQuestionText.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQuestionText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.lblQuestionText.Location = new System.Drawing.Point(12, 55);
            this.lblQuestionText.Name = "lblQuestionText";
            this.lblQuestionText.Size = new System.Drawing.Size(1060, 100);
            this.lblQuestionText.TabIndex = 2;
            this.lblQuestionText.Text = "ما الذي يجب على السائق فعله عند الإشارة\r\n الحمراء؟";
            this.lblQuestionText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // grpAnswers
            // 
            this.grpAnswers.Controls.Add(this.rbOptionD);
            this.grpAnswers.Controls.Add(this.rbOptionC);
            this.grpAnswers.Controls.Add(this.rbOptionB);
            this.grpAnswers.Controls.Add(this.rbOptionA);
            this.grpAnswers.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpAnswers.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.grpAnswers.Location = new System.Drawing.Point(16, 158);
            this.grpAnswers.Name = "grpAnswers";
            this.grpAnswers.Size = new System.Drawing.Size(1056, 260);
            this.grpAnswers.TabIndex = 3;
            this.grpAnswers.TabStop = false;
            this.grpAnswers.Text = "اختر الإجابة الصحيحة";
            // 
            // rbOptionD
            // 
            this.rbOptionD.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbOptionD.Location = new System.Drawing.Point(210, 195);
            this.rbOptionD.Name = "rbOptionD";
            this.rbOptionD.Size = new System.Drawing.Size(840, 45);
            this.rbOptionD.TabIndex = 3;
            this.rbOptionD.TabStop = true;
            this.rbOptionD.Text = "الخيار الرابع";
            this.rbOptionD.UseVisualStyleBackColor = true;
            // 
            // rbOptionC
            // 
            this.rbOptionC.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbOptionC.Location = new System.Drawing.Point(210, 144);
            this.rbOptionC.Name = "rbOptionC";
            this.rbOptionC.Size = new System.Drawing.Size(840, 45);
            this.rbOptionC.TabIndex = 2;
            this.rbOptionC.TabStop = true;
            this.rbOptionC.Text = "الخيار الثالث";
            this.rbOptionC.UseVisualStyleBackColor = true;
            // 
            // rbOptionB
            // 
            this.rbOptionB.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbOptionB.Location = new System.Drawing.Point(210, 93);
            this.rbOptionB.Name = "rbOptionB";
            this.rbOptionB.Size = new System.Drawing.Size(840, 45);
            this.rbOptionB.TabIndex = 1;
            this.rbOptionB.TabStop = true;
            this.rbOptionB.Text = "الخيار الثاني";
            this.rbOptionB.UseVisualStyleBackColor = true;
            // 
            // rbOptionA
            // 
            this.rbOptionA.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbOptionA.Location = new System.Drawing.Point(210, 42);
            this.rbOptionA.Name = "rbOptionA";
            this.rbOptionA.Size = new System.Drawing.Size(840, 45);
            this.rbOptionA.TabIndex = 0;
            this.rbOptionA.TabStop = true;
            this.rbOptionA.Text = "الخيار الأول";
            this.rbOptionA.UseVisualStyleBackColor = true;
            // 
            // btnNext
            // 
            this.btnNext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.btnNext.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNext.ForeColor = System.Drawing.Color.White;
            this.btnNext.Location = new System.Drawing.Point(172, 457);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(150, 50);
            this.btnNext.TabIndex = 4;
            this.btnNext.Text = "التالي";
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnPrevious
            // 
            this.btnPrevious.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(61)))), ((int)(((byte)(125)))));
            this.btnPrevious.Enabled = false;
            this.btnPrevious.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPrevious.ForeColor = System.Drawing.Color.White;
            this.btnPrevious.Location = new System.Drawing.Point(16, 457);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(150, 50);
            this.btnPrevious.TabIndex = 5;
            this.btnPrevious.Text = "السابق";
            this.btnPrevious.UseVisualStyleBackColor = false;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnSubmitExam
            // 
            this.btnSubmitExam.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(128)))), ((int)(((byte)(61)))));
            this.btnSubmitExam.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSubmitExam.ForeColor = System.Drawing.Color.White;
            this.btnSubmitExam.Location = new System.Drawing.Point(892, 457);
            this.btnSubmitExam.Name = "btnSubmitExam";
            this.btnSubmitExam.Size = new System.Drawing.Size(180, 50);
            this.btnSubmitExam.TabIndex = 6;
            this.btnSubmitExam.Text = "تسليم الامتحان";
            this.btnSubmitExam.UseVisualStyleBackColor = false;
            this.btnSubmitExam.Click += new System.EventHandler(this.btnSubmitExam_Click);
            // 
            // lblSaveStatus
            // 
            this.lblSaveStatus.AutoSize = true;
            this.lblSaveStatus.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSaveStatus.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblSaveStatus.Location = new System.Drawing.Point(19, 421);
            this.lblSaveStatus.Name = "lblSaveStatus";
            this.lblSaveStatus.Size = new System.Drawing.Size(132, 18);
            this.lblSaveStatus.TabIndex = 7;
            this.lblSaveStatus.Text = "لم يتم اختيار إجابة";
            // 
            // frmExamQuestions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(1084, 519);
            this.Controls.Add(this.lblSaveStatus);
            this.Controls.Add(this.btnSubmitExam);
            this.Controls.Add(this.btnPrevious);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.grpAnswers);
            this.Controls.Add(this.lblQuestionText);
            this.Controls.Add(this.lblRemainingTime);
            this.Controls.Add(this.lblQuestionProgress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmExamQuestions";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "أسئلة الامتحان الكتابي";
            this.grpAnswers.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblQuestionProgress;
        private System.Windows.Forms.Label lblRemainingTime;
        private System.Windows.Forms.Label lblQuestionText;
        private System.Windows.Forms.GroupBox grpAnswers;
        private System.Windows.Forms.RadioButton rbOptionA;
        private System.Windows.Forms.RadioButton rbOptionC;
        private System.Windows.Forms.RadioButton rbOptionB;
        private System.Windows.Forms.RadioButton rbOptionD;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnSubmitExam;
        private System.Windows.Forms.Label lblSaveStatus;
    }
}