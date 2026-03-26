using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tankkkos
{
    internal class Player
    {

        BasicGeometry cube;
        Camera camera;

        Terrain terrain;
        Vector3 position;

        public float CameraDistance = 5;
        public float MovementSpeed = 0.3f;


        public Player( GraphicsDevice dev, Terrain terrain, Vector3 position, Camera camera ) 
        {

            this.terrain = terrain;
            this.position = position;
            this.camera = camera;

            cube = BasicGeometry.CreateCube(dev);
         
            update();

        }


        public void Draw()
        {
            cube.Draw(
                Matrix.CreateTranslation(position.X, position.Y, position.Z),
                camera.View,
                camera.Projection);
        }

        public void update() 
        {
            updatePosition();
            updateCamera();
        }

        private void updatePosition() 
        {
            var camDir = Vector3.Normalize( camera.Direction ) * MovementSpeed;

            var ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.W))
                position += new Vector3( camDir.X, 0, camDir.Z );
            if (ks.IsKeyDown(Keys.S))
                position -= new Vector3(camDir.X, 0, camDir.Z);
            if (ks.IsKeyDown(Keys.A))
                position -= new Vector3(-camDir.Z, 0, camDir.X);
            if (ks.IsKeyDown(Keys.D))
                position += new Vector3(-camDir.Z, 0, camDir.X);

            position.Y = terrain.GetHeightAtPoint(position.X, position.Z) + 1f;
        }

        private void updateCamera() 
        {
            var ks = Keyboard.GetState();

            if( ks.IsKeyDown( Keys.Q ))
                camera.Direction = Vector3.Transform( camera.Direction, Matrix.CreateRotationY( 0.05f ) );

            if (ks.IsKeyDown(Keys.E))
                camera.Direction = Vector3.Transform(camera.Direction, Matrix.CreateRotationY(-0.05f));

            camera.Position = position - Vector3.Normalize(camera.Direction) * CameraDistance;


        }

    }
}
