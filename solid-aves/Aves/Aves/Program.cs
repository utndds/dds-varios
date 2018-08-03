using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aves
{
    class Program
    {
        static void Main()
        {

            var ave = new Loro();
            ave.setColor ("rojo");
            ave.setAltitud(25);
            ave.volar(20);
            ave.volar(15);

            Console.WriteLine("Color del ave: " + ave.getColor());
            Console.WriteLine("Altitud: " + ave.getAltitud());
            Console.WriteLine("Horas voladas: " + ave.getHorasVoladas());

            Console.WriteLine(ave.record("mp3"));
            Console.WriteLine(ave.record("wma"));
            Console.WriteLine(ave.record("ffg"));

            Console.ReadKey();
        }
    }
}
