using BankApp.Models;
using BankApp.Services;

namespace BankApp.Forms;

public class CreditPaymentForm : Form
{
    readonly Credit credit; TextBox amount=Ui.TextBox();
    public CreditPaymentForm(Credit c)
    {
        credit=c;Text="BankApp — Оплата кредита";Ui.Style(this);
        var p=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(55)};
        p.Controls.Add(Ui.Label($"Остаток: {c.Remaining:N2} ₸"),0,0);p.SetColumnSpan(p.GetControlFromPosition(0,0),2);
        p.Controls.Add(Ui.Label("Сумма платежа:"),0,1);p.Controls.Add(amount,1,1);
        p.Controls.Add(Ui.Button("Оплатить",Pay),1,2);Controls.Add(p);
        amount.KeyPress+=(s,e)=>{if(!char.IsControl(e.KeyChar)&&!char.IsDigit(e.KeyChar)&&e.KeyChar!=',')e.Handled=true;};
    }
    void Pay(object?s,EventArgs e)
    {
        if(!decimal.TryParse(amount.Text,out var v)||v<=0||v>credit.Remaining){MessageBox.Show("Некорректная сумма.");return;}
        var u=BankDataService.CurrentUser!;if(v>u.Balance){MessageBox.Show("Недостаточно средств.");return;}
        u.Balance-=v;credit.Remaining=Math.Max(0,credit.Remaining-v);
        credit.NextPaymentDate=DateTime.Now.AddMonths(1);BankDataService.Save();
        MessageBox.Show(credit.IsPaid?"Поздравляем! Кредит полностью погашен.":"Платёж по кредиту принят.");
        DialogResult=DialogResult.OK;Close();
    }
}
