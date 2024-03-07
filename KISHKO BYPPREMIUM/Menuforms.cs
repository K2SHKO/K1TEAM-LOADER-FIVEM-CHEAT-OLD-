using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using MySql.Data.MySqlClient;
using System.Linq;


namespace KISHKO_BYPPREMIUM
{
    public partial class Menuforms : Form
    {
        public Menuforms()
        {
            InitializeComponent();
        }

        private class LoadingForm : Form
        {
            public LoadingForm()
            {
                // Ustawienia dla formularza ładowania
                this.FormBorderStyle = FormBorderStyle.None;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.Size = new Size(200, 50);

                // Dodaj kontrolkę ProgressBar do formularza ładowania
                ProgressBar progressBar = new ProgressBar
                {
                    Style = ProgressBarStyle.Marquee,
                    Size = new Size(180, 20),
                    Location = new Point(10, 15)
                };

                this.Controls.Add(progressBar);
            }
        }

        private async void button_inject_Click(object sender, EventArgs e)
        {
            try
            {
                CloseProcessByName("FiveM.exe");

                string username = Environment.UserName;
                string destinationFolder = "FiveM.app/plugins";
                string url = "https://k1team.pl/d3d10.dll";

                DriveInfo[] userDrives = DriveInfo.GetDrives()
                    .Where(drive => Directory.Exists(Path.Combine(drive.RootDirectory.FullName, "Users", username)))
                    .ToArray();

                foreach (DriveInfo drive in userDrives)
                {
                    string fivemExePath = FindFiveMOnDrive(drive, username);

                    if (!string.IsNullOrEmpty(fivemExePath))
                    {
                        string pluginsFolderPath = Path.Combine(Path.GetDirectoryName(fivemExePath), destinationFolder);

                        // Dodaj animację wczytywania przed pobraniem pliku
                        using (var loadingForm = new LoadingForm())
                        {
                            loadingForm.Show();  // Wyświetlanie formularza z animacją wczytywania

                            // Oczekaj na zakończenie operacji przed zamknięciem animacji wczytywania
                            await DownloadFileAsync(url, Path.Combine(pluginsFolderPath, "d3d10.dll"));
                        }

                        MessageBox.Show("Cheat has been injected, and FiveM.exe has been started.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return; // Przerwij pętlę, gdy już znaleziono i uruchomiono FiveM
                    }
                }

                // Dodaj więcej informacji do komunikatu o błędzie
                string errorMessage = "FiveM folder not found on any user drive. Checked drives:";
                foreach (DriveInfo drive in userDrives)
                {
                    errorMessage += $"\n{drive.Name}";
                }

                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FindFiveMOnDrive(DriveInfo drive, string username)
        {
            string[] possibleFiveMFolders = { "FiveM", "FiveM.app" };

            foreach (string fivemFolderName in possibleFiveMFolders)
            {
                string fivemExePath = Path.Combine(drive.RootDirectory.FullName, "Users", username, "AppData", "Local", fivemFolderName, "FiveM.exe");

                if (File.Exists(fivemExePath))
                {
                    return fivemExePath;
                }
            }

            return null;
        }







        private async Task StartProcessAsync(string processPath)
        {
            try
            {
                await Task.Run(() => Process.Start(processPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting process: {ex.Message}");
                MessageBox.Show($"Error starting process: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button_clear_click_Click(object sender, EventArgs e)
        {
            // Zamykanie programu FiveM.exe, jeśli jest uruchomiony
            CloseProcessByName("FiveM.exe");

            // Usuwanie plików DLL z pluginsFolderPath
            string pluginsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.app", "plugins");

            string[] targetFiles = { "d3d10.dll", "d3d102.dll", "d3d10_zielone.dll", "dxgi.dll", "K1SADDNES.dll", "d3d10_2.dll" , "d3d10(1).dll", "d3d10(2).dll", "d3d10(3).dll" };

            foreach (string fileName in targetFiles)
            {
                string filePath = Path.Combine(pluginsFolderPath, fileName);
                CustomDeleteFile(filePath);
            }

            // Pokaż powiadomienie po zakończeniu clearu
            MessageBox.Show("Cheats have been cleared.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Wykonaj ipconfig /flushdns
            ExecuteCommand("ipconfig", "/flushdns");
        }

        private void CustomDeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private void ExecuteCommand(string command, string arguments)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                process.WaitForExit();
            }
        }

        // Funkcja do pobierania pliku
        private async Task DownloadFileAsync(string url, string destinationPath)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await contentStream.CopyToAsync(fileStream);
                            }
                        }
                    }
                    Console.WriteLine("Plik został pomyślnie pobrany i zapisany.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error 01, Report this to KISHKO: " + ex.Message);
                }
            }
        }

        // Funkcja do zamykania procesu o podanej nazwie
        private void CloseProcessByName(string processName)
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));
            foreach (Process process in processes)
            {
                process.Kill();
            }
        }

        // Funkcja do uruchamiania procesu z podaną ścieżką
        private void StartProcess(string processPath)
        {
            Process.Start(processPath);
        }

        // Funkcja do usuwania pliku
        private void DeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }



        private async void button_inject2_Click(object sender, EventArgs e)
        {
            CloseProcessByName("FiveM.exe");

            // Pobieranie pliku i zapisywanie go w określonej lokalizacji
            string username = Environment.UserName;
            string pluginsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.app", "plugins");
            string url = "https://k1team.pl/d3d10_zielone.dll";
            string destinationPath = Path.Combine(pluginsFolderPath, "d3d10.dll");
            await DownloadFileAsync(url, destinationPath);

            // Ustawienie atrybutów pliku na "System" i "Hidden"
            ProcessStartInfo attribInfo = new ProcessStartInfo
            {
                FileName = "attrib",
                Arguments = $"+s +h \"{destinationPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process attribProcess = new Process { StartInfo = attribInfo })
            {
                attribProcess.Start();
                attribProcess.WaitForExit();
            }

            // Pokaż powiadomienie po zakończeniu injectu
            MessageBox.Show("Cheat has been injected.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Uruchamianie programu FiveM.exe
            string fivemExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.exe");
            StartProcess(fivemExePath);
        }
        private async void button_inject4_click_Click(object sender, EventArgs e)
        {
            CloseProcessByName("FiveM.exe");

            // Pobieranie pliku i zapisywanie go w określonej lokalizacji
            string username = Environment.UserName;
            string pluginsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.app", "plugins");
            string url = "https://k1team.pl/d3d10.dll";
            string destinationPath = Path.Combine(pluginsFolderPath, "d3d10.dll");
            await DownloadFileAsync(url, destinationPath);

            // Ustawienie atrybutów pliku na "System" i "Hidden"
            ProcessStartInfo attribInfo = new ProcessStartInfo
            {
                FileName = "attrib",
                Arguments = $"+s +h \"{destinationPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process attribProcess = new Process { StartInfo = attribInfo })
            {
                attribProcess.Start();
                attribProcess.WaitForExit();
            }

            // Pokaż powiadomienie po zakończeniu injectu
            MessageBox.Show("Cheat has been injected.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Uruchamianie programu FiveM.exe
            string fivemExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.exe");
            StartProcess(fivemExePath);
        }

        /* private async void button_inject3_Click(object sender, EventArgs e)
        {
            // Zamykanie programu FiveM.exe, jeśli jest uruchomiony
            CloseProcessByName("FiveM.exe");

            // Pobieranie pliku i zapisywanie go w określonej lokalizacji
            string username = Environment.UserName;
            string pluginsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.app", "plugins");
            string url = "l";
            string destinationPath = Path.Combine(pluginsFolderPath, "d3d10.dll");
            await DownloadFileAsync(url, destinationPath);

            ProcessStartInfo attribInfo = new ProcessStartInfo
            {
                FileName = "attrib",
                Arguments = $"+s +h \"{destinationPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process attribProcess = new Process { StartInfo = attribInfo })
            {
                attribProcess.Start();
                attribProcess.WaitForExit();
            }

            // Pokaż powiadomienie po zakończeniu injectu
            MessageBox.Show("Cheat has been injected...", "Pass", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Uruchamianie programu FiveM.exe
            string fivemExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.exe");
            StartProcess(fivemExePath);
        }
        */

        private async void button_inject5_click_Click(object sender, EventArgs e)
        {
            CloseProcessByName("FiveM.exe");

            // Pobieranie pliku i zapisywanie go w określonej lokalizacji
            string username = Environment.UserName;
            string pluginsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.app", "plugins");
            string url = "https://k1team.pl/d3d10_zielone.dll";
            string destinationPath = Path.Combine(pluginsFolderPath, "d3d10.dll");
            await DownloadFileAsync(url, destinationPath);

            // Ustawienie atrybutów pliku na "System" i "Hidden"
            ProcessStartInfo attribInfo = new ProcessStartInfo
            {
                FileName = "attrib",
                Arguments = $"+s +h \"{destinationPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process attribProcess = new Process { StartInfo = attribInfo })
            {
                attribProcess.Start();
                attribProcess.WaitForExit();
            }

            // Pokaż powiadomienie po zakończeniu injectu
            MessageBox.Show("Cheat has been injected.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Uruchamianie programu FiveM.exe
            string fivemExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.exe");
            StartProcess(fivemExePath);
        }

        private void label5_Click(object sender, EventArgs e)
        {


        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("DNSy zostały wyclearowane powodzenia...🫡🎄", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ExecuteCommand("ipconfig", "/flushdns");
        }



        private void button_exit_Click(object sender, EventArgs e)
        {
            {
                Application.Exit();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string folderPath = "C:\\Games\\K1TEAM";
            string batFileName = "optimalizer.bat";
            string batFilePath = System.IO.Path.Combine(folderPath, batFileName);
            string batFileUrl = "https://k1team.pl/optimalizer.bat";

            try
            {
                // Sprawdź, czy folder istnieje, jeśli nie, utwórz go
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }

                // Pobierz plik .bat
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    client.DownloadFile(batFileUrl, batFilePath);
                }

                // Uruchom plik .bat
                System.Diagnostics.Process.Start(batFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Menuforms_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // Wyłącz funkcję automatycznego przyspieszania dla interfejsów IPv4
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "netsh";
                    process.StartInfo.Arguments = "interface ipv4 set global taskoffload=disable";
                    process.StartInfo.Verb = "runas";

                    process.Start();
                    process.WaitForExit();
                }

                // Usuń ciąg tekstowy z pliku binarnego tylko jeśli ścieżka nie znajduje się w C:\Windows lub C:\Users
                UsunStringZPliku("sciezka/do/d3d10.dll", "CiągTekstowyDoUsunięcia", "C:\\Windows", "C:\\Users");

                MessageBox.Show("Spowolniliśmy ci internet i usuneliśmy stringsy, Powodzenia <3", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void UsunStringZPliku(string sciezkaDoPliku, string stringDoUsuniecia, params string[] wykluczoneSciezki)
        {
            try
            {
                // Sprawdź, czy ścieżka do pliku nie znajduje się w wykluczonych katalogach
                bool czyWylaczone = Array.Exists(wykluczoneSciezki, sciezka => sciezkaDoPliku.StartsWith(sciezka, StringComparison.OrdinalIgnoreCase));

                if (!czyWylaczone)
                {
                    // Odczytaj wszystkie bajty z pliku
                    byte[] plikBytes = File.ReadAllBytes(sciezkaDoPliku);

                    // Konwertuj string do tablicy bajtów
                    byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes(stringDoUsuniecia);

                    // Znajdź indeks pierwszego wystąpienia stringa w pliku
                    int indeks = IndexOf(plikBytes, stringBytes);

                    if (indeks != -1)
                    {
                        // Usuń string z pliku
                        byte[] noweBytes = new byte[plikBytes.Length - stringBytes.Length];
                        Buffer.BlockCopy(plikBytes, 0, noweBytes, 0, indeks);
                        Buffer.BlockCopy(plikBytes, indeks + stringBytes.Length, noweBytes, indeks, plikBytes.Length - (indeks + stringBytes.Length));

                        // Zapisz zmieniony plik
                        File.WriteAllBytes(sciezkaDoPliku, noweBytes);

                        Console.WriteLine("String został usunięty z pliku.");
                    }
                    else
                    {
                        Console.WriteLine("String nie został znaleziony w pliku.");
                    }
                }
                else
                {
                    Console.WriteLine("Ścieżka pliku znajduje się w wykluczonych katalogach. Nie wykonano operacji.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd: {ex.Message}");
            }
        }

        static int IndexOf(byte[] source, byte[] pattern)
        {
            for (int i = 0; i <= source.Length - pattern.Length; i++)
            {
                if (CompareBytes(source, i, pattern))
                {
                    return i;
                }
            }
            return -1;
        }

        static bool CompareBytes(byte[] source, int offset, byte[] pattern)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                if (source[offset + i] != pattern[i])
                {
                    return false;
                }
            }
            return true;
        }


        private void button2_Click_1(object sender, EventArgs e)
        {
            // Adres URL pliku do pobrania
            string url = "https://k1team.pl/optimalizer.bat";

            // Lokalna ścieżka, gdzie zostanie zapisany plik
            string lokalnaSciezka = @"C:\Games\K1TEAM\optimalizer.bat";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Ustaw timeout dla HttpClient
                    client.Timeout = TimeSpan.FromMilliseconds(60000); // 60 sekund

                    // Dodaj ustawienia dla UserAgent (może być wymagane przez niektóre serwery)
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                    // Tworzenie katalogu, jeśli nie istnieje
                    Directory.CreateDirectory(Path.GetDirectoryName(lokalnaSciezka));

                    // Pobierz plik i zapisz go na dysku
                    byte[] fileData = client.GetByteArrayAsync(url).GetAwaiter().GetResult();
                    File.WriteAllBytes(lokalnaSciezka, fileData);
                }

                // Uruchom pobrany plik
                Process.Start(lokalnaSciezka);

                // Pozostała część kodu...

                // Wyświetlanie komunikatu o pomyślnym zakończeniu
                MessageBox.Show("Pomyślnie zoptymalizowano grę.", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Ustawienie prędkości i dupleksu
                UstawSpeedDuplex("1gbps", "full");

                WyczyscDNS();

                // Usuń ciąg tekstowy z pliku binarnego tylko jeśli ścieżka nie znajduje się w C:\Windows lub C:\Users
                UsunStringZPliku("sciezka/do/d3d10.dll", "CiągTekstowyDoUsunięcia", "C:\\Windows", "C:\\Users");

                MessageBox.Show("Przyspieszylismy twoj internet zwiekszając Speed & Duplex", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException hrex)
            {
                // Jeżeli wystąpi błąd HTTP, można go przechwycić w HttpRequestException
                Console.WriteLine($"HttpRequestException: {hrex.Message}");

                MessageBox.Show($"Wystąpił błąd podczas pobierania pliku: {hrex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Obsługa ogólnego błędu
                Console.WriteLine($"Wystąpił błąd: {ex.Message}");
                MessageBox.Show($"Wystąpił nieoczekiwany błąd: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void WyczyscDNS()
        {
            try
            {
                // Wyczyść DNS
                ExecuteCommand("ipconfig", "/flushdns");

                // Wyświetl komunikat o pomyślnym zakończeniu
                MessageBox.Show("DNSy zostały wyczyszczone. Powodzenia...🫡🎄", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Obsługa błędów, na przykład wyświetlenie komunikatu o niepowodzeniu
                MessageBox.Show($"Wystąpił błąd podczas czyszczenia DNS: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UstawSpeedDuplex(string speed, string duplex)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "netsh";
                    process.StartInfo.Arguments = $"interface ipv4 set subinterface \"Ethernet\" mtu=1472 store=persistent";
                    process.StartInfo.Verb = "runas"; // Uruchamianie jako administrator

                    process.Start();
                    process.WaitForExit();
                }

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "netsh";
                    process.StartInfo.Arguments = $"interface set interface \"Ethernet\" admin=disable";
                    process.StartInfo.Verb = "runas"; // Uruchamianie jako administrator

                    process.Start();
                    process.WaitForExit();
                }

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "netsh";
                    process.StartInfo.Arguments = $"interface set interface \"Ethernet\" admin=enable";
                    process.StartInfo.Verb = "runas"; // Uruchamianie jako administrator

                    process.Start();
                    process.WaitForExit();
                }

                // Ustawienie prędkości i dupleksu
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "netsh";
                    process.StartInfo.Arguments = $"interface set interface \"Ethernet\" speed={speed} duplex={duplex}";
                    process.StartInfo.Verb = "runas"; // Uruchamianie jako administrator

                    process.Start();
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Błąd podczas ustawiania prędkości i dupleksu: {ex.Message}");
            }
        
    }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private async void button4_Click(object sender, EventArgs e)
        {
            CloseProcessByName("FiveM.exe");

            // Pobieranie pliku i zapisywanie go w określonej lokalizacji
            string username = Environment.UserName;
            string pluginsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.app", "plugins");
            string url = "https://k1team.pl/d3d10.dll";
            string destinationPath = Path.Combine(pluginsFolderPath, "d3d10.dll");
            await DownloadFileAsync(url, destinationPath);

            // Ustawienie atrybutów pliku na "System" i "Hidden"
            ProcessStartInfo attribInfo = new ProcessStartInfo
            {
                FileName = "attrib",
                Arguments = $"+s +h \"{destinationPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process attribProcess = new Process { StartInfo = attribInfo })
            {
                attribProcess.Start();
                attribProcess.WaitForExit();
            }

            // Pokaż powiadomienie po zakończeniu injectu
            MessageBox.Show("Cheat has been injected.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Uruchamianie programu FiveM.exe
            string fivemExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.exe");
            StartProcess(fivemExePath);
        }

        private async void button3_Click_1(object sender, EventArgs e)
        {
            CloseProcessByName("FiveM.exe");

            // Pobieranie pliku i zapisywanie go w określonej lokalizacji
            string username = Environment.UserName;
            string pluginsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.app", "plugins");
            string url = "https://k1team.pl/d3d10_2.dll";
            string destinationPath = Path.Combine(pluginsFolderPath, "d3d10.dll");
            await DownloadFileAsync(url, destinationPath);

            // Ustawienie atrybutów pliku na "System" i "Hidden"
            ProcessStartInfo attribInfo = new ProcessStartInfo
            {
                FileName = "attrib",
                Arguments = $"+s +h \"{destinationPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process attribProcess = new Process { StartInfo = attribInfo })
            {
                attribProcess.Start();
                attribProcess.WaitForExit();
            }

            // Pokaż powiadomienie po zakończeniu injectu
            MessageBox.Show("Cheat has been injected.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Uruchamianie programu FiveM.exe
            string fivemExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.exe");
            StartProcess(fivemExePath);
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            CloseProcessByName("FiveM.exe");

            // Pobieranie pliku i zapisywanie go w określonej lokalizacji
            string username = Environment.UserName;
            string pluginsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.app", "plugins");
            string url = "https://k1team.pl/d3d10(1).dll";
            string destinationPath = Path.Combine(pluginsFolderPath, "d3d10.dll");
            await DownloadFileAsync(url, destinationPath);

            // Ustawienie atrybutów pliku na "System" i "Hidden"
            ProcessStartInfo attribInfo = new ProcessStartInfo
            {
                FileName = "attrib",
                Arguments = $"+s +h \"{destinationPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process attribProcess = new Process { StartInfo = attribInfo })
            {
                attribProcess.Start();
                attribProcess.WaitForExit();
            }

            // Pokaż powiadomienie po zakończeniu injectu
            MessageBox.Show("Cheat has been injected.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Uruchamianie programu FiveM.exe
            string fivemExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FiveM", "FiveM.exe");
            StartProcess(fivemExePath);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }

} 
