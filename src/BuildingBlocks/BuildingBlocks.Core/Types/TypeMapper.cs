using System.Collections.Concurrent;
using System.Reflection;
using Ardalis.GuardClauses;
using BuildingBlocks.Core.Utils;

namespace BuildingBlocks.Core.Types;

public static class TypeMapper
{
    private static readonly ConcurrentDictionary<Type, string> _typeNameMap = new();
    private static readonly ConcurrentDictionary<string, Type> _typeMap = new();

    /// <summary>
    /// Gets the type name from a generic Type class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string GetTypeName<T>() => ToName(typeof(T));

    /// <summary>
    /// Gets the type name from a Type class.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetTypeName(Type type) => ToName(type);

    /// <summary>
    /// Gets the type name from a instance object.
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static string GetTypeNameByObject(object o) => ToName(o.GetType());

    /// <summary>
    /// Gets the type class from a type name.
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static Type GetType(string typeName, Assembly? assembly = null) => ToType(typeName, assembly);

    public static void AddType<T>(string name) => AddType(typeof(T), name);

    private static void AddType(Type type, string name)
    {
        ToName(type);
        ToType(name, null);
    }

    public static bool IsTypeRegistered<T>() => _typeNameMap.ContainsKey(typeof(T));

    private static string ToName(Type type)
    {
        Guard.Against.Null(type, nameof(type));

        return _typeNameMap.GetOrAdd(type, _ =>
        {
            var eventTypeName = type.FullName!.Replace(".", "_", StringComparison.Ordinal);

            _typeMap.GetOrAdd(eventTypeName, type);

            return eventTypeName;
        });
    }

    private static Type ToType(string typeName, Assembly? assembly) => _typeMap.GetOrAdd(typeName, _ =>
    {
        Guard.Against.NullOrEmpty(typeName, nameof(typeName));

        return _typeMap.GetOrAdd(typeName, _ =>
        {
            var type = assembly is { }
                ? ReflectionUtilities.GetFirstMatchingTypeFromAssembly(
                    typeName.Replace("_", ".", StringComparison.Ordinal), assembly)
                : ReflectionUtilities.GetFirstMatchingTypeFromCurrentDomainAssemblies(
                    typeName.Replace("_", ".", StringComparison.Ordinal))!;

            if (type == null)
                throw new System.Exception($"Type map for '{typeName}' wasn't found!");

            return type;
        });
    });
}
