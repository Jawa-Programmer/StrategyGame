using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyGame
{
    struct camera
    {
        public float X, Y;
    };
    static class global
    {
        public static camera Camera = new camera() { X = 0, Y = 0 };
        public const int WIDTH = 400, HEIGHT = 400;
        public static bool isRuning = true;
    }
}
