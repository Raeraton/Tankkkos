using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Windows.Forms;

namespace Tankkkos
{
    internal class Terrain
    {

            VertexBuffer vertexBuffer;
            BasicEffect effect;


        public float ScaleX = 100f;
        public float ScaleY = 4f;
        public float ScaleZ = 100f;

        public float OffsetX = -50f;
        public float OffsetY = 0f;
        public float OffsetZ = -50f;


        public Terrain(GraphicsDevice dev, Texture2D tex, int width, int height, float textureSize )
            {

                var tempVbData = new VertexPositionTexture[width * height];
                for (int i = 0; i < width * height; i++)
                {
                    int x = i % width;
                    int y = i / width;

                    float vx = x / (float)(width - 1);
                    float vy = y / (float)(height - 1);
                    float vertexHeight = getPointHeight(vx, vy);

                    var pos = new Vector3(vx, vertexHeight, vy);
                    tempVbData[i] = new VertexPositionTexture(pos, new Vector2(pos.X*ScaleX/textureSize, pos.Z*ScaleZ/textureSize));

                }

                // index buffer
                // magic that im afraid to touch
                var indices = new uint[(height - 1) * (width * 2 + 1)];
                int idx = 0;
                int dir = 1;
                for (int j = 1; j < width; j++)
                {
                    int i = dir > 0 ? 0 : width - 1;
                    for (; i >= 0 && i < width; i += dir)
                    {
                        indices[idx++] = (uint)(j * width + i);
                        indices[idx++] = (uint)((j - 1) * width + i);
                    }
                    indices[idx++] = (uint)((j) * width + i - dir);
                    dir = -dir;
                }


            var vbData = new VertexPositionTexture[(height - 1) * (width * 2 + 1)];
            for( uint i=0;  i<indices.Length;  i++) 
            {
                vbData[i] = tempVbData[indices[i]];
            }


            vertexBuffer = new VertexBuffer(dev, VertexPositionTexture.VertexDeclaration, vbData.Length,
            BufferUsage.WriteOnly);
            vertexBuffer.SetData(vbData);


            // effect
            effect = new BasicEffect(dev);
            effect.Texture = tex;
            effect.TextureEnabled = true;

        }

            public void Draw(Camera cam)
            {
                var dev = vertexBuffer.GraphicsDevice;
                dev.SetVertexBuffer(vertexBuffer);
                effect.World = Matrix.CreateScale(ScaleX, ScaleY, ScaleZ) * Matrix.CreateTranslation(OffsetX, OffsetY, OffsetZ);
                effect.View = cam.View;
                effect.Projection = cam.Projection;
                effect.CurrentTechnique.Passes[0].Apply();
            dev.DrawPrimitives(PrimitiveType.TriangleStrip, 0, vertexBuffer.VertexCount);
            }

            float getPointHeight(float x, float z)
            {
                float hx = 0;
                float hz = 0;

                for (int i = 1; i <= 100; i++)
                {

                    float baseAmp = 1f / MathF.Pow(2, i);
                    float baseFreq = MathF.Pow(2f, i);

                    hx += baseAmp * MathF.Sin(2f * MathF.PI * baseFreq * x);
                    hz += baseAmp * MathF.Sin(2f * MathF.PI * baseFreq * 1.4f * z);

                }

                return (hz + hx) / 2f;
            }

        public float GetHeightAtPoint( float x, float z)
        {
            return getPointHeight((x-OffsetX) / ScaleX + OffsetX, (z-OffsetZ) / ScaleZ) * ScaleY + OffsetY;
        }




    }
}
