using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marada.Libs.Utils
{
    public static class Thrower
    {
        public static void Throw<E>(Func<bool> test, string message) where E : Exception, new()
        {
            bool result = test.Invoke();
            var e = new Exception(message);
            E x = (E)e;
            if(result) throw x;
        }
    }
}
