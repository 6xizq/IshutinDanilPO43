using BankApp.Services;

namespace BankApp.Forms;

public class MoneyForm : Form
{
    readonly bool deposit; TextBox amount=Ui.TextBox();

    public MoneyForm(bool deposit)
    {
        this.deposit=deposit; Text=$"BankApp — {(deposit?"Пополнение":"Снятие")}"; Ui.Style(this);
        var p=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(60)};
        p.Controls.Add(Ui.Label("Сумма (₸):"),0,0);p.Controls.Add(amount,1,0);
        p.Controls.Add(Ui.Button(deposit?"Пополнить":"Снять",Do),1,1); Controls.Add(p);
        amount.KeyPress+=(s,e)=>{if(!char.IsControl(e.KeyChar)&&!char.IsDigit(e.KeyChar)&&e.KeyChar!=',')e.Handled=true;};
    }
    void Do(object?s,EventArgs e)
    {
        if(!decimal.TryParse(amount.Text,out var v)||v<=0){MessageBox.Show("Введите корректную сумму.");return;}
        var u=BankDataService.CurrentUser!;
        if(!deposit&&v>u.Balance){MessageBox.Show("Недостаточно средств.");return;}
        u.Balance += deposit?v:-v; BankDataService.Save(); MessageBox.Show("Операция выполнена.");Close();
    }
}
