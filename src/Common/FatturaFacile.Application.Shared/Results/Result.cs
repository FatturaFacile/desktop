using FatturaFacile.Domain.Shared.Errors;

namespace FatturaFacile.Application.Shared.Results;

/// <summary>
/// Represents the outcome of an application or domain operation.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the descriptive error message if the operation failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets the semantic error code if the operation failed.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Gets the strongly typed domain error definition.
    /// </summary>
    public DomainErrorDefinition DomainError { get; }

    protected Result(bool isSuccess, DomainErrorDefinition error)
    {
        if (isSuccess && error != DomainErrorDefinition.None)
            throw new InvalidOperationException("Successful result cannot have an error.");
        if (!isSuccess && error == DomainErrorDefinition.None)
            throw new InvalidOperationException("Failed result must provide an error.");

        IsSuccess = isSuccess;
        DomainError = error;
        Error = error == DomainErrorDefinition.None ? null : error.Message;
        ErrorCode = error == DomainErrorDefinition.None ? null : error.Code;
    }

    protected Result(bool isSuccess, string? error, string? errorCode = null)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Successful result cannot have an error.");
        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("Failed result must provide an error message.");

        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
        DomainError = isSuccess ? DomainErrorDefinition.None : new DomainErrorDefinition(errorCode ?? "Operation.Failed", error ?? string.Empty);
    }

    public static Result Success() => new(true, DomainErrorDefinition.None);
    public static Result Failure(DomainErrorDefinition error) => new(false, error);
    public static Result Failure(string error, string? errorCode = null) => new(false, error, errorCode);
    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);
    public static Result<TValue> Failure<TValue>(DomainErrorDefinition error) => Result<TValue>.Failure(error);
    public static Result<TValue> Failure<TValue>(string error, string? errorCode = null) => Result<TValue>.Failure(error, errorCode);
}

/// <summary>
/// Represents the typed outcome of an operation producing a return value.
/// </summary>
/// <typeparam name="TValue">The type of the returned value on success.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, DomainErrorDefinition error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    protected internal Result(TValue? value, bool isSuccess, string? error, string? errorCode = null)
        : base(isSuccess, error, errorCode)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the payload value when the result is successful.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failure result.");

    public static Result<TValue> Success(TValue value) => new(value, true, DomainErrorDefinition.None);
    public new static Result<TValue> Failure(DomainErrorDefinition error) => new(default, false, error);
    public new static Result<TValue> Failure(string error, string? errorCode = null) => new(default, false, error, errorCode);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
