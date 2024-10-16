namespace Lanceur.SharedKernel.DI;

/// <summary>
/// Attribute to mark a class for Dependency Injection (DI) as a singleton.
/// When applied, it indicates that this type should be registered as a singleton 
/// in the service collection (i.e., only one instance will be created and shared across the application).
/// </summary>
public class SingletonAttribute : Attribute { }
