using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace StrategyGame.objects
{

    /// <summary>класс юнита без специализации.</summary>
    class Unit : MoveableGameObject
    {
        protected Queue<GameTask> activeTasks = new Queue<GameTask>();
        /// <summary>
        /// Активная задача. Этой свойство возвращает текущую активную задачу. 
        /// Но обратите внимание, при попытке установить новую задачу в это свойство, задача не заменить текущую активную задачу, а добавится в конец очереди задач данного юнита. 
        /// Будьте внимательны в этом моменте.
        /// </summary>
        public GameTask ActiveTask { get { return activeTasks.Peek(); } set { activeTasks.Enqueue(value); } }
        /// <summary>
        /// Возврощает количество задач, запланированных данным юнитом.
        /// </summary>
        public int CountOfPlannedTasks { get { return activeTasks.Count; } }
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
        public bool IsBusy { get { return activeTasks.Count > 0; } }
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

        public override bool Update(float delta)
        {
            if (hungry <= 0) isAlive = false;
            if (!isAlive) return false;
            if (IsBusy)
            {
                GameTask.TaskStatus status = activeTasks.Peek().proceed(delta, this);
                if (status == GameTask.TaskStatus.FINISHED || status == GameTask.TaskStatus.ABORTED)
                {
                    activeTasks.Dequeue();
                    if (activeTasks.Count > 0) activeTasks.Peek().CheckTargets(); //перед началом исполнения следующей задачи, проверяем, актуальна ли она.
                }
            }
            pos.X += speed * direction.X * delta;
            pos.Y += speed * direction.Y * delta;
            return isAlive;
        }
    }
}
