// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Reflection;
using CRM.Api.Controllers;
using CRM.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

public class ControllerSmokeTests
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        var assembly = typeof(AuthController).Assembly;
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Controller", StringComparison.Ordinal))
            .Where(t => t.Namespace != null && t.Namespace.Contains("CRM.Api.Controllers", StringComparison.Ordinal))
            .OrderBy(t => t.Name)
            .Select(t => new object[] { t });

        return types;
    }

    [Theory]
    [MemberData(nameof(ControllerTypes))]
    public void Controllers_ShouldBeConstructible(Type controllerType)
    {
        var ctor = controllerType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        ctor.Should().NotBeNull($"{controllerType.Name} should have a public constructor");

        var parameters = ctor!.GetParameters()
            .Select(p => CreateParameter(p.ParameterType))
            .ToArray();

        var instance = Activator.CreateInstance(controllerType, parameters);
        instance.Should().NotBeNull($"{controllerType.Name} should be constructible");
    }

    [Theory]
    [MemberData(nameof(ControllerTypes))]
    public void Controllers_ShouldHaveActionMethods(Type controllerType)
    {
        var actionMethods = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => !m.IsSpecialName)
            .Where(m => m.DeclaringType == controllerType)
            .ToList();

        actionMethods.Should().NotBeEmpty($"{controllerType.Name} should define at least one action method");
    }

    private static object CreateParameter(Type parameterType)
    {
        if (parameterType == typeof(IConfiguration))
        {
            return new ConfigurationBuilder().Build();
        }

        if (parameterType == typeof(IMemoryCache))
        {
            return new MemoryCache(new MemoryCacheOptions());
        }

        if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(ILogger<>))
        {
            var loggerType = typeof(ILogger<>).MakeGenericType(parameterType.GenericTypeArguments[0]);
            var mockType = typeof(Mock<>).MakeGenericType(loggerType);
            return ((Mock)Activator.CreateInstance(mockType)!).Object;
        }

        if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(IOptions<>))
        {
            var optionsType = parameterType.GenericTypeArguments[0];
            var optionsInstance = Activator.CreateInstance(optionsType) ?? GetDefault(optionsType);
            var createMethod = typeof(Options).GetMethods()
                .First(m => m.Name == nameof(Options.Create) && m.IsGenericMethod)
                .MakeGenericMethod(optionsType);
            return createMethod.Invoke(null, new[] { optionsInstance })!;
        }

        if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
        {
            return CreateInMemoryOptions(parameterType.GenericTypeArguments[0]);
        }

        if (parameterType == typeof(CrmDbContext))
        {
            var options = (DbContextOptions<CrmDbContext>)CreateInMemoryOptions(typeof(CrmDbContext));
            return new CrmDbContext(options, new ConfigurationBuilder().Build());
        }

        if (parameterType.IsInterface || parameterType.IsAbstract)
        {
            var mockType = typeof(Mock<>).MakeGenericType(parameterType);
            return ((Mock)Activator.CreateInstance(mockType)!).Object;
        }

        var parameterlessCtor = parameterType.GetConstructor(Type.EmptyTypes);
        if (parameterlessCtor != null)
        {
            return Activator.CreateInstance(parameterType)!;
        }

        var ctor = parameterType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (ctor != null)
        {
            var args = ctor.GetParameters()
                .Select(p => CreateParameter(p.ParameterType))
                .ToArray();
            return Activator.CreateInstance(parameterType, args)!;
        }

        var fallbackMockType = typeof(Mock<>).MakeGenericType(parameterType);
        return ((Mock)Activator.CreateInstance(fallbackMockType)!).Object;
    }

    private static object CreateInMemoryOptions(Type dbContextType)
    {
        var builderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(dbContextType);
        var builder = Activator.CreateInstance(builderType)!;
        var useInMemoryMethod = typeof(InMemoryDbContextOptionsExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(InMemoryDbContextOptionsExtensions.UseInMemoryDatabase)
                        && m.GetParameters().Length >= 2
                        && m.GetParameters()[0].ParameterType.IsAssignableFrom(builderType));

        var parameters = useInMemoryMethod.GetParameters().Length == 2
            ? new object?[] { builder, $"ControllerSmokeTests_{Guid.NewGuid():N}" }
            : new object?[] { builder, $"ControllerSmokeTests_{Guid.NewGuid():N}", null };

        useInMemoryMethod.Invoke(null, parameters);

        var optionsProperty = builderType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .First(p => p.Name == nameof(DbContextOptionsBuilder.Options)
                        && p.PropertyType.IsGenericType
                        && p.PropertyType.GetGenericTypeDefinition() == typeof(DbContextOptions<>));

        return optionsProperty.GetValue(builder)!;
    }

    private static object? GetDefault(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
