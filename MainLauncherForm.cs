using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO.Compression;
using System.Net.Http;
using System.IO;
using Force.Crc32;

namespace GameLauncher
{
    public partial class MainLauncherForm : Form
    {
        // Добавляем поле WebClient, которое будет доступно везде внутри класса
        private WebClient client = new WebClient();
        private string login;
        private string userSuperPswd;
        private string apiUrl = "https://user81700.clients-cdnnow.ru/r2club";
        private string xmlFilePath = "/UpdateInfo.xml"; // Путь к XML-файлу с информацией об обновлениях

        public MainLauncherForm(string login, string userSuperPswd)
        {
            InitializeComponent();

            // Сохраняем переданные логин и пароль в поля класса
            this.login = login;
            this.userSuperPswd = userSuperPswd;
        }

        private string GetDownloadPath(FileModel file)
        {
            // Получаем путь к папке, где находится исполняемый файл приложения
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Формируем путь для сохранения загруженного архива (без расширения .zip)
            string downloadPath = Path.Combine(baseDirectory, "GameFiles", file.Path, file.Name);

            // Создаем директории для сохранения архива, если не существуют
            Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));

            return downloadPath;
        }


        private string GetFileUrl(FileModel file)
        {
            string fileNameWithZip = file.Name + ".zip";
            string filePath = file.Path.Replace("\\", "/");
            string fileUrl = $"{apiUrl}/{filePath}/{fileNameWithZip}";
            return fileUrl;
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(userSuperPswd))
            {
                MessageBox.Show("Ошибка получения пароля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Кодирование логина и пароля в формате base64
            string encodedLogin = Convert.ToBase64String(Encoding.UTF8.GetBytes(login));
            string encodedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(userSuperPswd));
            // Подготовка параметров командной строки для запуска R2Client.exe
            string arguments = $"\"P=&H1=OTkuMDUuMDMuMDQ=&P0={encodedLogin}&P1=Q19SMg==&P2=NDYxMg==&P3=&P4={encodedPassword}&P5=&PC1=Tg==&PC2=Tg==\" -872656310 -872660104 -872656161 -872656310 -872654256 -872656071";

            // Запуск R2Client.exe с правами администратора и передача параметров
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "R2Client.exe"; // Замените на реальный путь к R2Client.exe, если он отличается
            startInfo.Verb = "runas"; // Запуск с правами администратора
            startInfo.Arguments = arguments;

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске R2Client.exe: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool IsUpdateRequired(UpdateInfo updateInfo)
        {
            foreach (var file in updateInfo.Files)
            {
                string localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles", file.Path, file.Name);
                if (!File.Exists(localFilePath) || !CompareFileContent(localFilePath, file.Hash))
                {
                    return true; // Если хотя бы один файл требует обновления, вернем true
                }
            }

            return false; // Если все файлы актуальны, вернем false
        }
        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            ClearUpdateLogs();
            UpdateInfo updateInfo = LoadUpdateInfoFromXml();

            if (updateInfo != null)
            {
                // Сбрасываем прогресс в 0 перед обновлением
                progressBar.Value = 0;

                // Используем возвращаемое значение метода UpdateClientFiles
                bool isUpdateRequired = await UpdateClientFiles(updateInfo, new Progress<int>(value =>
                {
                    // Обновляем значение прогресса на ProgressBar
                    progressBar.Value = value;
                }));

                // После завершения обновления снова сбрасываем прогресс в 0
                progressBar.Value = 0;

                if (!isUpdateRequired)
                {
                    MessageBox.Show("Обновление не требуется. Файлы уже актуальны.", "Обновление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Обновление файлов клиента завершено.", "Обновление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Ошибка загрузки информации об обновлениях.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        // Добавим метод для сериализации UpdateInfo обратно в XML (для проверки)
        private string SerializeUpdateInfoToXml(UpdateInfo updateInfo)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(typeof(UpdateInfo));

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    serializer.Serialize(memoryStream, updateInfo);
                    memoryStream.Position = 0;
                    xmlDoc.Load(memoryStream);
                }

                return xmlDoc.OuterXml;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while serializing UpdateInfo to XML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        private void MainLauncherForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void MainLauncherForm_Load(object sender, EventArgs e)
        {
            // Код, который должен выполняться при загрузке формы MainLauncherForm
        }
        private void ClearUpdateLogs()
        {
            // Код для очистки логов обновления
            // Например, можно удалить старые лог-файлы или очистить отображение логов в окне
        }

        private UpdateInfo LoadUpdateInfoFromXml()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    // Соберите полный URL для загрузки XML-файла
                    string fullUrl = apiUrl + xmlFilePath;

                    // Download XML data from the specified URL
                    string xmlData = client.DownloadString(fullUrl);

                    if (!string.IsNullOrEmpty(xmlData))
                    {
                        // Parse the XML data and populate the UpdateInfo object
                        UpdateInfo updateInfo = ParseUpdateInfo(xmlData);
                        return updateInfo;
                    }
                    else
                    {
                        MessageBox.Show("XML data is empty or invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while loading XML data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        private UpdateInfo ParseUpdateInfo(string xmlData)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlData);

                UpdateInfo updateInfo = new UpdateInfo();
                updateInfo.Folders = new List<FolderModel>();
                updateInfo.Files = new List<FileModel>();

                // Get the root element
                XmlNode rootFolderNode = xmlDoc.DocumentElement.SelectSingleNode("Folder");

                // Process the root element recursively to get the FolderModel and FileModel objects
                ProcessFolderModelNode(rootFolderNode, updateInfo.Folders, updateInfo.Files);

                return updateInfo;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while parsing XML data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }


        private void ProcessFolderModelNode(XmlNode folderNode, List<FolderModel> folders, List<FileModel> files)
        {
            FolderModel currentFolder = new FolderModel();
            currentFolder.Name = folderNode.SelectSingleNode("Name")?.InnerText;
            currentFolder.Folders = new List<FolderModel>();
            currentFolder.Files = new List<FileModel>();

            folders.Add(currentFolder);

            // Process Folders recursively
            foreach (XmlNode subFolderNode in folderNode.SelectSingleNode("Folders").ChildNodes)
            {
                ProcessFolderModelNode(subFolderNode, currentFolder.Folders, currentFolder.Files);
            }

            // Process Files
            foreach (XmlNode fileNode in folderNode.SelectSingleNode("Files").ChildNodes)
            {
                FileModel file = new FileModel();
                file.Name = fileNode.SelectSingleNode("Name")?.InnerText;
                file.Path = fileNode.SelectSingleNode("Path")?.InnerText;
                file.Size = Convert.ToInt64(fileNode.SelectSingleNode("Size")?.InnerText);
                file.Hash = fileNode.SelectSingleNode("Hash")?.InnerText;
                file.QuickUpdate = Convert.ToBoolean(fileNode.SelectSingleNode("QuickUpdate")?.InnerText);
                file.CheckHash = Convert.ToBoolean(fileNode.SelectSingleNode("CheckHash")?.InnerText);

                currentFolder.Files.Add(file);
            }
        }

        private async Task<bool> UpdateClientFiles(UpdateInfo updateInfo, IProgress<int> progress)
        {
            using (WebClient client = new WebClient())
            {
                // Устанавливаем обработчики событий для отображения прогресса загрузки
                client.DownloadProgressChanged += (s, e) =>
                {
                    // Передаем прогресс в объект progress
                    progress.Report(e.ProgressPercentage);
                };

                bool isUpdateRequired = false; // Переменная для отслеживания необходимости обновления

                // Перебираем все файлы и папки для обновления
                foreach (FolderModel folder in updateInfo.Folders)
                {
                    // Рекурсивно обновляем файлы в каждой папке
                    if (await UpdateFilesInFolder(client, folder, progress))
                    {
                        // Если хотя бы один файл требует обновления, устанавливаем флаг в true
                        isUpdateRequired = true;
                    }
                }

                return isUpdateRequired;
            }
        }


        private string CalculateFileHash(string filePath)
        {
            try
            {
                using (var crc32 = new Crc32Algorithm())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        byte[] hashBytes = crc32.ComputeHash(stream);
                        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок вычисления хеша
                MessageBox.Show($"Ошибка при вычислении хеша файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        async Task<bool> UpdateFilesInFolder(WebClient client, FolderModel folder, IProgress<int> progress)
        {
            // Код для обновления файлов в текущей папке
            bool isUpdateRequired = false;

            // Загрузка файлов
            foreach (FileModel file in folder.Files)
            {
                if (await UpdateFile(client, file, progress))
                {
                    // Если файл требует обновления, устанавливаем флаг в true
                    isUpdateRequired = true;
                }
            }

            // Рекурсивно обновляем файлы в подпапках
            foreach (FolderModel subFolder in folder.Folders)
            {
                if (await UpdateFilesInFolder(client, subFolder, progress))
                {
                    // Если хотя бы один файл в подпапке требует обновления, устанавливаем флаг в true
                    isUpdateRequired = true;
                }
            }

            return isUpdateRequired;
        }



        private string FixUrl(string url)
        {
            // Заменяем обратный слеш на прямой слеш
            return url.Replace("\\", "/");
        }
        private bool CompareFileContent(string localFilePath, string remoteFileHash)
        {
            // Рассчитываем хеш локального файла
            string localFileHash = CalculateFileHash(localFilePath);

            // Сравниваем хеш локального файла с хешем на сервере
            if (localFileHash == remoteFileHash)
            {
                // Хеши совпадают, содержимое файлов одинаково
                return true;
            }
            else
            {
                // Хеши не совпадают, содержимое файлов различается
                return false;
            }
        }

        async Task<bool> UpdateFile(WebClient client, FileModel file, IProgress<int> progress)
        {
            // Подготавливаем URL для загрузки архива и исправляем его
            string fileUrl = FixUrl($"{apiUrl}/{file.Path}/{file.Name}.zip");

            // Подготавливаем путь для сохранения загруженного архива
            string downloadPath = GetDownloadPath(file);

            // Проверяем, существует ли локальный файл
            bool localFileExists = File.Exists(downloadPath);

            // Если файл существует, проверяем, совпадает ли хеш и содержимое локального файла с хешем и содержимым на сервере
            if (localFileExists && CompareFileContent(downloadPath, file.Hash))
            {
                // Файл уже существует и его хеш совпадает, пропускаем загрузку и распаковку
                return false;
            }

            try
            {
                // Загружаем файл
                await client.DownloadFileTaskAsync(fileUrl, downloadPath);
            }
            catch (Exception ex)
            {
                // Обработка ошибок загрузки файла
                MessageBox.Show($"Ошибка при загрузке файла '{file.Name}': {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Подготавливаем путь для распаковки архива
            string extractFolderPath = Path.GetDirectoryName(downloadPath);

            // Распаковываем содержимое архива в папку extractFolderPath
            byte[] zipData = File.ReadAllBytes(downloadPath);
            using (MemoryStream stream = new MemoryStream(zipData))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                // Обрабатываем каждый файл в архиве
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Подготавливаем путь для распаковки файла
                    string extractFilePath = Path.Combine(extractFolderPath, entry.FullName);

                    // Создаем папки, если нужно, чтобы файл мог быть распакован
                    Directory.CreateDirectory(Path.GetDirectoryName(extractFilePath));

                    // Распаковываем файл
                    entry.ExtractToFile(extractFilePath, true);
                }
            }

            // Удаляем архив

            return true;
        }


        async Task DownloadFileWithRetry(HttpClient httpClient, string fileUrl, string downloadPath, int maxRetries = 3)
        {
            int retries = 0;
            while (retries < maxRetries)
            {
                try
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        // Проверяем успешность загрузки
                        response.EnsureSuccessStatusCode();

                        // Загружаем контент и сохраняем в файл
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        using (FileStream fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await contentStream.CopyToAsync(fileStream);
                        }

                        return;
                    }
                }
                catch
                {
                    // Если возникла ошибка загрузки, пауза перед следующей попыткой
                    retries++;
                    await Task.Delay(1000); // Пауза в 1 секунду (можно увеличить, если требуется)
                }
            }

            throw new Exception($"Не удалось загрузить файл {fileUrl} после {maxRetries} попыток.");
        }

        bool IsFileHashMatching(string filePath, string expectedHash)
        {
            if (string.IsNullOrEmpty(expectedHash))
            {
                // Если хеш файла не указан в XML, считаем, что он совпадает (вернем true)
                return true;
            }

            // Вычисляем CRC32 хеш файла
            string actualHash = CalculateCRC32Hash(filePath);

            // Сравниваем вычисленный хеш с ожидаемым хешем
            return actualHash == expectedHash;
        }

        string CalculateCRC32Hash(string filePath)
        {
            using (var crc32 = new Crc32Algorithm())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = crc32.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        async Task DownloadFile(WebClient client, FileModel file)
        {
            // Подготавливаем URL для загрузки архива и исправляем его
            string fileUrl = FixUrl($"{apiUrl}/{file.Path}/{file.Name}.zip"); // Здесь добавляем .zip к имени файла

            // Устанавливаем путь для сохранения загруженного архива
            string downloadPath = Path.Combine("GameFiles", file.Path, file.Name + ".zip"); // Здесь также добавляем .zip к имени файла

            // Проверяем, нужно ли загружать файл (если он уже существует и его хеш совпадает, пропускаем загрузку)
            if (File.Exists(downloadPath))
            {
                string localFileHash = CalculateFileHash(downloadPath);
                if (localFileHash == file.Hash)
                {
                    MessageBox.Show($"Файл '{file.Name}' уже актуален. Пропускаем загрузку.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    MessageBox.Show($"Хеш файла '{file.Name}' не совпадает. Требуемый хеш: {file.Hash}, фактический хеш: {localFileHash}", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show($"Файл '{file.Name}' не существует локально. Начинаем загрузку.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            try
            {
                // Загружаем файл
                await client.DownloadFileTaskAsync(fileUrl, downloadPath);
            }
            catch (Exception ex)
            {
                // Обработка ошибок загрузки файла
                MessageBox.Show($"Ошибка при загрузке файла '{file.Name}': {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Обновление ProgressBar с прогрессом загрузки в главном UI-потоке
            progressBar.Invoke((MethodInvoker)delegate
            {
                progressBar.Value = e.ProgressPercentage;
            });
        }
    }


    // Модель для представления информации об обновлениях из XML
    public class UpdateInfo
    {
        public List<FolderModel> Folders { get; set; }
        public List<FileModel> Files { get; set; }
    }

    public class FolderModel
    {
        public string Name { get; set; }
        public List<FolderModel> Folders { get; set; }
        public List<FileModel> Files { get; set; }
    }

    public class FileModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public string Hash { get; set; }
        public bool QuickUpdate { get; set; }
        public bool CheckHash { get; set; }
        public List<FileModel> FileUpdates { get; set; }
    }
}
