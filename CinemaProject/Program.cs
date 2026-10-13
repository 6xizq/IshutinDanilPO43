using System.Text;
using System.Text.Json;
using System.Drawing.Drawing2D;

namespace CinemaProject;

public class Session
{
    public string Name { get; set; } = "";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public decimal Price { get; set; }
    public bool IsChild { get; set; }
    public override string ToString() => Name;
}

public class SeatState
{
    public int Number { get; set; }
    public int Row { get; set; }
    public bool IsVip { get; set; }
    public string SessionKey { get; set; } = "";
    public bool Reserved { get; set; }
    public bool Purchased { get; set; }
}

public class Ticket
{
    public string SessionName { get; set; } = "";
    public DateTime SessionStart { get; set; }
    public int SeatNumber { get; set; }
    public decimal Price { get; set; }
    public DateTime PurchaseTime { get; set; }
}

public class CinemaData
{
    public List<Session> Sessions { get; set; } = new();
    public List<SeatState> Seats { get; set; } = new();
    public List<Ticket> Tickets { get; set; } = new();
    public bool QuarantineMode { get; set; }
}

static class Storage
{
    static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "cinema-data.json");
    static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static CinemaData Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var loaded = JsonSerializer.Deserialize<CinemaData>(File.ReadAllText(FilePath), Options);
                if (loaded is not null)
                {
                    Normalize(loaded);
                    return loaded;
                }
            }
        }
        catch { }
        return CreateDefault();
    }

    public static void Save(CinemaData data)
    {
        Normalize(data);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(data, Options));
    }

    public static void Normalize(CinemaData data)
    {
        if (data.Sessions.Count == 0)
        {
            data.Sessions = DefaultSessions();
        }

        // Migration from the old version where 50 seats were shared by all sessions.
        if (data.Seats.Count > 0 && data.Seats.All(x => string.IsNullOrWhiteSpace(x.SessionKey)))
        {
            var old = data.Seats.ToList();
            data.Seats.Clear();
            foreach (var s in data.Sessions)
            {
                var key = SessionKey(s);
                foreach (var baseSeat in old)
                {
                    data.Seats.Add(new SeatState
                    {
                        Number = baseSeat.Number,
                        Row = baseSeat.Row,
                        IsVip = baseSeat.IsVip,
                        SessionKey = key,
                        Reserved = baseSeat.Reserved,
                        Purchased = baseSeat.Purchased
                    });
                }
            }
        }

        foreach (var session in data.Sessions)
            EnsureSeats(data, session);
    }

    public static string SessionKey(Session session) =>
        $"{session.Name}|{session.Start:yyyyMMddHHmm}|{session.End:yyyyMMddHHmm}";

    public static List<SeatState> GetSeats(CinemaData data, Session session)
    {
        EnsureSeats(data, session);
        var key = SessionKey(session);
        return data.Seats.Where(x => x.SessionKey == key).OrderBy(x => x.Number).ToList();
    }

    static void EnsureSeats(CinemaData data, Session session)
    {
        var key = SessionKey(session);
        if (data.Seats.Any(x => x.SessionKey == key)) return;

        for (int i = 1; i <= 50; i++)
        {
            int row = (i - 1) / 10 + 1;
            data.Seats.Add(new SeatState
            {
                Number = i,
                Row = row,
                IsVip = i <= 12,
                SessionKey = key
            });
        }
    }

    static List<Session> DefaultSessions()
    {
        var d = DateTime.Today;
        return new()
        {
            new Session { Name = "Детский сеанс", Start = d.AddHours(10), End = d.AddHours(12), Price = 1500, IsChild = true },
            new Session { Name = "2 сеанс", Start = d.AddHours(12).AddMinutes(30), End = d.AddHours(14).AddMinutes(30), Price = 2200 },
            new Session { Name = "3 сеанс", Start = d.AddHours(15), End = d.AddHours(17), Price = 2500 },
            new Session { Name = "4 сеанс", Start = d.AddHours(17).AddMinutes(30), End = d.AddHours(19).AddMinutes(30), Price = 2800 },
            new Session { Name = "5 сеанс", Start = d.AddHours(20), End = d.AddHours(22), Price = 3000 }
        };
    }

    static CinemaData CreateDefault()
    {
        var data = new CinemaData { Sessions = DefaultSessions() };
        foreach (var s in data.Sessions)
            EnsureSeats(data, s);
        return data;
    }
}

static class Ui
{
    public static readonly Color Ink = Color.FromArgb(42, 47, 52);
    public static readonly Color Ink2 = Color.FromArgb(24, 34, 45);
    public static readonly Color Cream = Color.FromArgb(247, 246, 238);
    public static readonly Color Green = Color.FromArgb(72, 180, 100);
    public static readonly Color Blue = Color.FromArgb(76, 139, 215);
    public static readonly Color Red = Color.FromArgb(191, 70, 70);
    public static readonly Color Gold = Color.FromArgb(255, 230, 132);
    public static readonly Color SeatFree = Color.FromArgb(191, 251, 202);
    public static readonly Color SeatUnavailable = Color.FromArgb(221, 223, 221);
    public static readonly Color SeatReserved = Color.FromArgb(235, 179, 87);
    public static readonly Color SeatPurchased = Color.FromArgb(198, 84, 84);

    public static Button Button(string text, Color back, int width = 170, int height = 40)
    {
        var b = new Button
        {
            Text = text,
            Width = width,
            Height = height,
            BackColor = back,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance = { BorderSize = 0 },
            Font = new Font("Segoe UI Semibold", 10F),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 10)
        };
        return b;
    }

    public static Label Label(string text, float size = 10, FontStyle style = FontStyle.Regular, Color? color = null)
        => new()
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Segoe UI", size, style),
            ForeColor = color ?? Color.White
        };

    public static void Round(Control c, int radius = 10)
    {
        c.Resize += (_, _) =>
        {
            using var path = new GraphicsPath();
            var r = Math.Min(radius, Math.Min(c.Width, c.Height) / 2);
            path.AddArc(0, 0, r * 2, r * 2, 180, 90);
            path.AddArc(c.Width - r * 2, 0, r * 2, r * 2, 270, 90);
            path.AddArc(c.Width - r * 2, c.Height - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(0, c.Height - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            c.Region = new Region(path);
        };
    }
}

public class MainForm : Form
{
    readonly CinemaData data;
    ComboBox sessionBox = new();
    DateTimePicker datePicker = new();
    CheckBox quarantine = new();
    FlowLayoutPanel quickActions = new();
    TableLayoutPanel seatGrid = new();
    Label selectedLabel = new();
    Label modeLabel = new();
    Label hallInfo = new();
    Label status = new();
    Button buyButton = null!;
    Button reserveButton = null!;
    Button refundButton = null!;
    Button editorButton = null!;
    Button aboutButton = null!;
    readonly List<int> selectedSeats = new();

    Session? CurrentSession => sessionBox.SelectedItem as Session;

    public MainForm()
    {
        data = Storage.Load();

        Text = "Кинотеатр — зал и продажа билетов";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1024, 768);
        ClientSize = new Size(1024, 768);
        Font = new Font("Segoe UI", 10F);
        BackColor = Ui.Cream;

        BuildUi();
        LoadSessions();
        RenderSeats();
        FormClosing += (_, _) => Storage.Save(data);
    }

    void BuildUi()
    {
        Controls.Clear();

        // Root layout is used instead of mixed Dock/BringToFront behavior.
        // This guarantees a stable 1024x768 layout: header and footer span
        // the full window, while the left sidebar and hall are placed below.
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = Ui.Cream
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 228));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        Controls.Add(root);

        var header = BuildHeader();
        root.Controls.Add(header, 0, 0);
        root.SetColumnSpan(header, 2);

        var left = BuildSidebar();
        root.Controls.Add(left, 0, 1);

        var content = BuildHallContent();
        root.Controls.Add(content, 1, 1);

        status = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Ui.Ink,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 9.5F),
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        root.Controls.Add(status, 0, 2);
        root.SetColumnSpan(status, 2);
    }

    Panel BuildHeader()
    {
        var header = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Ui.Ink,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        var title = Ui.Label("КИНОТЕАТР", 24, FontStyle.Bold);
        title.Location = new Point(24, 16);
        header.Controls.Add(title);

        var subtitle = Ui.Label("Главный зал  •  50 мест  •  12 VIP", 10, FontStyle.Regular, Color.FromArgb(210, 215, 218));
        subtitle.Location = new Point(26, 55);
        header.Controls.Add(subtitle);

        var sessionCaption = Ui.Label("СЕАНС", 9, FontStyle.Bold, Color.FromArgb(180, 188, 194));
        sessionCaption.Location = new Point(250, 17);
        header.Controls.Add(sessionCaption);

        sessionBox = new ComboBox
        {
            Location = new Point(250, 40),
            Width = 280,
            Height = 32,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 10F)
        };
        sessionBox.SelectedIndexChanged += (_, _) =>
        {
            selectedSeats.Clear();
            RenderSeats();
        };
        header.Controls.Add(sessionBox);

        var dateCaption = Ui.Label("ДАТА", 9, FontStyle.Bold, Color.FromArgb(180, 188, 194));
        dateCaption.Location = new Point(552, 17);
        header.Controls.Add(dateCaption);

        datePicker = new DateTimePicker
        {
            Location = new Point(552, 40),
            Width = 130,
            Height = 32,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd.MM.yyyy",
            Value = DateTime.Today,
            Font = new Font("Segoe UI", 10F)
        };
        datePicker.ValueChanged += (_, _) => RenderSeats();
        header.Controls.Add(datePicker);

        quarantine = new CheckBox
        {
            Text = "Карантинный режим (25 мест)",
            Location = new Point(700, 43),
            AutoSize = true,
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Checked = data.QuarantineMode,
            Font = new Font("Segoe UI", 9.5F),
            Cursor = Cursors.Hand
        };
        quarantine.CheckedChanged += (_, _) =>
        {
            data.QuarantineMode = quarantine.Checked;
            selectedSeats.Clear();
            Storage.Save(data);
            RenderSeats();
        };
        header.Controls.Add(quarantine);

        editorButton = Ui.Button("Редактор сеансов", Color.FromArgb(74, 84, 92), 124, 32);
        editorButton.Location = new Point(24, 78);
        editorButton.Click += (_, _) => new SessionEditorForm(data, LoadSessions).ShowDialog(this);
        header.Controls.Add(editorButton);

        aboutButton = Ui.Button("О программе", Color.FromArgb(74, 84, 92), 112, 32);
        aboutButton.Location = new Point(156, 78);
        aboutButton.Click += (_, _) => ShowAbout();
        header.Controls.Add(aboutButton);

        return header;
    }

    Panel BuildSidebar()
    {
        var left = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Ui.Ink2,
            Padding = new Padding(15, 14, 15, 10),
            Margin = Padding.Empty
        };

        modeLabel = Ui.Label("", 10.5F, FontStyle.Bold, Ui.Gold);
        modeLabel.Location = new Point(16, 14);
        left.Controls.Add(modeLabel);

        hallInfo = Ui.Label("", 9.5F, FontStyle.Regular, Color.FromArgb(205, 211, 214));
        hallInfo.Location = new Point(16, 45);
        left.Controls.Add(hallInfo);

        var selectionCard = new Panel
        {
            Location = new Point(15, 91),
            Width = 198,
            Height = 92,
            BackColor = Color.FromArgb(35, 48, 61),
            Padding = new Padding(12)
        };
        Ui.Round(selectionCard, 10);
        left.Controls.Add(selectionCard);

        var selectionTitle = Ui.Label("ВЫБРАНО", 8.5F, FontStyle.Bold, Color.FromArgb(166, 176, 184));
        selectionTitle.Location = new Point(12, 10);
        selectionCard.Controls.Add(selectionTitle);

        selectedLabel = Ui.Label("—", 16, FontStyle.Bold, Color.White);
        selectedLabel.Location = new Point(12, 32);
        selectionCard.Controls.Add(selectedLabel);

        buyButton = Ui.Button("Купить билет", Ui.Green, 198, 40);
        buyButton.Location = new Point(15, 202);
        buyButton.Click += (_, _) => BuyTickets();
        left.Controls.Add(buyButton);

        reserveButton = Ui.Button("Забронировать", Ui.Blue, 198, 40);
        reserveButton.Location = new Point(15, 250);
        reserveButton.Click += (_, _) => ReserveSeats();
        left.Controls.Add(reserveButton);

        refundButton = Ui.Button("Вернуть билет", Ui.Red, 198, 40);
        refundButton.Location = new Point(15, 298);
        refundButton.Click += (_, _) => RefundTicket();
        left.Controls.Add(refundButton);

        var legendTitle = Ui.Label("УСЛОВНЫЕ ОБОЗНАЧЕНИЯ", 8.5F, FontStyle.Bold, Color.FromArgb(166, 176, 184));
        legendTitle.Location = new Point(16, 360);
        left.Controls.Add(legendTitle);
        AddLegend(left, 386, Ui.SeatFree, "Свободное место");
        AddLegend(left, 416, Ui.Gold, "VIP место");
        AddLegend(left, 446, Ui.SeatReserved, "Забронировано");
        AddLegend(left, 476, Ui.SeatPurchased, "Куплено");

        var hint = Ui.Label("Нажмите на место, чтобы\nвыбрать его для покупки или брони.", 9.2F, FontStyle.Regular, Color.FromArgb(205, 211, 214));
        hint.Location = new Point(16, 525);
        left.Controls.Add(hint);

        return left;
    }

    Panel BuildHallContent()
    {
        var content = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Ui.Cream,
            Margin = Padding.Empty,
            Padding = new Padding(18, 12, 18, 8)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Ui.Cream
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.Controls.Add(layout);

        var sectionTitle = Ui.Label("СХЕМА ЗАЛА", 11, FontStyle.Bold, Ui.Ink);
        sectionTitle.Dock = DockStyle.Fill;
        sectionTitle.TextAlign = ContentAlignment.MiddleLeft;
        layout.Controls.Add(sectionTitle, 0, 0);

        var screenLabel = new Label
        {
            Text = "Э К Р А Н",
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 10F),
            BackColor = Color.FromArgb(36, 43, 49),
            Margin = new Padding(0, 5, 0, 5)
        };
        Ui.Round(screenLabel, 8);
        layout.Controls.Add(screenLabel, 0, 1);

        var stageHint = new Label
        {
            Text = "Экран находится перед первым рядом",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(112, 119, 124),
            Font = new Font("Segoe UI", 9F),
            BackColor = Ui.Cream,
            Margin = Padding.Empty
        };
        layout.Controls.Add(stageHint, 0, 2);

        seatGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 10,
            RowCount = 5,
            BackColor = Color.Transparent,
            Padding = new Padding(10, 4, 10, 4),
            Margin = Padding.Empty
        };
        for (int i = 0; i < 10; i++)
            seatGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
        for (int i = 0; i < 5; i++)
            seatGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        layout.Controls.Add(seatGrid, 0, 3);

        return content;
    }

    void AddLegend(Control parent, int y, Color color, string text)
    {
        var swatch = new Panel { Location = new Point(18, y), Width = 14, Height = 14, BackColor = color };
        Ui.Round(swatch, 3);
        parent.Controls.Add(swatch);

        var label = Ui.Label(text, 9.5F, FontStyle.Regular, Color.FromArgb(225, 229, 231));
        label.Location = new Point(40, y - 2);
        parent.Controls.Add(label);
    }

    void LoadSessions()
    {
        Storage.Normalize(data);
        sessionBox.Items.Clear();
        foreach (var s in data.Sessions.OrderBy(x => x.Start)) sessionBox.Items.Add(s);
        sessionBox.DisplayMember = nameof(Session.Name);
        if (sessionBox.Items.Count > 0 && sessionBox.SelectedIndex < 0) sessionBox.SelectedIndex = 0;
    }

    void RenderSeats()
    {
        seatGrid.SuspendLayout();
        seatGrid.Controls.Clear();

        var session = CurrentSession;
        if (session is null)
        {
            seatGrid.ResumeLayout();
            return;
        }

        var seats = Storage.GetSeats(data, session);
        var allowed = new HashSet<int>();
        for (int n = 1; n <= 50; n++)
        {
            int pos = (n - 1) % 10 + 1;
            if (!data.QuarantineMode || pos % 2 == 1)
                allowed.Add(n);
        }

        modeLabel.Text = data.QuarantineMode ? "КАРАНТИН • 25 МЕСТ" : "ОБЫЧНЫЙ РЕЖИМ • 50 МЕСТ";
        hallInfo.Text = $"{session.Name}\n{session.Start:HH:mm}–{session.End:HH:mm}\nЦена: {session.Price:N0} ₸";

        for (int row = 1; row <= 5; row++)
        {
            for (int pos = 1; pos <= 10; pos++)
            {
                int number = (row - 1) * 10 + pos;
                var seat = seats.First(x => x.Number == number);
                var b = new Button
                {
                    Text = number.ToString(),
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                    Tag = seat,
                    Font = new Font("Segoe UI Semibold", 10F),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false,
                    TabStop = false
                };
                b.FlatAppearance.BorderSize = 0;
                Ui.Round(b, 10);

                if (!allowed.Contains(number))
                {
                    b.Enabled = false;
                    b.Text = "";
                    b.BackColor = Ui.SeatUnavailable;
                    b.ForeColor = Color.FromArgb(155, 160, 163);
                }
                else if (seat.Purchased)
                {
                    b.BackColor = Ui.SeatPurchased;
                    b.ForeColor = Color.White;
                }
                else if (seat.Reserved)
                {
                    b.BackColor = Ui.SeatReserved;
                    b.ForeColor = Ui.Ink;
                }
                else if (selectedSeats.Contains(number))
                {
                    b.BackColor = Ui.Ink;
                    b.ForeColor = Color.White;
                }
                else if (seat.IsVip)
                {
                    b.BackColor = Ui.Gold;
                    b.ForeColor = Ui.Ink;
                }
                else
                {
                    b.BackColor = Ui.SeatFree;
                    b.ForeColor = Ui.Ink;
                }

                b.Click += (_, _) => ToggleSeat(seat);
                seatGrid.Controls.Add(b, pos - 1, row - 1);
            }
        }

        decimal selectedTotal = selectedSeats.Count * session.Price;
        selectedLabel.Text = selectedSeats.Count == 0
            ? "—"
            : $"{selectedSeats.Count} мест\n{selectedTotal:N0} ₸";

        status.Text = $"{session.Name}   •   {session.Start:HH:mm}–{session.End:HH:mm}   •   {session.Price:N0} ₸ за билет";
        seatGrid.ResumeLayout();
    }

    void ToggleSeat(SeatState seat)
    {
        if (seat.Purchased || seat.Reserved) return;

        if (selectedSeats.Contains(seat.Number)) selectedSeats.Remove(seat.Number);
        else selectedSeats.Add(seat.Number);
        RenderSeats();
    }

    void ReserveSeats()
    {
        var session = CurrentSession;
        if (session is null || selectedSeats.Count == 0)
        {
            MessageBox.Show("Сначала выберите места.", "Бронь", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        foreach (var n in selectedSeats)
        {
            var seat = Storage.GetSeats(data, session).First(x => x.Number == n);
            seat.Reserved = true;
        }

        selectedSeats.Clear();
        Storage.Save(data);
        RenderSeats();
        MessageBox.Show("Места успешно забронированы.", "Бронь", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    void BuyTickets()
    {
        var session = CurrentSession;
        if (session is null || selectedSeats.Count == 0)
        {
            MessageBox.Show("Сначала выберите места.", "Покупка", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var seats = Storage.GetSeats(data, session);
        decimal total = selectedSeats.Count * session.Price;

        var answer = MessageBox.Show(
            $"Мест: {selectedSeats.Count}\nСумма: {total:N0} ₸\n\nОплатить онлайн?",
            "Оплата билетов", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) return;

        foreach (var n in selectedSeats)
        {
            var seat = seats.First(x => x.Number == n);
            seat.Reserved = false;
            seat.Purchased = true;
            data.Tickets.Add(new Ticket
            {
                SessionName = session.Name,
                SessionStart = session.Start,
                SeatNumber = n,
                Price = session.Price,
                PurchaseTime = DateTime.Now
            });
        }

        var receipt = CreateReceipt(session, selectedSeats, total);
        selectedSeats.Clear();
        Storage.Save(data);
        RenderSeats();

        using var dlg = new SaveFileDialog
        {
            Filter = "Текстовый чек (*.txt)|*.txt",
            FileName = $"Чек_{DateTime.Now:yyyyMMdd_HHmmssfff}.txt"
        };
        if (dlg.ShowDialog() == DialogResult.OK) File.WriteAllText(dlg.FileName, receipt, Encoding.UTF8);

        MessageBox.Show(receipt, "Покупка завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    static string CreateReceipt(Session session, List<int> seats, decimal total)
    {
        var sb = new StringBuilder();
        sb.AppendLine("========================================");
        sb.AppendLine("              КИНОТЕАТР");
        sb.AppendLine("             КАССОВЫЙ ЧЕК");
        sb.AppendLine("========================================");
        sb.AppendLine($"Сеанс: {session.Name}");
        sb.AppendLine($"Время: {session.Start:dd.MM.yyyy HH:mm}");
        sb.AppendLine($"Покупка: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine("----------------------------------------");
        foreach (var n in seats.OrderBy(x => x))
            sb.AppendLine($"Место {n,-3} {(n <= 12 ? "VIP" : "обычное"),-8} {session.Price:N0} ₸");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"ИТОГО: {total:N0} ₸");
        sb.AppendLine("========================================");
        return sb.ToString();
    }

    void RefundTicket()
    {
        var session = CurrentSession;
        if (session is null) return;

        var seats = Storage.GetSeats(data, session);
        var purchased = data.Tickets
            .Where(x => x.SessionName == session.Name && x.SessionStart == session.Start)
            .OrderBy(x => x.SeatNumber)
            .ToList();

        var choices = new List<RefundChoice>();

        // Purchased tickets can be returned with the 10% retention required by the TЗ.
        foreach (var ticket in purchased)
        {
            choices.Add(new RefundChoice
            {
                SeatNumber = ticket.SeatNumber,
                Price = ticket.Price,
                Status = "КУПЛЕН",
                Ticket = ticket
            });
        }

        // Reservations are not purchases, but the user still needs to be able
        // to see and cancel them. Cancellation of a reservation has no refund.
        foreach (var seat in seats.Where(x => x.Reserved && !x.Purchased).OrderBy(x => x.Number))
        {
            choices.Add(new RefundChoice
            {
                SeatNumber = seat.Number,
                Price = session.Price,
                Status = "ЗАБРОНИРОВАН",
                Ticket = null
            });
        }

        choices = choices.OrderBy(x => x.SeatNumber).ToList();

        if (choices.Count == 0)
        {
            MessageBox.Show("Для выбранного сеанса нет купленных или забронированных мест.", "Возврат", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new RefundForm(choices);
        if (dlg.ShowDialog(this) != DialogResult.OK || dlg.SelectedChoice is null) return;

        var choice = dlg.SelectedChoice;
        var seatToChange = seats.First(x => x.Number == choice.SeatNumber);

        if (choice.Ticket is null)
        {
            var cancel = MessageBox.Show(
                $"Место: {choice.SeatNumber}\nСтатус: забронировано\n\nОтменить бронь?",
                "Отмена брони", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (cancel != DialogResult.Yes) return;

            seatToChange.Reserved = false;
            Storage.Save(data);
            RenderSeats();
            MessageBox.Show("Бронь отменена.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedTicket = choice.Ticket;
        decimal refund = selectedTicket.Price * 0.90m;
        var result = MessageBox.Show(
            $"Место: {selectedTicket.SeatNumber}\nСтоимость: {selectedTicket.Price:N0} ₸\nВозврат после удержания 10%: {refund:N0} ₸\n\nПродолжить?",
            "Возврат билета", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes) return;

        seatToChange.Purchased = false;
        seatToChange.Reserved = false;
        data.Tickets.Remove(selectedTicket);
        Storage.Save(data);
        RenderSeats();

        MessageBox.Show($"Билет возвращён.\nК возврату: {refund:N0} ₸", "Возврат выполнен", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    void ShowAbout()
    {
        MessageBox.Show(
            "КИНОТЕАТР\n\nУчебный проект по ООП на C# Windows Forms.\n\n50 мест • 12 VIP • бронь • покупка • возврат −10% • карантинный режим 25 мест.",
            "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

public class SessionEditorForm : Form
{
    readonly CinemaData data;
    readonly Action refresh;
    readonly DataGridView grid = new();

    public SessionEditorForm(CinemaData data, Action refresh)
    {
        this.data = data;
        this.refresh = refresh;

        Text = "Редактор сеансов";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(820, 500);
        MinimumSize = new Size(820, 500);
        BackColor = Ui.Cream;
        Font = new Font("Segoe UI", 10F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = Ui.Cream
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        Controls.Add(root);

        // Шапка
        var header = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Ui.Ink,
            Margin = Padding.Empty,
            Padding = new Padding(20, 0, 20, 0)
        };
        root.Controls.Add(header, 0, 0);

        var title = Ui.Label("РЕДАКТОР СЕАНСОВ", 18, FontStyle.Bold);
        title.Dock = DockStyle.Left;
        title.AutoSize = true;
        title.TextAlign = ContentAlignment.MiddleLeft;
        header.Controls.Add(title);

        // Таблица
        grid.Dock = DockStyle.Fill;
        grid.Margin = Padding.Empty;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.RowHeadersVisible = false;
        grid.AutoGenerateColumns = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.ReadOnly = false;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Ui.Ink,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft
        };
        grid.ColumnHeadersHeight = 36;
        grid.RowTemplate.Height = 34;
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = Color.FromArgb(42, 47, 52),
            SelectionBackColor = Color.FromArgb(214, 236, 255),
            SelectionForeColor = Color.FromArgb(25, 45, 65),
            Padding = new Padding(6, 0, 6, 0)
        };

        var nameCol = new DataGridViewTextBoxColumn
        {
            HeaderText = "Название",
            DataPropertyName = nameof(Session.Name),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 28
        };
        var startCol = new DataGridViewTextBoxColumn
        {
            HeaderText = "Начало",
            DataPropertyName = nameof(Session.Start),
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" }
        };
        var endCol = new DataGridViewTextBoxColumn
        {
            HeaderText = "Окончание",
            DataPropertyName = nameof(Session.End),
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" }
        };
        var priceCol = new DataGridViewTextBoxColumn
        {
            HeaderText = "Цена, ₸",
            DataPropertyName = nameof(Session.Price),
            Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
        };
        var childCol = new DataGridViewCheckBoxColumn
        {
            HeaderText = "Детский",
            DataPropertyName = nameof(Session.IsChild),
            Width = 90
        };

        grid.Columns.AddRange(nameCol, startCol, endCol, priceCol, childCol);
        grid.DataSource = data.Sessions.OrderBy(x => x.Start).ToList();
        root.Controls.Add(grid, 0, 1);

        // Нижняя панель
        var bottom = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Ui.Cream,
            Margin = Padding.Empty,
            Padding = new Padding(18, 14, 18, 14)
        };
        root.Controls.Add(bottom, 0, 2);

        var save = Ui.Button("Сохранить", Ui.Green, 130, 40);
        save.Location = new Point(18, 14);
        save.Click += (_, _) =>
        {
            grid.EndEdit();
            CurrencyManager? cm = grid.DataSource as CurrencyManager;
            if (cm != null) cm.EndCurrentEdit();

            Storage.Save(data);
            refresh();
            MessageBox.Show("Расписание сохранено.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        };
        bottom.Controls.Add(save);

        var cancel = Ui.Button("Закрыть", Ui.Red, 110, 40);
        cancel.Location = new Point(158, 14);
        cancel.Click += (_, _) => Close();
        bottom.Controls.Add(cancel);
    }
}

public class RefundChoice
{
    public int SeatNumber { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = "";
    public Ticket? Ticket { get; set; }
}

public class RefundForm : Form
{
    readonly ListBox list = new();
    readonly List<RefundChoice> choices;
    public RefundChoice? SelectedChoice { get; private set; }

    public RefundForm(List<RefundChoice> choices)
    {
        this.choices = choices;

        Text = "Возврат / отмена брони";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(560, 390);
        MinimumSize = new Size(560, 390);
        MaximumSize = new Size(560, 390);
        BackColor = Ui.Cream;
        Font = new Font("Segoe UI", 10F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            BackColor = Ui.Cream
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        Controls.Add(root);

        var header = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Ink, Margin = Padding.Empty };
        root.Controls.Add(header, 0, 0);
        var title = Ui.Label("ВЫБЕРИТЕ БИЛЕТ ИЛИ БРОНЬ", 17, FontStyle.Bold);
        title.Location = new Point(18, 16);
        header.Controls.Add(title);

        list.Dock = DockStyle.Fill;
        list.BorderStyle = BorderStyle.FixedSingle;
        list.Font = new Font("Segoe UI", 10F);
        list.BackColor = Color.White;
        list.IntegralHeight = false;
        foreach (var c in choices)
        {
            var icon = c.Ticket is null ? "БРОНЬ" : "КУПЛЕН";
            list.Items.Add($"Место {c.SeatNumber,-2}   •   {icon,-9}   •   {c.Price:N0} ₸");
        }
        root.Controls.Add(list, 0, 1);

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(15, 16, 15, 16),
            Margin = Padding.Empty,
            BackColor = Ui.Cream
        };
        root.Controls.Add(bottom, 0, 2);

        var ok = Ui.Button("Вернуть / отменить", Ui.Green, 170, 38);
        ok.Margin = new Padding(0, 0, 12, 0);
        ok.Click += (_, _) =>
        {
            if (list.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите билет или бронь.", "Возврат", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SelectedChoice = choices[list.SelectedIndex];
            DialogResult = DialogResult.OK;
            Close();
        };
        bottom.Controls.Add(ok);

        var cancel = Ui.Button("Отмена", Ui.Red, 120, 38);
        cancel.Margin = Padding.Empty;
        cancel.Click += (_, _) => Close();
        bottom.Controls.Add(cancel);
    }
}

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
