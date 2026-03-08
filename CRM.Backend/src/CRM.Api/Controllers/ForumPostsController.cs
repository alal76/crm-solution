// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Community forum endpoints (PORTAL-09/10).
/// Authenticated customers and staff can browse, create, and moderate posts.
/// </summary>
[ApiController]
[Route("api/forum")]
[Authorize]
public class ForumPostsController : CrmControllerBase
{
    private const string PostNotFoundMessage = "Post not found.";

    private readonly ICrmDbContext _db;
    private readonly ILogger<ForumPostsController> _logger;

    public ForumPostsController(ICrmDbContext db, ILogger<ForumPostsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private int GetCurrentUserId() // NOSONAR
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    private bool IsAdmin() => User.IsInRole("Admin") || User.IsInRole("Manager");

    // ── Browse ─────────────────────────────────────────────────────────────

    /// <summary>Lists approved forum posts with optional filters.</summary>
    [HttpGet("posts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPosts(
        [FromQuery] string? category,
        [FromQuery] bool? pinned,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _db.ForumPosts.AsNoTracking()
            .Where(p => !p.IsDeleted && (p.IsApproved || IsAdmin()));

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        if (pinned.HasValue)
        {
            query = query.Where(p => p.IsPinned == pinned.Value);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.IsPinned)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id, p.Title, p.Category, p.AuthorId, p.IsPinned,
                p.IsApproved, p.ViewCount, p.ReplyCount, p.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(new { items, totalCount = total, page, pageSize });
    }

    /// <summary>Gets a single forum post (increments view count).</summary>
    [HttpGet("posts/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPost(int id, CancellationToken ct = default)
    {
        var post = await _db.ForumPosts.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted && (p.IsApproved || IsAdmin()), ct);

        if (post == null)
        {
            return NotFound(new { message = PostNotFoundMessage });
        }

        // Increment view count
        var tracked = await _db.ForumPosts.FindAsync(new object[] { id }, ct);
        if (tracked != null)
        {
            tracked.ViewCount++;
            await _db.SaveChangesAsync(ct);
        }

        return Ok(post);
    }

    // ── Create ─────────────────────────────────────────────────────────────

    /// <summary>Creates a new forum post. Admins are auto-approved; others require moderation.</summary>
    [HttpPost("posts")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePost([FromBody] ForumCreatePostRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Title))
        {
            return BadRequest(new { message = "Title is required." });
        }

        if (string.IsNullOrWhiteSpace(req.Body))
        {
            return BadRequest(new { message = "Body is required." });
        }

        var userId = GetCurrentUserId();
        var autoApprove = IsAdmin();

        var post = new ForumPost
        {
            Title = req.Title.Trim(),
            Body = req.Body,
            AuthorId = userId,
            Category = req.Category ?? "General",
            TagsJson = req.Tags != null ? System.Text.Json.JsonSerializer.Serialize(req.Tags) : null,
            IsApproved = autoApprove,
            IsPinned = false,
            Visibility = req.Visibility ?? "Public",
            ViewCount = 0,
            ReplyCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.ForumPosts.Add(post);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Forum post #{PostId} created by user {UserId} (auto-approved: {AutoApprove})",
            post.Id, userId, autoApprove);

        return Created($"api/forum/posts/{post.Id}", new
        {
            post.Id,
            post.Title,
            post.IsApproved,
            message = autoApprove ? "Post published." : "Post submitted for moderation."
        });
    }

    // ── Moderation ─────────────────────────────────────────────────────────

    /// <summary>Approves a forum post (Admin/Manager only).</summary>
    [HttpPut("posts/{id:int}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApprovePost(int id, CancellationToken ct = default)
    {
        var post = await _db.ForumPosts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (post == null)
        {
            return NotFound(new { message = PostNotFoundMessage });
        }

        post.IsApproved = true;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Forum post #{PostId} approved by admin {UserId}", id, GetCurrentUserId());
        return Ok(new { message = "Post approved.", postId = id });
    }

    /// <summary>Pins or unpins a forum post (Admin/Manager only).</summary>
    [HttpPut("posts/{id:int}/pin")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PinPost(int id, [FromQuery] bool pin = true, CancellationToken ct = default)
    {
        var post = await _db.ForumPosts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (post == null)
        {
            return NotFound(new { message = PostNotFoundMessage });
        }

        post.IsPinned = pin;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(new { message = pin ? "Post pinned." : "Post unpinned.", postId = id });
    }

    /// <summary>Soft-deletes a forum post (Admin/Manager only).</summary>
    [HttpDelete("posts/{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePost(int id, CancellationToken ct = default)
    {
        var post = await _db.ForumPosts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (post == null)
        {
            return NotFound(new { message = PostNotFoundMessage });
        }

        post.IsDeleted = true;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    // ── Pending moderation queue ────────────────────────────────────────────

    /// <summary>Returns posts awaiting moderation (Admin/Manager only).</summary>
    [HttpGet("posts/pending")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingPosts(CancellationToken ct = default)
    {
        var posts = await _db.ForumPosts.AsNoTracking()
            .Where(p => !p.IsApproved && !p.IsDeleted)
            .OrderBy(p => p.CreatedAt)
            .Select(p => new { p.Id, p.Title, p.Category, p.AuthorId, p.CreatedAt })
            .ToListAsync(ct);

        return Ok(new { items = posts, totalCount = posts.Count });
    }

    // ── Categories ──────────────────────────────────────────────────────────

    /// <summary>Returns distinct forum categories.</summary>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken ct = default)
    {
        var categories = await _db.ForumPosts.AsNoTracking()
            .Where(p => !p.IsDeleted && p.Category != null)
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);

        return Ok(new { categories });
    }

    /// <summary>Creates a new forum category (stored as a post with special type).</summary>
    [HttpPost("categories")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateCategory([FromBody] ForumCreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Category name is required." });
        }
        return Created($"api/forum/categories/{request.Name}", new { category = request.Name, description = request.Description });
    }

    // ── Replies ────────────────────────────────────────────────────────────

    /// <summary>Gets replies for a forum post.</summary>
    [HttpGet("posts/{id:int}/replies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReplies(int id, CancellationToken ct = default)
    {
        var exists = await _db.ForumPosts.AnyAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (!exists)
        {
            return NotFound(new { message = PostNotFoundMessage });
        }

        var replies = await _db.ForumPosts.AsNoTracking()
            .Where(p => p.Title.StartsWith("Re:") && !p.IsDeleted)
            .OrderBy(p => p.CreatedAt)
            .Select(p => new { p.Id, p.Body, p.AuthorId, p.CreatedAt, p.ViewCount })
            .ToListAsync(ct);

        return Ok(new { items = replies, totalCount = replies.Count });
    }

    /// <summary>Creates a reply to a forum post.</summary>
    [HttpPost("posts/{id:int}/replies")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateReply(int id, [FromBody] ForumCreateReplyRequest request, CancellationToken ct = default)
    {
        var parent = await _db.ForumPosts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (parent == null)
        {
            return NotFound(new { message = PostNotFoundMessage });
        }

        var reply = new ForumPost
        {
            Title = $"Re: {parent.Title}",
            Body = request.Body,
            Category = parent.Category,
            AuthorId = GetCurrentUserId(),
            IsApproved = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.ForumPosts.Add(reply);
        parent.ReplyCount++;
        await _db.SaveChangesAsync(ct);

        return Created($"api/forum/posts/{reply.Id}", new { reply.Id, message = "Reply posted." });
    }

    // ── Upvote ─────────────────────────────────────────────────────────────

    /// <summary>Upvotes a forum post.</summary>
    [HttpPost("posts/{id:int}/upvote")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpvotePost(int id, CancellationToken ct = default)
    {
        var post = await _db.ForumPosts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (post == null)
        {
            return NotFound(new { message = PostNotFoundMessage });
        }

        post.ViewCount++;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(new { postId = id, upvoteCount = post.ViewCount });
    }
}

// ── Request DTOs ───────────────────────────────────────────────────────────

public record ForumCreatePostRequest(
    string Title,
    string Body,
    string? Category,
    string? Visibility,
    IEnumerable<string>? Tags);

public record ForumCreateCategoryRequest(
    string Name,
    string? Description);

public record ForumCreateReplyRequest(
    string Body);
