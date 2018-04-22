using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LowLevelControls.Natives;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OverlayWindow;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using llc = LowLevelControls;

namespace ScreenColorInspector
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : OverlayGame
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D pixel;
        llc.MouseHook mHook;
        const int viewSize = 50;
        const int viewBorder = 2;
        const int delta = 10;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = GetScreenBounds().Width;
            graphics.PreferredBackBufferHeight = GetScreenBounds().Height;
            graphics.ApplyChanges();
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            mHook = new llc.MouseHook();
            mHook.MouseDownEvent += mHook_MouseDownEvent;
            mHook.InstallGlobalHook();
        }

        private bool mHook_MouseDownEvent(llc.MouseHook sender, uint vkCode, int x, int y, int delta, bool injected)
        {
            if (vkCode == (uint)System.Windows.Forms.Keys.MButton)
            {
                MouseState mState = Mouse.GetState();
                Color color = GetColorAt(mState.X, mState.Y);
                Clipboard.SetText("#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2"));
                return true;
            }
            return false;
        }

        /*private bool kbdHook_KeyDownEvent(llc.KeyboardHook sender, uint vkCode, bool injected)
        {
            if (vkCode == (uint)'G' && llc.Keyboard.IsKeyDown((int)System.Windows.Forms.Keys.Menu))
            {
                MouseState mState = Mouse.GetState();
                Color color = GetColorAt(mState.X, mState.Y);
                Clipboard.SetText("#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2"));
                return true;
            }
            return false;
        }*/

        private void SystemEvents_DisplaySettingsChanged(object sender, System.EventArgs e)
        {
            graphics.PreferredBackBufferWidth = GetScreenBounds().Width;
            graphics.PreferredBackBufferHeight = GetScreenBounds().Height;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixel.SetData<Color>(new Color[] { Color.White });
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            mHook.UninstallGlobalHook();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);

            // TODO: Add your drawing code here
            //Get the pixel under current cursor.
            MouseState mState = Mouse.GetState();
            Color color = GetColorAt(mState.X, mState.Y);
            Color colorInv = new Color(255 - color.R, 255 - color.G, 255 - color.B, color.A);
            Rectangle rect = new Rectangle(mState.X + delta, mState.Y + delta, viewSize, viewSize);
            Rectangle r;
            //Fit the preview box in screen.
            if (mState.X + rect.Width + delta >= Window.ClientBounds.Width)
                rect.X = mState.X - rect.Width - delta;
            if (mState.Y + rect.Height + delta >= Window.ClientBounds.Width)
                rect.Y = mState.Y - rect.Height - delta;
            spriteBatch.Begin();
            //Border.
            r = new Rectangle(rect.X - viewBorder, rect.Y - viewBorder, rect.Width + 2 * viewBorder, viewBorder);
            spriteBatch.Draw(pixel, r, colorInv);
            r = new Rectangle(rect.X - viewBorder, rect.Y - viewBorder, viewBorder, rect.Height + 2 * viewBorder);
            spriteBatch.Draw(pixel, r, colorInv);
            r = new Rectangle(rect.X - viewBorder, rect.Bottom, rect.Height + 2 * viewBorder, viewBorder);
            spriteBatch.Draw(pixel, r, colorInv);
            r = new Rectangle(rect.Right, rect.Y - viewBorder, viewBorder, rect.Height + 2 * viewBorder);
            spriteBatch.Draw(pixel, r, colorInv);
            //Fill.
            spriteBatch.Draw(pixel, rect, color);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        Bitmap bmp = new Bitmap(4, 4);
        Color GetColorAt(int x, int y)
        {
            System.Drawing.Rectangle bounds = new System.Drawing.Rectangle(x, y, 4, 4);
            using (Graphics g = Graphics.FromImage(bmp))
                g.CopyFromScreen(bounds.Location, System.Drawing.Point.Empty, bounds.Size);
            System.Drawing.Color color = bmp.GetPixel(0, 0);
            return new Color(new Vector4(color.R, color.G, color.B, color.A) / 255.0f);
        }

        /*[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        Color GetColorAt(int x, int y)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int ret = BitBlt(hSrcDC, 0, 0, 1, 1, hDC, x, y, (int)CopyPixelOperation.SourceCopy);
                    if (ret == 0)
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }
            System.Drawing.Color color = screenPixel.GetPixel(0, 0);
            return new Color(color.R, color.G, color.B, color.A);
        }*/
    }
}
