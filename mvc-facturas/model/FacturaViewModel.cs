using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace FacturArMvcApi.Models
{
    public class FacturaViewModel
    {
        public long ClienteId { get; set; }
        public decimal Importe { get; set; }
        public DateTime Fecha { get; set; }
        public string EstadoActual { get; set; }
        public long FormaPagoId { get; set; }
        public int DiasPagoFactura { get; set; }
        public long PresupuestoId { get; set; }
        public long NroFactura { get; set; }
        public DateTime? FechaCobro { get; set; }
        public DateTime fechaVencimiento { get; set; }
        public string Letra { get; set; } = "C";
        public List<FacturaDetalleViewModel> Detalles { get; set; }
        public string LastSavedJson { get; set; }

        public void AddDetalle()
        {
            Detalles.Add(new FacturaDetalleViewModel());
        }

        public void DeleteDetalle(int index)
        {
            if (index >= 0 && index < Detalles.Count)
                Detalles.RemoveAt(index);
        }
        public void SaveJson()
        {
            LastSavedJson = new JavaScriptSerializer().Serialize(this);
        }
    }

    public class FacturaDetalleViewModel
    {
        public long FacturaId { get; set; }
        public short Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string Item { get; set; }
    }

}