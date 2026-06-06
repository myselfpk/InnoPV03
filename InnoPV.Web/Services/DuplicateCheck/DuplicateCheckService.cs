using InnoPV.Domain.Enums;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.DuplicateCheck;
using Microsoft.EntityFrameworkCore;

namespace InnoPV.Web.Services.DuplicateCheck;

public class DuplicateCheckService : IDuplicateCheckService
{
    private const int DuplicateConfidenceThreshold = 55;

    private readonly ApplicationDbContext _context;

    public DuplicateCheckService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DuplicateCheckViewModel?> GetDuplicateCheckAsync(long caseId)
    {
        var currentCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);

        if (currentCase == null)
        {
            return null;
        }

        var candidates = await _context.PvCases
            .Where(x =>
                x.Id != caseId &&
                !x.IsDeleted &&
                x.Status != PvCaseStatus.MarkedAsInvalid)
            .ToListAsync();

        var scoredCandidates = new List<DuplicateCandidateViewModel>();

        foreach (var candidate in candidates)
        {
            var scoredCandidate = ScoreCandidate(currentCase, candidate);

            if (scoredCandidate != null)
            {
                scoredCandidates.Add(scoredCandidate);
            }
        }

        return new DuplicateCheckViewModel
        {
            PvCaseId = currentCase.Id,
            CaseNo = currentCase.CaseNo,
            PatientIdentifier = currentCase.InitialPatientIdentifier,
            ReporterName = currentCase.InitialReporterName,
            ProductName = currentCase.InitialProductName,
            EventTerm = currentCase.InitialEventTerm,
            ReceiptDate = currentCase.ReceiptDate,
            Status = currentCase.Status,
            DuplicateConfidenceThreshold = DuplicateConfidenceThreshold,
            Candidates = scoredCandidates
                .OrderByDescending(x => x.MatchingScore)
                .ThenByDescending(x => x.ReceiptDate)
                .ToList()
        };
    }

    public async Task<DuplicateCandidateViewModel?> GetDuplicateCandidateAsync(long caseId, long candidateCaseId)
    {
        if (caseId == candidateCaseId)
        {
            return null;
        }

        var currentCase = await _context.PvCases
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDeleted);

        var candidate = await _context.PvCases
            .FirstOrDefaultAsync(x =>
                x.Id == candidateCaseId &&
                !x.IsDeleted &&
                x.Status != PvCaseStatus.MarkedAsInvalid);

        if (currentCase == null || candidate == null)
        {
            return null;
        }

        return ScoreCandidate(currentCase, candidate);
    }

    private static DuplicateCandidateViewModel? ScoreCandidate(InnoPV.Domain.Entities.PvCase currentCase, InnoPV.Domain.Entities.PvCase candidate)
    {
        var score = 0;
        var reasons = new List<string>();

        if (IsSameText(currentCase.InitialPatientIdentifier, candidate.InitialPatientIdentifier))
        {
            score += 35;
            reasons.Add("Patient identifier matched");
        }
        else if (IsPartialTextMatch(currentCase.InitialPatientIdentifier, candidate.InitialPatientIdentifier))
        {
            score += 15;
            reasons.Add("Patient identifier partially matched");
        }

        if (IsSameText(currentCase.InitialProductName, candidate.InitialProductName))
        {
            score += 25;
            reasons.Add("Product matched");
        }
        else if (IsPartialTextMatch(currentCase.InitialProductName, candidate.InitialProductName))
        {
            score += 10;
            reasons.Add("Product partially matched");
        }

        if (IsSameText(currentCase.InitialEventTerm, candidate.InitialEventTerm))
        {
            score += 25;
            reasons.Add("Adverse event term matched");
        }
        else if (IsPartialTextMatch(currentCase.InitialEventTerm, candidate.InitialEventTerm))
        {
            score += 10;
            reasons.Add("Adverse event term partially matched");
        }

        if (IsSameText(currentCase.InitialReporterName, candidate.InitialReporterName))
        {
            score += 8;
            reasons.Add("Reporter matched");
        }
        else if (IsPartialTextMatch(currentCase.InitialReporterName, candidate.InitialReporterName))
        {
            score += 4;
            reasons.Add("Reporter partially matched");
        }

        if (currentCase.IsSerious == candidate.IsSerious)
        {
            score += 5;
            reasons.Add("Seriousness matched");
        }

        var dateDifference = Math.Abs((currentCase.ReceiptDate.Date - candidate.ReceiptDate.Date).Days);

        if (dateDifference <= 7)
        {
            score += 10;
            reasons.Add($"Receipt date within {dateDifference} day(s)");
        }
        else if (dateDifference <= 30)
        {
            score += 5;
            reasons.Add($"Receipt date close ({dateDifference} day difference)");
        }

        if (score < DuplicateConfidenceThreshold)
        {
            return null;
        }

        return new DuplicateCandidateViewModel
        {
            Id = candidate.Id,
            CaseNo = candidate.CaseNo,
            PatientIdentifier = candidate.InitialPatientIdentifier,
            ReporterName = candidate.InitialReporterName,
            ProductName = candidate.InitialProductName,
            EventTerm = candidate.InitialEventTerm,
            ReceiptDate = candidate.ReceiptDate,
            Status = candidate.Status,
            MatchingScore = Math.Min(score, 100),
            ConfidenceBand = GetConfidenceBand(score),
            MatchReasons = string.Join("; ", reasons)
        };
    }

    private static bool IsSameText(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        return Normalize(left) == Normalize(right);
    }

    private static bool IsPartialTextMatch(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        var normalizedLeft = Normalize(left);
        var normalizedRight = Normalize(right);

        if (normalizedLeft.Length < 4 || normalizedRight.Length < 4)
        {
            return false;
        }

        return normalizedLeft.Contains(normalizedRight, StringComparison.Ordinal)
            || normalizedRight.Contains(normalizedLeft, StringComparison.Ordinal);
    }

    private static string GetConfidenceBand(int score)
    {
        if (score >= 80)
        {
            return "High";
        }

        if (score >= 65)
        {
            return "Medium";
        }

        return "Low";
    }

    private static string Normalize(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        normalized = normalized.Replace("-", string.Empty, StringComparison.Ordinal);
        normalized = normalized.Replace(" ", string.Empty, StringComparison.Ordinal);
        return normalized;
    }
}
