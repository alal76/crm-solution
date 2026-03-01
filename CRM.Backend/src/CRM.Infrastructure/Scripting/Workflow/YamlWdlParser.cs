// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections.Generic;
using System.IO;
using CRM.Core.Scripting.Workflow;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CRM.Infrastructure.Scripting.Workflow;

/// <summary>Parses YAML WDL files into <see cref="WorkflowDefinition"/> objects.</summary>
public class YamlWdlParser
{
    private readonly IDeserializer _deserializer;

    /// <summary>Initializes a new instance of <see cref="YamlWdlParser"/>.</summary>
    public YamlWdlParser()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>Parses YAML content into a <see cref="WorkflowDefinition"/>.</summary>
    public WorkflowDefinition Parse(string yamlContent)
    {
        if (string.IsNullOrWhiteSpace(yamlContent))
        {
            throw new ArgumentException("YAML content cannot be empty", nameof(yamlContent));
        }

        return _deserializer.Deserialize<WorkflowDefinition>(yamlContent);
    }

    /// <summary>Reads a YAML file from disk and parses it.</summary>
    public WorkflowDefinition ParseFile(string filePath)
    {
        var content = File.ReadAllText(filePath);
        return Parse(content);
    }

    /// <summary>Validates a parsed <see cref="WorkflowDefinition"/> for structural correctness.</summary>
    public WdlValidationResult Validate(WorkflowDefinition definition)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(definition.Id))
        {
            errors.Add("Workflow 'id' is required");
        }

        if (string.IsNullOrEmpty(definition.Name))
        {
            errors.Add("Workflow 'name' is required");
        }

        if (definition.Steps.Count == 0)
        {
            errors.Add("Workflow must have at least one step");
        }

        var stepNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var step in definition.Steps)
        {
            if (string.IsNullOrEmpty(step.Name))
            {
                errors.Add("Each step must have a 'name'");
                continue;
            }

            if (!stepNames.Add(step.Name))
            {
                errors.Add($"Duplicate step name: '{step.Name}'");
            }

            if (step.Type == WorkflowStepType.Script && string.IsNullOrEmpty(step.Script))
            {
                errors.Add($"Step '{step.Name}': script steps require 'script' field");
            }

            if (step.Type == WorkflowStepType.Tool && string.IsNullOrEmpty(step.Tool))
            {
                errors.Add($"Step '{step.Name}': tool steps require 'tool' field");
            }
        }

        return new WdlValidationResult(errors.Count == 0, errors);
    }
}

/// <summary>Result of validating a <see cref="WorkflowDefinition"/>.</summary>
public record WdlValidationResult(bool IsValid, IReadOnlyList<string> Errors);
