using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvesStrategy
{
    class Program
    {
        static void Main()
        {

            var ave = new Loro();
            ave.setColor("rojo");
            ave.setAltitud(25);
            ave.volar(20);
            ave.volar(15);

            Console.WriteLine("Color del ave: " + ave.getColor());
            Console.WriteLine("Altitud: " + ave.getAltitud());
            Console.WriteLine("Horas voladas: " + ave.getHorasVoladas());

            // Two contexts following different strategies

            ave.SetRecordStrategy(new AudioMp3());
            ave.record();

            ave.SetRecordStrategy(new AudioWma());
            ave.record();

            // Wait for user
            Console.ReadKey();
        }
    }
}
