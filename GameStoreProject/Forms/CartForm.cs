using GameStore.Models;
using GameStore.Services;

namespace GameStore.Forms;

public class CartForm : Form
{
    private readonly DataGridView grid = new();
    private readonly Label total = new();

    public CartForm()
    {
        Text = "Game Store — Корзина";
        ClientSize = new Size(700, 520);
        MinimumSize = new Size(650, 480);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Arial", 10);
        BackColor = Color.FromArgb(191, 251, 202);

        Build();
        RefreshCart();
    }

    private void Build()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 58,
            BackColor = Color.FromArgb(17, 101, 80)
        };
        var title = new Label
        {
            Text = "КОРЗИНА",
            ForeColor = Color.White,
            Font = new Font("Arial", 18, FontStyle.Bold),
            AutoSize = true,
            Left = 18,
            Top = 16
        };
        header.Controls.Add(title);

        var content = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15)
        };
        grid.Dock = DockStyle.Fill;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.AutoGenerateColumns = false;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Name",
            HeaderText = "Игра",
            DataPropertyName = "Name",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 220
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Qty",
            HeaderText = "Кол.",
            DataPropertyName = "Qty",
            Width = 70
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Sum",
            HeaderText = "Сумма",
            DataPropertyName = "Sum",
            Width = 130,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }
        });

        grid.CellDoubleClick += (_, _) => RemoveSelectedOne();
        content.Controls.Add(grid);

        var bottom = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 105,
            ColumnCount = 3,
            RowCount = 2,
            BackColor = Color.FromArgb(191, 251, 202),
            Padding = new Padding(0, 10, 0, 0)
        };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        bottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        bottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        content.Controls.Add(bottom);

        total.Text = "Итого: 0 тг";
        total.Font = new Font("Arial", 14, FontStyle.Bold);
        total.Dock = DockStyle.Fill;
        total.TextAlign = ContentAlignment.MiddleLeft;
        bottom.Controls.Add(total, 0, 0);
        bottom.SetColumnSpan(total, 3);

        var pay = Button("Оплатить", Color.SeaGreen);
        pay.Click += Pay;
        bottom.Controls.Add(pay, 1, 1);

        var clear = Button("Очистить", Color.IndianRed);
        clear.Click += (_, _) =>
        {
            if (StoreService.Cart.Count == 0) return;
            if (MessageBox.Show("Очистить корзину?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            StoreService.ClearCart();
            RefreshCart();
        };
        bottom.Controls.Add(clear, 2, 1);

        var hint = new Label
        {
            Text = "Двойной клик по товару — уменьшить количество",
            Dock = DockStyle.Bottom,
            Height = 28,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft
        };
        content.Controls.Add(hint);

        Controls.Add(content);
        Controls.Add(header);
    }

    private static Button Button(string text, Color color)
    {
        return new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(3),
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
    }

    private void RefreshCart()
    {
        var rows = StoreService.Cart.Select(x => new CartRow
        {
            Id = x.Product.Id,
            Name = x.Product.Name,
            Qty = x.Quantity,
            Sum = x.Total
        }).ToList();

        grid.DataSource = rows;
        total.Text = $"Итого: {StoreService.CartTotal():N0} тг";
    }

    private void RemoveSelectedOne()
    {
        if (grid.CurrentRow?.DataBoundItem is not CartRow row) return;
        StoreService.RemoveFromCart(row.Id);
        RefreshCart();
    }

    private void Pay(object? sender, EventArgs e)
    {
        if (StoreService.Cart.Count == 0)
        {
            MessageBox.Show("Корзина пуста.", "Оплата", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var amount = StoreService.CartTotal();
        if (MessageBox.Show($"Оплатить {amount:N0} тг?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var file = StoreService.CreateReceipt();
        StoreService.ClearCart();
        RefreshCart();

        MessageBox.Show(
            "Оплата выполнена.\nЧек сохранён:\n" + file,
            "Успешно",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private sealed class CartRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Qty { get; set; }
        public decimal Sum { get; set; }
    }
}
