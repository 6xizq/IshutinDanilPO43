using GameStore.Services;

namespace GameStore.Forms;

public class LoginForm : Form
{
    TextBox login = new(), password = new();
    public LoginForm()
    {
        Text = "Game Store — Вход"; Width=480; Height=360; StartPosition=FormStartPosition.CenterScreen; BackColor=Color.FromArgb(191,251,202); Font=new Font("Arial",10);
        var title=new Label{Text="GAME STORE",Font=new Font("Arial",26,FontStyle.Bold),AutoSize=true,Left=125,Top=35,ForeColor=Color.FromArgb(17,101,80)}; Controls.Add(title);
        AddLabel("Логин",70,105); login.SetBounds(70,130,320,32); Controls.Add(login);
        AddLabel("Пароль",70,175); password.SetBounds(70,200,320,32); password.PasswordChar='●'; Controls.Add(password);
        var b=new Button{Text="Войти",Bounds=new Rectangle(70,250,150,40),BackColor=Color.SeaGreen,ForeColor=Color.White,FlatStyle=FlatStyle.Flat}; b.Click+=Login; Controls.Add(b);
        var r=new Button{Text="Регистрация",Bounds=new Rectangle(240,250,150,40),BackColor=Color.White,FlatStyle=FlatStyle.Flat}; r.Click+=(s,e)=>new RegisterForm().ShowDialog(); Controls.Add(r);
        var c=new LinkLabel{Text="Сменить пароль",AutoSize=true,Left=70,Top=305}; c.Click+=(s,e)=>new ChangePasswordForm().ShowDialog(); Controls.Add(c);
        AcceptButton=b;
    }
    void AddLabel(string text,int x,int y)=>Controls.Add(new Label{Text=text,Left=x,Top=y,AutoSize=true});
    void Login(object? s,EventArgs e){if(StoreService.Login(login.Text,password.Text)){Hide(); new MainForm().ShowDialog(); Show(); password.Clear();}else MessageBox.Show("Неверный логин или пароль.","Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);}
}
