using OpenTK.Graphics.OpenGL;
using System;

namespace StrategyGame.objects
{

    /// <summary>класс юнита без специализации.</summary>
    class Unit : MoveableGameObject
    {
        protected GameTask activeTask;
        public GameTask ActiveTask { get { return activeTask; } set { activeTask = value; } }
        /// <summary> мера сытости данного юнита. Если становится равна нулю, то юнит умирает.</summary>
        protected float hungry = 100f;
        /// <summary> мера сытости данного юнита (от 0f до 100f) Если становится равна нулю, то юнит умирает.</summary>
        public float Hungry { get { return hungry; } set { hungry = Math.Min(Math.Max(0f, value), 100f); } }
        /// <summary>радиус</summary>
        const float Radius = 5f;
        /// <summary>детализация (количество граней, достаточное, что бы многогранник выглядел круг)</summary>
        const int Details = 8;
        /// <summary>
        /// предмет, который юнит держит в данный момент
        /// </summary>
        protected Item takenItem;

        public Unit(float x, float y) : base(x, y)
        {
        }

        /// <summary>
        /// предмет, который юнит держит в данный момент
        /// </summary>
        public Item TakenItem { get { return takenItem; } }
        /// <summary>
        /// Занят ли сейчас юнит. True, если имеется активная задача
        /// </summary>
        public bool IsBusy { get { return activeTask != null; } }
        /// <summary>
        /// Несёт ли юнит предмет. True, если юнит несёт предмет прямо сейчас
        /// </summary>
        public bool IsTakedItem { get { return takenItem != null; } }
        /// <summary>
        /// Подобрать предмет. После выполнения этой процедуры, предмет будет закреплен за юнитом, а юнит за предметом.
        /// В случае, если предмет или юнит уже заняты, ничего не произойдет.
        /// </summary>
        /// <param name="item"></param>
        public void TakeItem(Item item)
        {
            if (IsTakedItem || item.IsTaken) return;
            takenItem = item;
            item.Take(this);
        }
        /// <summary>
        /// Если юнит держал предмет, вызов этой процедуры заставит его выбросить предмет. Предмет и юнит станут свободны.
        /// </summary>
        public void DropItem() { if (IsTakedItem) takenItem.Drop(); takenItem = null; }

        public override void Draw()
        {
            if (global.Camera.X - Radius > pos.X || global.Camera.X + global.WIDTH + Radius < pos.X || global.Camera.Y - Radius > pos.Y || global.Camera.Y + global.HEIGHT + Radius < pos.Y) return;
            GL.Begin(PrimitiveType.Polygon);
            GL.Color3(0, 0, 0);
            for (int i = 0; i < Details; i++)
            {
                GL.Vertex3(pos.X + Radius * Math.Cos(2 * Math.PI * i / Details) - global.Camera.X, pos.Y + Radius * Math.Sin(2 * Math.PI * i / Details) - global.Camera.Y, 0);
            }
            GL.End();
            if (IsTakedItem)
            {
                GL.LineWidth(3);
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(0.7f, 0.3f, 0.8f);
                GL.Vertex3(pos.X - global.Camera.X, pos.Y - global.Camera.Y, 0.1);
                GL.Vertex3(takenItem.Position.X - global.Camera.X, takenItem.Position.Y - global.Camera.Y, 1.1);
                GL.End();
            }
        }

        public override void Update(float delta)
        {
            if (activeTask != null)
            {
                GameTask.TaskStatus status = activeTask.proceed(delta);
                if (status == GameTask.TaskStatus.FINISHED || status == GameTask.TaskStatus.ABORTED) activeTask = null;
            }
            pos.X += speed * direction.X * delta;
            pos.Y += speed * direction.Y * delta;
        }
    }
}
