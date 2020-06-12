using System;

namespace StrategyGame
{
    /// <summary>
    /// структура двумерный вектор.
    /// </summary>
    struct vector2f
    {
        public float X, Y;
        /// <summary>
        /// оператор суммы векторов. Выполняет покоординатоное суммирование векторов
        /// </summary>
        /// <param name="a">вектор-слогаемое</param>
        /// <param name="b">вектор-слогаемое</param>
        /// <returns>сумма векторов</returns>
        public float Length { get { return (float)Math.Sqrt(X * X + Y * Y); } }
        static public vector2f operator +(vector2f a, vector2f b) { return new vector2f { X = a.X + b.X, Y = a.Y + b.Y }; }
        /// <summary>
        /// оператор разности векторов. Выполняет покоординатоное вычитание векторов
        /// </summary>
        /// <param name="a">вектор-уменьшаемое</param>
        /// <param name="b">вектор-вычитаемое</param>
        /// <returns>разность векторов</returns>
        static public vector2f operator -(vector2f a, vector2f b) { return new vector2f { X = a.X - b.X, Y = a.Y - b.Y }; }
    }
    /// <summary>
    /// этот статический класс хранит глобальные переменные и константы, доступ к которым необходим из всего проекта.
    /// </summary>
    static class global
    {
        /// <summary>
        /// позиция камеры. Нужна при расчете координат отрисовки объектов.
        /// </summary>
        public static vector2f Camera = new vector2f() { X = 0, Y = 0 };
        /// <summary>
        /// размер игрового поля, помещающегося в камеру
        /// </summary>
        public const int WIDTH = 600, HEIGHT = 400;
        /// <summary>
        /// флаг выполнения игры. При установи в false обработка физики в игре останавлевается по завершении текущей итерации.
        /// </summary>
        public static bool isRuning = true;
        /// <summary>
        /// функция ищет гипотенузу треугольника, с заданными катетами
        /// </summary>
        /// <param name="a">первый катет</param>
        /// <param name="b">второй катет</param>
        /// <returns>гипотенуза</returns>
        public static float Hypot(float a, float b) { return (float)Math.Sqrt(a * a + b * b); }
        /// <summary>
        /// находит расстояние от точки, заданной первым вектором, до точки, заданной вторым вектором
        /// </summary>
        /// <param name="a">вектор координат первой точки</param>
        /// <param name="b">вектор координат второй точки</param>
        /// <returns>расстояние между точками</returns>
        public static float Hypot(vector2f a, vector2f b) { return (a - b).Length; }
    }
}
