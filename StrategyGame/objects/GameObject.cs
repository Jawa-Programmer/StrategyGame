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
        /// <summary>
        /// координаты объекта
        /// </summary>
        protected float X, Y;

        /// <summary>
        /// отрисовка объекта на холсте OpenGL
        /// </summary>
        public abstract void Draw();
        /// <summary>
        /// обработка поведения объекта
        /// параметром передается прошедшее с прошлой обработки время
        /// </summary>
        public abstract void Update(float delta);
    }
    /// <summary>
    /// класс юнита без специализации.
    /// </summary>
    class Unit : GameObject
    {
        /// <summary> величина скорости </summary>
        float Speed = 100f;
        /// <summary>направление движения (применение к ним hypot должно возвращать 1)</summary>
        float dX = 0, dY = 1;
        /// <summary>радиус</summary>
        const float R = 5f;
        /// <summary>детализация (количество граней, достаточное, что бы многогранник выглядел круг)</summary>
        const int Details = 8;
        public Unit(float x, float y) { X = x; Y = y; }
        public override void Draw()
        {
            if (global.Camera.X - R > X || global.Camera.X + global.WIDTH + R < X || global.Camera.Y - R > Y || global.Camera.Y + global.HEIGHT + R < Y) return;
            GL.Begin(PrimitiveType.Polygon);
            GL.Color3(0, 0, 0);
            for (int i = 0; i < Details; i++)
            {
                GL.Vertex2(X + R * Math.Cos(2 * Math.PI * i / Details) - global.Camera.X, Y + R * Math.Sin(2 * Math.PI * i / Details) - global.Camera.Y);
            }
            GL.End();
        }

        public override void Update(float delta)
        {
            X += Speed * dX * delta;
            Y += Speed * dY * delta;
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
        public enum TaskStatus {
            /// <summary>
            /// Задача завершена
            /// </summary>
            FINISHED, 
            /// <summary>
            /// Задача выполняется
            /// </summary>
            PROCEED,
            /// <summary>
            /// Задача прервана
            /// </summary>
            ABORTED };
        /// <summary>
        /// указатель на юнита, который выполняет задачу
        /// </summary>
        Unit executer;
        /// <summary>
        /// конструктор. Параметром передается юнит, который будет исполнять задачу
        /// </summary>
        /// <param name="ex">Юнит, который будет исполнять задачу</param>
        public GameTask(Unit ex) { executer = ex; }
        /// <summary>
        /// список подзадач.
        /// </summary>
        Queue<GameTask> tasks;
        /// <summary>
        /// Метод выполнения задачи. Если задача не является атомарной, она просто передает управление текущей подзадаче и обробатывает ее ответ. Возвращает статус задачи.
        /// </summary>
        /// <param name="dt">Время в секундах, прошедшее с прошлой итерации обновления объектов (используется для подсчета перемещения по формуле r = v * t </param>
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
}
