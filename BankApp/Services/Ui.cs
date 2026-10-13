using System.Drawing;
using System.Windows.Forms;

namespace BankApp.Services;

public static class Ui
{
    public static readonly Color Orange = ColorTranslator.FromHtml("#FC9F57");
    public static readonly Color Dark = ColorTranslator.FromHtml("#040052");
    public static readonly Color TextLight = ColorTranslator.FromHtml("#FFFFFF");
    public static readonly Color TextDark = ColorTranslator.FromHtml("#BABABA");
    public static readonly Font MainFont = new("Century Gothic", 10F);

    public static void Style(Form f, bool dark = false)
    {
        f.Font = MainFont;
        f.StartPosition = FormStartPosition.CenterScreen;
        f.MinimumSize = new Size(700, 450);
        f.BackColor = dark ? Dark : Orange;
        f.ForeColor = dark ? TextDark : TextLight;
        f.FormBorderStyle = FormBorderStyle.Sizable;
    }

    public static Button Button(string text, EventHandler click, bool danger=false)
    {
        var b = new Button {
            Text = text, AutoSize = false, Height = 42, Width = 190,
            FlatStyle = FlatStyle.Flat, Font = new Font(MainFont, FontStyle.Bold),
            BackColor = danger ? Color.Firebrick : Color.White,
            ForeColor = danger ? Color.White : Dark,
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderSize = 0;
        b.Click += click;
        return b;
    }

    public static Label Label(string text, int size=10)
        => new() { Text=text, AutoSize=true, Font=new Font(MainFont.FontFamily,size), Padding=new Padding(3) };

    public static TextBox TextBox(bool password=false)
        => new() { Width=300, Height=32, Font=MainFont, UseSystemPasswordChar=password };
}
