using System;

namespace StrategyGame
{
    /// <summary>
    /// структура хранит позицию верхнего левого угла камеры
    /// </summary>
    struct camera
    {
        public float X, Y;
    };
    /// <summary>
    /// этот статический класс хранит глобальные переменные и константы, доступ к которым необходим из всего проекта.
    /// </summary>
    static class global
    {
        /// <summary>
        /// позиция камеры. Нужна при расчете координат отрисовки объектов.
        /// </summary>
        public static camera Camera = new camera() { X = 0, Y = 0 };
        /// <summary>
        /// размер игрового поля, помещающегося в камеру
        /// </summary>
        public const int WIDTH = 600, HEIGHT = 400;
        /// <summary>
        /// 
        /// </summary>
        public static bool isRuning = true;

        public static float Hypot(float a, float b) { return (float)Math.Sqrt(a * a + b * b); }
    }
}
