using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using FacturArLib;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Web;

namespace FacturArMvcApi.Controllers
{
    [Authorize(Roles = "Monotributista")]
    public class FacturaController : BaseController
    {
        private FacturArDbContext db = new FacturArDbContext();
        private Monotributista currentUser = Monotributista.GetMonotributistaActual;

        // GET: /Factura/
        public ActionResult Index()
        {
            var facturas = db.Facturas
                .Where(f => f.MonotributistaId == currentUser.Id)
                .Include(f => f.Cliente)
                .Include(f => f.Monotributista)
                .Include(f => f.Comprobante)
                .Include(f => f.FormaPago)
                .Include(f => f.Presupuesto)
                .Include(f => f.RecorridoCobranza);

            return View(facturas.ToList().OrderByDescending(f => f.Numero));
        }
        public ActionResult ProximosVencimientos()
        {
            var estaSemana = DateTime.Today.AddDays(5);

            var facturas = db.Facturas
                .Where(f => f.MonotributistaId == currentUser.Id)
                .Where(f => f.FechaVencimiento <= estaSemana)
                .Where(f => 
                    f.EstadoActual == (int)Estado.Valores.Emitida || 
                    f.EstadoActual == (int)Estado.Valores.Enviada || 
                    f.EstadoActual == (int)Estado.Valores.EmitidaMobile || 
                    f.EstadoActual == (int)Estado.Valores.Vencida)
                .Include(f => f.Cliente)
                .Include(f => f.Monotributista)
                .Include(f => f.Comprobante)
                .Include(f => f.FormaPago)
                .Include(f => f.Presupuesto)
                .Include(f => f.RecorridoCobranza);

            return View("Index", facturas.ToList().OrderBy(f => f.FechaVencimiento));
        }
        // GET: /Factura/ByComprobante/5
        public ActionResult ByComprobante(long id)
        {
            Factura factura = db.Facturas.Where(i => i.ComprobanteId == id).Single();
            return RedirectToAction($"Details/{factura.Id}");
        }

        // GET: /Factura/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factura factura = db.Facturas.Find(id);
            if (factura == null)
            {
                return HttpNotFound();
            }
            return View(factura);
        }

        // GET: /Factura/Bitacora/5
        public ActionResult Bitacora(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factura factura = db.Facturas.Find(id);
            if (factura == null)
            {
                return HttpNotFound();
            }
            var bitacoraOrdenada = factura.Bitacoras
                .OrderByDescending(b => b.DateCreated)
                .ToList();
            factura.Bitacoras = bitacoraOrdenada;
            return View(factura);
        }

        // GET: /Factura/Create
        public ActionResult Create()
        {
            var factura = new Factura
            {
                Numero = Monotributista.NroUltimaFactura + 1,
                Fecha = DateTime.Today,
                Letra = "C",
                Uid = Guid.NewGuid(),
                EstadoActual = (int)Estado.Valores.Emitida,
                Monotributista = currentUser
            };

            var clientesFiltrados = db.Clientes
                .Where(c => c.MonotributistaId == Monotributista.IdUsuarioActual)
                .Where(c => c.Activo)
                .Select(c => new SelectListItem { Text = c.Nombre, Value = c.Id.ToString() });

            IEnumerable<SelectListItem> presupuestosFiltrados = db.Presupuestos
                .Where(p => p.MonotributistaId == Monotributista.IdUsuarioActual)
                .Where(p => p.EstadoActual == (int)Estado.Valores.Aprobado) 
                .Select(p => new SelectListItem { Text = p.Numero.ToString(), Value = p.Id.ToString()});

            ViewBag.ClienteId = clientesFiltrados;
            ViewBag.FormaPagoId = new SelectList(db.FormasPago, "Id", "Nombre");
            ViewBag.PresupuestoId = presupuestosFiltrados;

            Session["factura_detalles"] = new List<FacturaDetalleDtoAjax>();

            return View(factura);
        }

        // GET: /Factura/CreateFacturaFromPresupuesto
        public ActionResult CreateFacturaFromPresupuesto(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Presupuesto presupuesto = db.Presupuestos.Find(id);
            if (presupuesto == null)
            {
                return HttpNotFound();
            }

            var factura = new Factura
            {
                Numero = Monotributista.NroUltimaFactura + 1,
                Fecha = DateTime.Today,
                Letra = "C",
                Uid = Guid.NewGuid(),
                EstadoActual = (int)Estado.Valores.Emitida,
                ClienteId = presupuesto.ClienteId,
                PresupuestoId = presupuesto.Id,
                Importe = presupuesto.Importe,
                Monotributista = currentUser
            };

            List<FacturaDetalleDtoAjax> detalles = new List<FacturaDetalleDtoAjax>();
            foreach (var detalle in presupuesto.PresupuestoDetalles)
            {
                detalles.Add(FacturaDetalleDtoAjax.DtoFromModel(
                    new FacturaDetalle {
                        FacturaId = factura.Id,
                        Cantidad = detalle.Cantidad,
                        Precio = detalle.Precio,
                        Item = detalle.Item
                    }));
            }
            detalles.Sort((x, y) => x.Id.CompareTo(y.Id) * -1);
            Session["factura_detalles"] = detalles;

            var clientesFiltrados = db.Clientes
                .Where(c => c.MonotributistaId == Monotributista.IdUsuarioActual)
                .Where(c => c.Activo)
                .Select(c => new SelectListItem { Text = c.Nombre, Value = c.Id.ToString() });
            ViewData["Clientes"] = new SelectList(clientesFiltrados, "Value","Text");

            ViewBag.FormaPagoId = new SelectList(db.FormasPago, "Id", "Nombre");

            return View("CreateFromPresupuesto", factura);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Factura factura)
        {
            if (ModelState.IsValid)
            {
                List<FacturaDetalleDtoAjax> detalles = (List<FacturaDetalleDtoAjax>)Session["factura_detalles"] ?? new List<FacturaDetalleDtoAjax>();
                factura.FacturaDetalles = detalles.Select(FacturaDetalleDtoAjax.ModelFromDto).ToList();

                Cliente clienteSeleccionado = db.Clientes.Find(factura.ClienteId);
                factura.FechaVencimiento = factura.Fecha.AddDays(factura.DiasPagoFactura);
                factura.MonotributistaId = Monotributista.IdUsuarioActual;
                factura.NombreArchivo = factura.MonotributistaId + "_Fact" + factura.Numero.ToString("0000") + ".pdf";

                factura.EstadoActual = (int)Estado.Valores.Emitida;

                db.Comprobantes.Add(factura);
                db.SaveChanges();

                clienteSeleccionado.TotalFacturado += factura.Importe;

                #region Notificación y Bitácora
                Notificacion.CrearNotificacionFactura(factura);

                var bitacora = new Bitacora
                {
                    EstadoId = factura.EstadoActual,
                    ComprobanteId = factura.ComprobanteId,
                    Fecha = DateTime.Today
                };

                factura.Comprobante.Bitacoras.Add(bitacora);

                db.Entry(factura).State = EntityState.Modified;
                db.SaveChanges();
                #endregion

                Session.Remove("factura_detalles");

                return RedirectToAction("Index");
            }

            ViewBag.ClienteId = new SelectList(db.Clientes, "Id", "Nombre", factura.ClienteId);
            //ViewBag.MonotributistaId = new SelectList(db.Monotributistas, "Id", "CUIT", factura.MonotributistaId);
            //ViewBag.ComprobanteId = new SelectList(db.Comprobantes, "Id", "EstadoActual", factura.ComprobanteId);
            ViewBag.FormaPagoId = new SelectList(db.FormasPago, "Id", "Nombre", factura.FormaPagoId);
            ViewBag.PresupuestoId = new SelectList(db.Comprobantes, "Id", "Numero", factura.PresupuestoId);
            //ViewBag.RecorridoCobranzaId = new SelectList(db.RecorridosCobranzas, "Id", "Descripcion", factura.RecorridoCobranzaId);
            return View(factura);
        }

        // GET: /Factura/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factura factura = db.Facturas.Find(id);
            if (factura == null)
            {
                return HttpNotFound();
            }
            
            var estadosPermitidos = new string[] { };
            
            switch (factura.EstadoActual)
            {
                case (int)Estado.Valores.Cobrada:
                    return View("Details", factura);
                case (int)Estado.Valores.Anulada:
                    return View("Details", factura);
                case (int)Estado.Valores.Emitida:
                    estadosPermitidos = new string[] { Estado.Valores.Emitida.ToString(), Estado.Valores.Cobrada.ToString(), Estado.Valores.Anulada.ToString() };
                    break;
                case (int)Estado.Valores.EmitidaMobile:
                    estadosPermitidos = new string[] { Estado.Valores.EmitidaMobile.ToString(), Estado.Valores.Cobrada.ToString(), Estado.Valores.Anulada.ToString() };
                    break;
                case (int)Estado.Valores.Enviada:
                    estadosPermitidos = new string[] { Estado.Valores.Enviada.ToString(), Estado.Valores.Cobrada.ToString(), Estado.Valores.Anulada.ToString() };
                    break;
                case (int)Estado.Valores.Vencida:
                    estadosPermitidos = new string[] { Estado.Valores.Vencida.ToString(), Estado.Valores.Cobrada.ToString(), Estado.Valores.Anulada.ToString() };
                    break;
                default:
                    break;
            }

            IEnumerable<SelectListItem> estadosFiltrados = db.Estados
                .Where(x => estadosPermitidos.Contains(x.Nombre))
                .Select(c => new SelectListItem { Selected = (c.Id.ToString() == factura.EstadoActual.ToString()), Text = c.Nombre, Value = c.Id.ToString()});
            ViewData["Estados"] = new SelectList(estadosFiltrados, "Value", "Text");
            
            return View(factura);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Factura factura)
        {
            if (ModelState.IsValid) 
            {
                if(factura.EstadoActual == (int)Estado.Valores.Cobrada && factura.FechaCobro == null)
                {
                    factura.FechaCobro = DateTime.Today;
                }

                db.Entry(factura).State = EntityState.Modified;
                db.SaveChanges();

                #region Notificación y Bitácora
                Notificacion.CrearNotificacionFactura(factura);

                var bitacora = new Bitacora
                {
                    EstadoId = factura.EstadoActual,
                    ComprobanteId = factura.ComprobanteId,
                    Fecha = DateTime.Today
                };

                db.Bitacoras.Add(bitacora);
                db.SaveChanges();
                #endregion

                return RedirectToAction("Index");
            }

            ViewBag.EstadoActual = new SelectList(db.Estados, "Id", "Nombre", factura.EstadoActual);

            return View(factura);
        }

        // GET: /Factura/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factura factura = db.Facturas.Find(id);
            if (factura == null)
            {
                return HttpNotFound();
            }
            return View(factura);
        }

        // POST: /Factura/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Factura factura = db.Facturas.Find(id);

            factura.EstadoActual = (int) Estado.Valores.Anulada;

            #region Notificación y Bitácora
            Notificacion.CrearNotificacionFactura(factura);

            var bitacora = new Bitacora
            {
                EstadoId = factura.EstadoActual,
                ComprobanteId = factura.ComprobanteId,
                Fecha = DateTime.Today
            };

            db.Bitacoras.Add(bitacora);
            db.SaveChanges();
            #endregion

            db.Entry(factura).State = EntityState.Modified;
            db.SaveChanges();


            return RedirectToAction("Index");
        }

        //public ActionResult GetPdfNoMostrar(long? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Factura factura = db.Facturas.Find(id);
        //    if (factura == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    Cliente clienteSeleccionado = db.Clientes.Find(factura.ClienteId);
        //    var builder = new FacturaPdfBuilder(factura);
        //    builder.GetPdf();

        //    return RedirectToAction("Index");
        //}

        public ActionResult GetPdf(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factura factura = db.Facturas.Find(id);
            if (factura == null)
            {
                return HttpNotFound();
            }
            //Cliente clienteSeleccionado = db.Clientes.Find(factura.ClienteId);
            var builder = new FacturaPdfBuilder(factura);
            return builder.GetPdf();
        }

        public ActionResult ImprimirPantalla(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factura factura = db.Facturas.Find(id);
            if (factura == null)
            {
                return HttpNotFound();
            }
            return View(factura);
        }

        // GET: Factura/Enviar/5
        public ActionResult Enviar(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factura factura = db.Facturas.Find(id);
            if (factura == null)
            {
                return HttpNotFound();
            }
            Cliente clienteSeleccionado = db.Clientes.Find(factura.ClienteId);
            Comprobante comprobante = db.Comprobantes.Find(factura.Id);
            var email = new Email
            {
                From = factura.Monotributista.Email,
                To = clienteSeleccionado.Email,
                Subject = "Factura C#" + factura.Numero.ToString("0000"),
                ComprobanteId = comprobante.Id,
                Attachments = 1
            };
            email.Comprobante = comprobante;
            return View(email);
        }

        // POST: Factura/Enviar/5
        [HttpPost, ValidateInput(false), ActionName("Enviar")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Enviar(Email email)
        {
            if (ModelState.IsValid)
            {
                email.Html = email.Text;

                Comprobante comprobante = db.Comprobantes.Find(email.ComprobanteId);
                if (comprobante == null)
                {
                    return HttpNotFound();
                }

                Factura factura = db.Facturas.Find(comprobante.Id);
                if (factura == null)
                {
                    return HttpNotFound();
                }

                var builder = new FacturaPdfSender(factura, email);
                await builder.GetPdf();

                #region Notificación y Bitácora
                factura.EstadoActual = (int)Estado.Valores.Enviada;

                db.Entry(factura).State = EntityState.Modified;
                db.SaveChanges();

                Notificacion.CrearNotificacionFactura(factura);

                var bitacora = new Bitacora
                {
                    EstadoId = factura.EstadoActual,
                    ComprobanteId = factura.ComprobanteId,
                    Fecha = DateTime.Today
                };

                db.Bitacoras.Add(bitacora);
                db.SaveChanges();
                #endregion

                db.Emails.Add(email);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(email);
        }

        [Route("Factura/AddDetalle/")]
        [HttpGet]
        public async Task<ActionResult> AddDetalle(short cantidad, decimal precio, string descripcion)
        {
            var detalles = Session["factura_detalles"] as List<FacturaDetalleDtoAjax>;
            detalles.Add(new FacturaDetalleDtoAjax() { Item = descripcion, Cantidad = cantidad, Precio = precio });
            Session["factura_detalles"] = detalles;

            return Json(detalles, JsonRequestBehavior.AllowGet);
        }
        [Route("Factura/GetDetalles/")]
        [HttpGet]
        public async Task<ActionResult> GetDetalles()
        {
            var detalles = Session["factura_detalles"] as List<FacturaDetalleDtoAjax>;

            return Json(detalles, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult CalcultarTotalGeneral()
        {
            decimal total = 0;
            //List<FacturaDetalleDtoAjax> detalles = (List<FacturaDetalleDtoAjax>)Session["factura_detalles"] ?? new List<FacturaDetalleDtoAjax>();
            var detalles = Session["factura_detalles"] as List<FacturaDetalleDtoAjax>;

            foreach (var detalle in detalles)
            {
                total += detalle.Precio * detalle.Cantidad;
            }

            return Json(new { total = total.ToString("#.00") });
        }

        public async Task<ActionResult> EnviarSms(long? id)
        {

            Comprobante comprobante = db.Comprobantes.Find(id);
            if (comprobante == null)
            {
                return HttpNotFound();
            }

            await EnvioSMS.SendAsync(comprobante);


            db.Entry(comprobante).State = EntityState.Modified;
            db.SaveChanges();

            return View(comprobante);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
