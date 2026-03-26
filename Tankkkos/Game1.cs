

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tankkkos
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Terrain terrain;
        SkyBox skyBox;
        BasicGeometry cube;

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

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(GraphicsDevice);

            terrain = new Terrain(GraphicsDevice, Content.Load<Texture2D>("grassTerrain"),
                1024, 1024, 2);

            skyBox = new SkyBox(GraphicsDevice, Content.Load<Texture2D>("skybox"));

            cube = BasicGeometry.CreateCube(GraphicsDevice);

            player = new Player(GraphicsDevice, terrain, new Vector3(0, 0, 0), activeCamera);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            player.update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            activeCamera.AspectRatio = GraphicsDevice.Viewport.AspectRatio;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            player.Draw();

            terrain.Draw(
                activeCamera);

            skyBox.Draw(activeCamera);

            base.Draw(gameTime);
        }


    }
}
