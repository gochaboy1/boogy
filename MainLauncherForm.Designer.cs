namespace GameLauncher
{
    partial class MainLauncherForm
    {
        // Вставьте сюда код для инициализации элементов управления формы, например, кнопки "Начать играть" и "Обновить клиент".
        private void InitializeComponent()
        {
            btnPlay = new Button();
            btnUpdate = new Button();
            progressBar = new ProgressBar();
            SuspendLayout();
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(69, 91);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(150, 40);
            btnPlay.TabIndex = 0;
            btnPlay.Text = "Начать играть";
            btnPlay.Click += btnPlay_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(69, 137);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(150, 40);
            btnUpdate.TabIndex = 1;
            btnUpdate.Text = "Обновить клиент";
            btnUpdate.Click += btnUpdate_Click;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(69, 191);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(150, 20);
            progressBar.TabIndex = 2;
            // 
            // lblProgress
            // 

            // 
            // MainLauncherForm
            // 
            ClientSize = new Size(284, 261);
            Controls.Add(progressBar);
            Controls.Add(btnPlay);
            Controls.Add(btnUpdate);
            Name = "MainLauncherForm";
            FormClosed += MainLauncherForm_FormClosed;
            Load += MainLauncherForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        // Определите поля для элементов управления, которые вы добавили выше.
        // Вам нужно добавить соответствующие переменные для кнопок и прогресс бара:

        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button txtLog;
        private System.Windows.Forms.ProgressBar progressBar;

        // ... остальной сгенерированный код, опущенный здесь ...
    }
}
