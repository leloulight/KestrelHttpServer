// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.AspNet.Server.Kestrel.Networking
{
    public static class PlatformApis
    {
        static PlatformApis()
        {
#if DOTNET5_4 || DNXCORE50
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            IsDarwin = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#else
            var p = (int)Environment.OSVersion.Platform;
            IsWindows = (p != 4) && (p != 6) && (p != 128);

            if (!IsWindows)
            {
                // When running on Mono in Darwin OSVersion doesn't return Darwin. It returns Unix instead.
                // Fallback to use uname.
                IsDarwin = string.Equals(GetUname(), "Darwin", StringComparison.Ordinal);
            }
#endif
        }

        public static bool IsWindows { get; }

        public static bool IsDarwin { get; }

        [DllImport("libc")]
        static extern int uname(IntPtr buf);

        static unsafe string GetUname()
        {
            var buffer = new byte[8192];
            try
            {
                fixed (byte* buf = buffer)
                {
                    if (uname((IntPtr)buf) == 0)
                    {
                        return Marshal.PtrToStringAnsi((IntPtr)buf);
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
