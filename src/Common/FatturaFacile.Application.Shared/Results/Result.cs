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
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the result is successful.</param>
    /// <param name="error">The domain error definition.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="error"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result state is inconsistent with the provided error.</exception>
    protected Result(bool isSuccess, DomainErrorDefinition error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (isSuccess && error != DomainErrorDefinition.None)
        {
            throw new InvalidOperationException("Successful result cannot have an error.");
        }

        if (!isSuccess && error == DomainErrorDefinition.None)
        {
            throw new InvalidOperationException("Failed result must provide an error.");
        }

        IsSuccess = isSuccess;
        DomainError = error;
        Error = error == DomainErrorDefinition.None ? null : error.Message;

        ErrorCode = error == DomainErrorDefinition.None ? null : error.Code;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the result is successful.</param>
    /// <param name="error">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <exception cref="InvalidOperationException">Thrown when the result state is inconsistent with the provided error.</exception>
    protected Result(bool isSuccess, string? error, string? errorCode = null)
    {
        if (isSuccess && error is not null)
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

        DomainError = isSuccess
            ? DomainErrorDefinition.None
            : new DomainErrorDefinition(errorCode ?? "Operation.Failed", error ?? string.Empty);
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
    /// Gets the error message.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Gets the domain error.
    /// </summary>
    public DomainErrorDefinition DomainError { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static Result Success() => new(isSuccess: true, DomainErrorDefinition.None);

    /// <summary>
    /// Creates a successful typed result.
    /// </summary>
    /// <typeparam name="TValue">The returned value type.</typeparam>
    /// <param name="value">The value of the successful result.</param>
    /// <returns>A successful typed result.</returns>
    public static Result<TValue> Success<TValue>(TValue value) => new(
        value,
        isSuccess: true,
        DomainErrorDefinition.None);

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    /// <param name="error">The domain error definition.</param>
    /// <returns>A failure result.</returns>
    public static Result Failure(DomainErrorDefinition error) => new(isSuccess: false, error);

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <returns>A failure result.</returns>
    public static Result Failure(string error, string? errorCode = null) =>
        new(isSuccess: false, error, errorCode);

    /// <summary>
    /// Creates a failed typed result.
    /// </summary>
    /// <typeparam name="TValue">The returned value type.</typeparam>
    /// <param name="error">The domain error definition.</param>
    /// <returns>A failed typed result.</returns>
    public static Result<TValue> Failure<TValue>(DomainErrorDefinition error) =>
        new(default, isSuccess: false, error);

    /// <summary>
    /// Creates a failed typed result.
    /// </summary>
    /// <typeparam name="TValue">The returned value type.</typeparam>
    /// <param name="error">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <returns>A failed typed result.</returns>
    public static Result<TValue> Failure<TValue>(string error, string? errorCode = null) =>
        new(default, isSuccess: false, error, errorCode);
}

/// <summary>
/// Represents the typed outcome of an operation.
/// </summary>
/// <typeparam name="TValue">
/// The returned value type.
/// </typeparam>
#pragma warning disable SA1402
public sealed class Result<TValue> : Result
#pragma warning restore SA1402
{
#pragma warning disable IDE0032
    private readonly TValue? _value;
#pragma warning restore IDE0032

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value of the successful result.</param>
    /// <param name="isSuccess">Indicates whether the result is successful.</param>
    /// <param name="error">The error message.</param>
    internal Result(TValue? value, bool isSuccess, DomainErrorDefinition error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// Gets the value of the successful result.
    /// </summary>
    /// <param name="value">The value of the successful result.</param>
    /// <param name="isSuccess">Indicates whether the result is successful.</param>
    /// <param name="error">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    internal Result(TValue? value, bool isSuccess, string? error, string? errorCode = null) : base(
        isSuccess,
        error,
        errorCode)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value of the successful result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when attempting to access the value of a failed result.</exception>
    public TValue Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access the value of a failed result.");

    /// <summary>
    ///  Implicitly converts a value to a successful <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> containing the value.</returns>
    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
