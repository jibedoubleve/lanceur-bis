namespace Lanceur.SharedKernel.IoC;

/// <summary>
///     Marks a class to be registered as a singleton in the dependency injection (DI) container,
///     meaning that a single instance is created and shared across the application.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SingletonAttribute : Attribute;