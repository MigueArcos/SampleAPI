using System;
using System.Collections.Generic;
using System.Linq;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Services.Application.EntityCrudService;
using ArchitectureTest.TestUtils;
using FluentAssertions;
using Xunit;

namespace ArchitectureTest.Domain.Tests.ApplicationServices;

public class ApplicationModelsMappingProfileTests
{
    [Fact]
    public void FlattenChecklistDetails_WhenEverythingIsOK_ShouldReturnPlainList()
    {
        // Arrange
        var inputData = BuildComplexChecklistDetailsAsTree();

        // Act
        var flattenedDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(inputData);

        // Assert
        flattenedDetails.Should().NotBeNullOrEmpty();
        flattenedDetails.ForEach(d =>
        {
            d.SubItems.Should().BeNullOrEmpty();
            d.ChecklistId.Should().Be(StubData.ChecklistId);
        });
        var expectedDetailsIDs = new Dictionary<int, (string DetailId, string? ParentDetailId)>
        {
            [0] = ("1", null),
            [1] = ("4", "1"),
            [2] = ("6", "4"),
            [3] = ("8", "6"),
            [4] = ("7", "4"),
            [5] = ("9", "7"),
            [6] = ("10", "7"),
            [7] = ("12", "10"),
            [8] = ("13", "12"),
            [9] = ("14", "13"),
            [10] = ("15", "14"),
            [11] = ("16", "14"),
            [12] = ("11", "7"),
            [13] = ("5", "1"),
            [14] = ("2", null),
            [15] = ("17", "2"),
            [16] = ("18", "2"),
            [17] = ("3", null)
        };
        flattenedDetails.Count.Should().Be(expectedDetailsIDs.Count);
        foreach (var kvp in expectedDetailsIDs)
        {
            flattenedDetails[kvp.Key].Id.Should().Be(kvp.Value.DetailId);
            flattenedDetails[kvp.Key].ParentDetailId.Should().Be(kvp.Value.ParentDetailId);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FlattenChecklistDetails_WhenInputListIsNullOrEmpty_ShouldReturnPlainList(bool useEmpty)
    {
        // Arrange
        List<ChecklistDetail>? inputData = useEmpty ? [] : null;

        // Act
        var flattenedDetails = ApplicationModelsMappingProfile.FlattenChecklistDetails(inputData);

        // Assert
        flattenedDetails.Should().BeEmpty();
    }

    [Fact]
    public void FlattenAndPopulateChecklistDetails_WhenEverythingIsOK_ShouldReturnPlainList()
    {
        // Arrange
        var inputData = BuildComplexChecklistDetailsAsTree();

        // Act
        var flattenedDetails = ApplicationModelsMappingProfile.FlattenAndPopulateChecklistDetails(
            StubData.ChecklistId, inputData
        );

        // Assert
        flattenedDetails.Should().NotBeNullOrEmpty();
        flattenedDetails.ForEach(d =>
        {
            d.SubItems.Should().BeNullOrEmpty();
            d.ChecklistId.Should().Be(StubData.ChecklistId);
            d.Id.Should().NotBeNull();
            Guid.TryParse(d.Id, out _).Should().BeTrue();
            if (!string.IsNullOrWhiteSpace(d.ParentDetailId))
                Guid.TryParse(d.ParentDetailId, out _).Should().BeTrue();
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FlattenAndPopulateChecklistDetails_WhenInputListIsNullOrEmpty_ShouldReturnPlainList(bool useEmpty)
    {
        // Arrange
        List<ChecklistDetail>? inputData = useEmpty ? [] : null;

        // Act
        var flattenedDetails = ApplicationModelsMappingProfile.FlattenAndPopulateChecklistDetails(
            StubData.ChecklistId, inputData
        );

        // Assert
        flattenedDetails.Should().BeEmpty();
    }

    [Fact]
    public void FormatChecklistDetails_WhenEverythingIsOK_ShouldReturnListAsTree()
    {
        // Arrange
        void resetDates(IList<ChecklistDetail> details)
        {
            foreach (var d in details)
            {
                d.CreationDate = DateTime.MinValue;
                d.ModificationDate = null;

                if (d.SubItems == null)
                {
                    d.SubItems = [];
                    continue;
                }

                if (d.SubItems.Count > 0)
                    resetDates(d.SubItems);
            }
        }
        var inputData = BuildChecklistDetailsAsList();

        // Act
        var result = ApplicationModelsMappingProfile.FormatChecklistDetails(inputData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var expectedTree = BuildComplexChecklistDetailsAsTree();
        resetDates(expectedTree);
        resetDates(result!);
        ObjectComparer.JsonCompare(expectedTree, result).Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FormatChecklistDetails_WhenDetailsAreNullOrEmpty_ShouldReturnEmptyList(bool useEmpty)
    {
        // Arrange
        var inputData = useEmpty ? new List<ChecklistDetail>() : null;

        // Act
        var result = ApplicationModelsMappingProfile.FormatChecklistDetails(inputData);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FindAllDetailsToRemove_WhenEverythingIsOK_ShouldReturnCompleteListToRemove()
    {
        // Arrange
        var flattenedDetails = BuildChecklistDetailsAsList();
        var inputData = new List<string> { "3", "4", "17"};

        // Act
        var result = ApplicationModelsMappingProfile.FindAllDetailsToRemove(flattenedDetails, inputData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var expectedResult = new List<string> { "3", "4", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17" };
        result.Count.Should().Be(expectedResult.Count);
        result.Except(expectedResult).Should().BeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FindAllDetailsToRemove_WhenDetailsAreEmptyOrNull_ShouldReturnSameListAsInput(bool useEmpty)
    {
        // Arrange
        var flattenedDetails = useEmpty ? new List<ChecklistDetail>() : null;
        var inputData = new List<string> { "3", "4", "17"};

        // Act
        var result = ApplicationModelsMappingProfile.FindAllDetailsToRemove(flattenedDetails, inputData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Count.Should().Be(inputData.Count);
        result.Except(inputData).Should().BeEmpty();
    }

    private List<ChecklistDetail> BuildComplexChecklistDetailsAsTree()
    {
        return new List<ChecklistDetail>{
            BuildChecklistDetail(
                detailId: "1",
                checklistId: StubData.ChecklistId,
                taskName: "one",
                parentDetailId: null,
                subItems: [
                    BuildChecklistDetail(
                        detailId: "4",
                        checklistId: StubData.ChecklistId,
                        taskName: "four",
                        parentDetailId: "1",
                        subItems: [
                            BuildChecklistDetail(
                                detailId: "6",
                                checklistId: StubData.ChecklistId,
                                taskName: "six",
                                parentDetailId: "4",
                                subItems: [
                                    BuildChecklistDetail(
                                        detailId: "8",
                                        checklistId: StubData.ChecklistId,
                                        taskName: "eight",
                                        parentDetailId: "6"
                                    )
                                ]
                            ),
                            BuildChecklistDetail(
                                detailId: "7",
                                checklistId: StubData.ChecklistId,
                                taskName: "seven",
                                parentDetailId: "4",
                                subItems: [
                                    BuildChecklistDetail(
                                        detailId: "9",
                                        checklistId: StubData.ChecklistId,
                                        taskName: "nine",
                                        parentDetailId: "7"
                                    ),
                                    BuildChecklistDetail(
                                        detailId: "10",
                                        checklistId: StubData.ChecklistId,
                                        taskName: "ten",
                                        parentDetailId: "7",
                                        subItems: [
                                            BuildChecklistDetail(
                                                detailId: "12",
                                                checklistId: StubData.ChecklistId,
                                                taskName: "twelve",
                                                parentDetailId: "10",
                                                subItems: [
                                                    BuildChecklistDetail(
                                                        detailId: "13",
                                                        checklistId: StubData.ChecklistId,
                                                        taskName: "thirteen",
                                                        parentDetailId: "12",
                                                        subItems: [
                                                            BuildChecklistDetail(
                                                                detailId: "14",
                                                                checklistId: StubData.ChecklistId,
                                                                taskName: "fourteen",
                                                                parentDetailId: "13",
                                                                subItems: [
                                                                    BuildChecklistDetail(
                                                                        detailId: "15",
                                                                        checklistId: StubData.ChecklistId,
                                                                        taskName: "fifteen",
                                                                        parentDetailId: "14"
                                                                    ),
                                                                    BuildChecklistDetail(
                                                                        detailId: "16",
                                                                        checklistId: StubData.ChecklistId,
                                                                        taskName: "sixteen",
                                                                        parentDetailId: "14"
                                                                    )
                                                                ]
                                                            )
                                                        ]
                                                    )
                                                ]
                                            )
                                        ]
                                    ),
                                    BuildChecklistDetail(
                                        detailId: "11",
                                        checklistId: StubData.ChecklistId,
                                        taskName: "eleven",
                                        parentDetailId: "7"
                                    )
                                ]
                            )
                        ]
                    ),
                    BuildChecklistDetail(
                        detailId: "5",
                        checklistId: StubData.ChecklistId,
                        taskName: "five",
                        parentDetailId: "1"
                    )
                ]
            ),
            BuildChecklistDetail(
                detailId: "2",
                checklistId: StubData.ChecklistId,
                taskName: "two",
                parentDetailId: null,
                subItems: [
                    BuildChecklistDetail(
                        detailId: "17",
                        checklistId: StubData.ChecklistId,
                        taskName: "seventeen",
                        parentDetailId: "2"
                    ),
                    BuildChecklistDetail(
                        detailId: "18",
                        checklistId: StubData.ChecklistId,
                        taskName: "eighteen",
                        parentDetailId: "2"
                    )
                ]
            ),
            BuildChecklistDetail(
                detailId: "3",
                checklistId: StubData.ChecklistId,
                taskName: "three",
                parentDetailId: null
            )
        };
    }

    private List<ChecklistDetail> BuildChecklistDetailsAsList()
    {
        return new List<ChecklistDetail>{
            BuildChecklistDetail(
                detailId: "1",
                checklistId: StubData.ChecklistId,
                taskName: "one",
                parentDetailId: null
            ),
            BuildChecklistDetail(
                detailId: "2",
                checklistId: StubData.ChecklistId,
                taskName: "two",
                parentDetailId: null
            ),
            BuildChecklistDetail(
                detailId: "3",
                checklistId: StubData.ChecklistId,
                taskName: "three",
                parentDetailId: null
            ),
            BuildChecklistDetail(
                detailId: "4",
                checklistId: StubData.ChecklistId,
                taskName: "four",
                parentDetailId: "1"
            ),
            BuildChecklistDetail(
                detailId: "5",
                checklistId: StubData.ChecklistId,
                taskName: "five",
                parentDetailId: "1"
            ),
            BuildChecklistDetail(
                detailId: "6",
                checklistId: StubData.ChecklistId,
                taskName: "six",
                parentDetailId: "4"
            ),
            BuildChecklistDetail(
                detailId: "7",
                checklistId: StubData.ChecklistId,
                taskName: "seven",
                parentDetailId: "4"
            ),
            BuildChecklistDetail(
                detailId: "8",
                checklistId: StubData.ChecklistId,
                taskName: "eight",
                parentDetailId: "6"
            ),
            BuildChecklistDetail(
                detailId: "9",
                checklistId: StubData.ChecklistId,
                taskName: "nine",
                parentDetailId: "7"
            ),
            BuildChecklistDetail(
                detailId: "10",
                checklistId: StubData.ChecklistId,
                taskName: "ten",
                parentDetailId: "7"
            ),
            BuildChecklistDetail(
                detailId: "11",
                checklistId: StubData.ChecklistId,
                taskName: "eleven",
                parentDetailId: "7"
            ),
            BuildChecklistDetail(
                detailId: "12",
                checklistId: StubData.ChecklistId,
                taskName: "twelve",
                parentDetailId: "10"
            ),
            BuildChecklistDetail(
                detailId: "13",
                checklistId: StubData.ChecklistId,
                taskName: "thirteen",
                parentDetailId: "12"
            ),
            BuildChecklistDetail(
                detailId: "14",
                checklistId: StubData.ChecklistId,
                taskName: "fourteen",
                parentDetailId: "13"
            ),
            BuildChecklistDetail(
                detailId: "15",
                checklistId: StubData.ChecklistId,
                taskName: "fifteen",
                parentDetailId: "14"
            ),
            BuildChecklistDetail(
                detailId: "16",
                checklistId: StubData.ChecklistId,
                taskName: "sixteen",
                parentDetailId: "14"
            ),
            BuildChecklistDetail(
                detailId: "17",
                checklistId: StubData.ChecklistId,
                taskName: "seventeen",
                parentDetailId: "2"
            ),
            BuildChecklistDetail(
                detailId: "18",
                checklistId: StubData.ChecklistId,
                taskName: "eighteen",
                parentDetailId: "2"
            )
        };
    }

    private ChecklistDetail BuildChecklistDetail(
        string? detailId = null, string checklistId = StubData.ChecklistId, string taskName = StubData.ChecklistTaskName,
        string? parentDetailId = null, bool status = true, DateTime? creationDate = null, DateTime? modificationDate = null,
        List<ChecklistDetail>? subItems = null
    )
    {
        return new ChecklistDetail
        {
            Id = string.IsNullOrWhiteSpace(detailId) ? Guid.CreateVersion7().ToString("N") : detailId,
            ChecklistId = checklistId,
            TaskName = taskName,
            ParentDetailId = parentDetailId,
            Status = status,
            CreationDate = creationDate ?? StubData.Today,
            ModificationDate = modificationDate ?? StubData.NextWeek,
            SubItems = subItems
        };
    }
}
