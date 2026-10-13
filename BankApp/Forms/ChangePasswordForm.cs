using BankApp.Services;

namespace BankApp.Forms;

public class ChangePasswordForm : Form
{
    TextBox login=Ui.TextBox(), oldPass=Ui.TextBox(true), newPass=Ui.TextBox(true), newPass2=Ui.TextBox(true);

    public ChangePasswordForm()
    {
        Text="BankApp — Смена пароля"; Ui.Style(this);
        var p=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(50)};
        string[] l={"Логин","Старый пароль","Новый пароль","Повтор нового пароля"};
        Control[] c={login,oldPass,newPass,newPass2};
        for(int i=0;i<l.Length;i++){p.Controls.Add(Ui.Label(l[i]+":"),0,i);p.Controls.Add(c[i],1,i);}
        p.Controls.Add(Ui.Button("Изменить пароль",Change),1,4); Controls.Add(p);
    }

    void Change(object?s,EventArgs e)
    {
        var u=BankDataService.Users.FirstOrDefault(x=>x.Login.Equals(login.Text.Trim(),StringComparison.OrdinalIgnoreCase)&&x.Password==oldPass.Text);
        if(u==null){MessageBox.Show("Неверные данные.");return;}
        if(newPass.Text.Length<4||newPass.Text!=newPass2.Text){MessageBox.Show("Пароли не совпадают или слишком короткие.");return;}
        u.Password=newPass.Text; BankDataService.Save(); MessageBox.Show("Пароль изменён.");Close();
    }
}
