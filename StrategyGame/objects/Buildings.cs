
using System;
using OpenTK.Graphics.OpenGL;

namespace StrategyGame.objects
{

    /// <summary>
    /// абстрактный класс зданий. Все здания наследуются от данного класса
    /// </summary>
    abstract class Buildings : GameObject
    {
        public Buildings(vector2f ps) : base(ps)
        {
        }

        public Buildings(float x, float y) : base(x, y)
        {
        }
    }
    /// <summary>
    /// абстрактный класс фабрик. Все здания производства унаследованны от него
    /// </summary>
    abstract class Factory : Buildings
    {
        public Factory(vector2f ps) : base(ps)
        {
        }

        public Factory(float x, float y) : base(x, y)
        {
        }
        public override bool Update(float delta)
        {
            return isAlive;
        }
    }

    class CookFactory : Factory
    {
        const float Radius = 20f;
        const int Details = 12;
        public CookFactory(vector2f ps) : base(ps)
        {
        }
        public CookFactory(float x, float y) : base(x, y)
        {
        }
        public override void Draw()
        {
            if (global.Camera.X - Radius > pos.X || global.Camera.X + global.WIDTH + Radius < pos.X || global.Camera.Y - Radius > pos.Y || global.Camera.Y + global.HEIGHT + Radius < pos.Y) return;
            GL.Begin(PrimitiveType.Polygon);
            GL.Color3(0.3f, 0.3f, 0.05f);
            for (int i = 0; i < Details; i++)
            {
                GL.Vertex3(pos.X + Radius * Math.Cos(2 * Math.PI * i / Details) - global.Camera.X, pos.Y + Radius * Math.Sin(2 * Math.PI * i / Details) - global.Camera.Y, 2);
            }
            GL.End();
        }

    }

}
