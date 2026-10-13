using GameStore.Models;
using GameStore.Services;

namespace GameStore.Forms;

public class ProductForm : Form
{
    public ProductForm(Product p)
    {
        Text = p.Name;
        ClientSize = new Size(620, 430);
        MinimumSize = new Size(620, 430);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Arial", 10);
        BackColor = Color.White;

        var path = Path.Combine(StoreService.AssetsDir, p.ImageFile);
        var pic = new PictureBox
        {
            Bounds = new Rectangle(20, 20, 270, 280),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = LoadImageWithoutLock(path),
            Cursor = Cursors.Hand,
            BackColor = Color.White
        };
        pic.Click += (_, _) => new ImageForm(pic.Image, p.Name).ShowDialog(this);
        Controls.Add(pic);

        Controls.Add(new Label
        {
            Text = p.Name,
            Left = 320, Top = 25, Width = 270, Height = 55,
            Font = new Font("Arial", 18, FontStyle.Bold)
        });

        Controls.Add(new Label
        {
            Text = $"Жанр: {p.Genre}\nПлатформа: {p.Platform}\nЦена: {p.Price:N0} тг",
            Left = 320, Top = 95, Width = 270, Height = 85
        });

        Controls.Add(new Label
        {
            Text = p.Description,
            Left = 320, Top = 190, Width = 270, Height = 90
        });

        var b = new Button
        {
            Text = "Добавить в корзину",
            Bounds = new Rectangle(320, 320, 230, 45),
            BackColor = Color.SeaGreen,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        b.Click += (_, _) =>
        {
            StoreService.AddToCart(p);
            MessageBox.Show("Товар добавлен в корзину.", "Корзина", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        };
        Controls.Add(b);
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
