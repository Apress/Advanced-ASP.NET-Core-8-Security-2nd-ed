using Microsoft.EntityFrameworkCore.Diagnostics;

namespace JuiceShopDotNet.Safe.Data.Interceptors;

public class DefaultInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var toSave = eventData.Context.ChangeTracker.Entries();
        return base.SavingChanges(eventData, result);
    }
}
