// <copyright file="Result.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Errors;

namespace FatturaFacile.Application.Shared.Results;

/// <summary>
/// Represents the outcome of an application or domain operation.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class from a domain error definition.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The domain error definition.</param>
    /// <exception cref="InvalidOperationException">Thrown when success/error state invariants are violated.</exception>
    protected Result(bool isSuccess, DomainErrorDefinition error)
    {
        ArgumentNullException.ThrowIfNull(error);

        switch (isSuccess)
        {
            case true when error != DomainErrorDefinition.None:
                throw new InvalidOperationException("Successful result cannot have an error.");
            case false when error == DomainErrorDefinition.None:
                throw new InvalidOperationException("Failed result must provide an error.");
            default:
// Valid state, no action needed
                break;
        }

        IsSuccess = isSuccess;
        DomainError = error;
        Error = error == DomainErrorDefinition.None ? null : error.Message;
        ErrorCode = error == DomainErrorDefinition.None ? null : error.Code;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class with an error message and optional code.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The descriptive error message.</param>
    /// <param name="errorCode">The semantic error code.</param>
    /// <exception cref="InvalidOperationException">Thrown when success/error state invariants are violated.</exception>
    protected Result(bool isSuccess, string? error, string? errorCode = null)
    {
        if (isSuccess && error != null)
        {
            throw new InvalidOperationException("Successful result cannot have an error.");
        }

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
        {
            throw new InvalidOperationException("Failed result must provide an error message.");
        }

        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
        DomainError = isSuccess ? DomainErrorDefinition.None : new DomainErrorDefinition(errorCode ?? "Operation.Failed", error ?? string.Empty);
    }

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

    /// <summary>
    /// Creates a successful <see cref="Result"/>.
    /// </summary>
    /// <returns>A successful <see cref="Result"/> instance.</returns>
    public static Result Success() => new(isSuccess: true, DomainErrorDefinition.None);

    /// <summary>
    /// Creates a failure <see cref="Result"/> with a strongly-typed domain error.
    /// </summary>
    /// <param name="error">The domain error definition.</param>
    /// <returns>A failed <see cref="Result"/> instance.</returns>
    public static Result Failure(DomainErrorDefinition error) => new(isSuccess: false, error);

    /// <summary>
    /// Creates a failure <see cref="Result"/> with an error message and optional error code.
    /// </summary>
    /// <param name="error">The descriptive error message.</param>
    /// <param name="errorCode">The semantic error code.</param>
    /// <returns>A failed <see cref="Result"/> instance.</returns>
    public static Result Failure(string error, string? errorCode = null) => new(isSuccess: false, error, errorCode);

    /// <summary>
    /// Creates a successful typed <see cref="Result{TValue}"/> with the specified payload.
    /// </summary>
    /// <typeparam name="TValue">The payload return type.</typeparam>
    /// <param name="value">The payload value.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> containing the value.</returns>
    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    /// <summary>
    /// Creates a failed typed <see cref="Result{TValue}"/> with a domain error definition.
    /// </summary>
    /// <typeparam name="TValue">The payload return type.</typeparam>
    /// <param name="error">The domain error definition.</param>
    /// <returns>A failed <see cref="Result{TValue}"/> instance.</returns>
    public static Result<TValue> Failure<TValue>(DomainErrorDefinition error) => Result<TValue>.Failure(error);

    /// <summary>
    /// Creates a failed typed <see cref="Result{TValue}"/> with an error message and optional code.
    /// </summary>
    /// <typeparam name="TValue">The payload return type.</typeparam>
    /// <param name="error">The descriptive error message.</param>
    /// <param name="errorCode">The semantic error code.</param>
    /// <returns>A failed <see cref="Result{TValue}"/> instance.</returns>
    public static Result<TValue> Failure<TValue>(string error, string? errorCode = null) => Result<TValue>.Failure(error, errorCode);
}

/// <summary>
/// Represents the typed outcome of an operation producing a return value.
/// </summary>
/// <typeparam name="TValue">The type of the returned value on success.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class with a value and domain error.
    /// </summary>
    /// <param name="value">The returned payload value.</param>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The domain error definition.</param>
    protected internal Result(TValue? value, bool isSuccess, DomainErrorDefinition error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class with a value, message, and error code.
    /// </summary>
    /// <param name="value">The returned payload value.</param>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The descriptive error message.</param>
    /// <param name="errorCode">The semantic error code.</param>
    protected internal Result(TValue? value, bool isSuccess, string? error, string? errorCode = null)
        : base(isSuccess, error, errorCode)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the payload value when the result is successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when attempting to access the value of a failure result.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failure result.");

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="TValue"/> to a successful <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="value">The payload value.</param>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Creates a successful typed <see cref="Result{TValue}"/> containing the specified value.
    /// </summary>
    /// <param name="value">The successful payload value.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> instance.</returns>
    public static Result<TValue> Success(TValue value) => new(value, isSuccess: true, DomainErrorDefinition.None);

    /// <summary>
    /// Creates a failed typed <see cref="Result{TValue}"/> with a domain error.
    /// </summary>
    /// <param name="error">The domain error definition.</param>
    /// <returns>A failed <see cref="Result{TValue}"/> instance.</returns>
    public new static Result<TValue> Failure(DomainErrorDefinition error) => new(default, isSuccess: false, error);

    /// <summary>
    /// Creates a failed typed <see cref="Result{TValue}"/> with an error message and code.
    /// </summary>
    /// <param name="error">The descriptive error message.</param>
    /// <param name="errorCode">The semantic error code.</param>
    /// <returns>A failed <see cref="Result{TValue}"/> instance.</returns>
    public new static Result<TValue> Failure(string error, string? errorCode = null) => new(default, isSuccess: false, error, errorCode);
}
