using System.Net;
using ResultLibrary;
using ResultLibrary.AspNetCore;

namespace DirectoryService.Tests.ErrorMapping;

public sealed class ErrorTypeStatusCodeTests
{
    [Fact]
    public void NotFoundError_Has_NotFound_Type()
    {
        Error error = Error.NotFoundError("Объект не найден.");

        Assert.Equal(new NotFoundErrorType(), error.Type);
    }

    [Fact]
    public void NotFoundError_Maps_To_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, StatusOf(Error.NotFoundError("Объект не найден.")));
    }

    [Fact]
    public void EntityDeletedError_Maps_To_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, StatusOf(Error.EntityDeletedError()));
    }

    [Fact]
    public void ValidationError_Maps_To_400()
    {
        Assert.Equal(HttpStatusCode.BadRequest, StatusOf(Error.ValidationError("Неверные данные.")));
    }

    [Fact]
    public void ConflictError_Maps_To_409()
    {
        Assert.Equal(HttpStatusCode.Conflict, StatusOf(Error.ConflictError("Конфликт.")));
    }

    [Fact]
    public void ExceptionalError_Maps_To_500()
    {
        Assert.Equal(HttpStatusCode.InternalServerError, StatusOf(Error.ExceptionalError("Сбой.")));
    }

    [Fact]
    public void UnauthorizedError_Maps_To_401()
    {
        Error error = new("Нет доступа.", new UnauthorizedErrorType());

        Assert.Equal(HttpStatusCode.Unauthorized, StatusOf(error));
    }

    [Fact]
    public void NotFoundError_Is_Distinguishable_From_ValidationError()
    {
        Error notFound = Error.NotFoundError("Объект не найден.");
        Error validation = Error.ValidationError("Неверные данные.");

        Assert.NotEqual(notFound.Type, validation.Type);
    }

    private static HttpStatusCode StatusOf(Error error)
    {
        Result result = Result.Fail(error);
        EnvelopeTemplate template = EnvelopeTemplate.FromResult(result, "test");
        return (HttpStatusCode)template.OperationStatus;
    }
}
