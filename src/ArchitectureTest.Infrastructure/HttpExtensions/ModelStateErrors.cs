using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace ArchitectureTest.Infrastructure.HttpExtensions;

public static class ModelStateErrors {
	public static IList<string> GetErrors(this ModelStateDictionary modelState) {
		return modelState.Values.SelectMany(v => v.Errors)
								.Select(v => v.ErrorMessage).ToList();
	}
}
