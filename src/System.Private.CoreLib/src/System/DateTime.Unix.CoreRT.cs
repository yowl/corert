// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
    public readonly partial struct DateTime
    {
        internal const bool s_systemSupportsLeapSeconds = false;

        public static DateTime UtcNow
        {
            get
            {
                return new DateTime(((ulong)(Interop.Sys.GetSystemTimeAsTicks() + DateTime.UnixEpochTicks)) | KindUtc);
            }
        }

        internal static DateTime FromFileTimeLeapSecondsAware(long fileTime) => default;
        internal static long ToFileTimeLeapSecondsAware(long ticks) => default;

        // IsValidTimeWithLeapSeconds is not expected to be called at all for now on non-Windows platforms
        internal static bool IsValidTimeWithLeapSeconds(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind) => false;
    }
}
