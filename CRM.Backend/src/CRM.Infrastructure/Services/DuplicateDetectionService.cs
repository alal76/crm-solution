// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Resolve ambiguity between CRM.Core.Entities.MatchType and System.IO.MatchType
using MatchType = CRM.Core.Entities.MatchType;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of duplicate detection service with configurable matching algorithms
/// </summary>
public class DuplicateDetectionService : IDuplicateDetectionService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<DuplicateDetectionService> _logger;

    public DuplicateDetectionService(ICrmDbContext context, ILogger<DuplicateDetectionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Duplicate Detection

    public async Task<DuplicateCheckResult> CheckForDuplicatesAsync(
        string entityType,
        Dictionary<string, string?> fieldValues,
        int? excludeRecordId = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new DuplicateCheckResult();

        try
        {
            // Parse entity type
            if (!Enum.TryParse<DuplicateEntityType>(entityType, true, out var duplicateEntityType))
            {
                _logger.LogWarning("Invalid entity type: {EntityType}", entityType);
                return result;
            }

            // Get active rules for this entity type
            var rules = await GetActiveRulesAsync(duplicateEntityType);
            if (!rules.Any())
            {
                _logger.LogDebug("No active duplicate rules for entity type: {EntityType}", entityType);
                return result;
            }

            // Use the highest priority rule
            var rule = rules.OrderBy(r => r.Priority).First();
            result.AppliedRule = new DuplicateRuleInfo
            {
                Id = rule.Id,
                Name = rule.Name,
                MatchThreshold = rule.MatchThreshold,
                Action = rule.Action
            };
            result.RecommendedAction = rule.Action.ToString();

            // Get candidate records based on entity type
            var candidates = await GetCandidateRecordsAsync(duplicateEntityType, fieldValues, excludeRecordId, cancellationToken);
            result.RecordsScanned = candidates.Count;

            // Calculate match scores for each candidate
            foreach (var candidate in candidates)
            {
                var matchResult = CalculateMatchScore(fieldValues, candidate, rule);

                if (matchResult.TotalScore >= rule.MatchThreshold)
                {
                    var duplicateMatch = new DuplicateMatch
                    {
                        RecordId = candidate.Id,
                        EntityType = entityType,
                        MatchScore = (int)Math.Round(matchResult.PercentageMatch),
                        RecordSummary = candidate.Summary,
                        FieldComparisons = matchResult.FieldResults.ToDictionary(
                            f => f.FieldName,
                            f => new FieldComparison
                            {
                                FieldName = f.FieldName,
                                DisplayName = GetFieldDisplayName(f.FieldName),
                                NewValue = f.Value1,
                                ExistingValue = f.Value2,
                                IsMatch = f.IsMatch,
                                MatchWeight = f.Weight,
                                MatchType = f.MatchingType.ToString(),
                                SimilarityPercent = f.SimilarityPercent
                            })
                    };

                    result.Duplicates.Add(duplicateMatch);
                }
            }

            // Sort by match score descending
            result.Duplicates = result.Duplicates.OrderByDescending(d => d.MatchScore).ToList();

            stopwatch.Stop();
            result.DetectionTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Duplicate check completed for {EntityType}: {DuplicatesFound} duplicates found in {TimeMs}ms",
                entityType, result.Duplicates.Count, result.DetectionTimeMs);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for duplicates for {EntityType}", entityType);
            throw;
        }
    }

    public async Task<IEnumerable<DuplicateRule>> GetActiveRulesAsync(DuplicateEntityType entityType)
    {
        return await _context.Set<DuplicateRule>()
            .Include(r => r.MatchFields)
            .Where(r => r.IsActive && !r.IsDeleted && r.EntityType == entityType)
            .OrderBy(r => r.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<DuplicateRule>> GetAllRulesAsync()
    {
        return await _context.Set<DuplicateRule>()
            .Include(r => r.MatchFields)
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.EntityType)
            .ThenBy(r => r.Priority)
            .ToListAsync();
    }

    public async Task<DuplicateRule> SaveRuleAsync(DuplicateRule rule)
    {
        if (rule.Id == 0)
        {
            _context.Set<DuplicateRule>().Add(rule);
        }
        else
        {
            _context.Set<DuplicateRule>().Update(rule);
        }

        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task<bool> DeleteRuleAsync(int ruleId)
    {
        var rule = await _context.Set<DuplicateRule>().FindAsync(ruleId);
        if (rule == null)
            return false;

        rule.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<DuplicateCandidate>> ScanForDuplicatesAsync(
        DuplicateEntityType entityType,
        int? ruleId = null,
        CancellationToken cancellationToken = default)
    {
        var candidates = new List<DuplicateCandidate>();

        try
        {
            // Get active rules for this entity type
            var rules = await GetActiveRulesAsync(entityType);
            if (ruleId.HasValue)
            {
                rules = rules.Where(r => r.Id == ruleId.Value);
            }

            var rulesList = rules.ToList();
            if (!rulesList.Any())
            {
                _logger.LogDebug("No active duplicate rules for entity type: {EntityType}", entityType);
                return candidates;
            }

            // Get all records for the entity type
            var allRecords = await GetAllRecordsForEntityTypeAsync(entityType, cancellationToken);

            _logger.LogInformation("Scanning {Count} records for duplicates of type {EntityType}",
                allRecords.Count, entityType);

            // Compare each record with all others
            for (int i = 0; i < allRecords.Count; i++)
            {
                for (int j = i + 1; j < allRecords.Count; j++)
                {
                    var record1 = allRecords[i];
                    var record2 = allRecords[j];

                    foreach (var rule in rulesList)
                    {
                        var matchResult = CalculateMatchScore(record1.FieldValues, record2, rule);

                        if (matchResult.TotalScore >= rule.MatchThreshold)
                        {
                            // Check if this pair already exists as a candidate
                            var existingCandidate = await _context.Set<DuplicateCandidate>()
                                .FirstOrDefaultAsync(c =>
                                    !c.IsDeleted &&
                                    c.EntityType == entityType &&
                                    c.DuplicateRuleId == rule.Id &&
                                    ((c.SourceRecordId == record1.Id && c.TargetRecordId == record2.Id) ||
                                     (c.SourceRecordId == record2.Id && c.TargetRecordId == record1.Id)),
                                    cancellationToken);

                            if (existingCandidate == null)
                            {
                                var candidate = new DuplicateCandidate
                                {
                                    EntityType = entityType,
                                    SourceRecordId = record1.Id,
                                    SourceRecordType = entityType.ToString(),
                                    TargetRecordId = record2.Id,
                                    TargetRecordType = entityType.ToString(),
                                    DuplicateRuleId = rule.Id,
                                    MatchScore = (int)Math.Round(matchResult.PercentageMatch),
                                    MatchingFields = JsonSerializer.Serialize(matchResult.FieldResults
                                        .Where(f => f.IsMatch)
                                        .Select(f => f.FieldName)),
                                    Status = DuplicateCandidateStatus.Pending,
                                    DetectedAt = DateTime.UtcNow,
                                    CreatedAt = DateTime.UtcNow
                                };

                                _context.Set<DuplicateCandidate>().Add(candidate);
                                candidates.Add(candidate);
                            }
                        }
                    }
                }
            }

            if (candidates.Any())
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Found {Count} new duplicate candidates for {EntityType}",
                    candidates.Count, entityType);
            }

            return candidates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning for duplicates for {EntityType}", entityType);
            throw;
        }
    }

    private async Task<List<CandidateRecord>> GetAllRecordsForEntityTypeAsync(
        DuplicateEntityType entityType,
        CancellationToken cancellationToken)
    {
        return entityType switch
        {
            DuplicateEntityType.Lead => await GetAllLeadRecordsAsync(cancellationToken),
            DuplicateEntityType.Contact => await GetAllContactRecordsAsync(cancellationToken),
            DuplicateEntityType.Account => await GetAllAccountRecordsAsync(cancellationToken),
            _ => new List<CandidateRecord>()
        };
    }

    private async Task<List<CandidateRecord>> GetAllLeadRecordsAsync(CancellationToken cancellationToken)
    {
        var leads = await _context.Set<Lead>()
            .Where(l => !l.IsDeleted && !l.IsMergedDuplicate)
            .ToListAsync(cancellationToken);

        return leads.Select(l => new CandidateRecord
        {
            Id = l.Id,
            Summary = new RecordSummary
            {
                Id = l.Id,
                FirstName = l.FirstName,
                LastName = l.LastName,
                Email = l.Email,
                Phone = l.Phone,
                CompanyName = l.CompanyName,
                CreatedAt = l.CreatedAt
            },
            FieldValues = new Dictionary<string, string?>
            {
                ["FirstName"] = l.FirstName,
                ["LastName"] = l.LastName,
                ["Email"] = l.Email,
                ["Phone"] = l.Phone,
                ["CompanyName"] = l.CompanyName
            }
        }).ToList();
    }

    private async Task<List<CandidateRecord>> GetAllContactRecordsAsync(CancellationToken cancellationToken)
    {
        var contacts = await _context.Set<Contact>()
            .Where(c => c.Status != ContactStatus.Archived && !c.IsMergedDuplicate)
            .ToListAsync(cancellationToken);

        return contacts.Select(c => new CandidateRecord
        {
            Id = c.Id,
            Summary = new RecordSummary
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.EmailPrimary,
                Phone = c.PhonePrimary,
                CompanyName = c.Company,
                CreatedAt = c.DateAdded
            },
            FieldValues = new Dictionary<string, string?>
            {
                ["FirstName"] = c.FirstName,
                ["LastName"] = c.LastName,
                ["EmailPrimary"] = c.EmailPrimary,
                ["PhonePrimary"] = c.PhonePrimary,
                ["Company"] = c.Company
            }
        }).ToList();
    }

    private async Task<List<CandidateRecord>> GetAllAccountRecordsAsync(CancellationToken cancellationToken)
    {
        var accounts = await _context.Set<Account>()
            .Where(a => !a.IsDeleted && !a.IsMergedDuplicate)
            .ToListAsync(cancellationToken);

        return accounts.Select(a => new CandidateRecord
        {
            Id = a.Id,
            Summary = new RecordSummary
            {
                Id = a.Id,
                CompanyName = a.Company,
                Email = a.Email,
                Phone = a.Phone,
                CreatedAt = a.CreatedAt
            },
            FieldValues = new Dictionary<string, string?>
            {
                ["Company"] = a.Company,
                ["Email"] = a.Email,
                ["Phone"] = a.Phone,
                ["Website"] = a.Website
            }
        }).ToList();
    }

    public async Task<IEnumerable<DuplicateCandidate>> GetPendingCandidatesAsync(
        DuplicateEntityType? entityType = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.Set<DuplicateCandidate>()
            .Where(c => !c.IsDeleted && c.Status == DuplicateCandidateStatus.Pending);

        if (entityType.HasValue)
        {
            query = query.Where(c => c.EntityType == entityType.Value);
        }

        return await query
            .OrderByDescending(c => c.MatchScore)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<DuplicateCandidate?> UpdateCandidateStatusAsync(
        int candidateId,
        DuplicateCandidateStatus status,
        int userId,
        string? notes = null)
    {
        var candidate = await _context.Set<DuplicateCandidate>().FindAsync(candidateId);
        if (candidate == null)
            return null;

        candidate.Status = status;
        candidate.ReviewedById = userId;
        candidate.ReviewedAt = DateTime.UtcNow;
        if (notes != null)
            candidate.Notes = notes;

        await _context.SaveChangesAsync();
        return candidate;
    }

    #endregion

    #region Private Methods - Candidate Retrieval

    private async Task<List<CandidateRecord>> GetCandidateRecordsAsync(
        DuplicateEntityType entityType,
        Dictionary<string, string?> fieldValues,
        int? excludeRecordId,
        CancellationToken cancellationToken)
    {
        return entityType switch
        {
            DuplicateEntityType.Lead => await GetLeadCandidatesAsync(fieldValues, excludeRecordId, cancellationToken),
            DuplicateEntityType.Contact => await GetContactCandidatesAsync(fieldValues, excludeRecordId, cancellationToken),
            DuplicateEntityType.Account => await GetAccountCandidatesAsync(fieldValues, excludeRecordId, cancellationToken),
            _ => new List<CandidateRecord>()
        };
    }

    private async Task<List<CandidateRecord>> GetLeadCandidatesAsync(
        Dictionary<string, string?> fieldValues,
        int? excludeRecordId,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<Lead>()
            .Where(l => !l.IsDeleted && !l.IsMergedDuplicate);

        if (excludeRecordId.HasValue)
        {
            query = query.Where(l => l.Id != excludeRecordId.Value);
        }

        // Pre-filter by email if provided (for performance)
        if (fieldValues.TryGetValue("Email", out var email) && !string.IsNullOrWhiteSpace(email))
        {
            var emailDomain = email.Split('@').LastOrDefault()?.ToLower();
            if (!string.IsNullOrEmpty(emailDomain))
            {
                query = query.Where(l => l.Email.ToLower().Contains(emailDomain));
            }
        }

        var leads = await query.Take(500).ToListAsync(cancellationToken);

        return leads.Select(l => new CandidateRecord
        {
            Id = l.Id,
            FieldValues = new Dictionary<string, string?>
            {
                { "Email", l.Email },
                { "FirstName", l.FirstName },
                { "LastName", l.LastName },
                { "Phone", l.Phone },
                { "CompanyName", l.CompanyName },
                { "Title", l.Title },
                { "Website", l.Website }
            },
            Summary = new RecordSummary
            {
                Id = l.Id,
                FirstName = l.FirstName,
                LastName = l.LastName,
                Email = l.Email,
                Phone = l.Phone,
                CompanyName = l.CompanyName,
                Title = l.Title,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            }
        }).ToList();
    }

    private async Task<List<CandidateRecord>> GetContactCandidatesAsync(
        Dictionary<string, string?> fieldValues,
        int? excludeRecordId,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<Contact>()
            .Where(c => c.Status != ContactStatus.Archived && !c.IsMergedDuplicate);

        if (excludeRecordId.HasValue)
        {
            query = query.Where(c => c.Id != excludeRecordId.Value);
        }

        // Pre-filter by email if provided
        if (fieldValues.TryGetValue("Email", out var email) && !string.IsNullOrWhiteSpace(email))
        {
            var emailDomain = email.Split('@').LastOrDefault()?.ToLower();
            if (!string.IsNullOrEmpty(emailDomain) && !string.IsNullOrEmpty(email))
            {
                query = query.Where(c =>
                    (c.EmailPrimary != null && c.EmailPrimary.ToLower().Contains(emailDomain)) ||
                    (c.EmailWork != null && c.EmailWork.ToLower().Contains(emailDomain)));
            }
        }

        var contacts = await query.Take(500).ToListAsync(cancellationToken);

        return contacts.Select(c => new CandidateRecord
        {
            Id = c.Id,
            FieldValues = new Dictionary<string, string?>
            {
                { "Email", c.EmailPrimary ?? c.EmailWork },
                { "FirstName", c.FirstName },
                { "LastName", c.LastName },
                { "Phone", c.PhonePrimary ?? c.PhoneMobile },
                { "CompanyName", c.Company },
                { "Title", c.JobTitle }
            },
            Summary = new RecordSummary
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.EmailPrimary ?? c.EmailWork,
                Phone = c.PhonePrimary ?? c.PhoneMobile,
                CompanyName = c.Company,
                Title = c.JobTitle,
                CreatedAt = c.DateAdded,
                UpdatedAt = c.LastModified
            }
        }).ToList();
    }

    private async Task<List<CandidateRecord>> GetAccountCandidatesAsync(
        Dictionary<string, string?> fieldValues,
        int? excludeRecordId,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<Account>()
            .Where(a => !a.IsDeleted && !a.IsMergedDuplicate);

        if (excludeRecordId.HasValue)
        {
            query = query.Where(a => a.Id != excludeRecordId.Value);
        }

        // Pre-filter by company name or email domain if provided
        if (fieldValues.TryGetValue("CompanyName", out var companyName) && !string.IsNullOrWhiteSpace(companyName))
        {
            var searchTerm = companyName.ToLower();
            query = query.Where(a => a.Company.ToLower().Contains(searchTerm) ||
                                    (a.LegalName != null && a.LegalName.ToLower().Contains(searchTerm)));
        }

        var accounts = await query.Take(500).ToListAsync(cancellationToken);

        return accounts.Select(a => new CandidateRecord
        {
            Id = a.Id,
            FieldValues = new Dictionary<string, string?>
            {
                { "Email", a.Email },
                { "FirstName", a.FirstName },
                { "LastName", a.LastName },
                { "Phone", a.Phone },
                { "CompanyName", a.Company },
                { "Website", a.Website },
                { "Name", a.Category == AccountCategory.Organization ? a.Company : $"{a.FirstName} {a.LastName}" }
            },
            Summary = new RecordSummary
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Name = a.Category == AccountCategory.Organization ? a.Company : $"{a.FirstName} {a.LastName}",
                Email = a.Email,
                Phone = a.Phone,
                CompanyName = a.Company,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }
        }).ToList();
    }

    #endregion

    #region Private Methods - Matching Algorithms

    private DuplicateMatchScore CalculateMatchScore(
        Dictionary<string, string?> fieldValues,
        CandidateRecord candidate,
        DuplicateRule rule)
    {
        var result = new DuplicateMatchScore();
        var matchFields = rule.MatchFields.OrderBy(f => f.Order).ToList();

        // If no match fields configured, use default fields
        if (!matchFields.Any())
        {
            matchFields = GetDefaultMatchFields();
        }

        foreach (var matchField in matchFields)
        {
            var newValue = fieldValues.GetValueOrDefault(matchField.FieldName);
            var existingValue = candidate.FieldValues.GetValueOrDefault(matchField.FieldName);

            var fieldResult = new FieldMatchResult
            {
                FieldName = matchField.FieldName,
                Value1 = newValue,
                Value2 = existingValue,
                Weight = matchField.Weight,
                MatchingType = matchField.MatchType
            };

            // Skip if both values are null/empty and configured to ignore nulls
            if (matchField.IgnoreNullValues &&
                string.IsNullOrWhiteSpace(newValue) &&
                string.IsNullOrWhiteSpace(existingValue))
            {
                continue;
            }

            // Calculate match based on match type
            var (isMatch, similarity) = EvaluateMatch(newValue, existingValue, matchField);
            fieldResult.IsMatch = isMatch;
            fieldResult.SimilarityPercent = similarity;
            fieldResult.Score = isMatch ? matchField.Weight : 0;

            result.FieldResults.Add(fieldResult);
            result.MaxPossibleScore += matchField.Weight;
            result.TotalScore += fieldResult.Score;
        }

        return result;
    }

    private (bool IsMatch, int? Similarity) EvaluateMatch(
        string? value1,
        string? value2,
        DuplicateMatchField matchField)
    {
        if (string.IsNullOrWhiteSpace(value1) || string.IsNullOrWhiteSpace(value2))
        {
            return (false, null);
        }

        // Apply transforms
        var v1 = ApplyTransform(value1, matchField.Transform);
        var v2 = ApplyTransform(value2, matchField.Transform);

        return matchField.MatchType switch
        {
            MatchType.Exact => (IsExactMatch(v1, v2), IsExactMatch(v1, v2) ? 100 : 0),
            MatchType.Fuzzy => EvaluateFuzzyMatch(v1, v2, matchField.FuzzyTolerance ?? 20),
            MatchType.Phonetic => (IsPhoneticMatch(v1, v2), IsPhoneticMatch(v1, v2) ? 100 : 0),
            MatchType.Contains => (ContainsMatch(v1, v2), ContainsMatch(v1, v2) ? 100 : 0),
            MatchType.StartsWith => (StartsWithMatch(v1, v2), StartsWithMatch(v1, v2) ? 100 : 0),
            MatchType.Normalized => (IsNormalizedMatch(v1, v2), IsNormalizedMatch(v1, v2) ? 100 : 0),
            MatchType.EmailDomain => (IsEmailDomainMatch(v1, v2), IsEmailDomainMatch(v1, v2) ? 100 : 0),
            _ => (IsExactMatch(v1, v2), IsExactMatch(v1, v2) ? 100 : 0)
        };
    }

    private string ApplyTransform(string value, string? transform)
    {
        if (string.IsNullOrEmpty(transform))
            return value;

        var result = value;
        foreach (var t in transform.Split(','))
        {
            result = t.Trim().ToLower() switch
            {
                "lowercase" => result.ToLowerInvariant(),
                "uppercase" => result.ToUpperInvariant(),
                "trim" => result.Trim(),
                "removewhitespace" => result.Replace(" ", ""),
                "alphanumericonly" => new string(result.Where(char.IsLetterOrDigit).ToArray()),
                "digitsonly" => new string(result.Where(char.IsDigit).ToArray()),
                _ => result
            };
        }
        return result;
    }

    private bool IsExactMatch(string value1, string value2)
        => string.Equals(value1?.Trim(), value2?.Trim(), StringComparison.OrdinalIgnoreCase);

    private (bool IsMatch, int Similarity) EvaluateFuzzyMatch(string value1, string value2, int tolerance)
    {
        var distance = CalculateLevenshteinDistance(value1.ToLower(), value2.ToLower());
        var maxLength = Math.Max(value1.Length, value2.Length);
        var similarity = maxLength > 0 ? (int)((maxLength - distance) * 100.0 / maxLength) : 0;
        var threshold = 100 - tolerance;

        return (similarity >= threshold, similarity);
    }

    private int CalculateLevenshteinDistance(string s1, string s2)
    {
        var m = s1.Length;
        var n = s2.Length;
        var d = new int[m + 1, n + 1];

        for (var i = 0; i <= m; i++)
            d[i, 0] = i;
        for (var j = 0; j <= n; j++)
            d[0, j] = j;

        for (var j = 1; j <= n; j++)
        {
            for (var i = 1; i <= m; i++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[m, n];
    }

    private bool IsPhoneticMatch(string value1, string value2)
    {
        var soundex1 = GetSoundex(value1);
        var soundex2 = GetSoundex(value2);
        return soundex1 == soundex2;
    }

    private string GetSoundex(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "0000";

        var soundex = new StringBuilder();
        soundex.Append(char.ToUpper(input[0]));

        var prevCode = GetSoundexCode(input[0]);
        for (var i = 1; i < input.Length && soundex.Length < 4; i++)
        {
            var code = GetSoundexCode(input[i]);
            if (code != '0' && code != prevCode)
            {
                soundex.Append(code);
            }
            prevCode = code;
        }

        while (soundex.Length < 4)
            soundex.Append('0');
        return soundex.ToString();
    }

    private char GetSoundexCode(char c)
    {
        c = char.ToUpper(c);
        return c switch
        {
            'B' or 'F' or 'P' or 'V' => '1',
            'C' or 'G' or 'J' or 'K' or 'Q' or 'S' or 'X' or 'Z' => '2',
            'D' or 'T' => '3',
            'L' => '4',
            'M' or 'N' => '5',
            'R' => '6',
            _ => '0'
        };
    }

    private bool ContainsMatch(string value1, string value2)
        => value1.Contains(value2, StringComparison.OrdinalIgnoreCase) ||
           value2.Contains(value1, StringComparison.OrdinalIgnoreCase);

    private bool StartsWithMatch(string value1, string value2)
        => value1.StartsWith(value2, StringComparison.OrdinalIgnoreCase) ||
           value2.StartsWith(value1, StringComparison.OrdinalIgnoreCase);

    private bool IsNormalizedMatch(string value1, string value2)
    {
        var normalized1 = NormalizeString(value1);
        var normalized2 = NormalizeString(value2);
        return string.Equals(normalized1, normalized2, StringComparison.OrdinalIgnoreCase);
    }

    private string NormalizeString(string input)
    {
        // Remove accents, trim, lowercase
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString().ToLowerInvariant().Trim();
    }

    private bool IsEmailDomainMatch(string email1, string email2)
    {
        var domain1 = email1.Split('@').LastOrDefault()?.ToLower();
        var domain2 = email2.Split('@').LastOrDefault()?.ToLower();
        return !string.IsNullOrEmpty(domain1) && domain1 == domain2;
    }

    private List<DuplicateMatchField> GetDefaultMatchFields()
    {
        return new List<DuplicateMatchField>
        {
            new() { FieldName = "Email", MatchType = MatchType.Exact, Weight = 100, Order = 1 },
            new() { FieldName = "FirstName", MatchType = MatchType.Fuzzy, Weight = 30, FuzzyTolerance = 20, Order = 2 },
            new() { FieldName = "LastName", MatchType = MatchType.Fuzzy, Weight = 30, FuzzyTolerance = 20, Order = 3 },
            new() { FieldName = "Phone", MatchType = MatchType.Normalized, Weight = 50, Transform = "digitsonly", Order = 4 },
            new() { FieldName = "CompanyName", MatchType = MatchType.Fuzzy, Weight = 40, FuzzyTolerance = 25, Order = 5 }
        };
    }

    private string GetFieldDisplayName(string fieldName)
    {
        return fieldName switch
        {
            "FirstName" => "First Name",
            "LastName" => "Last Name",
            "CompanyName" => "Company",
            "EmailPrimary" => "Email",
            "PhonePrimary" => "Phone",
            _ => fieldName
        };
    }

    #endregion

    #region Helper Classes

    private class CandidateRecord
    {
        public int Id { get; set; }
        public Dictionary<string, string?> FieldValues { get; set; } = new();
        public RecordSummary? Summary { get; set; }
    }

    #endregion
}
