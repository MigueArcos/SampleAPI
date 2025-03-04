using System;
using System.Collections.Generic;
using System.Security.Claims;
using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.Models.Application;
using ArchitectureTest.Domain.Services.Application.EntityCrudService;
using AutoMapper;

namespace ArchitectureTest.TestUtils;

public static class TestDataBuilders
{
    private static readonly Random _random = new();
    private static readonly IMapper _mapper;

    static TestDataBuilders(){
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<ApplicationModelsMappingProfile>());
        mapperConfig.AssertConfigurationIsValid();

        _mapper = mapperConfig.CreateMapper();
    }

    public static JsonWebToken BuildJwt(
        string userId = StubData.UserId, string email = StubData.Email,
        string token = StubData.JwtToken, string refreshToken = StubData.RefreshToken
    ) {
        return new JsonWebToken {
            UserId = userId,
            Email = email,
            ExpiresIn = 3600,
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public static UserTokenIdentity BuildUserTokenIdentity(
        string userId = StubData.UserId, string email = StubData.Email, string name = StubData.UserName
    ) {
        return new UserTokenIdentity {
            UserId = userId,
            Email = email,
            Name = name
        };
    }

    public static UserToken BuildUserToken(string userId = StubData.UserId, string token = StubData.RefreshToken)
    {
        return new UserToken {
            Id = Guid.CreateVersion7().ToString("N"),
            UserId = userId,
            TokenTypeId = $"{(int) Domain.Enums.TokenType.RefreshToken}",
            Token = token,
            ExpiryTime = DateTime.Now.AddYears(1)
        };
    }

    public static User BuildUser(
        string userId = StubData.UserId, string email = StubData.Email,
        string name = StubData.UserName, string password = StubData.HashedPassword
    ) {
        return new User {
            Id = userId,
            Email = email,
            Password = password,
            Name = name,
            CreationDate = DateTime.Now,
            ModificationDate = null
        };
    }

    public static ClaimsPrincipal BuildClaimsPrincipal(
        string userId = StubData.UserId, string email = StubData.Email, string userName = StubData.UserName
    ){
        var userClaims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, userName),
        };
        var identity = new ClaimsIdentity(userClaims, "TestAuthType");

        return new ClaimsPrincipal(identity);
    }

    public static Note BuildNote(
        string noteId = StubData.NoteId, string title = StubData.NoteTitle, string content = StubData.NoteContent,
        string userId = StubData.UserId, DateTime? creationDate = null, DateTime? modificationDate = null
    ) {
        return new Note {
            Id = noteId,
            Title = title,
            Content = content,
            UserId = userId,
            CreationDate = creationDate ?? StubData.Today,
            ModificationDate = modificationDate ?? StubData.NextWeek
        };
    }

    public record ChecklistDetailValueObject(
        string ChecklistId, string ParentDetailId, string TaskName, bool Status, List<ChecklistDetailValueObject>? SubItems
    );

    public record ChecklistValueObject(string UserId, string Title, List<ChecklistDetailValueObject>? Details);

    public record UpdateChecklistDTOValueObject(
        string UserId, string Title, List<ChecklistDetailValueObject>? DetailsToAdd,
        List<ChecklistDetailValueObject>? DetailsToUpdate, List<string>? DetailsToDelete
    );

    public static Checklist BuildChecklist(
        string checklistId = StubData.ChecklistId, string userId = StubData.UserId, string title = StubData.ChecklistTitle,
        DateTime? creationDate = null, DateTime? modificationDate = null, List<ChecklistDetail>? details = null
    ) {
        details ??= BuildRandomDetails(checklistId);

        return new Checklist {
            Id = checklistId,
            UserId = userId,
            Title = title,
            Details = details,
            CreationDate = creationDate ?? StubData.Today,
            ModificationDate = modificationDate ?? StubData.NextWeek
        };
    }

    // I needed to use a converterFunc to change the details Data type, this function is going to be the IMapper mapping
    // function, I could not use IMapper because this project would need an extra dependency for AutoMapper only for this
    public static UpdateChecklistDTO BuildUpdateChecklistModel(
        string checklistId = StubData.ChecklistId, string userId = StubData.UserId, 
        string title = StubData.ChecklistTitle, List<ChecklistDetail>? detailsToAdd = null,
        List<ChecklistDetail>? detailsToUpdate = null, List<string>? detailsToDelete = null
    ) {
        detailsToAdd ??= BuildRandomDetails(checklistId);
        // detailsToUpdate ??= BuildRandomDetails(checklistId);
        // detailsToDelete ??= 

        return new UpdateChecklistDTO {
            Id = checklistId,
            UserId = userId,
            Title = title,
            DetailsToAdd = _mapper.Map<List<ChecklistDetailDTO>>(detailsToAdd),
            DetailsToUpdate = _mapper.Map<List<ChecklistDetailDTO>>(detailsToUpdate),
            DetailsToDelete = detailsToDelete
        };
    }

    public static List<ChecklistDetail>? BuildRandomDetails(string checklistId, int depth = 0, string? parentDetailId = null)
    {
        var details = new List<ChecklistDetail>();
        int detailsNumber = _random.Next(depth == 0 ? 1 : 0, 5 - depth);
        for (int i = 0; i < detailsNumber; i++)
        {
            var detail = BuildChecklistDetail(
                checklistId: checklistId, taskName: StubData.CreateRandomString(), parentDetailId: parentDetailId
            );
            detail.SubItems = BuildRandomDetails(checklistId, depth + 1, detail.Id);
            // detail.SubItems = [];
            details.Add(detail);
        }
        return details;
    }

    public static ChecklistDetail BuildChecklistDetail(
        string? detailId = null, string checklistId = StubData.ChecklistId, string taskName = StubData.ChecklistTaskName,
        string? parentDetailId = null, bool status = true, DateTime? creationDate = null, DateTime? modificationDate = null,
        List<ChecklistDetail>? subItems = null
    ){
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

    public static bool RandomBool() => _random.NextDouble() >= 0.5;

    public static (List<ChecklistDetail> DetailToUpdate, List<string> DetailsToDelete) PickRandomDetails(
        List<ChecklistDetail> flattenedDetails
    ){
        var detailsToUpdate = new List<ChecklistDetail>();
        var detailsToDelete = new List<string>();
    
        flattenedDetails.ForEach(detail => {
            if (RandomBool())
                detailsToUpdate.Add(detail);
            if (RandomBool())
                detailsToDelete.Add(detail.Id);
        });
        
        return (detailsToUpdate, detailsToDelete);
    }
}
