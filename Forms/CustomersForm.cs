using SmartMarket.Data;
using SmartMarket.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartMarket;

public class CustomersForm : Form
{
    readonly SmartMarketDbContext _db; readonly DataGridView _grid=Grid(); readonly TextBox _search=new();
    public CustomersForm(SmartMarketDbContext db){_db=db;Text="Покупатели";StartPosition=FormStartPosition.CenterParent;ClientSize=new Size(820,560);
        var top=new Panel{Dock=DockStyle.Top,Height=48,Padding=new Padding(10)};_search.PlaceholderText="Поиск по ФИО или телефону";_search.Width=350;top.Controls.Add(_search);_search.TextChanged+=(_,_)=>LoadData();Controls.Add(top);
        Controls.Add(_grid);Controls.Add(Bar("Добавить",Add,"Изменить",Edit,"Удалить",Delete,"Обновить",LoadData));Shown+=(_,_)=>LoadData();}
    static DataGridView Grid()=>new(){Dock=DockStyle.Fill,AutoGenerateColumns=true,ReadOnly=true,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
    static FlowLayoutPanel Bar(params object[] x){var p=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=58,Padding=new Padding(10)};for(int i=0;i<x.Length;i+=2){var b=new Button{Text=(string)x[i],Width=120,Height=36};var a=(Action)x[i+1];b.Click+=(_,_)=>a();p.Controls.Add(b);}return p;}
    void LoadData(){var q=_db.Customers.AsNoTracking().AsQueryable();var s=_search.Text.Trim();if(s.Length>0)q=q.Where(x=>x.FullName.Contains(s)||x.Phone.Contains(s));_grid.DataSource=q.OrderBy(x=>x.FullName).ToList();}
    Customer? Selected()=>_grid.CurrentRow?.DataBoundItem as Customer;
    void Add(){EditCore(null);} void Edit(){EditCore(Selected());}
    void EditCore(Customer? c){var f=new Form{Text=c==null?"Новый покупатель":"Изменение",StartPosition=FormStartPosition.CenterParent,ClientSize=new Size(420,220)};var n=new TextBox{PlaceholderText="ФИО",Left=20,Top=25,Width=360,Text=c?.FullName??""};var p=new TextBox{PlaceholderText="Телефон",Left=20,Top=75,Width=360,Text=c?.Phone??""};var ok=new Button{Text="Сохранить",Left=20,Top=130,Width=160};ok.Click+=(_,_)=>{if(string.IsNullOrWhiteSpace(n.Text)){MessageBox.Show("ФИО не должно быть пустым.");return;}if(c==null)_db.Customers.Add(new Customer{FullName=n.Text.Trim(),Phone=p.Text.Trim()});else{c.FullName=n.Text.Trim();c.Phone=p.Text.Trim();}try{_db.SaveChanges();f.DialogResult=DialogResult.OK;f.Close();}catch(Exception ex){MessageBox.Show(ex.Message);}};f.Controls.AddRange(new Control[]{n,p,ok});f.ShowDialog(this);LoadData();}
    void Delete(){var c=Selected();if(c==null)return;if(_db.Orders.Any(o=>o.CustomerId==c.CustomerId)){MessageBox.Show("Покупатель связан с заказами и не может быть удалён.");return;}if(MessageBox.Show("Удалить покупателя?","Подтверждение",MessageBoxButtons.YesNo)==DialogResult.Yes){_db.Customers.Remove(c);_db.SaveChanges();LoadData();}}
}
