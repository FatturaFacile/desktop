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
            throw new InvalidOperationException(
                "Successful result cannot have an error.");
        }

        if (!isSuccess && error == DomainErrorDefinition.None)
        {
            throw new InvalidOperationException(
                "Failed result must provide an error.");
        }

        IsSuccess = isSuccess;
        DomainError = error;
        Error = error == DomainErrorDefinition.None
            ? null
            : error.Message;

        ErrorCode = error == DomainErrorDefinition.None
            ? null
            : error.Code;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the result is successful.</param>
    /// <param name="error">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <exception cref="InvalidOperationException">Thrown when the result state is inconsistent with the provided error.</exception>
    protected Result(
        bool isSuccess,
        string? error,
        string? errorCode = null)
    {
        if (isSuccess && error is not null)
        {
            throw new InvalidOperationException(
                "Successful result cannot have an error.");
        }

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
        {
            throw new InvalidOperationException(
                "Failed result must provide an error message.");
        }

        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;

        DomainError = isSuccess
            ? DomainErrorDefinition.None
            : new DomainErrorDefinition(
                errorCode ?? "Operation.Failed",
                error ?? string.Empty);
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
    public static Result Success() =>
        new(isSuccess: true, DomainErrorDefinition.None);

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    /// <param name="error">The domain error definition.</param>
    public static Result Failure(
        DomainErrorDefinition error) =>
        new(isSuccess: false, error);

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    public static Result Failure(
        string error,
        string? errorCode = null) =>
        new(isSuccess: false, error, errorCode);

    /// <summary>
    /// Creates a successful typed result.
    /// </summary>
    /// <typeparam name="TValue">The returned value type.</typeparam>
    /// <param name="value">The value of the successful result.</param>
    public static Result<TValue> Success<TValue>(
        TValue value) =>
        new(value, isSuccess: true, DomainErrorDefinition.None);

    /// <summary>
    /// Creates a failed typed result.
    /// </summary>
    /// <typeparam name="TValue">The returned value type.</typeparam>
    /// <param name="error">The domain error definition.</param>
    public static Result<TValue> Failure<TValue>(
        DomainErrorDefinition error) =>
        new(default, isSuccess: false, error);

    /// <summary>
    /// Creates a failed typed result.
    /// </summary>
    /// <typeparam name="TValue">The returned value type.</typeparam>
    /// <param name="error">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    public static Result<TValue> Failure<TValue>(
        string error,
        string? errorCode = null) =>
        new(default, isSuccess: false, error, errorCode);
}

/// <summary>
/// Represents the typed outcome of an operation.
/// </summary>
/// <typeparam name="TValue">
/// The returned value type.
/// </typeparam>
public class Result<TValue> : Result
{
    /// <summary>
    /// Gets the value of the result.
    /// </summary>
    private readonly TValue? field;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value of the result.</param>
    /// <param name="isSuccess">Indicates whether the result is successful.</param>
    /// <param name="error">The domain error definition.</param>
    internal Result(
        TValue? value,
        bool isSuccess,
        DomainErrorDefinition error)
        : base(isSuccess, error)
    {
        field = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value of the result.</param>
    /// <param name="isSuccess">Indicates whether the result is successful.</param>
    /// <param name="error">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    internal Result(
        TValue? value,
        bool isSuccess,
        string? error,
        string? errorCode = null)
        : base(isSuccess, error, errorCode)
    {
        @field = value;
    }

    /// <summary>
    /// Gets the successful result value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when attempting to access the value of a failed result.</exception>
    public TValue Value =>
        IsSuccess
            ? @field!
            : throw new InvalidOperationException("Cannot access the value of a failed result.");

    /// <summary>
    /// Implicit conversion from value to successful result.
    /// </summary>
    /// <param name="value">The value to be converted to a successful result.</param>
    public static implicit operator Result<TValue>(
        TValue value) =>
        Result.Success(value);
}
