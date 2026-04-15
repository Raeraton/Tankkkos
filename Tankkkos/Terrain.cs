using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Tankkkos
{
    internal class Terrain
    {

        VertexBuffer vertexBuffer;
        Effect effect;


        public float HorizontalScale = 2000f;
        public float VerticalScale = 0.13f;

        private uint waveCount = 32;
        private float[] waveFrequencies;
        private float[] wavePhaseses;


        public Terrain(GraphicsDevice dev, Texture2D tex, int width, int height, Effect _effect, PointLight sun )
        {

            var tempVbData = new VertexPosition[width * height];
            for (int i = 0; i < width * height; i++)
            {
                int x = i % width;
                int y = i / width;

                float vx = x / (float)(width - 1);
                float vy = y / (float)(height - 1);

                var pos = new Vector3(vx, 0, vy);
                tempVbData[i] = new VertexPosition( pos );
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


            var vbData = new VertexPosition[(height - 1) * (width * 2 + 1)];
            for( uint i=0;  i<indices.Length;  i++) 
            {
                vbData[i] = tempVbData[indices[i]];
            }


            vertexBuffer = new VertexBuffer(dev, VertexPosition.VertexDeclaration, vbData.Length,
            BufferUsage.WriteOnly);
            vertexBuffer.SetData(vbData);


            // waveFrequencies
            waveFrequencies = new float[waveCount];
            wavePhaseses = new float[waveCount];
            var rand = new Random();
            for( int i=0;  i<waveCount; i++)
            {
                waveFrequencies[i] = 1f / (i+1);
                wavePhaseses[i] = (float)rand.NextDouble() * 2 * MathF.PI;
            }


            // effect
            effect = _effect;

            effect.Parameters["GrassTex"].SetValue(tex);

            effect.Parameters["WaveHeight"].SetValue(VerticalScale);
            effect.Parameters["WaveFreqs"].SetValue(waveFrequencies);
            effect.Parameters["WavePhas"].SetValue(wavePhaseses);


            effect.Parameters["SunPosition"].SetValue(sun.Position);
            effect.Parameters["SunShininess"].SetValue(sun.Power);

        }

            public void Draw(Camera cam)
            {
                var dev = vertexBuffer.GraphicsDevice;
                dev.SetVertexBuffer(vertexBuffer);

            effect.Parameters["World"].SetValue(
                Matrix.CreateScale(HorizontalScale, 1, HorizontalScale) *
                Matrix.CreateTranslation(new Vector3(cam.Position.X-(HorizontalScale/2), 0f, cam.Position.Z-(HorizontalScale/2))));

            effect.Parameters["ViewProj"].SetValue( cam.View * cam.Projection);

            effect.Parameters["CameraPosition"].SetValue(cam.Position);

            effect.CurrentTechnique.Passes[0].Apply();
                dev.DrawPrimitives(PrimitiveType.TriangleStrip, 0, vertexBuffer.VertexCount);
            }

            float getPointHeight(float x, float z)
            {

                float hx = 0;
                float hz = 0;

                for( uint i=0;  i<waveCount;  i++)
                {
                    float freq = waveFrequencies[i];
                    float ampl = 1 / freq;
                    float phase = wavePhaseses[i];
                    
                    hx += ampl * MathF.Sin(x*HorizontalScale*freq + phase);
                    hz += ampl * MathF.Sin(z*HorizontalScale*freq + phase);
                    
                }

                return (hz + hx) / 2f;
            }

        public float GetHeightAtPoint( float x, float z)
        {
            return getPointHeight((x) / HorizontalScale, (z) / HorizontalScale) * VerticalScale;
        }




    }
}
