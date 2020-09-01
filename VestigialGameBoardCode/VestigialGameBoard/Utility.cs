using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Utility
{
    static Random random = new Random();
    public static bool RandomBoolean()
    {
        return random.Next(0, 2) == 0;
    }
}
