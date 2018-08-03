using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    class Cuenta
    {
        public int id;
        public float valor;

        public void setValor(float valor)
        { this.valor = valor;  }

        public float getValor()
        { return this.valor; }

        public bool superaValor(float valor)
        {
        if (getValor() > valor)
            return true;
        else
            return false;
        }
    }
