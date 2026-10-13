using GameStore.Models;
using GameStore.Services;

namespace GameStore.Forms;

public class MainForm : Form
{
    private readonly FlowLayoutPanel products = new();
    private readonly TextBox search = new();
    private readonly NumericUpDown maxPrice = new();

    public MainForm()
    {
        Text = $"Game Store — {StoreService.CurrentUser?.Name}";
        ClientSize = new Size(1024, 768);
        MinimumSize = new Size(1024, 768);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Font = new Font("Arial", 10);
        BackColor = Color.FromArgb(191, 251, 202);

        Build();
        RefreshProducts();
    }

    private void Build()
    {
        var top = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.FromArgb(17, 101, 80)
        };
        Controls.Add(top);

        var title = new Label
        {
            Text = "GAME STORE",
            ForeColor = Color.White,
            Font = new Font("Arial", 20, FontStyle.Bold),
            AutoSize = true,
            Left = 18,
            Top = 18
        };
        top.Controls.Add(title);

        search.SetBounds(230, 18, 235, 34);
        search.PlaceholderText = "Поиск по названию...";
        search.TextChanged += (_, _) => RefreshProducts();
        top.Controls.Add(search);

        var maxLabel = new Label
        {
            Text = "Макс. цена",
            Left = 480,
            Top = 3,
            ForeColor = Color.White,
            AutoSize = true
        };
        top.Controls.Add(maxLabel);

        maxPrice.SetBounds(480, 18, 145, 34);
        maxPrice.Maximum = 100000;
        maxPrice.Value = 100000;
        maxPrice.Increment = 1000;
        maxPrice.ThousandsSeparator = true;
        maxPrice.ValueChanged += (_, _) => RefreshProducts();
        top.Controls.Add(maxPrice);

        var cartBtn = Btn("Корзина", Color.SeaGreen);
        cartBtn.SetBounds(645, 16, 105, 38);
        cartBtn.Click += OpenCart;
        top.Controls.Add(cartBtn);

        var about = Btn("О программе", Color.White);
        about.ForeColor = Color.FromArgb(17, 101, 80);
        about.SetBounds(760, 16, 120, 38);
        about.Click += (_, _) => MessageBox.Show(
            "Game Store\nУчебная практика по ООП\nC# Windows Forms",
            "О программе",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        top.Controls.Add(about);

        var exit = Btn("Выход", Color.IndianRed);
        exit.SetBounds(890, 16, 105, 38);
        exit.Click += (_, _) =>
        {
            StoreService.SaveCart();
            Close();
        };
        top.Controls.Add(exit);

        products.Dock = DockStyle.Fill;
        products.AutoScroll = true;
        products.Padding = new Padding(18, 18, 10, 18);
        products.WrapContents = true;
        Controls.Add(products);
    }

    private void OpenCart(object? sender, EventArgs e)
    {
        using var cart = new CartForm();
        cart.ShowDialog(this);
    }

    private Button Btn(string text, Color color)
    {
        return new Button
        {
            Text = text,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleCenter
        };
    }

    private void RefreshProducts()
    {
        products.SuspendLayout();
        try
        {
            products.Controls.Clear();
            var query = search.Text.Trim();
            var list = StoreService.Products
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) && p.Price <= maxPrice.Value)
                .ToList();

            foreach (var p in list)
                products.Controls.Add(ProductCard(p));
        }
        finally
        {
            products.ResumeLayout();
        }
    }

    private Control ProductCard(Product p)
    {
        var panel = new Panel
        {
            Width = 230,
            Height = 290,
            Margin = new Padding(6),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand
        };

        var path = Path.Combine(StoreService.AssetsDir, p.ImageFile);
        var image = LoadImageWithoutLock(path);
        var pic = new PictureBox
        {
            Bounds = new Rectangle(10, 10, 208, 142),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = image,
            Cursor = Cursors.Hand,
            BackColor = Color.White
        };
        pic.Click += (_, _) => new ImageForm(pic.Image, p.Name).ShowDialog(this);
        panel.Controls.Add(pic);

        var name = new Label
        {
            Text = p.Name,
            Left = 10,
            Top = 163,
            Width = 208,
            Height = 42,
            Font = new Font("Arial", 11, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        name.Click += (_, _) => new ProductForm(p).ShowDialog(this);
        panel.Controls.Add(name);

        var info = new Label
        {
            Text = $"{p.Genre} • {p.Platform}",
            Left = 10,
            Top = 207,
            Width = 208,
            Height = 22
        };
        panel.Controls.Add(info);

        var price = new Label
        {
            Text = $"{p.Price:N0} тг",
            Left = 10,
            Top = 238,
            Width = 105,
            Font = new Font("Arial", 11, FontStyle.Bold)
        };
        if (p.Price > 20000) price.ForeColor = Color.Red;
        panel.Controls.Add(price);

        var add = Btn("В корзину", Color.SeaGreen);
        add.SetBounds(112, 230, 106, 40);
        add.Click += (_, _) =>
        {
            StoreService.AddToCart(p);
            MessageBox.Show("Товар добавлен в корзину.", "Корзина", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        panel.Controls.Add(add);

        return panel;
    }

    private static Image? LoadImageWithoutLock(string path)
    {
        if (!File.Exists(path)) return null;

        try
        {
            using var source = Image.FromFile(path);
            return new Bitmap(source);
        }
        catch
        {
            return null;
        }
    }
}
