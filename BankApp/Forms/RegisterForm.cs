using BankApp.Models;
using BankApp.Services;

namespace BankApp.Forms;

public class RegisterForm : Form
{
    TextBox login=Ui.TextBox(), pass=Ui.TextBox(true), pass2=Ui.TextBox(true);
    TextBox name=Ui.TextBox(), phone=Ui.TextBox(), email=Ui.TextBox();

    public RegisterForm()
    {
        Text="BankApp — Регистрация"; Ui.Style(this); Width=700; Height=520;
        var p=new TableLayoutPanel { Dock=DockStyle.Fill, ColumnCount=2, Padding=new Padding(45), AutoScroll=true };
        string[] labels={"Логин","Пароль","Повтор пароля","ФИО","Телефон","Email"};
        Control[] controls={login,pass,pass2,name,phone,email};
        for(int i=0;i<labels.Length;i++){p.Controls.Add(Ui.Label(labels[i]+":"),0,i);p.Controls.Add(controls[i],1,i);}
        var b=Ui.Button("Создать аккаунт", Register); p.Controls.Add(b,1,6);
        Controls.Add(p);
    }

    void Register(object? s,EventArgs e)
    {
        if(string.IsNullOrWhiteSpace(login.Text)||string.IsNullOrWhiteSpace(name.Text)||
           string.IsNullOrWhiteSpace(pass.Text)||pass.Text!=pass2.Text)
        { MessageBox.Show("Заполните обязательные поля и проверьте пароли."); return; }
        if(BankDataService.LoginExists(login.Text)){MessageBox.Show("Такой логин уже существует.");return;}
        if(phone.Text.Any(c=>!char.IsDigit(c)&&c!='+'&&c!=' '&&c!='-'&&c!='('&&c!=')'))
        {MessageBox.Show("Некорректный телефон.");return;}
        BankDataService.Users.Add(new User{Login=login.Text.Trim(),Password=pass.Text,FullName=name.Text.Trim(),Phone=phone.Text,Email=email.Text,Balance=0});
        BankDataService.Save(); MessageBox.Show("Регистрация выполнена."); Close();
    }
}
