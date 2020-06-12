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
        public GameObject(float x, float y)
        {
            pos.X = x; pos.Y = y;
        }
        public GameObject(vector2f ps) { pos = ps; }
        public abstract void Draw();
        /// <summary>
        /// обработка поведения объекта
        /// параметром передается прошедшее с прошлой обработки время
        /// </summary>
        /// <param name="delta">Время в секундах, прошедшее с прошлой итерации обновления объектов (используется для подсчета перемещения по формуле r = v * t)</param>
        public abstract void Update(float delta);
    }
    /// <summary>
    /// абстрактный класс объекта, способного передвигаться.
    /// </summary>
    abstract class MoveableGameObject : GameObject
    {
        /// <summary> величина скорости</summary>
        protected float speed = 0f;
        /// <summary> величина скорости (от 0f до 50f)</summary>
        public float Speed { get { return speed; } set { speed = Math.Min(Math.Max(0f, value), 50f); } }
        /// <summary>направление движения (применение к ним hypot должно возвращать 1)</summary>
        protected vector2f direction = new vector2f { X = 1f, Y = 0f };

        public MoveableGameObject(float x, float y) : base(x, y)
        {
        }

        public MoveableGameObject(vector2f ps) : base(ps)
        {
        }

        /// <summary>
        /// Направление движения объекта.
        /// При передачи вектора направления не беспокойтесь о нормализации: она встроена в setter)))
        /// </summary>
        public vector2f Direction { set { float norm = value.Length; if (norm == 0) return; direction.X = value.X / norm; direction.Y = value.Y / norm; } }
    }

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
            pos.X += speed * direction.X * delta;
            pos.Y += speed * direction.Y * delta;
        }
    }
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
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(1f, 0.2f, 0.2f);
            GL.Vertex2(pos.X - Radius - global.Camera.X, pos.Y - Radius - global.Camera.Y);
            GL.Vertex2(pos.X + Radius - global.Camera.X, pos.Y - Radius - global.Camera.Y);
            GL.Vertex2(pos.X + Radius - global.Camera.X, pos.Y + Radius - global.Camera.Y);
            GL.Vertex2(pos.X - Radius - global.Camera.X, pos.Y + Radius - global.Camera.Y);
            GL.End();
        }
    }

}
