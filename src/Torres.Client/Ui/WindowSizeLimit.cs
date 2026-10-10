using System;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;

namespace Torres.Client.Ui
{
    internal static class WindowSizeLimit
    {
        private const string SetWindowMinimumSizeExport = "SDL_SetWindowMinimumSize";

        private static readonly string[] _sdlLibraryNames =
        {
            "libSDL2-2.0.0.dylib",
            "libSDL2-2.0.so.0",
            "SDL2.dll",
            "SDL2",
        };

        private static SetWindowMinimumSizeFunction? _setWindowMinimumSize;
        private static bool _isResolved;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetWindowMinimumSizeFunction(IntPtr windowHandle, int minimumWidth, int minimumHeight);

        internal static void ApplyMinimum(GameWindow gameWindow, int minimumWidth, int minimumHeight)
        {
            ArgumentNullException.ThrowIfNull(gameWindow);

            SetWindowMinimumSizeFunction? setWindowMinimumSize = ResolveSetWindowMinimumSize();
            setWindowMinimumSize?.Invoke(gameWindow.Handle, minimumWidth, minimumHeight);
        }

        private static SetWindowMinimumSizeFunction? ResolveSetWindowMinimumSize()
        {
            if (_isResolved)
            {
                return _setWindowMinimumSize;
            }

            _isResolved = true;
            foreach (string sdlLibraryName in _sdlLibraryNames)
            {
                _setWindowMinimumSize = LoadSetWindowMinimumSize(sdlLibraryName);
                if (_setWindowMinimumSize is not null)
                {
                    break;
                }
            }

            return _setWindowMinimumSize;
        }

        private static SetWindowMinimumSizeFunction? LoadSetWindowMinimumSize(string sdlLibraryName)
        {
            SetWindowMinimumSizeFunction? setWindowMinimumSize = null;
            if (NativeLibrary.TryLoad(sdlLibraryName, typeof(WindowSizeLimit).Assembly, null, out IntPtr libraryHandle)
                && NativeLibrary.TryGetExport(libraryHandle, SetWindowMinimumSizeExport, out IntPtr functionAddress))
            {
                setWindowMinimumSize = Marshal.GetDelegateForFunctionPointer<SetWindowMinimumSizeFunction>(functionAddress);
            }

            return setWindowMinimumSize;
        }
    }
}
