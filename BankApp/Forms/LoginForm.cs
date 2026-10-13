using System.Drawing;
using BankApp.Services;

namespace BankApp.Forms;

public class LoginForm : Form
{
    TextBox login = Ui.TextBox(), password = Ui.TextBox(true);
    Label captchaLabel = Ui.Label("");
    TextBox captcha = Ui.TextBox();
    Button loginButton;
    int failures = 0;
    bool captchaRequired = false;
    string captchaValue = "";

    public LoginForm()
    {
        Text = "BankApp — Авторизация";
        Ui.Style(this);
        Build();
    }

    void Build()
    {
        var title = Ui.Label("BANKAPP", 26);
        title.Font = new Font(title.Font, FontStyle.Bold);

        var panel = new TableLayoutPanel {
            Dock=DockStyle.Fill, ColumnCount=2, RowCount=7, Padding=new Padding(70),
            BackColor=Color.Transparent
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

        panel.Controls.Add(title,0,0); panel.SetColumnSpan(title,2);
        panel.Controls.Add(Ui.Label("Логин:"),0,1); panel.Controls.Add(login,1,1);
        panel.Controls.Add(Ui.Label("Пароль:"),0,2); panel.Controls.Add(password,1,2);

        captchaLabel.Visible=false; captcha.Visible=false;
        panel.Controls.Add(captchaLabel,0,3); panel.Controls.Add(captcha,1,3);

        loginButton = Ui.Button("Войти", Login);
        panel.Controls.Add(loginButton,1,4);

        var reg = Ui.Button("Регистрация", (_,__) => { using var f=new RegisterForm(); f.ShowDialog(this); });
        var change = Ui.Button("Сменить пароль", (_,__) => { using var f=new ChangePasswordForm(); f.ShowDialog(this); });
        panel.Controls.Add(reg,0,5); panel.Controls.Add(change,1,5);

        var hint = Ui.Label("Демо: admin / 1234", 9);
        panel.Controls.Add(hint,1,6);

        Controls.Add(panel);
    }

    void CreateCaptcha()
    {
        captchaRequired=true;
        var r=new Random();
        captchaValue = r.Next(1000,9999).ToString();
        captchaLabel.Text = $"Введите код: {captchaValue}";
        captchaLabel.Visible=captcha.Visible=true;
    }

    async void Login(object? sender, EventArgs e)
    {
        if (captchaRequired && captcha.Text.Trim()!=captchaValue)
        {
            MessageBox.Show("Неверная CAPTCHA.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            await Block();
            return;
        }

        var user=BankDataService.Authenticate(login.Text.Trim(), password.Text);
        if (user != null)
        {
            failures=0; BankDataService.CurrentUser=user;
            Hide();
            using var main=new MainForm();
            main.ShowDialog();
            Show();
            return;
        }

        failures++;
        if (failures>=3 && !captchaRequired) CreateCaptcha();
        if (captchaRequired) await Block();
        else MessageBox.Show("Неверный логин или пароль.");
    }

    async Task Block()
    {
        loginButton.Enabled=false;
        for(int i=10;i>0;i--)
        {
            loginButton.Text=$"Повтор через {i} с";
            await Task.Delay(1000);
        }
        loginButton.Text="Войти"; loginButton.Enabled=true;
    }
}
