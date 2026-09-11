namespace EventFlow.Domain.Common.Errors;

public class Error
{
    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code {get;}
    public string Description {get;}
    public ErrorType Type{get;}
}
