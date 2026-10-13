using SmartMarket.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace SmartMarket;

public class ReportsForm : Form
{
    readonly SmartMarketDbContext _db; readonly DataGridView _orders=Grid(), _items=Grid(); readonly DateTimePicker _from=new(), _to=new();
    public ReportsForm(SmartMarketDbContext db){_db=db;Text="Отчёты по продажам";StartPosition=FormStartPosition.CenterParent;ClientSize=new Size(1050,680);
        var top=new Panel{Dock=DockStyle.Top,Height=52,Padding=new Padding(10)};top.Controls.Add(new Label{Text="С",Left=10,Top=8,AutoSize=true});_from.SetBounds(35,5,130,28);_from.Value=DateTime.Today;top.Controls.Add(_from);top.Controls.Add(new Label{Text="по",Left=175,Top=8,AutoSize=true});_to.SetBounds(205,5,130,28);_to.Value=DateTime.Today;top.Controls.Add(_to);var filter=new Button{Text="Показать",Left=350,Top=4,Width=110,Height=32};filter.Click+=(_,_)=>LoadOrders();top.Controls.Add(filter);var csv=new Button{Text="Экспорт CSV",Left=475,Top=4,Width=120,Height=32};csv.Click+=(_,_)=>ExportCsv();top.Controls.Add(csv);Controls.Add(top);
        var split=new SplitContainer{Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,SplitterDistance=270};split.Panel1.Controls.Add(_orders);split.Panel2.Controls.Add(_items);Controls.Add(split);_orders.SelectionChanged+=(_,_)=>LoadItems();Shown+=(_,_)=>LoadOrders();}
    static DataGridView Grid()=>new(){Dock=DockStyle.Fill,AutoGenerateColumns=true,ReadOnly=true,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
    void LoadOrders(){var a=_from.Value.Date;var b=_to.Value.Date.AddDays(1);var q=from o in _db.Orders.AsNoTracking() join c in _db.Customers.AsNoTracking() on o.CustomerId equals c.CustomerId into cj from c in cj.DefaultIfEmpty() join e in _db.Employees.AsNoTracking() on o.EmployeeId equals e.EmployeeId select new OrderRow{OrderId=o.OrderId,OrderDate=o.OrderDate,Customer=c==null?"Без покупателя":c.FullName,Employee=e.FullName,TotalAmount=o.TotalAmount};_orders.DataSource=q.Where(x=>x.OrderDate>=a&&x.OrderDate<b).OrderByDescending(x=>x.OrderDate).ToList();LoadItems();}
    void LoadItems(){if(_orders.CurrentRow?.DataBoundItem is not OrderRow o){_items.DataSource=null;return;}var q=from i in _db.OrderItems.AsNoTracking() join p in _db.Products.AsNoTracking() on i.ProductId equals p.ProductId where i.OrderId==o.OrderId select new ItemRow{Product=p.Name,Quantity=i.Quantity,UnitPrice=i.UnitPrice,Sum=i.Quantity*i.UnitPrice};_items.DataSource=q.ToList();}
    void ExportCsv(){if(_orders.DataSource is not IEnumerable<OrderRow> rows)return;using var sfd=new SaveFileDialog{Filter="CSV (*.csv)|*.csv",FileName="SmartMarket_Sales.csv"};if(sfd.ShowDialog(this)!=DialogResult.OK)return;var sb=new StringBuilder("OrderId;Date;Customer;Employee;Total\r\n");foreach(var r in rows)sb.AppendLine($"{r.OrderId};{r.OrderDate:dd.MM.yyyy HH:mm};\"{r.Customer.Replace("\"","\"\"")}\";\"{r.Employee.Replace("\"","\"\"")}\";{r.TotalAmount.ToString(System.Globalization.CultureInfo.InvariantCulture)}");File.WriteAllText(sfd.FileName,sb.ToString(),Encoding.UTF8);MessageBox.Show("CSV сохранён.");}
    sealed class OrderRow{public int OrderId{get;set;}public DateTime OrderDate{get;set;}public string Customer{get;set;}="";public string Employee{get;set;}="";public decimal TotalAmount{get;set;}}
    sealed class ItemRow{public string Product{get;set;}="";public int Quantity{get;set;}public decimal UnitPrice{get;set;}public decimal Sum{get;set;}}
}
