using GameStore.Services;

namespace GameStore;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        StoreService.Initialize();
        Application.Run(new Forms.LoginForm());
    }
}
