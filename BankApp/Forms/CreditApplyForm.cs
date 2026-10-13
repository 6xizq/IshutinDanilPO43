using BankApp.Models;
using BankApp.Services;

namespace BankApp.Forms;

public class CreditApplyForm : Form
{
    TextBox amount=Ui.TextBox(), months=Ui.TextBox();
    public CreditApplyForm()
    {
        Text="BankApp — Оформление кредита";Ui.Style(this);
        var p=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(55)};
        p.Controls.Add(Ui.Label("Сумма кредита (₸):"),0,0);p.Controls.Add(amount,1,0);
        p.Controls.Add(Ui.Label("Срок (месяцев):"),0,1);p.Controls.Add(months,1,1);
        p.Controls.Add(Ui.Button("Оформить",Apply),1,2);Controls.Add(p);
        foreach(var t in new[]{amount,months})t.KeyPress+=(s,e)=>{if(!char.IsControl(e.KeyChar)&&!char.IsDigit(e.KeyChar))e.Handled=true;};
    }
    void Apply(object?s,EventArgs e)
    {
        if(!decimal.TryParse(amount.Text,out var a)||a<=0||!int.TryParse(months.Text,out var m)||m<=0){MessageBox.Show("Некорректные данные.");return;}
        decimal total=a*1.10m; var c=new Credit{Amount=a,Remaining=total,MonthlyPayment=Math.Round(total/m,2),StartDate=DateTime.Now,NextPaymentDate=DateTime.Now.AddMonths(1)};
        BankDataService.CurrentUser!.Credits.Add(c);BankDataService.Save();
        MessageBox.Show($"Кредит оформлен.\nСумма: {a:N2} ₸\nК оплате: {total:N2} ₸\nЕжемесячно: {c.MonthlyPayment:N2} ₸");
        DialogResult=DialogResult.OK;Close();
    }
}
