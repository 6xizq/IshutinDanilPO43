using Microsoft.EntityFrameworkCore;
using SmartMarket.Data;

namespace SmartMarket.Services;

public static class DbService
{
    public static bool CanConnect(SmartMarketDbContext db, out string message)
    {
        try
        {
            db.Database.CanConnect();
            message = "Подключение к SmartMarketDb успешно.";
            return true;
        }
        catch (Exception ex)
        {
            message = "Не удалось подключиться к SQL Server.\n\n" + ex.Message;
            return false;
        }
    }
}
