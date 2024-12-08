using System.Collections.Generic;
using System.Linq;

namespace ArchitectureTest.Tests.Shared;

public class TestCombinationsGenerator {
    public static IEnumerable<object[]> Combine(TestParam[] testParams) {
        return GenerateCombinations(testParams, new object[] { }, testParams.Length);
    }
    private static IEnumerable<object[]> GenerateCombinations(TestParam[] testParams, object[] currentCombination, int blanks) {
        IEnumerable<object[]> combinations = new List<object[]>();
        int paramIndex = (testParams.Length - blanks) % testParams.Length;
        object[] possibleParamValues = testParams[paramIndex].PossibleValues;
        int possibleParamValuesLength = possibleParamValues.Count();
        for (var i = 0; i < possibleParamValuesLength; i++) {
            var currentValue = possibleParamValues[i];
            if (blanks == 1) {
                combinations = combinations.Append(currentCombination.Append(currentValue).ToArray());
            }
            else {
                /// var possibleValuesExclusingThisOne = possibleValues.Where(v => v != currentValue).ToList();
                var moreCombinations = GenerateCombinations(
                    testParams, currentCombination.Append(currentValue).ToArray(), blanks - 1
                );
                combinations = combinations.Concat(moreCombinations);
            }
        }
        return combinations;
    }
}
public class TestParam {
    public string Name { get; set; }
    public object[] PossibleValues { get; set; }
}
