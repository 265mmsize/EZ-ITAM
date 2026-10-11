namespace EzItam.Api.Services;

/// <summary>업무 규칙 위반 시 발생하는 예외. 메시지는 사용자에게 그대로 표시됩니다.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
