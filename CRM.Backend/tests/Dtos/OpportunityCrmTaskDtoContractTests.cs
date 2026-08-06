// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Dtos
{
    /// <summary>
    /// Reflection-based contract test asserting that <see cref="OpportunityDto"/> and
    /// <see cref="CrmTaskDto"/> are a superset of their entities' business-relevant scalar
    /// properties.
    ///
    /// Why this exists: two real DTO gaps shipped silently — OpportunityDto was missing
    /// ForecastCategory/LossReasonCategory/LossReason/CompetitorWinnerId/WinLossNotes/ClosedDate,
    /// and CrmTaskDto was missing TaskType/StartDate/EstimatedMinutes/AccountId/OpportunityId —
    /// because nothing failed when a field was added to the entity but not the DTO. This test
    /// walks each entity's scalar properties via reflection and fails if a new one appears on
    /// the entity without a matching (or explicitly documented) counterpart on the DTO, so the
    /// next gap breaks the build instead of shipping silently.
    ///
    /// Scope, by design:
    /// - Only scalar, business-relevant properties are compared (see <see cref="IsScalarBusinessProperty"/>).
    ///   Navigation references/collections and <c>[NotMapped]</c> computed aliases are excluded —
    ///   they either don't serialize or are intentionally re-derived on the DTO under a different
    ///   name/shape (see the per-entity exclusion lists below).
    /// - Matching is by property NAME only (not type), because the established mapping convention
    ///   in this codebase intentionally changes types at the DTO boundary: enums become <c>int</c>
    ///   (see OpportunitiesController.MapToDto, TasksController.MapToDto) and DateTime becomes an
    ///   ISO 8601 string. Asserting name presence is what catches "field was never wired up";
    ///   asserting type equality would just fight the documented convention.
    /// - Renames and pre-existing, separately-tracked gaps are listed explicitly per entity so this
    ///   test can't silently start ignoring a *newly* introduced gap — anyone widening an exclusion
    ///   list has to do it consciously, in a diff, with a reason in the comment.
    /// </summary>
    public class OpportunityCrmTaskDtoContractTests
    {
        // ------------------------------------------------------------------
        // Opportunity -> OpportunityDto
        // ------------------------------------------------------------------

        /// <summary>
        /// Entity properties that are intentionally NOT expected to have a same-named DTO
        /// property, because they're re-exposed differently (alias) or are covered by a
        /// dedicated test/endpoint of their own.
        /// </summary>
        private static readonly string[] OpportunityExcluded =
        {
            "UserId", // [NotMapped] alias for SalesOwnerId
            "EstimatedValue", // [NotMapped] alias for Amount
        };

        /// <summary>Entity property name -> DTO property name, for intentional renames.</summary>
        private static readonly (string EntityProperty, string DtoProperty)[] OpportunityRenames =
            Array.Empty<(string, string)>();

        [Fact]
        public void OpportunityDto_ShouldContainEveryBusinessScalarProperty_OfOpportunityEntity()
        {
            AssertDtoIsSupersetOfEntity(
                typeof(Opportunity),
                typeof(OpportunityDto),
                OpportunityExcluded,
                OpportunityRenames);
        }

        [Fact]
        public void OpportunityDto_ShouldHaveTheSixWinLossAndForecastFields_AddedForTheOpportunityDtoGap()
        {
            // Explicit regression guard for the specific gap this change closed, in addition to
            // the generic reflection sweep above.
            var dtoProps = typeof(OpportunityDto).GetProperties().Select(p => p.Name).ToHashSet();

            dtoProps.Should().Contain(new[]
            {
                nameof(Opportunity.ForecastCategory),
                nameof(Opportunity.LossReasonCategory),
                nameof(Opportunity.LossReason),
                nameof(Opportunity.CompetitorWinnerId),
                nameof(Opportunity.WinLossNotes),
                nameof(Opportunity.ClosedDate),
            });
        }

        // ------------------------------------------------------------------
        // CrmTask -> CrmTaskDto
        // ------------------------------------------------------------------

        private static readonly string[] CrmTaskExcluded =
        {
            // Pre-existing gap, out of scope for this change. The 2026-08-06 review in
            // docs/FIELD_GAP_REMEDIATION_PLAN.md (REV-FGAP-002) enumerated the CrmTaskDto gap
            // as exactly TaskType/StartDate/EstimatedMinutes/AccountId/OpportunityId — this
            // field was not part of that finding and fixing it is a separate change. Remove
            // this exclusion once AssignedToUserId is added to CrmTaskDto.
            "AssignedToUserId",
        };

        /// <summary>Entity property name -> DTO property name, for intentional renames.</summary>
        private static readonly (string EntityProperty, string DtoProperty)[] CrmTaskRenames =
        {
            // Documented intentional rename: docs/FIELD_GAP_REMEDIATION_PLAN.md line ~481.
            (nameof(CrmTask.Subject), nameof(CrmTaskDto.Title)),
        };

        [Fact]
        public void CrmTaskDto_ShouldContainEveryBusinessScalarProperty_OfCrmTaskEntity()
        {
            AssertDtoIsSupersetOfEntity(
                typeof(CrmTask),
                typeof(CrmTaskDto),
                CrmTaskExcluded,
                CrmTaskRenames);
        }

        [Fact]
        public void CrmTaskDto_ShouldHaveTheFiveFields_AddedForTheCrmTaskDtoGap()
        {
            var dtoProps = typeof(CrmTaskDto).GetProperties().Select(p => p.Name).ToHashSet();

            dtoProps.Should().Contain(new[]
            {
                nameof(CrmTask.TaskType),
                nameof(CrmTask.StartDate),
                nameof(CrmTask.EstimatedMinutes),
                nameof(CrmTask.AccountId),
                nameof(CrmTask.OpportunityId),
            });
        }

        // ------------------------------------------------------------------
        // Shared reflection machinery
        // ------------------------------------------------------------------

        /// <summary>
        /// Asserts every "business scalar" property on <paramref name="entityType"/> has a
        /// same-named property on <paramref name="dtoType"/>, unless it's in
        /// <paramref name="excluded"/> or is the source side of a mapping in
        /// <paramref name="renames"/> (in which case the target name must be present instead).
        /// </summary>
        private static void AssertDtoIsSupersetOfEntity(
            Type entityType,
            Type dtoType,
            string[] excluded,
            (string EntityProperty, string DtoProperty)[] renames)
        {
            var dtoPropertyNames = dtoType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name)
                .ToHashSet(StringComparer.Ordinal);

            var renameMap = renames.ToDictionary(r => r.EntityProperty, r => r.DtoProperty, StringComparer.Ordinal);
            var excludedSet = excluded.ToHashSet(StringComparer.Ordinal);

            var entityScalarProperties = entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(IsScalarBusinessProperty)
                .Select(p => p.Name)
                .ToList();

            var missing = entityScalarProperties
                .Where(name => !excludedSet.Contains(name))
                .Where(name =>
                {
                    var expectedDtoName = renameMap.TryGetValue(name, out var renamed) ? renamed : name;
                    return !dtoPropertyNames.Contains(expectedDtoName);
                })
                .ToList();

            missing.Should().BeEmpty(
                $"every business-relevant scalar property on {entityType.Name} should have a " +
                $"same-named (or explicitly renamed/excluded) counterpart on {dtoType.Name}. " +
                "If this failed because you added a new entity field, add it to the DTO and the " +
                "controller's MapToDto method — or, if it's genuinely internal/deferred, add it " +
                "to this test's exclusion list with a comment explaining why.");
        }

        /// <summary>
        /// True for properties that represent real, DTO-worthy business data: primitives,
        /// strings, DateTime, decimal, byte[], and enums (nullable or not). False for EF
        /// navigation references/collections and anything marked <c>[NotMapped]</c> (computed
        /// aliases like Opportunity.WeightedAmount, Opportunity.UserId).
        /// </summary>
        private static bool IsScalarBusinessProperty(PropertyInfo property)
        {
            if (property.GetCustomAttribute<NotMappedAttribute>() != null)
            {
                return false;
            }

            // Indexers aren't real data properties.
            if (property.GetIndexParameters().Length > 0)
            {
                return false;
            }

            var type = property.PropertyType;
            var underlying = Nullable.GetUnderlyingType(type) ?? type;

            if (underlying == typeof(string) || underlying == typeof(byte[]))
            {
                return true;
            }

            if (underlying.IsEnum)
            {
                return true;
            }

            if (underlying.IsPrimitive || underlying == typeof(decimal) || underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset) || underlying == typeof(Guid))
            {
                return true;
            }

            // Everything else (navigation references, ICollection<T>/List<T> navigation
            // properties, complex owned types) is not a flat scalar DTO field.
            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                return false;
            }

            return false;
        }
    }
}
