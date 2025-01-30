using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace LoL_AutoType
{
    public partial class MainForm : Form
    {
        private const string CredentialsFile = "credentials.dat";
        private const string EncryptionKey = "l3ItKQCfzKLMU4t4lpVHfw=="; // Replace with a secure, unique key

        public MainForm()
        {
            InitializeComponent();
            LoadCredentials();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd)
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        private void BringWindowToFront(IntPtr hWnd)
        {
            ShowWindow(hWnd, SW_RESTORE);
            ShowWindow(hWnd, SW_SHOW);
            SetForegroundWindow(hWnd);
            Thread.Sleep(500); // Give Windows time to bring the window forward
        }
        private string GetCredentialsPath()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LoL_AutoType");
            Directory.CreateDirectory(appDataPath);
            return Path.Combine(appDataPath, CredentialsFile);
        }
        private void btnAutoType_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            IntPtr hWnd = FindWindow(null, "Riot Client");
            if (hWnd != IntPtr.Zero)
            {
                BringWindowToFront(hWnd);
                Thread.Sleep(2000); // Waits 2 second
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(200); // Waits 0.2 second
                SendKeys.SendWait("{TAB}");
                SendKeys.SendWait(username);
                SendKeys.SendWait("{TAB}");
                SendKeys.SendWait(password);
                SendKeys.SendWait("{ENTER}");
            }
            else
            {
                MessageBox.Show("League of Legends login window not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveCredentials(txtUsername.Text, txtPassword.Text);
            MessageBox.Show("Credentials saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveCredentials(string username, string password)
        {
            try
            {
                string credentialsData = $"{username}:{password}";
                string encryptedData = EncryptString(credentialsData, EncryptionKey);
                File.WriteAllText(GetCredentialsPath(), encryptedData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving credentials: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCredentials()
        {
            try
            {
                string credentialsPath = GetCredentialsPath();
                if (File.Exists(credentialsPath))
                {
                    string encryptedData = File.ReadAllText(credentialsPath);
                    string decryptedData = DecryptString(encryptedData, EncryptionKey);
                    string[] credentials = decryptedData.Split(':');
                    if (credentials.Length == 2)
                    {
                        txtUsername.Text = credentials[0];
                        txtPassword.Text = credentials[1];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading credentials: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private static string EncryptString(string plainText, string key)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32));
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        private static string DecryptString(string cipherText, string key)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32));
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

    }
}
