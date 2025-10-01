namespace DirectoryService.Core.Common.Exceptions;

/// <summary>
/// Исключение, которое можно выбросить в конструкторе, если он вызывается.
/// Предполагается, в использовании стандартных конструкторов для структур.
/// </summary>
public sealed class ConstructorShallNotBeCalledException : Exception
{
    public ConstructorShallNotBeCalledException(string typeName) :
    base($"Данный конструктор для типа: {typeName} не должен вызываться.")
    { }

    public ConstructorShallNotBeCalledException(string typeName, Exception inner) :
    base($"Данный конструктор для типа: {typeName} не должен вызываться.", inner)
    { }

    public ConstructorShallNotBeCalledException(Type type, Exception inner) :
    base($"Данный конструктор для типа: {type.Name} не должен вызываться.", inner)
    { }
}
