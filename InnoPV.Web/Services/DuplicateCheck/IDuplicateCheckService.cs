using InnoPV.Web.Models.DuplicateCheck;

namespace InnoPV.Web.Services.DuplicateCheck;

public interface IDuplicateCheckService
{
    Task<DuplicateCheckViewModel?> GetDuplicateCheckAsync(long caseId);
    Task<DuplicateCandidateViewModel?> GetDuplicateCandidateAsync(long caseId, long candidateCaseId);
}
