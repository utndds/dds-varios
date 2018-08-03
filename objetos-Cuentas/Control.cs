using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    class Control
    {
        private float _saldoMinimo;

        public List<Cliente> clientes = new List<Cliente>();

        public Control()
        {
        clientes.Add(new Cliente() { init = 100,  dni = "123" });
        clientes.Add(new Cliente() { init = 200,  dni = "200" });
        clientes.Add(new Cliente() { init = 300,  dni = "783" });
        }

        public void setSaldoMinimo(float saldoMinimo)
        { this._saldoMinimo = saldoMinimo;  }

        private Cliente buscarCliente(string dni)
        {
            for (int i=0;i<clientes.Count();i++)
            {
                if (Equals(clientes[i].dni, dni))
                { return clientes[i];  }
            }

        return null;
        }

        public int cantidadCuentasValorMinimo(string dni)
        {
            return buscarCliente(dni).getCountCuentasSupera(this._saldoMinimo );
        }
    }
