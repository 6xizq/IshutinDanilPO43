using SmartMarket.Data;
using SmartMarket.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartMarket;

public class OrdersForm : Form
{
    readonly SmartMarketDbContext _db;
    readonly ComboBox _customer = new(), _employee = new();
    readonly DataGridView _products = Grid(), _cart = Grid();
    readonly NumericUpDown _qty = new(); readonly Label _total = new();
    readonly List<CartLine> _lines = new();

    public OrdersForm(SmartMarketDbContext db)
    {
        _db=db; Text="Оформление продажи"; StartPosition=FormStartPosition.CenterParent; ClientSize=new Size(1100,680); MinimumSize=new Size(1000,620);
        var top=new Panel{Dock=DockStyle.Top,Height=82,Padding=new Padding(12),BackColor=Color.FromArgb(245,247,250)};
        top.Controls.Add(LabelFor("Покупатель",12,8)); _customer.SetBounds(12,30,300,30); _customer.DropDownStyle=ComboBoxStyle.DropDownList; top.Controls.Add(_customer);
        top.Controls.Add(LabelFor("Кассир",330,8)); _employee.SetBounds(330,30,300,30); _employee.DropDownStyle=ComboBoxStyle.DropDownList; top.Controls.Add(_employee);
        var add=new Button{Text="Добавить товар →",Left=650,Top=26,Width=150,Height=34};add.Click+=(_,_)=>AddLine();top.Controls.Add(add);
        _qty.SetBounds(815,30,80,30);_qty.Minimum=1;_qty.Maximum=1000;_qty.Value=1;top.Controls.Add(_qty);top.Controls.Add(LabelFor("Количество",815,8));
        Controls.Add(top);

        var split=new SplitContainer{Dock=DockStyle.Fill,Orientation=Orientation.Vertical,SplitterDistance=520,FixedPanel=FixedPanel.Panel1};split.Panel1.Padding=new Padding(10);split.Panel2.Padding=new Padding(10);Controls.Add(split);
        split.Panel1.Controls.Add(_products);split.Panel2.Controls.Add(_cart);
        _products.CellDoubleClick+=(_,_)=>AddLine();

        var bottom=new Panel{Dock=DockStyle.Bottom,Height=62,Padding=new Padding(10)};_total.Text="Итого: 0 ₸";_total.Font=new Font("Segoe UI",13,FontStyle.Bold);_total.AutoSize=true;_total.Left=12;_total.Top=17;bottom.Controls.Add(_total);var save=new Button{Text="Сохранить заказ",Width=180,Height=38,Left=850,Top=10,BackColor=Color.FromArgb(46,125,115),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};save.Click+=(_,_)=>SaveOrder();bottom.Controls.Add(save);var clear=new Button{Text="Очистить",Width=120,Height=38,Left=720,Top=10};clear.Click+=(_,_)=>{_lines.Clear();RefreshCart();};bottom.Controls.Add(clear);Controls.Add(bottom);
        Shown+=(_,_)=>LoadData();
    }
    static Label LabelFor(string text,int x,int y)=>new(){Text=text,Left=x,Top=y,Width=220,Height=20,ForeColor=Color.DimGray};
    static DataGridView Grid()=>new(){Dock=DockStyle.Fill,AutoGenerateColumns=true,ReadOnly=true,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
    void LoadData(){_customer.DataSource=_db.Customers.AsNoTracking().OrderBy(x=>x.FullName).ToList();_customer.DisplayMember="FullName";_customer.ValueMember="CustomerId";_customer.SelectedIndex=-1;_employee.DataSource=_db.Employees.AsNoTracking().OrderBy(x=>x.FullName).ToList();_employee.DisplayMember="FullName";_employee.ValueMember="EmployeeId";if(_employee.Items.Count>0)_employee.SelectedIndex=0;var q=from p in _db.Products.AsNoTracking() where p.Quantity>0 select new ProductRow{ProductId=p.ProductId,Name=p.Name,Price=p.Price,Stock=p.Quantity};_products.DataSource=q.OrderBy(x=>x.Name).ToList();RefreshCart();}
    void AddLine(){if(_products.CurrentRow?.DataBoundItem is not ProductRow r)return;var line=_lines.FirstOrDefault(x=>x.ProductId==r.ProductId);var addQty=(int)_qty.Value;if(line==null)_lines.Add(new CartLine{ProductId=r.ProductId,Name=r.Name,UnitPrice=r.Price,Quantity=Math.Min(addQty,r.Stock)});else line.Quantity=Math.Min(line.Quantity+addQty,r.Stock);RefreshCart();}
    void RefreshCart(){_cart.DataSource=null;_cart.DataSource=_lines.Select((x,i)=>new CartRow{Index=i+1,Name=x.Name,Quantity=x.Quantity,UnitPrice=x.UnitPrice,Sum=x.Quantity*x.UnitPrice}).ToList();_total.Text=$"Итого: {_lines.Sum(x=>x.Quantity*x.UnitPrice):N2} ₸";}
    void SaveOrder(){if(_employee.SelectedItem is not Employee emp){MessageBox.Show("Выберите кассира.");return;}if(_lines.Count==0){MessageBox.Show("Добавьте хотя бы один товар.");return;}using var tx=_db.Database.BeginTransaction();try{var products=_db.Products.Where(p=>_lines.Select(l=>l.ProductId).Contains(p.ProductId)).ToDictionary(p=>p.ProductId);foreach(var l in _lines){if(!products.TryGetValue(l.ProductId,out var p)||p.Quantity<l.Quantity){throw new InvalidOperationException($"Недостаточно товара: {l.Name}");}}var order=new Order{OrderDate=DateTime.Now,CustomerId=_customer.SelectedItem is Customer c?c.CustomerId:null,EmployeeId=emp.EmployeeId,TotalAmount=_lines.Sum(l=>l.Quantity*l.UnitPrice)};_db.Orders.Add(order);_db.SaveChanges();foreach(var l in _lines){var p=products[l.ProductId];p.Quantity-=l.Quantity;_db.OrderItems.Add(new OrderItem{OrderId=order.OrderId,ProductId=l.ProductId,Quantity=l.Quantity,UnitPrice=l.UnitPrice});}_db.SaveChanges();tx.Commit();MessageBox.Show($"Заказ №{order.OrderId} сохранён.\nСумма: {order.TotalAmount:N2} ₸","Продажа",MessageBoxButtons.OK,MessageBoxIcon.Information);_lines.Clear();LoadData();}catch(Exception ex){try{tx.Rollback();}catch{}MessageBox.Show(ex.Message,"Ошибка сохранения заказа",MessageBoxButtons.OK,MessageBoxIcon.Error);}}
    sealed class ProductRow{public int ProductId{get;set;}public string Name{get;set;}="";public decimal Price{get;set;}public int Stock{get;set;}}
    sealed class CartLine{public int ProductId{get;set;}public string Name{get;set;}="";public decimal UnitPrice{get;set;}public int Quantity{get;set;}}
    sealed class CartRow{public int Index{get;set;}public string Name{get;set;}="";public int Quantity{get;set;}public decimal UnitPrice{get;set;}public decimal Sum{get;set;}}
}
