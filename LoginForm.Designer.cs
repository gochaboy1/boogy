namespace GameLauncher
{
    partial class LoginForm
    {
        // ... остальной сгенерированный код, опущенный здесь ...

        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.CheckBox chkRememberMe; // Добавляем галочку "Запомнить меня"

        private void InitializeComponent()
        {

            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnLogin = new Button();
            chkRememberMe = new CheckBox(); // Инициализируем галочку "Запомнить меня"
            SuspendLayout();
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(100, 100);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(200, 23);
            txtUsername.TabIndex = 0;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(100, 150);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(200, 23);
            txtPassword.TabIndex = 1;
            txtPassword.PasswordChar = '*'; // Устанавливаем символ пароля
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(150, 200);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(100, 30);
            btnLogin.TabIndex = 2;
            btnLogin.Text = "Войти";
            btnLogin.Click += btnLogin_Click;
            // 
            // chkRememberMe
            // 
            chkRememberMe.AutoSize = true; // Устанавливаем тип галочки как автоматический размер
            chkRememberMe.Location = new Point(100, 180);
            chkRememberMe.Name = "chkRememberMe";
            chkRememberMe.Size = new Size(120, 19);
            chkRememberMe.TabIndex = 3;
            chkRememberMe.Text = "Запомнить меня";
            chkRememberMe.CheckedChanged += chkRememberMe_CheckedChanged;
            // 
            // LoginForm
            // 
            ClientSize = new Size(370, 261);
            Controls.Add(txtUsername);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(chkRememberMe); // Добавляем галочку на форму
            Name = "LoginForm";
            Load += LoginForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}

        // Определите поля для элементов управления, которые вы добавили выше.
        // Вам нужно добавить соответствующие переменные для текстовых полей, кнопки и галочки.


