

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Tankkkos
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        PointLight sun = new PointLight ( 200f, 100f, 1000f, 1f, 0.4f );

        Terrain terrain;
        SkyBox skyBox;

        Player player;

        Camera activeCamera = Camera.Main;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            Window.AllowUserResizing = true;
            Window.Title = "Tankkkos xd";

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(GraphicsDevice);

            terrain = new Terrain(GraphicsDevice,
                Content.Load<Texture2D>("grassTerrain256"),
                1024, 1024, Content.Load<Effect>("Terrain"), sun );

            skyBox = new SkyBox(GraphicsDevice, Content.Load<Texture2D>("skybox"));

            player = new Player(GraphicsDevice, terrain, new Vector3(0, 0, 0), activeCamera,
                        Content.Load<Model>("tank"), Content.Load<Effect>("Player"), sun );

        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            player.Update();
            Console.WriteLine(Camera.Main.Position);

            base.Update(gameTime);
        }

        long secStart = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        uint frameCounter = 0;
        protected override void Draw(GameTime gameTime)
        {
            var _tn = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            frameCounter++;
            if( _tn - secStart >= 1000) {
                Window.Title = $"FPS: {frameCounter}";
                frameCounter = 0;
                secStart = _tn;
            }

            activeCamera.AspectRatio = GraphicsDevice.Viewport.AspectRatio;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            player.Draw();

            terrain.Draw(activeCamera);

            skyBox.Draw(activeCamera);

            base.Draw(gameTime);
        }


    }
}
