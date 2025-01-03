using System.Threading.Tasks;
using ArchitectureTest.Domain.Entities;

namespace ArchitectureTest.Domain.Services;

public interface IChecklistRepository : IRepository<Checklist> {
    Task<int> DeleteDetails(string checklistId, bool autoSave = true);
}
