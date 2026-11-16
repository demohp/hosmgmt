using Microsoft.AspNetCore.Http;
using Moq;

namespace ClinicManagement.UnitTests.Helpers;

/// <summary>Helper utilities for setting up session mocks in tests.</summary>
public static class SessionMockHelper
{
    /// <summary>
    /// Sets up a session mock to return the specified integer value for a given key.
    /// Uses the same byte encoding as ASP.NET Core's ISession.SetInt32 (big-endian).
    /// </summary>
    public static void SetupSessionInt32(Mock<ISession> sessionMock, string key, int? value)
    {
        if (value.HasValue)
        {
            // ASP.NET Core stores int32 as big-endian (MSB first)
            byte[] bytes = new byte[]
            {
                (byte)(value.Value >> 24),
                (byte)(0xFF & (value.Value >> 16)),
                (byte)(0xFF & (value.Value >> 8)),
                (byte)(0xFF & value.Value)
            };
            sessionMock.Setup(s => s.TryGetValue(key, out bytes!)).Returns(true);
        }
        else
        {
            byte[]? nullBytes = null;
            sessionMock.Setup(s => s.TryGetValue(key, out nullBytes!)).Returns(false);
        }
    }

    /// <summary>
    /// Sets up a session mock with UserId and UserType values.
    /// </summary>
    public static void SetupUserSession(Mock<ISession> sessionMock, int? userId, int? userType)
    {
        SetupSessionInt32(sessionMock, "UserId", userId);
        SetupSessionInt32(sessionMock, "UserType", userType);
    }
}
