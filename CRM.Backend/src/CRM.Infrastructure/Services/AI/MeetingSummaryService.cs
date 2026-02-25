// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.AI;

/// <summary>
/// Generates meeting summaries from interaction records,
/// optionally enriched by an AI provider if available.
/// Implements TODO-AI-08.
/// </summary>
public class MeetingSummaryService : IMeetingSummaryService
{
    private readonly ICrmDbContext _db;
    private readonly IAIPort? _aiPort;
    private readonly ILogger<MeetingSummaryService> _logger;

    public MeetingSummaryService(
        ICrmDbContext db,
        ILogger<MeetingSummaryService> logger,
        IAIPort? aiPort = null)
    {
        _db = db;
        _aiPort = aiPort;
        _logger = logger;
    }

    public async Task<MeetingSummaryDto?> GenerateSummaryAsync(int interactionId, CancellationToken ct = default)
    {
        var interaction = await _db.Interactions
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == interactionId && !i.IsDeleted, ct);

        if (interaction is null) return null;

        // Build content to summarise
        var rawContent = BuildRawContent(interaction);
        var isAiGenerated = false;
        var summary = string.Empty;
        var actionItems = Array.Empty<string>();

        if (_aiPort is not null)
        {
            try
            {
                var prompt = $"Summarise this meeting in 2-3 sentences and list any action items:\n\n{rawContent}";
                summary = await _aiPort.SummarizeAsync(prompt, 300, ct);
                isAiGenerated = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI summarisation failed for interaction {Id}, falling back to heuristic", interactionId);
            }
        }

        if (!isAiGenerated)
        {
            // Heuristic fallback: first 300 chars of notes
            var notes = string.IsNullOrWhiteSpace(interaction.MeetingNotes)
                ? interaction.Description
                : interaction.MeetingNotes;

            summary = string.IsNullOrWhiteSpace(notes)
                ? $"Meeting: {interaction.Subject} on {interaction.InteractionDate:d}"
                : notes.Length > 300 ? notes[..300] + "…" : notes;

            actionItems = ExtractActionItems(interaction);
        }

        // Parse attendees from JSON string or plain text
        var attendees = ParseAttendees(interaction.Attendees);

        return new MeetingSummaryDto
        {
            InteractionId = interactionId,
            Summary = summary,
            ActionItems = actionItems,
            Attendees = attendees,
            IsAiGenerated = isAiGenerated,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static string BuildRawContent(Interaction interaction)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(interaction.Subject)) parts.Add($"Subject: {interaction.Subject}");
        if (!string.IsNullOrWhiteSpace(interaction.MeetingAgenda)) parts.Add($"Agenda: {interaction.MeetingAgenda}");
        if (!string.IsNullOrWhiteSpace(interaction.MeetingNotes)) parts.Add($"Notes: {interaction.MeetingNotes}");
        if (!string.IsNullOrWhiteSpace(interaction.Description)) parts.Add($"Description: {interaction.Description}");
        return string.Join("\n", parts);
    }

    private static string[] ExtractActionItems(Interaction interaction)
    {
        var items = new List<string>();
        var text = (interaction.MeetingNotes ?? string.Empty) + " " + (interaction.Description ?? string.Empty);
        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("- ", StringComparison.Ordinal) ||
                trimmed.StartsWith("* ", StringComparison.Ordinal) ||
                trimmed.StartsWith("□ ", StringComparison.Ordinal) ||
                trimmed.StartsWith("TODO ", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("Action: ", StringComparison.OrdinalIgnoreCase))
            {
                items.Add(trimmed.TrimStart('-', '*', '□', ' ').Trim());
            }
        }
        return items.ToArray();
    }

    private static string[] ParseAttendees(string? attendeesRaw)
    {
        if (string.IsNullOrWhiteSpace(attendeesRaw)) return Array.Empty<string>();

        // Simple split if comma/semicolon separated; JSON arrays will also partially work
        var cleaned = attendeesRaw.Trim('[', ']', '"', '\'');
        return cleaned
            .Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(a => a.Trim(' ', '"', '\''))
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToArray();
    }
}
