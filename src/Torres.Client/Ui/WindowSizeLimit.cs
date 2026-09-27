using System;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;

namespace Torres.Client.Ui
{
    internal static class WindowSizeLimit
    {
        private const string SdlLibrary = "SDL2";

        internal static void ApplyMinimum(GameWindow window, int width, int height)
        {
            ArgumentNullException.ThrowIfNull(window);

            SetWindowMinimumSize(window.Handle, width, height);
        }

        [DllImport(SdlLibrary, EntryPoint = "SDL_SetWindowMinimumSize", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetWindowMinimumSize(IntPtr window, int minimumWidth, int minimumHeight);
    }
}
