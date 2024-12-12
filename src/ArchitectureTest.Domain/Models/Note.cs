using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Models.Converters;
using System;

namespace ArchitectureTest.Domain.Models; 

public class NoteDTO : BasicDTO<long>, IEntityConverter<Note> {
	public long UserId { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Content { get; set; }
	public DateTime CreationDate { get; set; }
	public DateTime ModificationDate { get; set; }
	public Note ToEntity() {
		return new Note {
			Title = Title,
			Content = Content,
			UserId = UserId,
			Id = Id
		};
	}
}
