﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using StrategyGame.objects;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace StrategyGame
{
    class Program
    {
        static Unit u = new Unit(200, 200);

        static Task physic = new Task(() =>
        {
            Stopwatch stp = new Stopwatch();
            stp.Restart();
            float prew = stp.ElapsedMilliseconds / 1000f;
            while (global.isRuning)
            {
                // рассчитывем прошедшее время. Оно нужно что бы при подсчете перемещений на него умножали скорости. Скорости в пикс/сек. -> время в секундах
                float delta = stp.ElapsedMilliseconds / 1000f - prew;
                prew = stp.ElapsedMilliseconds / 1000f;
                // выполняем обработку поведения объектов, для обработки передаем прошедшее время
                u.Update(delta);

                // позволяем другим потокам делать свои делишки, всеравно 60 FPS, нет смысла обробатывать физику слишком часто
                Thread.Sleep(10);
            }
        });
        static GameWindow game = new GameWindow(800, 800);
        [STAThread]
        static void Main(string[] args)
        {

            game.Load += (sender, e) =>
            {
                game.VSync = VSyncMode.On;
            };

            game.Resize += (sender, e) =>
            {
                GL.Viewport(0, 0, game.Width, game.Height);
            };

            game.KeyUp += Game_KeyUp;
            game.Closing += (sender, e) => { global.isRuning = false; physic.Wait(); };
            game.RenderFrame += (sender, e) =>
            {
                GL.ClearColor(0, 1f, 0, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0, global.WIDTH, global.HEIGHT, 0, 0, 4);
                u.Draw();
                game.SwapBuffers();
            };
            physic.Start();
            game.Run(60.0);
        }


        private static void Game_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    game.Close();
                    break;
            }
        }
    }
}
