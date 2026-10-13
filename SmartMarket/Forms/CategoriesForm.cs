using SmartMarket.Data;
using SmartMarket.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartMarket;

public class CategoriesForm : Form
{
    readonly SmartMarketDbContext _db; readonly DataGridView _grid = Grid();
    public CategoriesForm(SmartMarketDbContext db)
    {
        _db=db; Text="Категории"; StartPosition=FormStartPosition.CenterParent; ClientSize=new Size(720,520);
        Controls.Add(_grid); var bar=Bar("Добавить",Add,"Изменить",Edit,"Удалить",Delete,"Обновить",LoadData); Controls.Add(bar);
        Shown+=(_,_)=>LoadData();
    }
    static DataGridView Grid()=>new(){Dock=DockStyle.Fill,AutoGenerateColumns=true,ReadOnly=true,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
    static FlowLayoutPanel Bar(params object[] x){var p=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=58,Padding=new Padding(10)}; for(int i=0;i<x.Length;i+=2){var b=new Button{Text=(string)x[i],Width=120,Height=36}; var a=(Action)x[i+1]; b.Click+=(_,_)=>a(); p.Controls.Add(b);} return p;}
    void LoadData(){_grid.DataSource=_db.Categories.AsNoTracking().OrderBy(x=>x.Name).ToList();}
    Category? Selected()=>_grid.CurrentRow?.DataBoundItem as Category;
    void Add(){var name=Prompt.Show("Название категории:"); if(string.IsNullOrWhiteSpace(name))return; _db.Categories.Add(new Category{Name=name.Trim()});_db.SaveChanges();LoadData();}
    void Edit(){var c=Selected();if(c==null)return;var name=Prompt.Show("Название:",c.Name);if(string.IsNullOrWhiteSpace(name))return;c.Name=name.Trim();_db.SaveChanges();LoadData();}
    void Delete(){var c=Selected();if(c==null)return; if(_db.Products.Any(p=>p.CategoryId==c.CategoryId)){MessageBox.Show("Нельзя удалить категорию, в которой есть товары.");return;}if(MessageBox.Show("Удалить категорию?","Подтверждение",MessageBoxButtons.YesNo)==DialogResult.Yes){_db.Categories.Remove(c);_db.SaveChanges();LoadData();}}
}
