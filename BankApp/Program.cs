using BankApp.Forms;
using BankApp.Services;

namespace BankApp;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        BankDataService.Initialize();
        Application.Run(new LoginForm());
    }
}
