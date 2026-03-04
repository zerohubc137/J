
// ─────────────────────────────────────────────────────────
// Stubs for Microsoft.Xna.Framework (MonoGame)
// ─────────────────────────────────────────────────────────
namespace Microsoft.Xna.Framework
{
    public struct Color
    {
        public byte R, G, B, A;
        public Color(int r, int g, int b) { R=(byte)r; G=(byte)g; B=(byte)b; A=255; }
        public Color(int r, int g, int b, int a) { R=(byte)r; G=(byte)g; B=(byte)b; A=(byte)a; }
        public static Color Black   => new(0,0,0);
        public static Color White   => new(255,255,255);
        public static Color Gray    => new(128,128,128);
        public static Color LightGray => new(200,200,200);
        public static Color Yellow  => new(255,255,0);
        public static Color LightGreen => new(144,238,144);
        public static Color Red     => new(255,0,0);
        public Color operator *(float f) => new((int)(R*f),(int)(G*f),(int)(B*f),(int)(A*f));
    }
    public struct Vector2
    {
        public float X, Y;
        public Vector2(float x, float y) { X=x; Y=y; }
    }
    public struct Rectangle
    {
        public int X, Y, Width, Height;
        public Rectangle(int x, int y, int w, int h) { X=x; Y=y; Width=w; Height=h; }
        public bool Contains(int x, int y) => x>=X && x<=X+Width && y>=Y && y<=Y+Height;
        public bool Contains(Vector2 v) => Contains((int)v.X,(int)v.Y);
    }
    public struct Point { public int X, Y; }
    public class Viewport { public int Width; public int Height; public Rectangle Bounds; }
}

namespace Microsoft.Xna.Framework.Graphics
{
    using Microsoft.Xna.Framework;
    public class Texture2D { }
    public class SpriteFont
    {
        public Vector2 MeasureString(string s) => new(s.Length * 7, 14);
    }
    public class SpriteBatch
    {
        public void Draw(Texture2D t, Rectangle r, Color c) { }
        public void DrawString(SpriteFont f, string s, Vector2 pos, Color c) { }
        public void Begin() { }
        public void End() { }
    }
    public class GraphicsDevice { public Viewport Viewport; }
    public class GraphicsDeviceManager { public GraphicsDevice GraphicsDevice; }
    public enum BlendState { AlphaBlend }
    public enum SamplerState { LinearClamp }
}

namespace Microsoft.Xna.Framework.Input
{
    using Microsoft.Xna.Framework;
    public enum Keys
    {
        None=0, Back=8, Tab=9, Enter=13, Shift=16, Control=17, Alt=18, Escape=27,
        Space=32, Left=37, Up=38, Right=39, Down=40,
        A=65, B=66, C=67, D=68, S=83, V=86, W=87, X=88, Y=89, Z=90,
        F1=112, F12=123,
        OemPeriod=190, OemComma=188
    }
    public enum Buttons { A, B, Back, Start, X, Y, LeftShoulder, RightShoulder, LeftStick, RightStick, BigButton, DPadDown, DPadLeft, DPadRight, DPadUp, LeftThumbstickDown, LeftThumbstickLeft, LeftThumbstickRight, LeftThumbstickUp, RightThumbstickDown, RightThumbstickLeft, RightThumbstickRight, RightThumbstickUp, LeftTrigger, RightTrigger }
    public enum ButtonState { Released, Pressed }
    public struct MouseState
    {
        public int X, Y;
        public ButtonState LeftButton, RightButton;
    }
    public static class Mouse
    {
        public static MouseState GetState() => new();
    }
    public struct KeyboardState
    {
        public bool IsKeyDown(Keys k) => false;
        public bool IsKeyUp(Keys k) => true;
    }
    public static class Keyboard
    {
        public static KeyboardState GetState() => new();
    }
    public struct GamePadState { }
}

namespace Microsoft.Xna.Framework.Content
{
    public class ContentManager { }
}
