namespace GameStore.Forms;
public class ImageForm:Form{public ImageForm(Image? image,string title){Text=title;Width=850;Height=650;StartPosition=FormStartPosition.CenterParent;var p=new PictureBox{Dock=DockStyle.Fill,Image=image,SizeMode=PictureBoxSizeMode.Zoom};Controls.Add(p);}}
