using BankApp.Services;

namespace BankApp.Forms;

public class TransactionForm : Form
{
    TextBox recipient=Ui.TextBox(), amount=Ui.TextBox(), desc=Ui.TextBox();
    Label timerLabel=Ui.Label("Осталось: 04:00",14);
    int seconds=240; System.Windows.Forms.Timer timer=new();

    public TransactionForm()
    {
        Text="BankApp — Транзакция"; Ui.Style(this); Width=750; Height=480;
        var p=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(50)};
        p.Controls.Add(timerLabel,0,0);p.SetColumnSpan(timerLabel,2);
        p.Controls.Add(Ui.Label("Получатель:"),0,1);p.Controls.Add(recipient,1,1);
        p.Controls.Add(Ui.Label("Сумма (₸):"),0,2);p.Controls.Add(amount,1,2);
        p.Controls.Add(Ui.Label("Назначение:"),0,3);p.Controls.Add(desc,1,3);
        p.Controls.Add(Ui.Button("Выполнить перевод",Send),1,4); Controls.Add(p);
        amount.KeyPress+=(s,e)=>{if(!char.IsControl(e.KeyChar)&&!char.IsDigit(e.KeyChar)&&e.KeyChar!=',')e.Handled=true;};
        timer.Interval=1000;timer.Tick+=Tick;timer.Start();
    }
    void Tick(object?s,EventArgs e)
    {
        seconds--; timerLabel.Text=$"Осталось: {seconds/60:00}:{seconds%60:00}";
        if(seconds<=0){timer.Stop();MessageBox.Show("Время выполнения транзакции истекло.");Close();}
    }
    void Send(object?s,EventArgs e)
    {
        if(string.IsNullOrWhiteSpace(recipient.Text)||!decimal.TryParse(amount.Text,out var v)||v<=0){MessageBox.Show("Проверьте данные.");return;}
        var u=BankDataService.CurrentUser!;if(v>u.Balance){MessageBox.Show("Недостаточно средств.");return;}
        u.Balance-=v;BankDataService.Save();timer.Stop();
        MessageBox.Show($"Перевод {v:N2} ₸ пользователю {recipient.Text} выполнен.");Close();
    }
    protected override void OnFormClosed(FormClosedEventArgs e){timer.Dispose();base.OnFormClosed(e);}
}
