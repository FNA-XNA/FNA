using System;
using Foundation;
using UIKit;
using System.Runtime.InteropServices;
using SDL2;

namespace iOSGame1
{
    class Program
    {
        private static Game1 game;

        internal static void RunGame()
        {
            game = new Game1();
            game.Run();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            // Enable high DPI "Retina" support. Trust us, you'll want this.
            Environment.SetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI", "1");

            SDL.SDL_SetHint("FNA3D_FORCE_DRIVER", "Metal");

            // Keep mouse and touch input separate.
            SDL.SDL_SetHint(SDL.SDL_HINT_MOUSE_TOUCH_EVENTS, "0");
            SDL.SDL_SetHint(SDL.SDL_HINT_TOUCH_MOUSE_EVENTS, "0");

            realArgs = args;
            SDL.SDL_UIKitRunApp(0, IntPtr.Zero, FakeMain);
        }

        static string[] realArgs;

        [ObjCRuntime.MonoPInvokeCallback(typeof(SDL2.SDL.SDL_main_func))]
        static int FakeMain(int argc, IntPtr argv)
        {
            RealMain(realArgs);
            return 0;
        }
        static void RealMain(string[] args)
        {
            RunGame();
        }
    }
}
