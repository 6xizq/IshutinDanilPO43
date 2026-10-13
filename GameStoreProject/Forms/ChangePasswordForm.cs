using GameStore.Services;
namespace GameStore.Forms;
public class ChangePasswordForm:Form{
 TextBox login=new(),oldP=new(),newP=new();
 public ChangePasswordForm(){Text="Смена пароля";Width=420;Height=300;StartPosition=FormStartPosition.CenterParent;Font=new Font("Arial",10);for(int i=0;i<3;i++){Controls.Add(new Label{Text=new[]{"Логин","Старый пароль","Новый пароль"}[i],Left=40,Top=25+i*65,AutoSize=true});var t=new[]{login,oldP,newP}[i];t.SetBounds(40,45+i*65,320,30);Controls.Add(t);}oldP.PasswordChar=newP.PasswordChar='●';var b=new Button{Text="Изменить",Bounds=new Rectangle(40,225,320,38),BackColor=Color.SeaGreen,ForeColor=Color.White};b.Click+=(_,_)=>{if(newP.Text.Length<4){MessageBox.Show("Новый пароль должен содержать минимум 4 символа.");return;}if(StoreService.ChangePassword(login.Text,oldP.Text,newP.Text)){MessageBox.Show("Пароль изменён.");Close();}else MessageBox.Show("Неверный логин или старый пароль.");};Controls.Add(b);}
}
