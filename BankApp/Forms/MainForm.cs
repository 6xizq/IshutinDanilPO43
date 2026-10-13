using BankApp.Services;
using BankApp.Models;

namespace BankApp.Forms;

public class MainForm : Form
{
    Label balanceLabel=new();
    User? User => BankDataService.CurrentUser;

    public MainForm()
    {
        Text="BankApp — Личный кабинет"; Ui.Style(this); Width=1000; Height=650;
        Build();
    }

    void Build()
    {
        var top=new Panel{Dock=DockStyle.Top,Height=110,Padding=new Padding(25)};
        var hello=Ui.Label($"Здравствуйте, {User?.FullName}",16);
        hello.Font=new Font(hello.Font,FontStyle.Bold);
        balanceLabel=Ui.Label($"Текущий счёт: {User?.Balance:N2} ₸",18);
        balanceLabel.Font=new Font(balanceLabel.Font,FontStyle.Bold);
        top.Controls.Add(hello); hello.Location=new Point(25,18);
        top.Controls.Add(balanceLabel); balanceLabel.Location=new Point(25,55);

        var grid=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=3,RowCount=3,Padding=new Padding(30)};
        Add(grid,"Пополнить счёт",(s,e)=>{using var f=new MoneyForm(true);f.ShowDialog(this);RefreshBalance();});
        Add(grid,"Снять со счёта",(s,e)=>{using var f=new MoneyForm(false);f.ShowDialog(this);RefreshBalance();});
        Add(grid,"Транзакция",(s,e)=>{using var f=new TransactionForm();f.ShowDialog(this);RefreshBalance();});
        Add(grid,"Кредиты",(s,e)=>{using var f=new CreditsForm();f.ShowDialog(this);RefreshBalance();});
        Add(grid,"Смена пароля",(s,e)=>{using var f=new ChangePasswordForm();f.ShowDialog(this);});
        Add(grid,"О программе",(s,e)=>MessageBox.Show("BankApp\nУчебная практика по ООП\nC# Windows Forms","О программе"));
        Add(grid,"Выйти",(s,e)=>Close(),true);

        Controls.Add(grid); Controls.Add(top);
        FormClosing += (_,__)=>BankDataService.Save();
    }

    void Add(TableLayoutPanel g,string text,EventHandler h,bool danger=false)
    {
        int i=g.Controls.Count; var b=Ui.Button(text,h,danger); g.Controls.Add(b,i%3,i/3);
    }

    void RefreshBalance()=>balanceLabel.Text=$"Текущий счёт: {User?.Balance:N2} ₸";
}
