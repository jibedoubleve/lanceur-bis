namespace Lanceur.SharedKernel.IoC;

/// <summary>
///     Indicates that a class decorated with this attribute must not be registered in the IoC container.
///     This is intended for classes that can only be instantiated at runtime, which makes registering
///     them useless.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ExcludeFromIoCAttribute : Attribute;