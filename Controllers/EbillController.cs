using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Unoamuchos.Models;
using Unoamuchos.Repository.Contract;

namespace Unoamuchos.Controllers
{
    public class EbillController : Controller
    {
        private readonly ILogger<EbillController> _logger;
        private readonly IData _data;

        public List<Items> listaItems = new List<Items>();

        public EbillController(ILogger<EbillController> logger, IData data)
        {
            _logger = logger;

            _data = data;
        }

        public async Task<IActionResult> Index()
        {
            List<BillDetail> _list = await _data.GetAllDetail();

            return View(_list);
        }

        public IActionResult View(int id)
        {
            try
            {
                var billdata = _data.View(id);

                if (billdata.CustomerName == null)
                {
                    TempData["ErrorMessage"] = "Bills details not available with the bill Id : " + id;
                    return RedirectToAction("Index");
                }
                return View(billdata);
            }

            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View();
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(BillDetail details)
        {
            try
            {

                bool result = await _data.Save(details);

                if (result)
                    //return StatusCode(StatusCodes.Status200OK, new { valor = result, msg = "ok" });
                    return RedirectToAction("Index", new { valor = result, msg = "ok" });
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new { valor = result, msg = "error" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la factura");
                return StatusCode(StatusCodes.Status500InternalServerError, new { valor = false, msg = "error" });
            }
        }

        [HttpPost]
        public IActionResult CreateItem(Items items)
        {
            var newItem = new Items
            {
                ProductName = items.ProductName,
                Price = items.Price,
                Quantity = items.Quantity,
                ItemIndex = items.ItemIndex
            };

            //System.Console.WriteLine("Item index es: " + newItem.ItemIndex);
            
            return PartialView("_CreateItem", newItem);
        }

        public IActionResult Edit(int id)
        {
            try
            {
                var billdata = _data.Edit(id);

                if (billdata.CustomerName == null)
                {
                    TempData["ErrorMessage"] = "Bills details not available with the bill Id : " + id;
                    return RedirectToAction("Index");
                }

                return View(billdata);
            }

            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View();
            }
        }

        /*
        public async Task<IActionResult> GetItems(int id)
        {
            List<Items> _lista = await _data.GetItems(id);

            return Json(_lista);
        }
        */

        [HttpPost]
        public async Task<IActionResult> Edit(BillDetail details)
        {
            try
            {
                var billdata_edit = _data.Edit(details);
                
                if (billdata_edit.Id == null)
                {
                    TempData["ErrorMessage"] = "Bills details not available with the bill Id : ";
                    return RedirectToAction("Index");
                }

                List<BillDetail> _list = await _data.GetAllDetail();

                return View("Index", _list);
            }

            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View();
            }

        }

        [HttpPost]
        public async Task<IActionResult> EditItem(Items items)
        {
            var newEditItem = new Items
            {
                ProductName = items.ProductName,
                Price = items.Price,
                Quantity = items.Quantity,
                ItemIndex = items.ItemIndex
            };

            System.Console.WriteLine("Item index es: " + newEditItem.ItemIndex + " y el nombre del producto es: " + newEditItem.ProductName);

            return PartialView("_EditItem", newEditItem);
        }
    }
}
