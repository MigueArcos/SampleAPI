using System;

namespace ArchitectureTest.Domain.Models;

// public static class Cris
// {
// 	public static T UnwrapResult<T, E>(this Result<T, E> result, Action<E> onErrorFound)
// 	{
// 		if (result.Error is not null)
// 			onErrorFound(result.Error);

// 		return result.Value;
// 	}

// 	public static T UnwrapWithGoto<T, E>(this Result<T, E> result, Action<E> onErrorFound)
// 	{
// 		if (result.Error is not null)
// 			onErrorFound(result.Error);

// 		return result.Value;
// 	}

// 	public static T UnwrapResult<T>(this Result<T, Error> result)
// 	{
// 		if (result.Error is not null)
// 			throw new ErrorWrapper(result.Error);

// 		return result.Value;
// 	}

// 	// public static Result<T, E> ResultTransaction<T, E>(Func<Result<T, E>> callback, Action<E> onErrorFound) {
// 	//     var result = callback().HandleResult(onErrorFound);

// 	//     if (result.Error is not null)
// 	//         return result.Error;

// 	//     return result.Value;
// 	// }
// }

public class Result<TValue, TError>
{
	public readonly TValue? Value;
	public readonly TError? Error;

	private bool _isSuccess;

	private Result(TValue value)
	{
		_isSuccess = true;
		Value = value;
		Error = default;
	}

	private Result(TError error)
	{
		_isSuccess = false;
		Value = default;
		Error = error;
	}

	//happy path
	public static implicit operator Result<TValue, TError>(TValue value) => new Result<TValue, TError>(value);

	//error path
	public static implicit operator Result<TValue, TError>(TError error) => new Result<TValue, TError>(error);

	public Result<TValue, TError> Match(Func<TValue, Result<TValue, TError>> success, Func<TError, Result<TValue, TError>> failure)
	{
		if (_isSuccess)
		{
			return success(Value!);
		}
		return failure(Error!);
	}
}
