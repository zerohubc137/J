
// ─────────────────────────────────────────────────────────
// Stubs for StardewValley
// ─────────────────────────────────────────────────────────
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
    public class Farmer
    {
        public string Name { get; set; } = "Player";
        public GameLocation currentLocation;
    }

    public class GameLocation
    {
        public string Name { get; set; } = "";
    }

    public static class Game1
    {
        public static Farmer player = new();
        public static string currentSeason = "spring";
        public static int dayOfMonth = 1;
        public static int year = 1;
        public static int timeOfDay = 600;
        public static Microsoft.Xna.Framework.Viewport uiViewport = new() { Width = 1280, Height = 720 };
        public static Microsoft.Xna.Framework.Viewport viewport = new() { Width = 1280, Height = 720 };
        public static Microsoft.Xna.Framework.Graphics.GraphicsDevice graphics { get; } = null;
        public static StardewValley.Menus.IClickableMenu activeClickableMenu { get; set; }

        public static Microsoft.Xna.Framework.Graphics.SpriteFont smallFont = new();
        public static Microsoft.Xna.Framework.Graphics.SpriteFont dialogueFont = new();
        public static Microsoft.Xna.Framework.Graphics.SpriteFont tinyFont = new();
        public static Microsoft.Xna.Framework.Graphics.Texture2D staminaRect = new();
        public static Microsoft.Xna.Framework.Graphics.Texture2D fadeToBlackRect = new();

        public static void drawDialogueBox(int x, int y, int w, int h, bool speak, bool drawOnlyBox, string message = null) { }
        public static void playSound(string cueName) { }

        public static void warpFarmer(string location, int tileX, int tileY, int facing) { }
    }
}

namespace StardewValley.Menus
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class IClickableMenu
    {
        public int xPositionOnScreen;
        public int yPositionOnScreen;
        public int width;
        public int height;

        protected ClickableTextureComponent upperRightCloseButton;

        public IClickableMenu(int x, int y, int w, int h, bool showUpperRightCloseButton = false)
        {
            xPositionOnScreen = x;
            yPositionOnScreen = y;
            width  = w;
            height = h;
        }

        public virtual void draw(SpriteBatch b) { }
        public virtual void receiveLeftClick(int x, int y, bool playSound = true) { }
        public virtual void receiveRightClick(int x, int y, bool playSound = true) { }
        public virtual void receiveKeyPress(Keys key) { }
        public virtual void receiveGamePadButton(Buttons b) { }
        public virtual void receiveScrollWheelAction(int direction) { }
        public virtual void update(GameTime time) { }
        public virtual void performHoverAction(int x, int y) { }
        public virtual bool readyToClose() => true;

        protected void exitThisMenu(bool playSound = true) { }
        protected static void drawMouse(SpriteBatch b) { }
    }

    public class ClickableComponent
    {
        public Rectangle bounds;
        public string name;
        public int myID;

        public ClickableComponent(Rectangle r, string name)
        { bounds = r; this.name = name; }

        public bool containsPoint(int x, int y) => bounds.Contains(x, y);
    }

    public class ClickableTextureComponent : ClickableComponent
    {
        public ClickableTextureComponent(Rectangle r, string name) : base(r, name) { }
        public bool containsPoint(int x, int y) => bounds.Contains(x, y);
    }

    public class TextBox
    {
        public int X, Y, Width;
        public string Text { get; set; } = "";
        public bool limitWidth;

        public TextBox(object t1, object t2, Microsoft.Xna.Framework.Graphics.SpriteFont font, Color color) { }
        public void Draw(SpriteBatch b, int x) { }
        public void SelectMe() { }
        public void RecieveTextInput(char c) { if (c == '\b') { if (Text.Length > 0) Text = Text[..^1]; } else if (c != '\0') Text += c; }
    }
}
