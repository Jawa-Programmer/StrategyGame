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
        /// <summary>Флаг состояния жизни. True, если предмет "жив" и false, если уже "умер". "Мертвые" объекты при первой же попытки обработки удаляются из списков игровых объектов.</summary>
        protected bool isAlive = true;
        /// <summary>Флаг состояния жизни. True, если предмет "жив" и false, если уже "умер". "Мертвые" объекты при первой же попытки обработки удаляются из списков игровых объектов.</summary>
        public bool IsAlive { get { return isAlive; } }
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
        /// возвращает флаг состояния (если объект уже "мертв", будет возвращен false)
        /// </summary>
        /// <param name="delta">Время в секундах, прошедшее с прошлой итерации обновления объектов (используется для подсчета перемещения по формуле r = v * t)</param>
        /// <returns>Флаг состояния жизни (true, если продолжает жить и false, если умер)</returns>
        public abstract bool Update(float delta);
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


}
