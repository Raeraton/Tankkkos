using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.DirectoryServices;
using static System.Net.Mime.MediaTypeNames;

namespace Tankkkos
{

    class Terrain_part : IDisposable
    {
        VertexBuffer vertexBuffer = null;
        IndexBuffer indexBuffer = null;
        Effect effect;

        public Vector3 baseMiddle { get; private set; }
        public Vector3 middle;
        public Vector3 scale;
        public float MaxHeight = 10f;
        public float TexScale;

        public Terrain_part(GraphicsDevice dev, Effect effect, Terrain terrain, Texture2D tex, int _width, int _height, float maxHeight, float texScale, Vector3 midd, Vector3 scl)
        {

            MaxHeight = maxHeight;

            middle = midd;
            baseMiddle = midd;
            scale = scl;
            TexScale = texScale;

            this.Update(dev, terrain, _width, _height);

            // effect
            this.effect = effect;

        }

        public void Update(GraphicsDevice dev, Terrain terrain, int _width, int _height)
        {

            if(vertexBuffer != null)
            {
                vertexBuffer.Dispose();
                indexBuffer.Dispose();
            }

            int w = _width;
            int h = _height;
            var vbData = new VertexPositionNormalTexture[w * h];


            // vertex buffer
            for (int i = 0; i < vbData.Length; i++)
            {
                int x = i % w;
                int y = i / w;

                float texX = x / (float)(w - 1);
                float texZ = y / (float)(h - 1);

                float posX = (texX-0.5f)*2f;
                float posZ = (texZ - 0.5f) * 2f;

                float height = terrain.GetHeightAtPoint(posX*scale.X + middle.X, posZ*scale.Z + middle.Z) * scale.Y + middle.Y;

                Vector3 normal = terrain.GetNormalAtPoint(posX * scale.X + middle.X, posZ * scale.Z + middle.Z);

                var pos = new Vector3(posX, height, posZ);
                vbData[i] = new VertexPositionNormalTexture(pos, normal, new Vector2(texX*TexScale, texZ*TexScale));
            }
            vertexBuffer = new VertexBuffer(dev, VertexPositionNormalTexture.VertexDeclaration, vbData.Length,
                BufferUsage.WriteOnly);
            vertexBuffer.SetData(vbData);



            // index buffer
            var indices = new ushort[(h - 1) * (w * 2 + 1)];
            int idx = 0;
            int dir = 1;
            for (int j = 1; j < w; j++)
            {
                int i = dir > 0 ? 0 : w - 1;
                for (; i >= 0 && i < w; i += dir)
                {
                    indices[idx++] = (ushort)(j * w + i);
                    indices[idx++] = (ushort)((j - 1) * w + i);
                }
                indices[idx++] = (ushort)((j) * w + i - dir);
                dir = -dir;
            }
            indexBuffer = new IndexBuffer(dev, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        public void Draw(Camera cam)
        {
            var dev = vertexBuffer.GraphicsDevice;
            dev.SetVertexBuffer(vertexBuffer);
            dev.Indices = indexBuffer;

            effect.Parameters["CamPos"].SetValue(cam.Position);

            effect.Parameters["World"].SetValue(Matrix.CreateScale(scale.X, MaxHeight, scale.Z) * Matrix.CreateTranslation(middle));

            effect.Parameters["ViewProj"].SetValue(cam.View * cam.Projection);

            effect.CurrentTechnique.Passes[0].Apply();
            dev.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, indexBuffer.IndexCount - 2);
        }

        public void Dispose()
        {
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        }
    }


    class TerrainLayer {
        public Terrain_part North, East, South, West, NorthEast, NorthWest, SouthEast, SouthWest;

        public TerrainLayer(GraphicsDevice dev, Effect effect, Texture2D tex, Terrain terrain, int resolution, float height, Vector3 middle, Vector3 closest_scale, uint layer) {
            
            int localResolution = (int)(resolution / (1 << (int)layer));
            localResolution = Math.Max(localResolution, 2); // prevent resolution from going to 0 or negative

            Vector3 scale = closest_scale * MathF.Pow(3, layer);
            scale.Y = 1f;

            float offsetX = scale.X * 2;
            float offsetZ = scale.Z * 2;
            float texScale = MathF.Pow(3, layer);

            NorthEast = new Terrain_part(dev, effect, terrain, tex, localResolution, localResolution, height, texScale,
                new Vector3( offsetX, 0, offsetZ ) + middle, scale);
            North = new Terrain_part(dev, effect, terrain, tex, localResolution, localResolution, height, texScale,
                new Vector3(0, 0, offsetZ) + middle, scale);
            NorthWest = new Terrain_part(dev, effect, terrain, tex, localResolution, localResolution, height, texScale,
                new Vector3(-offsetX, 0, offsetZ) + middle, scale);

            East = new Terrain_part(dev, effect, terrain, tex, localResolution, localResolution, height,texScale,
                new Vector3(offsetX, 0, 0) + middle, scale);
            West = new Terrain_part(dev, effect, terrain, tex, localResolution, localResolution, height,texScale,
                new Vector3(-offsetX, 0, 0) + middle, scale);

            SouthEast = new Terrain_part(dev, effect,   terrain, tex, localResolution, localResolution, height, texScale,
                new Vector3(offsetX, 0, -offsetZ) + middle, scale);
            South = new Terrain_part(dev, effect, terrain, tex, localResolution, localResolution, height, texScale, 
                new Vector3(0, 0, -offsetZ) + middle, scale);
            SouthWest = new Terrain_part(dev, effect, terrain, tex, localResolution, localResolution, height, texScale,
                new Vector3(-offsetX, 0, -offsetZ) + middle, scale);



        }

        public Terrain_part GetByVec( Vector2 v )
        {
            if( v.Y > 0) { // north
                if( v.X > 0)
                {
                    return NorthEast;
                }else if( v.X < 0)
                {
                    return NorthWest;
                }
                else
                {
                    return North;
                }
            }
            else if( v.Y < 0 ) { // south
                if (v.X > 0)
                {
                    return SouthEast;
                }
                else if (v.X < 0)
                {
                    return SouthWest;
                }
                else
                {
                    return South;
                }
            }
            else
            {
                if (v.X > 0)
                {
                    return East;
                }
                else if (v.X < 0)
                {
                    return West;
                }
            }
            return null; // x and y == 0
        }

        public void Draw(Camera camera)
        {
            North.Draw(camera);
            NorthEast.Draw(camera);
            East.Draw(camera);
            SouthEast.Draw(camera);
            South.Draw(camera);
            SouthWest.Draw(camera);
            West.Draw(camera);
            NorthWest.Draw(camera);
        }

        public void ForEach( Action<Terrain_part> action) {
            action(North);
            action(NorthEast);
            action(East);
            action(SouthEast);
            action(South);
            action(SouthWest);
            action(West);
            action(NorthWest);
        }

    }


    internal class Terrain
    {

        GraphicsDevice dev;
        Effect effect;
        Texture2D tex;
        int resolution;

        public float MaxHeight = 10f;

        Vector3 middlePoint = new Vector3(0, 0, 0);

        Terrain_part middle;
        TerrainLayer[] layers;
        public Terrain(GraphicsDevice dev, Effect effect, Texture2D tex, PointLight sun, int resolution, Vector3 closest_scale, uint layerCount)
        {
            this.dev = dev;
            this.effect = effect;
            this.tex = tex;
            this.resolution = resolution;

            effect.Parameters["sunPos"].SetValue(sun.Position);
            effect.Parameters["sunShine"].SetValue(sun.Power);
            effect.Parameters["tex"].SetValue(tex);

            middle = new Terrain_part(dev, effect, this, tex, resolution, resolution, MaxHeight, 1.0f,
                middlePoint, closest_scale);

            layers = new TerrainLayer[layerCount];
            for (uint i = 0; i < layerCount; i++)
            {
                layers[i] = new TerrainLayer(dev, effect, tex, this, resolution, MaxHeight, middlePoint, closest_scale, i);
            }

        }

        public void Update( Vector3 playerPos)
        {

            float scaleLen = (new Vector2(middle.scale.X, middle.scale.Z)).Length();
            if (scaleLen < (middlePoint - playerPos).Length()) {
                middlePoint = playerPos;
                middlePoint.Y = 0;
                update(middle.scale);
            }
        }
        void update(Vector3 closest_scale)
        {

            middle.middle = middle.baseMiddle + middlePoint;
            middle.Update(dev, this, resolution, resolution);
            
            for (uint i = 0; i < layers.Length; i++) { 
                layers[i].ForEach( part => {
                    part.middle = part.baseMiddle + middlePoint;
                    part.Update(dev, this, resolution, resolution);
                    } );
            }
        }

        public void Draw(Camera cam)
        {
            middle.Draw(cam);
            for (uint i = 0; i < layers.Length; i++) { 
                layers[i].Draw(cam);
            }
        }

        public float GetHeightAtPoint(float x, float z)
        {

            float xPart = 0;
            float zPart = 0;

            float maxHeight = 0;

            float[] phshs = new float[] { 3f, 10f, 0, 9f, 2f, 7f, 3.1415f, 1.111f, 11.77f, 3.2f };

            for (int i = 0; i < 10; i++) {
                float amplitude = 1f / (i + 1);
                float frequency = (float)i+1f;

                maxHeight += amplitude*2;

                xPart += amplitude * MathF.Sin( frequency / 10 * x + phshs[i]);
                zPart += amplitude * MathF.Cos( frequency / 10 * z + phshs[i]);
            }

            // Placeholder for height retrieval logic
            return (xPart + zPart) / maxHeight;
        }

        public float GetHeightAtPointWorld(float x, float z)
        {
            return GetHeightAtPoint(x, z) * MaxHeight;
        }

        public Vector3 GetNormalAtPoint(float x, float z)
        {
            float dhdx = 0f;
            float dhdz = 0f;

            float maxHeight = 0f;

            float[] phshs = new float[] { 3f, 10f, 0, 9f, 2f, 7f, 3.1415f, 1.111f, 11.77f, 3.2f };


            for (int i = 0; i < 10; i++)
            {
                float amplitude = 1f / (i + 1);
                float frequency = i + 1f;

                maxHeight += amplitude * 2f;

                dhdx += amplitude * frequency * MathF.Cos(frequency / 10 * x + phshs[i]);
                dhdz += -amplitude * frequency * MathF.Sin(frequency / 10 * z + phshs[i]);
            }

            // match height normalization
            dhdx /= maxHeight;
            dhdz /= maxHeight;
            dhdx *= MaxHeight;
            dhdz *= MaxHeight;

            Vector3 normal = new Vector3(-dhdx, 1f, -dhdz);
            normal.Normalize();

            return normal;
        }


    }
}
