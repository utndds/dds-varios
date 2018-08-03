using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuentas
{
    class Program
    {
        static void Main(string[] args)
        {
 
            Control control = new Control();
            control.setSaldoMinimo(150);
            Console.WriteLine (control.cantidadCuentasValorMinimo("123"));
        
            // Wait for user
            Console.ReadKey();
        }

    }


}
