using SmartMarket.Data;

namespace SmartMarket;

public class MainForm : Form
{
    private readonly SmartMarketDbContext _db;
    private readonly Label _status;

    public MainForm(SmartMarketDbContext db)
    {
        _db = db;
        Text = "SmartMarket — электронный магазин";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1000, 680);
        MinimumSize = new Size(900, 620);
        BackColor = Color.FromArgb(245, 247, 250);

        var header = new Panel { Dock = DockStyle.Top, Height = 88, BackColor = Color.FromArgb(18, 52, 86) };
        var title = new Label { Text = "SMARTMARKET", ForeColor = Color.White, Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Location = new Point(26, 18) };
        var subtitle = new Label { Text = "Учёт товаров • покупатели • продажи", ForeColor = Color.FromArgb(210, 225, 240), AutoSize = true, Location = new Point(28, 55) };
        header.Controls.Add(title); header.Controls.Add(subtitle);
        Controls.Add(header);

        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(28), AutoScroll = true, WrapContents = true };
        Controls.Add(flow);
        flow.BringToFront();

        AddButton(flow, "Товары", "CRUD, поиск, остатки", () => new ProductsForm(_db).ShowDialog(this));
        AddButton(flow, "Категории", "CRUD категорий", () => new CategoriesForm(_db).ShowDialog(this));
        AddButton(flow, "Покупатели", "CRUD и поиск", () => new CustomersForm(_db).ShowDialog(this));
        AddButton(flow, "Оформить продажу", "Заказ из нескольких товаров", () => new OrdersForm(_db).ShowDialog(this));
        AddButton(flow, "Отчёты", "История продаж и CSV", () => new ReportsForm(_db).ShowDialog(this));
        AddButton(flow, "Проверить БД", "SQL Server connection", CheckDb);

        _status = new Label { Dock = DockStyle.Bottom, Height = 34, Padding = new Padding(12, 7, 12, 0), Text = "Готово", BackColor = Color.White, ForeColor = Color.DimGray };
        Controls.Add(_status);
    }

    private void AddButton(FlowLayoutPanel flow, string caption, string note, Action click)
    {
        var panel = new Panel { Width = 280, Height = 125, Margin = new Padding(10), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        var b = new Button { Text = caption, Width = 250, Height = 48, Location = new Point(14, 12), BackColor = Color.FromArgb(46, 125, 115), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
        var l = new Label { Text = note, Width = 250, Height = 38, Location = new Point(14, 69), ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleLeft };
        b.Click += (_, _) => SafeOpen(click);
        panel.Controls.Add(b); panel.Controls.Add(l); flow.Controls.Add(panel);
    }

    private void SafeOpen(Action action)
    {
        try { action(); } catch (Exception ex) { MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void CheckDb()
    {
        try
        {
            var ok = _db.Database.CanConnect();
            _status.Text = ok ? "Подключение к SQL Server: успешно" : "Подключение к SQL Server: не удалось";
            MessageBox.Show(this, ok ? "SmartMarketDb доступна." : "База SmartMarketDb недоступна.", "Проверка БД", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Ошибка SQL Server", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
