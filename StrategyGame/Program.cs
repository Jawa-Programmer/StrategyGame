using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using StrategyGame.objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace StrategyGame
{
    class Program
    {
        static Task physic = new Task(() =>
        {
            Stopwatch stp = new Stopwatch();
            stp.Restart();
            float prew = stp.ElapsedMilliseconds / 1000f;
            while (global.isRuning)
            {
                // рассчитывем прошедшее время. Оно нужно что бы при подсчете перемещений на него умножали скорости. Скорости в усл.ед./сек. -> время в секундах
                float delta = stp.ElapsedMilliseconds / 1000f - prew;
                prew = stp.ElapsedMilliseconds / 1000f;
                // выполняем обработку поведения объектов, для обработки передаем прошедшее время
                for (int i = 0; i < global.gameojects.Count; i++)
                {
                    GameObject gm = global.gameojects[i];
                    if (!gm.Update(delta))
                    {
                        global.gameojects.RemoveAt(i);
                        i--;
                    }
                }

                // позволяем другим потокам делать свои делишки, всеравно 60 FPS, нет смысла обробатывать физику слишком часто
                Thread.Sleep(10);
            }
        });
        static GameWindow game = new GameWindow(900, 600);
        [STAThread]
        static void Main(string[] args)
        {
            {
                Unit u = new Unit(200, 200);
                RawMeat meat = new RawMeat(100, 100);
                CookFactory factory = new CookFactory(200, 300);
                global.gameojects.Add(u);
                global.gameojects.Add(meat);
                global.gameojects.Add(factory);
                global.gameojects.Add(new CookedMeat(200, 100));
                u.ActiveTask = new MoveItemToBuilding(meat, factory);
                u.ActiveTask = new GoToTask(30, 20);
            }
            game.Load += (sender, e) =>
            {
                game.VSync = VSyncMode.On;
            };

            game.Resize += (sender, e) =>
            {
                game.Height = game.Width * 2 / 3;
                GL.Viewport(0, 0, game.Width, game.Height);
            };
            game.KeyUp += Game_KeyUp;
            game.Closing += (sender, e) => { global.isRuning = false; physic.Wait(); };
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            game.RenderFrame += (sender, e) =>
            {
                GL.ClearColor(0, 1f, 0, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0, global.WIDTH, global.HEIGHT, 0, 2.1, -2.1);
                foreach (GameObject gm in global.gameojects)
                    gm.Draw();
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
