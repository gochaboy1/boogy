using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.Win32;

namespace GameLauncher
{
    public partial class LoginForm : Form
    {
        private bool rememberPassword;
        private string userSuperPswd;
        private string login;
        private string password;
        private const string apiUrl = "https://r2classic.com/api/auth";
        private const string secretKey = "tKUK3E9KSEgZp2qrfGzQ4gxq3Aj5WSDj";

        public LoginForm()
        {
            InitializeComponent();
            // Загрузка сохраненных данных из реестра при инициализации формы
            LoadSavedData();

            // Добавляем обработчик события FormClosing
            this.FormClosing += LoginForm_FormClosing;
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            // Получаем значения логина и пароля из текстовых полей
            login = txtUsername.Text;
            password = txtPassword.Text;

            // Проверяем, что поля логина и пароля не пусты
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Создаем объект HttpClient для отправки POST-запроса
            using (var httpClient = new HttpClient())
            {
                // Подготавливаем данные для POST-запроса в формате JSON
                var requestData = new
                {
                    mUserId = login,
                    password,
                    secret_key = secretKey
                };

                // Преобразуем данные в формат JSON
                var requestDataJson = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(requestDataJson, Encoding.UTF8, "application/json");

                try
                {
                    // Отправляем POST-запрос к API для авторизации
                    HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                    response.EnsureSuccessStatusCode();

                    // Получаем JSON-ответ от сервера
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JObject.Parse(responseContent);

                    // Проверяем успешность авторизации
                    bool success = jsonResponse.GetValue("success").Value<bool>();
                    if (success)
                    {
                        // Если авторизация успешна, записываем JSON-ответ в файл
                        string jsonFilePath = "response.json";
                        File.WriteAllText(jsonFilePath, jsonResponse.ToString());

                        // Получение значения пароля из JSON-ответа
                        userSuperPswd = jsonResponse.GetValue("user")?["mUserSuperPswd"]?.ToString().Trim();

                        if (!string.IsNullOrEmpty(userSuperPswd))
                        {
                            // Если пароль получен, переходим к следующему окну
                            MainLauncherForm mainForm = new MainLauncherForm(login, userSuperPswd);
                            mainForm.Show();
                            this.Hide();

                            // Сохранение логина и пароля в реестр, если выбрана галочка "Запомнить меня"
                            if (rememberPassword)
                            {
                                SaveDataToRegistry();
                            }
                            else
                            {
                                // Если не выбрана галочка "Запомнить меня", то стираем сохраненные данные
                                ClearSavedData();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Ошибка получения пароля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ошибка авторизации. Проверьте введенные данные.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (HttpRequestException ex)
                {
                    // В случае ошибки при выполнении запроса, выводим сообщение об ошибке
                    MessageBox.Show("Ошибка при выполнении запроса: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }

        private void chkRememberMe_CheckedChanged(object sender, EventArgs e)
        {
            // Обработчик события изменения состояния галочки "Запомнить меня"
            rememberPassword = chkRememberMe.Checked;
        }

        private void LoadSavedData()
        {
            // Загрузка сохраненных логина и пароля из реестра, только если выбрана галочка "Запомнить меня"
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\GameLauncher");

            if (key != null)
            {
                string savedLogin = (string)key.GetValue("Login");
                string savedPassword = (string)key.GetValue("Password");

                if (!string.IsNullOrEmpty(savedLogin))
                {
                    login = savedLogin;
                    txtUsername.Text = login;
                }

                if (!string.IsNullOrEmpty(savedPassword))
                {
                    password = savedPassword;
                    txtPassword.Text = password;
                }

                // Загрузка значения галочки "Запомнить меня" из реестра
                rememberPassword = Convert.ToBoolean(key.GetValue("RememberPassword", false));

                // Устанавливаем состояние галочки "Запомнить меня" на форме
                chkRememberMe.Checked = rememberPassword;

                key.Close();
            }
        }

        private void SaveDataToRegistry()
        {
            // Сохранение логина, пароля и значения галочки "Запомнить меня" в реестр, только если выбрана галочка "Запомнить меня"
            RegistryKey key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\GameLauncher");

            if (key != null)
            {
                key.SetValue("Login", login);

                // Сохраняем пароль только если он не пустой
                if (!string.IsNullOrEmpty(password))
                {
                    key.SetValue("Password", password);
                }
                else
                {
                    key.DeleteValue("Password");
                }

                // Сохраняем значение галочки "Запомнить меня"
                key.SetValue("RememberPassword", rememberPassword);

                key.Close();
            }
        }

        private void ClearSavedData()
        {
            // Очистка сохраненных логина и пароля в реестре
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
            if (key != null)
            {
                // Проверяем, существует ли поддерево "GameLauncher" перед удалением
                if (key.GetSubKeyNames().Contains("GameLauncher"))
                {
                    key.DeleteSubKeyTree("GameLauncher");
                }
                key.Close();
            }
        }
        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // При закрытии формы стираем сохраненные данные, если галочка "Запомнить меня" не выбрана
            if (!rememberPassword)
            {
                ClearSavedData();
            }
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            // Обработчик события изменения текста в поле пароля
            // Записываем текущий ввод пароля в переменную, чтобы сохранить его в реестр, если нужно
            password = txtPassword.Text;
        }
    }
}
