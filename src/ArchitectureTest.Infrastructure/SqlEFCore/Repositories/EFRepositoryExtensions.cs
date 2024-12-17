using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ArchitectureTest.Infrastructure.SqlEFCore.Repostories;

public static class EFRepositoryExtensions {
    public static Task<T?> AvoidTracking<T>(this Task<T?> task, DbSet<T> dbSet) where T : class {
        return task.ContinueWith(t => {
            if (t.Result is not null)
                dbSet.Entry(t.Result).State = EntityState.Detached;

            return t.Result;
        }, TaskContinuationOptions.ExecuteSynchronously);
    }
}
