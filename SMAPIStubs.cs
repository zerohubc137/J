
// ─────────────────────────────────────────────────────────
// Stubs for StardewModdingAPI
// ─────────────────────────────────────────────────────────
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewModdingAPI
{
    public enum LogLevel { Trace, Debug, Info, Alert, Warn, Error }

    public interface IMonitor
    {
        void Log(string message, LogLevel level = LogLevel.Trace);
    }

    public interface IManifest
    {
        string Name { get; }
        string Description { get; }
        string Author { get; }
        ISemanticVersion Version { get; }
        string UniqueID { get; }
        string EntryDll { get; }
        string MinimumApiVersion { get; }
    }

    public interface ISemanticVersion { string ToString(); }

    public interface IModHelper
    {
        IModEvents Events { get; }
        IModRegistry ModRegistry { get; }
        T ReadConfig<T>() where T : class, new();
        void WriteConfig<T>(T config) where T : class, new();
        IInputHelper Input { get; }
    }

    public interface IInputHelper { }
    public interface IModRegistry
    {
        T GetApi<T>(string uniqueID) where T : class;
    }

    public interface IModEvents
    {
        IGameLoopEvents GameLoop { get; }
        IDisplayEvents Display { get; }
        IInputEvents Input { get; }
        IPlayerEvents Player { get; }
        IWorldEvents World { get; }
    }

    public interface IGameLoopEvents
    {
        event EventHandler<GameLaunchedEventArgs> GameLaunched;
        event EventHandler<SaveLoadedEventArgs> SaveLoaded;
        event EventHandler<ReturnedToTitleEventArgs> ReturnedToTitle;
        event EventHandler<DayEndingEventArgs> DayEnding;
        event EventHandler<DayStartedEventArgs> DayStarted;
        event EventHandler<UpdateTickedEventArgs> UpdateTicked;
        event EventHandler<TimeChangedEventArgs> TimeChanged;
    }

    public interface IDisplayEvents
    {
        event EventHandler<RenderedHudEventArgs> RenderedHud;
        event EventHandler<RenderedActiveMenuEventArgs> RenderedActiveMenu;
    }

    public interface IInputEvents
    {
        event EventHandler<ButtonPressedEventArgs> ButtonPressed;
        event EventHandler<CharacterInputEventArgs> CharacterInput;
    }

    public interface IPlayerEvents
    {
        event EventHandler<WarpedEventArgs> Warped;
    }

    public interface IWorldEvents { }

    // ── Event args ────────────────────────────────────────
    public class GameLaunchedEventArgs : EventArgs { }
    public class SaveLoadedEventArgs : EventArgs { }
    public class ReturnedToTitleEventArgs : EventArgs { }
    public class DayEndingEventArgs : EventArgs { }
    public class DayStartedEventArgs : EventArgs { }
    public class TimeChangedEventArgs : EventArgs { public int NewTime; public int OldTime; }
    public class UpdateTickedEventArgs : EventArgs
    {
        public ulong Ticks;
        public bool IsMultipleOf(uint i) => Ticks % i == 0;
    }
    public class RenderedHudEventArgs : EventArgs { public SpriteBatch SpriteBatch; }
    public class RenderedActiveMenuEventArgs : EventArgs { public SpriteBatch SpriteBatch; }
    public class ButtonPressedEventArgs : EventArgs { public SButton Button; public ICursorPosition Cursor; }
    public class CharacterInputEventArgs : EventArgs { public char Character; }
    public class WarpedEventArgs : EventArgs
    {
        public bool IsLocalPlayer;
        public StardewValley.GameLocation OldLocation;
        public StardewValley.GameLocation NewLocation;
    }

    public enum SButton
    {
        None, MouseLeft, MouseRight, MouseMiddle,
        ControllerA, ControllerB, ControllerX, ControllerY,
        Escape, Enter, Space, Back, Tab,
        LeftAlt, RightAlt, LeftControl, RightControl, LeftShift, RightShift,
        F1, F2, F3, F4, F12
    }

    public interface ICursorPosition
    {
        Vector2 AbsolutePixels { get; }
        Vector2 ScreenPixels { get; }
        Vector2 Tile { get; }
        Vector2 GrabTile { get; }
    }

    // ── Mod base class ────────────────────────────────────
    public abstract class Mod : IDisposable
    {
        public IMonitor Monitor { get; protected set; }
        public IModHelper Helper { get; protected set; }
        public IManifest ModManifest { get; protected set; }

        public abstract void Entry(IModHelper helper);

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing) { }
    }

    // ── Context ───────────────────────────────────────────
    public static class Context
    {
        public static bool IsWorldReady { get; set; }
        public static bool IsMainPlayer { get; set; }
        public static bool IsMultiplayer { get; set; }
    }
}

// ─────────────────────────────────────────────────────────
// StardewModdingAPI.Events namespace (alias for event args)
// ─────────────────────────────────────────────────────────
namespace StardewModdingAPI.Events
{
    // Re-export all event args into this namespace so
    // `using StardewModdingAPI.Events;` works
    public class GameLaunchedEventArgs   : StardewModdingAPI.GameLaunchedEventArgs { }
    public class SaveLoadedEventArgs     : StardewModdingAPI.SaveLoadedEventArgs { }
    public class ReturnedToTitleEventArgs: StardewModdingAPI.ReturnedToTitleEventArgs { }
    public class DayEndingEventArgs      : StardewModdingAPI.DayEndingEventArgs { }
    public class DayStartedEventArgs     : StardewModdingAPI.DayStartedEventArgs { }
    public class UpdateTickedEventArgs   : StardewModdingAPI.UpdateTickedEventArgs { }
    public class RenderedHudEventArgs    : StardewModdingAPI.RenderedHudEventArgs { }
    public class RenderedActiveMenuEventArgs : StardewModdingAPI.RenderedActiveMenuEventArgs { }
    public class ButtonPressedEventArgs  : StardewModdingAPI.ButtonPressedEventArgs { }
    public class CharacterInputEventArgs : StardewModdingAPI.CharacterInputEventArgs { }
    public class WarpedEventArgs         : StardewModdingAPI.WarpedEventArgs { }
    public class TimeChangedEventArgs    : StardewModdingAPI.TimeChangedEventArgs { }
}

namespace Microsoft.Xna.Framework
{
    public class GameTime
    {
        public System.TimeSpan TotalGameTime;
        public System.TimeSpan ElapsedGameTime;
    }
}
