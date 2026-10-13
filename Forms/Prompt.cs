namespace SmartMarket;

public static class Prompt
{
    public static string? Show(string title,string value="")
    {
        using var f=new Form{Text=title,StartPosition=FormStartPosition.CenterParent,ClientSize=new Size(420,150)};
        var t=new TextBox{Left=20,Top=20,Width=370,Text=value};var ok=new Button{Text="OK",Left=20,Top=65,Width=100,DialogResult=DialogResult.OK};var cancel=new Button{Text="Отмена",Left=130,Top=65,Width=100,DialogResult=DialogResult.Cancel};f.Controls.AddRange(new Control[]{t,ok,cancel});f.AcceptButton=ok;f.CancelButton=cancel;return f.ShowDialog()==DialogResult.OK?t.Text:null;
    }
}
