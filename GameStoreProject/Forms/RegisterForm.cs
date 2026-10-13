using GameStore.Models; using GameStore.Services;
namespace GameStore.Forms;
public class RegisterForm:Form{
 TextBox login=new(),pass=new(),name=new(),email=new();
 public RegisterForm(){Text="Регистрация";Width=430;Height=390;StartPosition=FormStartPosition.CenterParent;Font=new Font("Arial",10);BackColor=Color.FromArgb(191,251,202); string[] n={"Логин","Пароль","Имя","Email"}; TextBox[] t={login,pass,name,email}; for(int i=0;i<4;i++){Controls.Add(new Label{Text=n[i],Left=45,Top=30+i*60,AutoSize=true});t[i].SetBounds(45,50+i*60,320,30);Controls.Add(t[i]);} pass.PasswordChar='●'; var b=new Button{Text="Зарегистрироваться",Bounds=new Rectangle(45,300,320,40),BackColor=Color.SeaGreen,ForeColor=Color.White}; b.Click+=(_,_)=>{if(new[]{login.Text,pass.Text,name.Text,email.Text}.Any(string.IsNullOrWhiteSpace)||!email.Text.Contains('@')){MessageBox.Show("Заполните все поля корректно.");return;} if(StoreService.Register(new User{Login=login.Text.Trim(),Password=pass.Text,Name=name.Text.Trim(),Email=email.Text.Trim()})){MessageBox.Show("Регистрация выполнена.");Close();}else MessageBox.Show("Такой логин уже существует.");};Controls.Add(b);}
}
