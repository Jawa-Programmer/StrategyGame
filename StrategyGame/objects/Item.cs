using OpenTK.Graphics.OpenGL;
using System;

namespace StrategyGame.objects
{

    /// <summary>
    /// класс перетаскиевыемых предметов. Наследники данного класса реализуют ресурсы и материалы
    /// </summary>
    abstract class Item : MoveableGameObject
    {
        /// <summary>
        ///  Перерменная-флаг состояния. True, если предмет держит какой-либо юнит, иначе - false.
        /// </summary>
        public bool IsTaken { get { return owner != null; } }
        /// <summary>
        /// Юнит, который в данный момент держит предмет
        /// </summary>
        protected Unit owner;
        /// <summary>
        /// Юнит, который в данный момент держит этот предмет
        /// </summary>
        public Unit Owner { get { return owner; } }
        /// <summary>
        /// Если предмет был свободен, после вызова этой функции его будет держать юнит. Предмет будет закреплен за юнитом.
        /// Если предмет уже был взят другим юнитом, ничего не случится.
        /// </summary>
        /// <param name="owner">Юнит, который подобрал этот предмет</param>
        public void Take(Unit owner) { if (IsTaken) return; this.owner = owner; }
        /// <summary>
        /// Сообщает предмету о том, что его выбросили. Предмет становится свободным и может быть снова подобран.
        /// </summary>
        public void Drop() { owner = null; }
        /// <summary>
        /// наименование предмета
        /// </summary>
        public static string Title { get { return "предмет"; } }
        public Item(float x, float y) : base(x, y)
        {
        }

        public Item(vector2f ps) : base(ps)
        {
        }

        public override void Update(float delta)
        {
            if (IsTaken)
            {
                vector2f dv = owner.Position - pos;
                Direction = dv;
                speed = Math.Max(0f, (dv.Length - 5) * 1.5f);
                pos.X += speed * direction.X * delta;
                pos.Y += speed * direction.Y * delta;
            }
        }
    }
    /// <summary>
    /// класс предмета "Сырое мясо". Используется для готовки "Жаренного мяса" на заводе
    /// </summary>
    class RawMeat : Item
    {
        /// <summary>
        /// наименование предмета
        /// </summary>
        public static new string Title { get { return "сырое мясо"; } }
        /// <summary>радиус</summary>
        const float Radius = 5f;
        public RawMeat(vector2f ps) : base(ps)
        {
        }

        public RawMeat(float x, float y) : base(x, y)
        {
        }

        public override void Draw()
        {
            if (global.Camera.X - Radius > pos.X || global.Camera.X + global.WIDTH + Radius < pos.X || global.Camera.Y - Radius > pos.Y || global.Camera.Y + global.HEIGHT + Radius < pos.Y) return;
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(1f, 0.2f, 0.2f);
            GL.Vertex3(pos.X - Radius - global.Camera.X, pos.Y - Radius - global.Camera.Y, 1);
            GL.Vertex3(pos.X + Radius - global.Camera.X, pos.Y - Radius - global.Camera.Y, 1);
            GL.Vertex3(pos.X + Radius - global.Camera.X, pos.Y + Radius - global.Camera.Y, 1);
            GL.Vertex3(pos.X - Radius - global.Camera.X, pos.Y + Radius - global.Camera.Y, 1);
            GL.End();
        }
    }
    /// <summary>
    /// класс предмета "готовое мясо". Используется для кормления юнитов
    /// </summary>
    class CookedMeat : Item
    {
        /// <summary>
        /// наименование предмета
        /// </summary>
        public static new string Title { get { return "сырое мясо"; } }
        /// <summary>радиус</summary>
        const float Radius = 5f;
        public CookedMeat(vector2f ps) : base(ps)
        {
        }

        public CookedMeat(float x, float y) : base(x, y)
        {
        }

        public override void Draw()
        {
            if (global.Camera.X - Radius > pos.X || global.Camera.X + global.WIDTH + Radius < pos.X || global.Camera.Y - Radius > pos.Y || global.Camera.Y + global.HEIGHT + Radius < pos.Y) return;
            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(0.7f, 0.7f, 0.1f);
            GL.Vertex3(pos.X - Radius - global.Camera.X, pos.Y - Radius - global.Camera.Y, 1);
            GL.Vertex3(pos.X + Radius - global.Camera.X, pos.Y - Radius - global.Camera.Y, 1);
            GL.Vertex3(pos.X - global.Camera.X, pos.Y + Radius - global.Camera.Y, 1);
            GL.End();
        }
    }
}
