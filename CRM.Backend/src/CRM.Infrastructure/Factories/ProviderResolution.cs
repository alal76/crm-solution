// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using System.Reflection;

namespace CRM.Infrastructure.Factories;

/// <summary>
/// Resolves provider implementations by type name without enumerating the port interface.
/// Avoids recursive resolution when ports are also resolved via factories.
/// </summary>
internal static class ProviderResolution
{
    private static readonly ConcurrentDictionary<(Type PortType, string ProviderTypeName), Type?> ProviderTypeCache = new();

    public static TPort? ResolveByTypeName<TPort>(IServiceProvider serviceProvider, string providerTypeName)
        where TPort : class
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
        if (string.IsNullOrWhiteSpace(providerTypeName)) return null;

        var key = (typeof(TPort), providerTypeName);
        var providerType = ProviderTypeCache.GetOrAdd(key, static k => FindProviderType(k.PortType, k.ProviderTypeName));

        if (providerType == null)
        {
            return null;
        }

        return serviceProvider.GetService(providerType) as TPort;
    }

    private static Type? FindProviderType(Type portType, string providerTypeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (type == null)
                {
                    continue;
                }

                if (!portType.IsAssignableFrom(type))
                {
                    continue;
                }

                if (type.Name.Equals(providerTypeName, StringComparison.OrdinalIgnoreCase))
                {
                    return type;
                }
            }
        }

        return null;
    }
}
