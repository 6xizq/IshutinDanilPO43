using SmartMarket.Data;
using SmartMarket.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartMarket;

public class ProductsForm : Form
{
    readonly SmartMarketDbContext _db; readonly DataGridView _grid=Grid(); readonly TextBox _search=new();
    public ProductsForm(SmartMarketDbContext db){_db=db;Text="Товары";StartPosition=FormStartPosition.CenterParent;ClientSize=new Size(1000,600);
        var top=new Panel{Dock=DockStyle.Top,Height=50,Padding=new Padding(10)};_search.PlaceholderText="Поиск товара";_search.Width=360;top.Controls.Add(_search);_search.TextChanged+=(_,_)=>LoadData();Controls.Add(top);Controls.Add(_grid);Controls.Add(Bar("Добавить",Add,"Изменить",Edit,"Удалить",Delete,"Обновить",LoadData));Shown+=(_,_)=>LoadData();}
    static DataGridView Grid()=>new(){Dock=DockStyle.Fill,AutoGenerateColumns=true,ReadOnly=true,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
    static FlowLayoutPanel Bar(params object[] x){var p=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=58,Padding=new Padding(10)};for(int i=0;i<x.Length;i+=2){var b=new Button{Text=(string)x[i],Width=120,Height=36};var a=(Action)x[i+1];b.Click+=(_,_)=>a();p.Controls.Add(b);}return p;}
    void LoadData(){var q=from p in _db.Products.AsNoTracking() join c in _db.Categories.AsNoTracking() on p.CategoryId equals c.CategoryId select new ProductRow{ProductId=p.ProductId,Name=p.Name,Price=p.Price,Quantity=p.Quantity,Category=c.Name,CategoryId=p.CategoryId};var s=_search.Text.Trim();if(s.Length>0)q=q.Where(x=>x.Name.Contains(s)||x.Category.Contains(s));_grid.DataSource=q.OrderBy(x=>x.Name).ToList();}
    ProductRow? Selected()=>_grid.CurrentRow?.DataBoundItem as ProductRow;
    void Add()=>EditCore(null); void Edit(){var r=Selected();if(r==null)return;var p=_db.Products.Find(r.ProductId);if(p!=null)EditCore(p);}
    void EditCore(Product? p){var cats=_db.Categories.AsNoTracking().OrderBy(c=>c.Name).ToList();var f=new Form{Text=p==null?"Новый товар":"Изменение товара",StartPosition=FormStartPosition.CenterParent,ClientSize=new Size(500,330)};var name=new TextBox{Left=20,Top=22,Width=440,PlaceholderText="Название",Text=p?.Name??""};var price=new NumericUpDown{Left=20,Top=75,Width=180,Maximum=100000000,DecimalPlaces=2,Value=p?.Price??1};var qty=new NumericUpDown{Left=230,Top=75,Width=180,Maximum=1000000,Value=p?.Quantity??0};var cat=new ComboBox{Left=20,Top=128,Width=390,DropDownStyle=ComboBoxStyle.DropDownList,DataSource=cats,DisplayMember="Name",ValueMember="CategoryId"};if(p!=null)cat.SelectedValue=p.CategoryId;var ok=new Button{Left=20,Top=190,Width=180,Height=40,Text="Сохранить"};ok.Click+=(_,_)=>{if(string.IsNullOrWhiteSpace(name.Text)){MessageBox.Show("Название не должно быть пустым.");return;}if(p==null)_db.Products.Add(new Product{Name=name.Text.Trim(),Price=price.Value,Quantity=(int)qty.Value,CategoryId=(int)cat.SelectedValue!});else{p.Name=name.Text.Trim();p.Price=price.Value;p.Quantity=(int)qty.Value;p.CategoryId=(int)cat.SelectedValue!;}try{_db.SaveChanges();f.DialogResult=DialogResult.OK;f.Close();}catch(Exception ex){MessageBox.Show(ex.Message);}};f.Controls.AddRange(new Control[]{name,price,qty,cat,ok});f.ShowDialog(this);LoadData();}
    void Delete(){var r=Selected();if(r==null)return;if(_db.OrderItems.Any(x=>x.ProductId==r.ProductId)){MessageBox.Show("Товар используется в заказах. Лучше оставить его с остатком 0.");return;}if(MessageBox.Show("Удалить товар?","Подтверждение",MessageBoxButtons.YesNo)==DialogResult.Yes){_db.Products.Remove(_db.Products.Find(r.ProductId)!);_db.SaveChanges();LoadData();}}
    sealed class ProductRow{public int ProductId{get;set;}public string Name{get;set;}="";public decimal Price{get;set;}public int Quantity{get;set;}public string Category{get;set;}="";public int CategoryId{get;set;}}
}
