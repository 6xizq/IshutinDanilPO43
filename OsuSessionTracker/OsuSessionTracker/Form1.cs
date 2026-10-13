using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace OsuSessionTracker
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);
        private readonly string _configFile = "config.txt";
        private string _userId = "";
        private int _initialPlaycount = -1;
        private int _currentPlaycount = 0;
        private int _addedPlaycount = 0;
        private TimeSpan _sessionTime = TimeSpan.Zero;
        private bool _isRunning = false;
        private TabControl tabControl;
        private TabPage tabTracker;
        private TabPage tabAnalyzer;
        private Label lblAdded;
        private Label lblStats;
        private Label lblPace;
        private Label lblTimer;
        private Label lblCurrentMap;
        private Button btnToggle;
        private TextBox txtUserId;
        private Timer tmrWeb;
        private Timer tmrClock;
        private WebView2 webViewAnalyzer;
        private WebView2 webViewBackground;
        private bool _isBgWebViewInitialized = false;
        private bool _isAnalyzerWebViewInitialized = false;
        public Form1()
        {
            InitMyComponents();
            LoadSavedUserId();
        }
        private void InitMyComponents()
        {
            this.Text = "osu! Tracker + Skillset Analyzer";
            this.Size = new Size(340, 310);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.TopMost = true;
            this.BackColor = Color.FromArgb(20, 20, 24);
            this.ForeColor = Color.White;
            tabControl = new TabControl { Dock = DockStyle.Fill };
            tabTracker = new TabPage("Трекер") { BackColor = Color.FromArgb(20, 20, 24) };
            tabAnalyzer = new TabPage("Анализатор скиллов") { BackColor = Color.FromArgb(20, 20, 24) };
            tabControl.TabPages.Add(tabTracker);
            tabControl.TabPages.Add(tabAnalyzer);
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            lblAdded = new Label
            {
                Text = "+0",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 220, 130),
                Location = new Point(12, 10),
                Size = new Size(160, 50)
            };
            lblPace = new Label
            {
                Text = "0 плей/ч",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 180, 80),
                Location = new Point(185, 25),
                AutoSize = true
            };
            txtUserId = new TextBox
            {
                Text = "User ID",
                Font = new Font("Segoe UI", 9f),
                BackColor = Color.FromArgb(35, 35, 42),
                ForeColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(15, 68),
                Size = new Size(285, 23)
            };
            lblStats = new Label
            {
                Text = "Старт: -- | Сейчас: --",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(180, 180, 195),
                Location = new Point(15, 98),
                AutoSize = true
            };
            lblTimer = new Label
            {
                Text = "Время сессии: 00:00:00",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(150, 150, 170),
                Location = new Point(15, 120),
                AutoSize = true
            };
            lblCurrentMap = new Label
            {
                Text = "Карта: osu! не запущена",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(130, 160, 210),
                Location = new Point(15, 142),
                Size = new Size(285, 30)
            };
            btnToggle = new Button
            {
                Text = "Старт сессии",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(50, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(15, 180),
                Size = new Size(285, 35),
                Cursor = Cursors.Hand
            };
            btnToggle.FlatAppearance.BorderSize = 0;
            btnToggle.Click += BtnToggle_Click;
            tabTracker.Controls.Add(lblAdded);
            tabTracker.Controls.Add(lblPace);
            tabTracker.Controls.Add(txtUserId);
            tabTracker.Controls.Add(lblStats);
            tabTracker.Controls.Add(lblTimer);
            tabTracker.Controls.Add(lblCurrentMap);
            tabTracker.Controls.Add(btnToggle);
            webViewAnalyzer = new WebView2 { Dock = DockStyle.Fill };
            tabAnalyzer.Controls.Add(webViewAnalyzer);
            webViewBackground = new WebView2 { Size = new Size(0, 0), Visible = false };
            this.Controls.Add(webViewBackground);
            tmrWeb = new Timer { Interval = 5000 };
            tmrWeb.Tick += async (s, e) => await UpdateDataAsync();
            tmrClock = new Timer { Interval = 1000 };
            tmrClock.Tick += (s, e) =>
            {
                _sessionTime = _sessionTime.Add(TimeSpan.FromSeconds(1));
                lblTimer.Text = $"Время сессии: {_sessionTime:hh\\:mm\\:ss}";
                UpdatePace();
                UpdateMapTitle();
            };
            this.Controls.Add(tabControl);
        }
        private async void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabAnalyzer)
            {
                if (this.Width < 800)
                    this.Size = new Size(950, 650);
                if (!_isAnalyzerWebViewInitialized)
                {
                    await webViewAnalyzer.EnsureCoreWebView2Async(null);
                    _isAnalyzerWebViewInitialized = true;
                }
                webViewAnalyzer.CoreWebView2.Navigate("https://osu-skillset-analyzer.vercel.app/");
            }
        }
        private async void BtnToggle_Click(object sender, EventArgs e)
        {
            if (!_isRunning)
            {
                _userId = txtUserId.Text.Trim();
                if (string.IsNullOrEmpty(_userId) || _userId == "User ID")
                {
                    MessageBox.Show("Введите ваш User ID!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SaveUserId(_userId);
                btnToggle.Enabled = false;
                btnToggle.Text = "Подключение...";
                await UpdateDataAsync();
                if (_initialPlaycount != -1)
                {
                    _isRunning = true;
                    _sessionTime = TimeSpan.Zero;
                    txtUserId.Enabled = false;
                    tmrWeb.Start();
                    tmrClock.Start();
                    btnToggle.Text = "Завершить сессию";
                    btnToggle.BackColor = Color.FromArgb(220, 60, 60);
                }
                else
                {
                    MessageBox.Show("Не удалось получить данные профиля! Проверьте верность ID.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnToggle.Text = "Старт сессии";
                }
                btnToggle.Enabled = true;
            }
            else
            {
                _isRunning = false;
                tmrWeb.Stop();
                tmrClock.Stop();
                txtUserId.Enabled = true;
                btnToggle.Text = "Старт новой сессии";
                btnToggle.BackColor = Color.FromArgb(50, 130, 246);
                MessageBox.Show($"Сессия завершена!\n\nИтого сыграно: +{_addedPlaycount} плейкаунтов\nВремя сессии: {_sessionTime:hh\\:mm\\:ss}", "Итоги", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private async Task UpdateDataAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_userId) || _userId == "User ID") return;

                if (!_isBgWebViewInitialized)
                {
                    await webViewBackground.EnsureCoreWebView2Async(null);
                    _isBgWebViewInitialized = true;
                }
                var tcs = new TaskCompletionSource<bool>();
                void OnNavCompleted(object s, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
                {
                    webViewBackground.NavigationCompleted -= OnNavCompleted;
                    tcs.TrySetResult(e.IsSuccess);
                }
                webViewBackground.NavigationCompleted += OnNavCompleted;
                webViewBackground.CoreWebView2.Navigate($"https://osu.ppy.sh/users/{_userId}");
                await tcs.Task;
                string script = @"
            (function() {
                try {
                    // 1. Получаем Play Count из <dd class=""profile-stats__value"">
                    var pcEl = document.querySelector('.profile-stats__entry--key-play_count .profile-stats__value');
                    var playCountText = pcEl ? pcEl.innerText.replace(/,/g, '').trim() : null;

                    // 2. Получаем Total Play Time из .value-display
                    var playTimeText = null;
                    var labels = document.querySelectorAll('.value-display__label');
                    for (var i = 0; i < labels.length; i++) {
                        if (labels[i].textContent.trim() === 'Total Play Time') {
                            var valEl = labels[i].parentElement.querySelector('.value-display__value');
                            if (valEl) playTimeText = valEl.innerText.trim();
                            break;
                        }
                    }

                    if (playCountText) {
                        return JSON.stringify({
                            playCount: parseInt(playCountText),
                            playTime: playTimeText || ''
                        });
                    }
                } catch(e) {}
                return null;
            })();";
                int fetchedCount = -1;
                string fetchedTime = "";
                for (int i = 0; i < 15; i++)
                {
                    string rawJson = await webViewBackground.CoreWebView2.ExecuteScriptAsync(script);
                    if (!string.IsNullOrEmpty(rawJson) && rawJson != "null")
                    {
                        string json = System.Text.RegularExpressions.Regex.Unescape(rawJson.Trim('"'));
                        var matchCount = System.Text.RegularExpressions.Regex.Match(json, @"""playCount"":\s*(\d+)");
                        var matchTime = System.Text.RegularExpressions.Regex.Match(json, @"""playTime"":\s*""([^""]+)""");
                        if (matchCount.Success && int.TryParse(matchCount.Groups[1].Value, out int count))
                        {
                            fetchedCount = count;
                            if (matchTime.Success)
                                fetchedTime = matchTime.Groups[1].Value;
                            break;
                        }
                    }
                    await Task.Delay(400);
                }
                if (fetchedCount > 0)
                {
                    if (_initialPlaycount == -1)
                        _initialPlaycount = fetchedCount;
                    _currentPlaycount = fetchedCount;
                    _addedPlaycount = _currentPlaycount - _initialPlaycount;
                    lblAdded.Text = $"+{_addedPlaycount}";
                    lblStats.Text = $"Старт: {_initialPlaycount} | Сейчас: {_currentPlaycount}";
                    System.Diagnostics.Debug.WriteLine($"Play Time: {fetchedTime}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка фонового обновления: {ex.Message}");
            }
        }
        private void UpdatePace()
        {
            if (_sessionTime.TotalMinutes > 0.5)
            {
                double hours = _sessionTime.TotalHours;
                int pace = (int)(_addedPlaycount / hours);
                lblPace.Text = $"{pace} плей/ч";
            }
        }
        private void UpdateMapTitle()
        {
            Process[] processes = Process.GetProcessesByName("osu!");
            if (processes.Length > 0)
            {
                var sb = new System.Text.StringBuilder(256);
                GetWindowText(processes[0].MainWindowHandle, sb, sb.Capacity);
                string title = sb.ToString();
                if (title.StartsWith("osu! - "))
                    lblCurrentMap.Text = $"Карта: {title.Replace("osu! - ", "")}";
                else
                    lblCurrentMap.Text = "Карта: В меню / На паузе";
            }
            else
            {
                lblCurrentMap.Text = "Карта: osu! не запущена";
            }
        }
        private void LoadSavedUserId()
        {
            if (File.Exists(_configFile))
            {
                string saved = File.ReadAllText(_configFile).Trim();
                if (!string.IsNullOrEmpty(saved))
                    txtUserId.Text = saved;
            }
        }
        private void SaveUserId(string id)
        {
            try { File.WriteAllText(_configFile, id); } catch { }
        }
    }
}