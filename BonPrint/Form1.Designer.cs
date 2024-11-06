namespace BonPrint
{

    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            connectBtn = new Button();
            label1 = new Label();
            userTbox = new TextBox();
            label2 = new Label();
            passwordTbox = new TextBox();
            label3 = new Label();
            printerNameTbox = new TextBox();
            monitorBtn = new Button();
            label4 = new Label();
            printTestBtn = new Button();
            turnsLbl = new Label();
            dataGridView1 = new DataGridView();
            locatioLbl = new Label();
            locationTbox = new TextBox();
            locationSaveBtn = new Button();
            lastResetDateLbl = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // connectBtn
            // 
            connectBtn.Location = new Point(11, 75);
            connectBtn.Margin = new Padding(2, 1, 2, 1);
            connectBtn.Name = "connectBtn";
            connectBtn.Size = new Size(233, 28);
            connectBtn.TabIndex = 3;
            connectBtn.Text = "Probar y guardar conexión";
            connectBtn.UseVisualStyleBackColor = true;
            connectBtn.Click += Connect;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 34);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(47, 15);
            label1.TabIndex = 2;
            label1.Text = "Usuario";
            // 
            // userTbox
            // 
            userTbox.Location = new Point(11, 50);
            userTbox.Margin = new Padding(2, 1, 2, 1);
            userTbox.Name = "userTbox";
            userTbox.Size = new Size(110, 23);
            userTbox.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(125, 34);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(67, 15);
            label2.TabIndex = 4;
            label2.Text = "Contraseña";
            // 
            // passwordTbox
            // 
            passwordTbox.Location = new Point(125, 50);
            passwordTbox.Margin = new Padding(2, 1, 2, 1);
            passwordTbox.Name = "passwordTbox";
            passwordTbox.Size = new Size(119, 23);
            passwordTbox.TabIndex = 2;
            passwordTbox.UseSystemPasswordChar = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(11, 9);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(83, 15);
            label3.TabIndex = 6;
            label3.Text = "Configuración";
            // 
            // printerNameTbox
            // 
            printerNameTbox.Location = new Point(248, 50);
            printerNameTbox.Margin = new Padding(2, 1, 2, 1);
            printerNameTbox.Name = "printerNameTbox";
            printerNameTbox.Size = new Size(165, 23);
            printerNameTbox.TabIndex = 4;
            // 
            // monitorBtn
            // 
            monitorBtn.BackColor = Color.PaleGreen;
            monitorBtn.FlatAppearance.BorderColor = SystemColors.ControlLightLight;
            monitorBtn.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            monitorBtn.Location = new Point(11, 116);
            monitorBtn.Margin = new Padding(2, 1, 2, 1);
            monitorBtn.Name = "monitorBtn";
            monitorBtn.Size = new Size(233, 41);
            monitorBtn.TabIndex = 8;
            monitorBtn.Text = "Iniciar monitoreo del lector";
            monitorBtn.UseVisualStyleBackColor = false;
            monitorBtn.Click += MonitorBtn_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(249, 34);
            label4.Name = "label4";
            label4.Size = new Size(135, 15);
            label4.TabIndex = 9;
            label4.Text = "Nombre de la impresora";
            // 
            // printTestBtn
            // 
            printTestBtn.Location = new Point(248, 75);
            printTestBtn.Name = "printTestBtn";
            printTestBtn.Size = new Size(165, 28);
            printTestBtn.TabIndex = 5;
            printTestBtn.Text = "Probar y guardar impresora";
            printTestBtn.UseVisualStyleBackColor = true;
            printTestBtn.Click += PrintTestBtn_Click;
            // 
            // turnsLbl
            // 
            turnsLbl.AutoSize = true;
            turnsLbl.Font = new Font("Segoe UI Black", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            turnsLbl.Location = new Point(335, 106);
            turnsLbl.Name = "turnsLbl";
            turnsLbl.Size = new Size(156, 50);
            turnsLbl.TabIndex = 12;
            turnsLbl.Text = "Turno -";
            // 
            // dataGridView1
            // 
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 170);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Size = new Size(580, 242);
            dataGridView1.TabIndex = 13;
            // 
            // locatioLbl
            // 
            locatioLbl.AutoSize = true;
            locatioLbl.Location = new Point(419, 34);
            locatioLbl.Name = "locatioLbl";
            locatioLbl.Size = new Size(72, 15);
            locatioLbl.TabIndex = 14;
            locatioLbl.Text = "Localización";
            // 
            // locationTbox
            // 
            locationTbox.Location = new Point(419, 50);
            locationTbox.Name = "locationTbox";
            locationTbox.Size = new Size(173, 23);
            locationTbox.TabIndex = 6;
            // 
            // locationSaveBtn
            // 
            locationSaveBtn.Location = new Point(416, 75);
            locationSaveBtn.Name = "locationSaveBtn";
            locationSaveBtn.Size = new Size(176, 28);
            locationSaveBtn.TabIndex = 7;
            locationSaveBtn.Text = "Guardar localización";
            locationSaveBtn.UseVisualStyleBackColor = true;
            locationSaveBtn.Click += LocationSaveBtn_Click;
            // 
            // lastResetDateLbl
            // 
            lastResetDateLbl.AutoSize = true;
            lastResetDateLbl.Location = new Point(340, 152);
            lastResetDateLbl.Name = "lastResetDateLbl";
            lastResetDateLbl.Size = new Size(0, 15);
            lastResetDateLbl.TabIndex = 15;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(604, 424);
            Controls.Add(lastResetDateLbl);
            Controls.Add(locationSaveBtn);
            Controls.Add(locationTbox);
            Controls.Add(locatioLbl);
            Controls.Add(dataGridView1);
            Controls.Add(turnsLbl);
            Controls.Add(printTestBtn);
            Controls.Add(label4);
            Controls.Add(monitorBtn);
            Controls.Add(printerNameTbox);
            Controls.Add(label3);
            Controls.Add(passwordTbox);
            Controls.Add(label2);
            Controls.Add(userTbox);
            Controls.Add(label1);
            Controls.Add(connectBtn);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(2, 1, 2, 1);
            MaximizeBox = false;
            Name = "MainForm";
            Text = "BonPrint";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button connectBtn;
        private Label label1;
        private TextBox userTbox;
        private Label label2;
        private TextBox passwordTbox;
        private Label label3;
        private TextBox printerNameTbox;
        private Button monitorBtn;
        private Label label4;
        private Button printTestBtn;
        private Label turnsLbl;
        private DataGridView dataGridView1;
        private Label locatioLbl;
        private TextBox locationTbox;
        private Button locationSaveBtn;
        private Label lastResetDateLbl;
    }
}
