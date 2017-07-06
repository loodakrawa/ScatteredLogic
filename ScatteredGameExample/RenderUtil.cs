using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ScatteredGameExample
{
    public class RenderUtil
    {
        private readonly GraphicsDevice graphicsDevice;

        private readonly Texture2D debugTex;

        private readonly List<KeyValuePair<Vector2, Vector2>> lines = new List<KeyValuePair<Vector2, Vector2>>();

        public RenderUtil(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            debugTex = new Texture2D(graphicsDevice, 1, 1);
            Color[] color = new Color[] { Color.White };
            debugTex.SetData(color);
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (var pair in lines) DrawLine(batch, pair.Key, pair.Value);
            lines.Clear();
        }

        public void AddForDrawing(Vector2 v1, Vector2 v2)
        {
            lines.Add(new KeyValuePair<Vector2, Vector2>(v1, v2));
        }

        public void AddForDrawing(Rectangle r)
        {
            Vector2 bottomLeft = new Vector2(r.Left, r.Bottom);
            Vector2 topLeft = new Vector2(r.Left, r.Top);
            Vector2 topRight = new Vector2(r.Right, r.Top);
            Vector2 bottomRight = new Vector2(r.Right, r.Bottom);

            AddForDrawing(bottomLeft, topLeft);
            AddForDrawing(topLeft, topRight);
            AddForDrawing(topRight, bottomRight);
            AddForDrawing(bottomRight, bottomLeft);
        }

        private void DrawLine(SpriteBatch batch, Vector2 start, Vector2 end)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            Rectangle rec = new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 1);

            batch.Draw(debugTex, rec, null, Color.Green, angle, new Vector2(0, 0), SpriteEffects.None, 0);
        }
    }
}
