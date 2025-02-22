using System;
using System.Threading.Tasks;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using Microsoft.Extensions.Logging;

namespace ArchitectureTest.Domain.Services;

// Notes
// - Why this is an extension method and not part of the interface?
//   This simplifies testability, because if this would be part of the interface, then we would need to mock this method
//   to use this real implementation, the other option would be to use the concrete BaseUnitOfWork, but that class requires
//   a constructor with arguments (this makes harder to create a mock) and also that class belongs to infrastructure, so,
//   the domain tests would need to know the infrastructure layer     

// - Why the transactionFunc parameter is a Func<Task<Result<T, AppError>>> instead of only Task<Result<T, AppError>>?
//   This is to avoid race conditions, if we would pass directly a Task to the RunAsyncTransaction method then this Task
//   would try to be executed inmediately, basically it will be executed at the same time the StartTransaction method is 
//   being executed, by wrapping the Task in a Func we have control over when the Task is actually executed
//
//   Take a look at this example, this will compile, but the await on unitOfWork.RunAsyncTransaction and the awaits on
//   TransactionStatements method will be executed at the same time
//
//   public static async Task SomethingWeird(IUnitOfWork unitOfWork, ILogger logger){
//     var result = await unitOfWork.RunAsyncTransaction(TransactionStatements(unitOfWork), logger);
//   }
//
//   public static async Task<Result<bool, AppError>> TransactionStatements(IUnitOfWork unitOfWork) {
//     await unitOfWork.Repository<Note>().DeleteById(1);
//     await unitOfWork.Repository<Note>().Create(new Note{});
//     return true;
//   }

public static class UnitOfWorkExtensions {
    public static async Task<Result<T, AppError>> RunAsyncTransaction<T>(
        this IUnitOfWork unitOfWork, Func<Task<Result<T, AppError>>> transactionFunc,
        ILogger logger, Action? finallyCallback = null
    ){
        try
        {
            await unitOfWork.StartTransaction().ConfigureAwait(false);
            var result = await transactionFunc().ConfigureAwait(false);

            if (result.Error != null){
                await unitOfWork.Rollback().ConfigureAwait(false);
                return result.Error;
            }
            else {
                await unitOfWork.Commit().ConfigureAwait(false);
                return result.Value!;
            }
        }
        catch (Exception e) {
            await unitOfWork.Rollback().ConfigureAwait(false);
            logger.LogError(e, ErrorMessages.DbTransactionError);
            return new AppError(ErrorCodes.RepoProblem);
        }
        finally {
            finallyCallback?.Invoke();
        }
    }

    public static async Task<AppError?> RunAsyncTransaction(
        this IUnitOfWork unitOfWork, Func<Task> transactionFunc,
        ILogger logger, Action? finallyCallback = null
    ){
        try
        {
            await unitOfWork.StartTransaction().ConfigureAwait(false);
            await transactionFunc().ConfigureAwait(false);
            
            await unitOfWork.Commit().ConfigureAwait(false);
            return null;
        }
        catch (Exception e) {
            await unitOfWork.Rollback().ConfigureAwait(false);
            logger.LogError(e, ErrorMessages.DbTransactionError);
            return new AppError(ErrorCodes.RepoProblem);
        }
        finally {
            finallyCallback?.Invoke();
        }
    }
}
