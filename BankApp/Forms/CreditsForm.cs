using BankApp.Models;
using BankApp.Services;

namespace BankApp.Forms;

public class CreditsForm : Form
{
    ListView list=new(){Dock=DockStyle.Fill,View=View.Details,FullRowSelect=true,GridLines=true};
    public CreditsForm()
    {
        Text="BankApp — Кредиты";Ui.Style(this);Width=900;Height=550;
        list.Columns.Add("Статус",110);list.Columns.Add("Сумма",130);list.Columns.Add("Остаток",130);
        list.Columns.Add("Ежемесячно",140);list.Columns.Add("Следующий платёж",180);
        var top=new FlowLayoutPanel{Dock=DockStyle.Top,Height=65};
        top.Controls.Add(Ui.Button("Оформить кредит",(_,__)=>OpenApply()));
        top.Controls.Add(Ui.Button("Оплатить кредит",(_,__)=>Pay()));
        Controls.Add(list);Controls.Add(top);Load+=(_,__)=>RefreshList();
    }
    void RefreshList()
    {
        list.Items.Clear();
        foreach(var c in BankDataService.CurrentUser!.Credits)
        {
            var i=new ListViewItem(c.IsPaid?"Погашен":"Действующий");
            i.SubItems.Add($"{c.Amount:N2} ₸");i.SubItems.Add($"{Math.Max(0,c.Remaining):N2} ₸");
            i.SubItems.Add($"{c.MonthlyPayment:N2} ₸");i.SubItems.Add(c.NextPaymentDate.ToShortDateString());
            i.ForeColor=c.IsPaid?Color.Red:Color.Green; i.Tag=c;list.Items.Add(i);
        }
    }
    void OpenApply(){using var f=new CreditApplyForm();if(f.ShowDialog(this)==DialogResult.OK)RefreshList();}
    void Pay()
    {
        if(list.SelectedItems.Count==0){MessageBox.Show("Выберите кредит.");return;}
        var c=(Credit)list.SelectedItems[0].Tag!;
        using var f=new CreditPaymentForm(c);if(f.ShowDialog(this)==DialogResult.OK)RefreshList();
    }
}
