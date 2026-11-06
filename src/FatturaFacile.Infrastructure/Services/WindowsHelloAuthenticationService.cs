// <copyright file="WindowsHelloAuthenticationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using Windows.Security.Credentials.UI;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// Hardware-backed biometric and Windows Hello / PIN authentication service leveraging WinRT UserConsentVerifier.
/// </summary>
/// <remarks>
/// Fail-closed: in Release builds access is denied when Windows Hello is unavailable or fails.
/// The developer fallback (access granted without Windows Hello) exists only in Debug builds.
/// </remarks>
public sealed class WindowsHelloAuthenticationService : IUserAuthenticationService
{
#if DEBUG
    private const bool DeveloperFallbackEnabled = true;
#else
    private const bool DeveloperFallbackEnabled = false;
#endif

    /// <inheritdoc/>
    public async Task<bool> AuthenticateCurrentUserAsync(string promptMessage)
    {
        try
        {
            var availability = await UserConsentVerifier.CheckAvailabilityAsync();
            if (availability != UserConsentVerifierAvailability.Available)
            {
                return AllowDeveloperFallback();
            }

            var verificationResult = await UserConsentVerifier.RequestVerificationAsync(promptMessage);
            return verificationResult == UserConsentVerificationResult.Verified;
        }
        catch (Exception)
        {
            return AllowDeveloperFallback();
        }
    }

    private static bool AllowDeveloperFallback() => DeveloperFallbackEnabled;
}
