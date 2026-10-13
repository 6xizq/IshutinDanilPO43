using System;
using System.Windows.Forms;
using SmartMarket.Data;

namespace SmartMarket;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var db = new SmartMarketDbContext();
        Application.Run(new MainForm(db));
    }
}
