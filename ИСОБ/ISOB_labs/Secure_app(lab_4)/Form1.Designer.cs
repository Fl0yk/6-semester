namespace Secure_app_lab_4_
{
    partial class Form1
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


        private List<TextBox> loginFields = new();
        private List<TextBox> passwordsFields = new();
        private List<TextBox> messageFields = new();
        private List<Button> signInButtons = new();
        private List<Button> sendButtons = new();





        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        /// 
        private void InitializeComponent()
        {
            SignIn1 = new Button();
            Message1 = new TextBox();
            MessagesListBox = new ListBox();
            Message2 = new TextBox();
            SignIn2 = new Button();
            Message3 = new TextBox();
            SignIn3 = new Button();
            Message4 = new TextBox();
            SignIn4 = new Button();
            Message5 = new TextBox();
            SignIn5 = new Button();
            Message6 = new TextBox();
            SignIn6 = new Button();
            Login1 = new TextBox();
            Password1 = new TextBox();
            Login2 = new TextBox();
            Password2 = new TextBox();
            Login3 = new TextBox();
            Password3 = new TextBox();
            Login4 = new TextBox();
            Password4 = new TextBox();
            Login5 = new TextBox();
            Password5 = new TextBox();
            Login6 = new TextBox();
            Password6 = new TextBox();
            Send6 = new Button();
            Send5 = new Button();
            Send4 = new Button();
            Send3 = new Button();
            Send2 = new Button();
            Send1 = new Button();
            AttackDefensesCheckedListBox = new CheckedListBox();
            SignOut6 = new Button();
            SignOut5 = new Button();
            SignOut4 = new Button();
            SignOut3 = new Button();
            SignOut2 = new Button();
            SignOut1 = new Button();
            SuspendLayout();
            // 
            // SignIn1
            // 
            SignIn1.Location = new Point(162, 35);
            SignIn1.Name = "SignIn1";
            SignIn1.Size = new Size(82, 23);
            SignIn1.TabIndex = 2;
            SignIn1.Text = "Войти";
            SignIn1.UseVisualStyleBackColor = true;
            SignIn1.Click += SignIn_Click;
            // 
            // Message1
            // 
            Message1.Location = new Point(330, 50);
            Message1.Name = "Message1";
            Message1.Size = new Size(168, 23);
            Message1.TabIndex = 3;
            // 
            // MessagesListBox
            // 
            MessagesListBox.FormattingEnabled = true;
            MessagesListBox.ItemHeight = 15;
            MessagesListBox.Location = new Point(626, 26);
            MessagesListBox.Name = "MessagesListBox";
            MessagesListBox.Size = new Size(271, 409);
            MessagesListBox.TabIndex = 4;
            // 
            // Message2
            // 
            Message2.Location = new Point(330, 139);
            Message2.Name = "Message2";
            Message2.Size = new Size(168, 23);
            Message2.TabIndex = 8;
            // 
            // SignIn2
            // 
            SignIn2.Location = new Point(162, 124);
            SignIn2.Name = "SignIn2";
            SignIn2.Size = new Size(82, 23);
            SignIn2.TabIndex = 7;
            SignIn2.Text = "Войти";
            SignIn2.UseVisualStyleBackColor = true;
            SignIn2.Click += SignIn_Click;
            // 
            // Message3
            // 
            Message3.Location = new Point(330, 218);
            Message3.Name = "Message3";
            Message3.Size = new Size(168, 23);
            Message3.TabIndex = 12;
            // 
            // SignIn3
            // 
            SignIn3.Location = new Point(162, 203);
            SignIn3.Name = "SignIn3";
            SignIn3.Size = new Size(82, 23);
            SignIn3.TabIndex = 11;
            SignIn3.Text = "Войти";
            SignIn3.UseVisualStyleBackColor = true;
            SignIn3.Click += SignIn_Click;
            // 
            // Message4
            // 
            Message4.Location = new Point(330, 304);
            Message4.Name = "Message4";
            Message4.Size = new Size(168, 23);
            Message4.TabIndex = 16;
            // 
            // SignIn4
            // 
            SignIn4.Location = new Point(162, 289);
            SignIn4.Name = "SignIn4";
            SignIn4.Size = new Size(82, 23);
            SignIn4.TabIndex = 15;
            SignIn4.Text = "Войти";
            SignIn4.UseVisualStyleBackColor = true;
            SignIn4.Click += SignIn_Click;
            // 
            // Message5
            // 
            Message5.Location = new Point(330, 397);
            Message5.Name = "Message5";
            Message5.Size = new Size(168, 23);
            Message5.TabIndex = 20;
            // 
            // SignIn5
            // 
            SignIn5.Location = new Point(162, 382);
            SignIn5.Name = "SignIn5";
            SignIn5.Size = new Size(82, 23);
            SignIn5.TabIndex = 19;
            SignIn5.Text = "Войти";
            SignIn5.UseVisualStyleBackColor = true;
            SignIn5.Click += SignIn_Click;
            // 
            // Message6
            // 
            Message6.Location = new Point(330, 488);
            Message6.Name = "Message6";
            Message6.Size = new Size(168, 23);
            Message6.TabIndex = 24;
            // 
            // SignIn6
            // 
            SignIn6.Location = new Point(162, 473);
            SignIn6.Name = "SignIn6";
            SignIn6.Size = new Size(82, 23);
            SignIn6.TabIndex = 23;
            SignIn6.Text = "Войти";
            SignIn6.UseVisualStyleBackColor = true;
            SignIn6.Click += SignIn_Click;
            // 
            // Login1
            // 
            Login1.Location = new Point(44, 35);
            Login1.Name = "Login1";
            Login1.Size = new Size(100, 23);
            Login1.TabIndex = 0;
            // 
            // Password1
            // 
            Password1.Location = new Point(44, 64);
            Password1.Name = "Password1";
            Password1.Size = new Size(100, 23);
            Password1.TabIndex = 1;
            // 
            // Login2
            // 
            Login2.Location = new Point(44, 124);
            Login2.Name = "Login2";
            Login2.Size = new Size(100, 23);
            Login2.TabIndex = 5;
            // 
            // Password2
            // 
            Password2.Location = new Point(44, 153);
            Password2.Name = "Password2";
            Password2.Size = new Size(100, 23);
            Password2.TabIndex = 6;
            // 
            // Login3
            // 
            Login3.Location = new Point(44, 203);
            Login3.Name = "Login3";
            Login3.Size = new Size(100, 23);
            Login3.TabIndex = 9;
            // 
            // Password3
            // 
            Password3.Location = new Point(44, 232);
            Password3.Name = "Password3";
            Password3.Size = new Size(100, 23);
            Password3.TabIndex = 10;
            // 
            // Login4
            // 
            Login4.Location = new Point(44, 289);
            Login4.Name = "Login4";
            Login4.Size = new Size(100, 23);
            Login4.TabIndex = 13;
            // 
            // Password4
            // 
            Password4.Location = new Point(44, 318);
            Password4.Name = "Password4";
            Password4.Size = new Size(100, 23);
            Password4.TabIndex = 14;
            // 
            // Login5
            // 
            Login5.Location = new Point(44, 382);
            Login5.Name = "Login5";
            Login5.Size = new Size(100, 23);
            Login5.TabIndex = 17;
            // 
            // Password5
            // 
            Password5.Location = new Point(44, 411);
            Password5.Name = "Password5";
            Password5.Size = new Size(100, 23);
            Password5.TabIndex = 18;
            // 
            // Login6
            // 
            Login6.Location = new Point(44, 473);
            Login6.Name = "Login6";
            Login6.Size = new Size(100, 23);
            Login6.TabIndex = 21;
            // 
            // Password6
            // 
            Password6.Location = new Point(44, 502);
            Password6.Name = "Password6";
            Password6.Size = new Size(100, 23);
            Password6.TabIndex = 22;
            // 
            // Send6
            // 
            Send6.Location = new Point(514, 487);
            Send6.Name = "Send6";
            Send6.Size = new Size(82, 23);
            Send6.TabIndex = 30;
            Send6.Text = "Отправить";
            Send6.UseVisualStyleBackColor = true;
            Send6.Click += Send_Click;
            // 
            // Send5
            // 
            Send5.Location = new Point(514, 396);
            Send5.Name = "Send5";
            Send5.Size = new Size(82, 23);
            Send5.TabIndex = 29;
            Send5.Text = "Отправить";
            Send5.UseVisualStyleBackColor = true;
            Send5.Click += Send_Click;
            // 
            // Send4
            // 
            Send4.Location = new Point(514, 303);
            Send4.Name = "Send4";
            Send4.Size = new Size(82, 23);
            Send4.TabIndex = 28;
            Send4.Text = "Отправить";
            Send4.UseVisualStyleBackColor = true;
            Send4.Click += Send_Click;
            // 
            // Send3
            // 
            Send3.Location = new Point(514, 217);
            Send3.Name = "Send3";
            Send3.Size = new Size(82, 23);
            Send3.TabIndex = 27;
            Send3.Text = "Отправить";
            Send3.UseVisualStyleBackColor = true;
            Send3.Click += Send_Click;
            // 
            // Send2
            // 
            Send2.Location = new Point(514, 138);
            Send2.Name = "Send2";
            Send2.Size = new Size(82, 23);
            Send2.TabIndex = 26;
            Send2.Text = "Отправить";
            Send2.UseVisualStyleBackColor = true;
            Send2.Click += Send_Click;
            // 
            // Send1
            // 
            Send1.Location = new Point(514, 49);
            Send1.Name = "Send1";
            Send1.Size = new Size(82, 23);
            Send1.TabIndex = 25;
            Send1.Text = "Отправить";
            Send1.UseVisualStyleBackColor = true;
            Send1.Click += Send_Click;
            // 
            // AttackDefensesCheckedListBox
            // 
            AttackDefensesCheckedListBox.FormattingEnabled = true;
            AttackDefensesCheckedListBox.Items.AddRange(new object[] { "Минимизация привилегий", "Переполнение буфера", "DoS", "XSS" });
            AttackDefensesCheckedListBox.Location = new Point(626, 431);
            AttackDefensesCheckedListBox.Name = "AttackDefensesCheckedListBox";
            AttackDefensesCheckedListBox.Size = new Size(271, 94);
            AttackDefensesCheckedListBox.TabIndex = 31;
            // 
            // SignOut6
            // 
            SignOut6.Location = new Point(162, 502);
            SignOut6.Name = "SignOut6";
            SignOut6.Size = new Size(82, 23);
            SignOut6.TabIndex = 37;
            SignOut6.Text = "Выйти";
            SignOut6.UseVisualStyleBackColor = true;
            SignOut6.Click += SignOut_Click;
            // 
            // SignOut5
            // 
            SignOut5.Location = new Point(162, 411);
            SignOut5.Name = "SignOut5";
            SignOut5.Size = new Size(82, 23);
            SignOut5.TabIndex = 36;
            SignOut5.Text = "Выйти";
            SignOut5.UseVisualStyleBackColor = true;
            SignOut5.Click += SignOut_Click;
            // 
            // SignOut4
            // 
            SignOut4.Location = new Point(162, 318);
            SignOut4.Name = "SignOut4";
            SignOut4.Size = new Size(82, 23);
            SignOut4.TabIndex = 35;
            SignOut4.Text = "Выйти";
            SignOut4.UseVisualStyleBackColor = true;
            SignOut4.Click += SignOut_Click;
            // 
            // SignOut3
            // 
            SignOut3.Location = new Point(162, 232);
            SignOut3.Name = "SignOut3";
            SignOut3.Size = new Size(82, 23);
            SignOut3.TabIndex = 34;
            SignOut3.Text = "Выйти";
            SignOut3.UseVisualStyleBackColor = true;
            SignOut3.Click += SignOut_Click;
            // 
            // SignOut2
            // 
            SignOut2.Location = new Point(162, 153);
            SignOut2.Name = "SignOut2";
            SignOut2.Size = new Size(82, 23);
            SignOut2.TabIndex = 33;
            SignOut2.Text = "Выйти";
            SignOut2.UseVisualStyleBackColor = true;
            SignOut2.Click += SignOut_Click;
            // 
            // SignOut1
            // 
            SignOut1.Location = new Point(162, 64);
            SignOut1.Name = "SignOut1";
            SignOut1.Size = new Size(82, 23);
            SignOut1.TabIndex = 32;
            SignOut1.Text = "Выйти";
            SignOut1.UseVisualStyleBackColor = true;
            SignOut1.Click += SignOut_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Bisque;
            ClientSize = new Size(936, 559);
            Controls.Add(SignOut6);
            Controls.Add(SignOut5);
            Controls.Add(SignOut4);
            Controls.Add(SignOut3);
            Controls.Add(SignOut2);
            Controls.Add(SignOut1);
            Controls.Add(AttackDefensesCheckedListBox);
            Controls.Add(Send6);
            Controls.Add(Send5);
            Controls.Add(Send4);
            Controls.Add(Send3);
            Controls.Add(Send2);
            Controls.Add(Send1);
            Controls.Add(Message6);
            Controls.Add(SignIn6);
            Controls.Add(Password6);
            Controls.Add(Login6);
            Controls.Add(Message5);
            Controls.Add(SignIn5);
            Controls.Add(Password5);
            Controls.Add(Login5);
            Controls.Add(Message4);
            Controls.Add(SignIn4);
            Controls.Add(Password4);
            Controls.Add(Login4);
            Controls.Add(Message3);
            Controls.Add(SignIn3);
            Controls.Add(Password3);
            Controls.Add(Login3);
            Controls.Add(Message2);
            Controls.Add(SignIn2);
            Controls.Add(Password2);
            Controls.Add(Login2);
            Controls.Add(MessagesListBox);
            Controls.Add(Message1);
            Controls.Add(SignIn1);
            Controls.Add(Password1);
            Controls.Add(Login1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button SignIn1;
        private TextBox Message1;
        private ListBox MessagesListBox;
        private TextBox Message2;
        private Button SignIn2;
        private TextBox Message3;
        private Button SignIn3;
        private TextBox Message4;
        private Button SignIn4;
        private TextBox Message5;
        private Button SignIn5;
        private TextBox Message6;
        private Button SignIn6;
        private TextBox Login1;
        private TextBox Password1;
        private TextBox Login2;
        private TextBox Password2;
        private TextBox Login3;
        private TextBox Password3;
        private TextBox Login4;
        private TextBox Password4;
        private TextBox Login5;
        private TextBox Password5;
        private TextBox Login6;
        private TextBox Password6;
        private Button Send6;
        private Button Send5;
        private Button Send4;
        private Button Send3;
        private Button Send2;
        private Button Send1;
        private CheckedListBox AttackDefensesCheckedListBox;
        private Button SignOut6;
        private Button SignOut5;
        private Button SignOut4;
        private Button SignOut3;
        private Button SignOut2;
        private Button SignOut1;
    }
}
