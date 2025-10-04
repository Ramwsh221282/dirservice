namespace ResultLibrary.AspNetCore;

public static class EnvelopeHttpResultExtensions
{
    public static EnvelopeHttpResult FromResult(this Result result, string methodName)
    {
        EnvelopeTemplate template = EnvelopeTemplate.FromResult(result, methodName);
        return new EnvelopeHttpResult(template);
    }

    public static EnvelopeHttpResult<T> FromResult<T>(this Result<T> result, string methodName)
    {
        EnvelopeTemplate<T> template = EnvelopeTemplate<T>.FromResult(result, methodName);
        return new EnvelopeHttpResult<T>(template);
    }
}
