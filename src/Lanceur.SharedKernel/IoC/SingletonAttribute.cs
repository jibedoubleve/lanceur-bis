namespace Lanceur.SharedKernel.IoC;

/// <summary>
///     Attribute to mark a sealed class for Dependency Injection (DI) as a singleton.
///     When applied, it indicates that this type should be registered as a singleton
///     in the service collection (i.e., only one instance will be created and shared across the application).
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SingletonAttribute : Attribute { }