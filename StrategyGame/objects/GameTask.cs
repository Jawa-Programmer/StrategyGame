using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyGame.objects
{
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
        /// Флаг того, что задача успела устареть. Устанавливается после инициализации задачи, но до начала ее фактического исполнения.
        /// (например, пока задача ожидала освобождения юнита, цель задачи была уничтожена)
        /// </summary>
        protected bool IsDepeart = false;
        /// <summary>
        /// список подзадач.
        /// </summary>
        protected Queue<GameTask> tasks;
        /// <summary>
        /// Выполняет проверку актуальность цели задачи. Если в ходе проверки будет установлено, что продолжение задачи не имеет смысла, ближайший следующий вызов proceed завершится прерыванием задачи.
        /// Возвращает true, если задача устарела и будет прервана. Возвращает false, если задача все еще актуальна и будет выполнятся.
        /// </summary>
        public virtual bool CheckTargets() { return IsDepeart; }
        /// <summary>
        /// Метод выполнения задачи. Если задача не является атомарной, она просто передает управление текущей подзадаче и обробатывает ее ответ. Если задача атомарная, то метод манипулирует игровыми объектами напрямую. Возвращает статус задачи.
        /// </summary>
        /// <param name="executer">юнит, выполняющий данную задачу</param>
        /// <param name="dt">Время в секундах, прошедшее с прошлой итерации обновления объектов (используется для подсчета перемещения по формуле r = v * t)</param>
        /// <returns>Если все подзадачи выполнены, то возвращает FINISHED, если еще есть невыполненые подзадачи, то возвращает PROCEED. Если продолжение выполнения задачи невозможно, возвращается ABORTED</returns>
        public virtual TaskStatus proceed(float dt, Unit executer)
        {
            if (IsDepeart) return TaskStatus.ABORTED;
            if (tasks.Count > 0)
            {
                GameTask ts = tasks.Peek();
                TaskStatus status = ts.proceed(dt, executer);
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
        /// <param name="tX">X координата целевой позиции</param>
        /// <param name="tY">Y координата целевой позиции</param>
        public GoToTask(float tX, float tY) : base()
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
        public GoToTask(vector2f tar) : base()
        {
            TARGET_POS = tar;
            tasks = null;
        }
        public override TaskStatus proceed(float dt, Unit executer)
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
        /// <summary>
        /// конструктор задачи. В качестве параметров передается сущность, выполняющая задачу и набор позиций, по которым она должна пройти
        /// </summary>
        /// <param name="ex">юнит, выполняющий данную задачу</param>
        /// <param name="pos">массив векторов-координат точек, по которым пройдет юнит</param>
        public GoByPathTask(vector2f[] pos) : base()
        {
            tasks = new Queue<GameTask>(pos.Length);
            for (int i = 0; i < pos.Length; i++)
            {
                tasks.Enqueue(new GoToTask(pos[i]));
            }
        }
        /// <summary>
        /// конструктор задачи. В качестве параметров передается сущность, выполняющая задачу и набор позиций, по которым она должна пройти
        /// </summary>
        /// <param name="ex">юнит, выполняющий данную задачу</param>
        /// <param name="pos">массив координт точек, по которым пройдет юнит. Массив должен быть вида {x1,y1,x2,y2,x3,y3...}</param>
        public GoByPathTask(float[] pos) : base()
        {
            tasks = new Queue<GameTask>(pos.Length);
            for (int i = 0; i < pos.Length - 1; i += 2)
            {
                tasks.Enqueue(new GoToTask(pos[i], pos[i + 1]));
            }
        }
    }

    /// <summary>
    /// Класс элементарной задачи юнита: подобрать предмет. Имеется ввилу, только сам момент взятия предмета.
    /// Если предмет находится рядом с юнитом, задача сразу же завершается успешно и предмет закрепляется за юнитом.
    /// Если предмет слишком далеко от предмета, то задача сразу же завершается прерыванием.
    /// </summary>
    sealed class TakeItem : GameTask
    {
        Item target;
        public TakeItem(Item tar) : base()
        {
            target = tar;
        }
        public override TaskStatus proceed(float dt, Unit executer)
        {
            if (global.Hypot(executer.Position, target.Position) < 5) { executer.TakeItem(target); return TaskStatus.FINISHED; }
            return TaskStatus.ABORTED;
        }
    }
    /// <summary>
    /// Класс элементарной задачи юнита: выбросить предмет. 
    /// Если юнит держит предмет, он дождется, что бы предмет по инерции подъехал поближе, а затем бросит его и задача закончится успешно
    /// Если юнит не держит предмет, то если флаг IgnoreAboart установлен, задача завершится успешно, если нет, то с прерыванием.
    /// </summary>
    sealed class DropItem : GameTask
    {
        bool IgnoreAboart;
        public DropItem(bool ignoreAboat = true) : base()
        {
            IgnoreAboart = ignoreAboat;
        }
        public override TaskStatus proceed(float dt, Unit executer)
        {
            if (executer.IsTakedItem)
            {
                if (global.Hypot(executer.Position, executer.TakenItem.Position) < 7)
                {
                    executer.DropItem();
                    return TaskStatus.FINISHED;
                }
            }
            else if (IgnoreAboart) return TaskStatus.FINISHED;
            else return TaskStatus.ABORTED;
            return TaskStatus.PROCEED;
        }
    }
    /// <summary>
    /// задача юнита: отнести указанный предмет на указанную фабрику
    /// </summary>
    sealed class MoveItemToBuilding : GameTask
    {
        public MoveItemToBuilding(Item it, Buildings fc) : base()
        {
            tasks = new Queue<GameTask>();
            tasks.Enqueue(new GoToTask(it.Position));
            tasks.Enqueue(new TakeItem(it));
            tasks.Enqueue(new GoToTask(fc.Position));
            tasks.Enqueue(new DropItem(false));
        }
    }
}
