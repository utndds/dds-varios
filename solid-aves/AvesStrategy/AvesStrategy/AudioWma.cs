using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    class AudioWma : Audio
    {
        public override void record()
        {
            Console.WriteLine("Estoy grabando a un loro en formato wma");
        }
    }
