using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace StrategyGame.objects
{
    /// <summary>
    /// абстрактный класс, от него наследуются все рисуемые игровые объекты
    /// </summary>
    abstract class GameObject
    {
        /// <summary> Координаты объекта </summary>
        protected vector2f pos;
        /// <summary> Координаты объекта </summary>
        public vector2f Position { get { return pos; } set { pos = value; } }
        /// <summary> Координата X объекта </summary>
        public float X { get { return pos.X; } }
        /// <summary> Координата Y объекта </summary>
        public float Y { get { return pos.Y; } }
        public abstract void Draw();
        /// <summary>
        /// обработка поведения объекта
        /// параметром передается прошедшее с прошлой обработки время
        /// </summary>
        /// <param name="delta">Время в секундах, прошедшее с прошлой итерации обновления объектов (используется для подсчета перемещения по формуле r = v * t)</param>
        public abstract void Update(float delta);
    }
    /// <summary>класс юнита без специализации.</summary>
    class Unit : GameObject
    {
        protected GameTask activeTask;
        /// <summary> мера сытости данного юнита. Если становится равна нулю, то юнит умирает.</summary>
        protected float hungry = 100f;
        /// <summary> мера сытости данного юнита (от 0f до 100f) Если становится равна нулю, то юнит умирает.</summary>
        public float Hungry { get { return hungry; } set { hungry = Math.Min(Math.Max(0f, value), 100f); } }
        /// <summary> величина скорости</summary>
        protected float speed = 0f;
        /// <summary> величина скорости (от 0f до 50f)</summary>
        public float Speed { get { return speed; } set { speed = Math.Min(Math.Max(0f, value), 50f); } }
        /// <summary>направление движения (применение к ним hypot должно возвращать 1)</summary>
        protected vector2f direction = new vector2f { X = 1f, Y = 0f };
        /// <summary>
        /// Направление движения объекта.
        /// При передачи вектора направления не беспокойтесь о нормализации: она встроена в setter)))
        /// </summary>
        public vector2f Direction { set { float norm = global.Hypot(value.X, value.Y); direction.X = value.X / norm; direction.Y = value.Y / norm; } }

        /// <summary>радиус</summary>
        const float Radius = 5f;
        /// <summary>детализация (количество граней, достаточное, что бы многогранник выглядел круг)</summary>
        const int Details = 8;
        public Unit(float x, float y)
        {
            pos.X = x; pos.Y = y;
        }
        public Unit(vector2f ps) { pos = ps; }
        public override void Draw()
        {
            if (global.Camera.X - Radius > pos.X || global.Camera.X + global.WIDTH + Radius < pos.X || global.Camera.Y - Radius > pos.Y || global.Camera.Y + global.HEIGHT + Radius < pos.Y) return;
            GL.Begin(PrimitiveType.Polygon);
            GL.Color3(0, 0, 0);
            for (int i = 0; i < Details; i++)
            {
                GL.Vertex2(pos.X + Radius * Math.Cos(2 * Math.PI * i / Details) - global.Camera.X, pos.Y + Radius * Math.Sin(2 * Math.PI * i / Details) - global.Camera.Y);
            }
            GL.End();
        }

        public override void Update(float delta)
        {
            if (activeTask != null)
            {
                GameTask.TaskStatus status = activeTask.proceed(delta);
                if (status == GameTask.TaskStatus.FINISHED || status == GameTask.TaskStatus.ABORTED) activeTask = null;
            }
            pos.X += Speed * direction.X * delta;
            pos.Y += Speed * direction.Y * delta;
        }
    }
    /// <summary>
    /// Объект зададачи юнитов. В общем виде, задачи - список подзадач. Однако есть атомарные, неделимые задачи, которые не ссылаются на другие задачи, а манипулирую игровыми объктами напрямую (меняют их скорости и направления, и т.д.)
    /// </summary>
    class GameTask
    {
        /// <summary>
        /// статусы задач
        /// </summary>
        public enum TaskStatus
        {
            /// <summary>Задача завершена</summary>
            FINISHED,
            ///<summary>Задача выполняется</summary>
            PROCEED,
            /// <summary>Задача прервана</summary>
            ABORTED
        };
        /// <summary>
        /// указатель на юнита, который выполняет задачу
        /// </summary>
        protected Unit executer;
        /// <summary>
        /// конструктор. Параметром передается юнит, который будет исполнять задачу
        /// </summary>
        /// <param name="ex">Юнит, который будет исполнять задачу</param>
        public GameTask(Unit ex) { executer = ex; }
        /// <summary>
        /// список подзадач.
        /// </summary>
        protected Queue<GameTask> tasks;
        /// <summary>
        /// Метод выполнения задачи. Если задача не является атомарной, она просто передает управление текущей подзадаче и обробатывает ее ответ. Если задача атомарная, то метод манипулирует игровыми объектами напрямую. Возвращает статус задачи.
        /// </summary>
        /// <param name="dt">Время в секундах, прошедшее с прошлой итерации обновления объектов (используется для подсчета перемещения по формуле r = v * t)</param>
        /// <returns>Если все подзадачи выполнены, то возвращает FINISHED, если еще есть невыполненые подзадачи, то возвращает PROCEED. Если продолжение выполнения задачи невозможно, возвращается ABORTED</returns>
        public virtual TaskStatus proceed(float dt)
        {
            if (tasks.Count > 0)
            {
                GameTask ts = tasks.Peek();
                TaskStatus status = ts.proceed(dt);
                switch (status)
                {
                    case TaskStatus.FINISHED:
                        tasks.Dequeue();
                        break;
                    case TaskStatus.ABORTED:
                        tasks.Clear();
                        return status;
                        break;
                }
            }
            else return TaskStatus.FINISHED;
            return TaskStatus.PROCEED;
        }
    }
    /// <summary>
    /// Класс элементарной задачи юнита: идти в позицию.
    /// </summary>
    sealed class GoToTask : GameTask
    {
        vector2f TARGET_POS;
        /// <summary>
        /// Конструктор задачи. В качестве параметров передается сущность, выполняющая задачу и позиция, куда она должна прибыть.
        /// </summary>
        /// <param name="ex">юнит, выполняющий данную задачу</param>
        /// <param name="tX">X координата целевой позиции</param>
        /// <param name="tY">Y координата целевой позиции</param>
        public GoToTask(Unit ex, float tX, float tY) : base(ex)
        {
            TARGET_POS.X = tX;
            TARGET_POS.Y = tY;
            tasks = null;
        }
        /// <summary>
        /// Конструктор задачи. В качестве параметров передается сущность, выполняющая задачу и позиция, куда она должна прибыть.
        /// </summary>
        /// <param name="ex">юнит, выполняющий данную задачу</param>
        /// <param name="tar">вектор с координатами целевой позиции</param>
        public GoToTask(Unit ex, vector2f tar) : base(ex)
        {
            TARGET_POS = tar;
            tasks = null;
        }
        public override TaskStatus proceed(float dt)
        {
            if (global.Hypot(executer.Position, TARGET_POS) <= executer.Speed * dt)
            {
                executer.Position = TARGET_POS;
                executer.Speed = 0f;
                return TaskStatus.FINISHED;
            }
            executer.Speed = 50f;
            executer.Direction = TARGET_POS - executer.Position;
            return TaskStatus.PROCEED;
        }
    }
    /// <summary>
    /// Класс задачи юнита: идти по ломаной, заданной набором точек.
    /// </summary>
    sealed class GoByPathTask : GameTask
    {
        public GoByPathTask(Unit ex, vector2f[] pos) : base(ex)
        {
            tasks = new Queue<GameTask>(pos.Length);
            for (int i = 0; i < pos.Length; i++)
            {
                tasks.Enqueue(new GoToTask(ex, pos[i]));
            }
        }
        public GoByPathTask(Unit ex, float[] pos) : base(ex)
        {
            tasks = new Queue<GameTask>(pos.Length);
            for (int i = 0; i < pos.Length - 1; i += 2)
            {
                tasks.Enqueue(new GoToTask(ex, pos[i], pos[i + 1]));
            }
        }
    }
}
