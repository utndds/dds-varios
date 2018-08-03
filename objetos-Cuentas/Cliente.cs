using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    class Cliente
    {

    public string dni;
    public int init;

    List<Cuenta> cuentas = new List<Cuenta>();
    private int j;

    public Cliente()
    {
        cuentas.Add(new Cuenta() { id = init + 1,  valor = 20 });
        cuentas.Add(new Cuenta() { id = init + 2,  valor = 240 });
        cuentas.Add(new Cuenta() { id = init + 3, valor = 185 });
    }

    public int getCountCuentasCliente()
        { return cuentas.Count();  }

    public int getCountCuentasSupera(float valor)
    {

        for (int i=0; i<cuentas.Count(); i++)
        {
            if (cuentas[i].superaValor(valor))
            { j = j + 1;  }
        }

        return j;
    }
}
