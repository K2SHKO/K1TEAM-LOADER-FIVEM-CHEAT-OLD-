using System;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Management;
using System.Diagnostics;
using System.Net;
using System.IO;
using Discord;
using Discord.Webhook;
using System.Collections.Generic;


namespace KISHKO_BYPPREMIUM
{
    public partial class Form1 : Form
    {
        // zapis gdzie sie zapisuje plik config.txt ktory posaida w sobie zapisane haslo do loadera | Auto login
        private readonly string configFilePath = @"C:\mspdl\logs\config.txt";

        private static int loginCounter = 0;
        // Webhook link tutaj --->
        private const string webhookUrl = "https://discord.com/api/webhooks/";

        public Form1()
        {
            InitializeComponent();
        }





        private void label1_Click(object sender, EventArgs e)
        {
            try
            {
// kiszak wyjebal loader do wersji lokalnej, nie potrzeba bawic sie znow w ten syf z databasem
                string username = "K1TEAM";
                string password = "K1FREE";

                if (checkBox1.Checked && File.Exists(configFilePath))
                {
                    // If checkbox is checked and config.txt file exists, read logins from the file
                    string[] lines = File.ReadAllLines(configFilePath);
                    if (lines.Length == 2)
                    {
                        username = lines[0];
                        password = lines[1];
                    }
                    else
                    {
                        MessageBox.Show("Invalid login information in config file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Add a check for predefined login credentials
                if (txt_username.Text.Equals(username) && txt_password.Text.Equals(password))
                {
                    // Successful login
                    // Save user data, show the next form, etc.

                    // If checkbox is checked, save logins to the config.txt file
                    if (checkBox1.Checked)
                    {
                        SaveConfigToFile(username, password);
                    }

                    SendWebhookMessage();

                    Menuforms form2 = new Menuforms();
                    form2.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Invalid login details", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txt_username.Focus();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during login: {ex.Message}");
            }
        }




        //webhook pokazujacy kto uruchomil loader
        private void SendWebhookMessage()
        {
            try
            {
                var webhookClient = new DiscordWebhookClient(webhookUrl);

  
                string userIpAddress = GetIpAddress();


                var embedBuilder = new EmbedBuilder()
                    .WithTitle("K1TEAM LAUNCHER")
                    .WithDescription($"Launcher został uruchomiony przez użytkownika z IP: `{userIpAddress}` Wersja Launchera: `2.5.7`")
                    .WithColor(new Color(0, 255, 255));  

                webhookClient.SendMessageAsync(null, false, new List<Embed> { embedBuilder.Build() }).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending webhook message: {ex.Message}");
            }
        }

        private string GetIpAddress()
    {
        string ipAddress = string.Empty;

        try
        {
            ipAddress = new WebClient().DownloadString("https://api.myip.com").Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas pobierania adresu IP: {ex.Message}");
        }

        return ipAddress;
    }



        private void Clear_button_Click(object sender, EventArgs e)
        {
            txt_username.Clear();
            txt_password.Clear();
            txt_username.Focus();
        }

        private void button_exit_click(object sender, EventArgs e)
        {
            DialogResult res;
            res = MessageBox.Show("Do you want to exit", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                Application.Exit();
            }
            else
            {
                this.Show();
            }
        }

        private void txt_username_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void txt_password_TextChanged(object sender, EventArgs e)
        {
            txt_password.PasswordChar = '*';
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForUpdate();
        }

        private string remoteVersionUrl = "https://#website/version.txt";
        private string downloadUrl = "https://#website/K1_TIM_FREE_NO_VIRUS.rar";

        private bool updateButtonClicked = false;
        private Version currentVersion = new Version("2.6.7");

        private void CheckForUpdate()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string remoteVersion = client.DownloadString(remoteVersionUrl).Trim();

                    if (Version.TryParse(remoteVersion, out Version remote) && remote > currentVersion && !updateButtonClicked)
                    {
                        DialogResult result = MessageBox.Show($"Dostępna jest nowa wersja aplikacji ({remote}). Czy chcesz ją pobrać i zainstalować?", "Aktualizacja dostępna", MessageBoxButtons.YesNo);

                        if (result == DialogResult.Yes)
                        {
                            Process.Start(downloadUrl);
                            updateButtonClicked = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas sprawdzania aktualizacji: {ex.Message}", "Błąd");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (File.Exists(configFilePath))
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(configFilePath);
                        if (lines.Length == 2)
                        {
                            txt_username.Text = lines[0];
                            txt_password.Text = lines[1];
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reading login information from config file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                txt_username.Clear();
                txt_password.Clear();
            }
        }


        private void SaveConfigToFile(string username, string password)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(configFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllLines(configFilePath, new[] { username, password });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving login information to config file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
