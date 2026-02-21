// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Ports.Output.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace CRM.Infrastructure.AI.SK.Connectors;

/// <summary>
/// Bridges the CRM <see cref="IAIPort"/> embedding capabilities to Semantic Kernel's
/// <see cref="ITextEmbeddingGenerationService"/>. Automatically uses batch embedding
/// when multiple texts are provided for efficiency.
/// </summary>
#pragma warning disable SKEXP0001 // ITextEmbeddingGenerationService is experimental
public class CrmEmbeddingConnector : ITextEmbeddingGenerationService
#pragma warning restore SKEXP0001
{
    #region Fields

    private readonly IAIPort _aiPort;
    private readonly ILogger<CrmEmbeddingConnector> _logger;

    #endregion

    #region Properties

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Attributes { get; } = new Dictionary<string, object?>();

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CrmEmbeddingConnector"/> class.
    /// </summary>
    /// <param name="aiPort">The CRM AI port providing the underlying embedding implementation.</param>
    /// <param name="logger">Logger instance.</param>
    public CrmEmbeddingConnector(IAIPort aiPort, ILogger<CrmEmbeddingConnector> logger)
    {
        _aiPort = aiPort ?? throw new ArgumentNullException(nameof(aiPort));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region ITextEmbeddingGenerationService Implementation

    /// <inheritdoc />
    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IList<string> data,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        if (data == null || data.Count == 0)
        {
            _logger.LogWarning("GenerateEmbeddingsAsync called with empty data");
            return new List<ReadOnlyMemory<float>>();
        }

        _logger.LogDebug("Generating embeddings for {Count} text(s)", data.Count);

        var results = new List<ReadOnlyMemory<float>>();

        try
        {
            if (data.Count > 1)
            {
                // Use batch endpoint for multiple texts — more efficient
                var batchResponse = await _aiPort.GetEmbeddingsAsync(
                    data.ToList(), null, cancellationToken);

                foreach (var embedding in batchResponse.Embeddings)
                {
                    results.Add(new ReadOnlyMemory<float>(embedding));
                }

                _logger.LogDebug(
                    "Batch embedding completed: {Count} embeddings, {Tokens} tokens",
                    batchResponse.Embeddings.Count,
                    batchResponse.TotalTokens);
            }
            else
            {
                // Single text — use the simple endpoint
                var response = await _aiPort.GetEmbeddingAsync(
                    data[0], null, cancellationToken);
                results.Add(new ReadOnlyMemory<float>(response.Embedding));

                _logger.LogDebug(
                    "Single embedding completed: {Dimensions} dimensions",
                    response.Embedding.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedding generation failed for {Count} text(s)", data.Count);
            throw;
        }

        return results;
    }

    #endregion
}
