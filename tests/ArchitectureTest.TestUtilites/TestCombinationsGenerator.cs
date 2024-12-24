using System.Collections.Generic;
using System.Linq;

namespace ArchitectureTest.Tests.Shared;

public class TestCombinationsGenerator {
    public static IEnumerable<object[]> Combine(ITestParam[] testParams) {
        return GenerateCombinations(testParams, [], testParams.Length);
    }

    private static IEnumerable<object[]> GenerateCombinations(ITestParam[] testParams, object[] currentCombination, int blanks)
    {
        IEnumerable<object[]> combinations = [];
        int paramIndex = (testParams.Length - blanks) % testParams.Length;
        object[]? possibleParamValues = testParams[paramIndex].PossibleValues;
        int possibleParamValuesLength = possibleParamValues?.Length ?? 0;
        for (var i = 0; i < possibleParamValuesLength; i++) {
            var currentValue = possibleParamValues![i];
            if (blanks == 1) {
                combinations = combinations.Append([.. currentCombination, currentValue]);
            }
            else {
                /// var possibleValuesExclusingThisOne = possibleValues.Where(v => v != currentValue).ToList();
                var moreCombinations = GenerateCombinations(
                    testParams, [.. currentCombination, currentValue], blanks - 1
                );
                combinations = combinations.Concat(moreCombinations);
            }
        }
        return combinations;
    }
}

public interface ITestParam {
    public string? Name { get; set; }
    public object[]? PossibleValues { get; set; }
}

public class TestParam<T> : ITestParam {
    public string? Name { get; set; }
    public T[]? PossibleValues { get; set; }
    object[]? ITestParam.PossibleValues {
        get => PossibleValues?.Cast<object>().ToArray();
        set => PossibleValues = value as T[];
    }
}
