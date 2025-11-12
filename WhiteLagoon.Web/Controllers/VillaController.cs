using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.common.interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Infrastructure.Repository;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public VillaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            //var villas = _db.villas.ToList();
            var villas = _unitOfWork.Villa.GetAll();
            return View(villas);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Villa obj)
        {
            if (obj.Name == obj.Description) 
            {
                ModelState.AddModelError("name","The description cannot exactly match the Name.");
            }
            if (ModelState.IsValid)
            {
                //_db.villas.Add(obj);
                //_db.SaveChanges();
                //TempData["success"] = "The villa has been created successfully";
                //return RedirectToAction(nameof(Index));
                //_villaRepo.Add(obj);
                //_villaRepo.Save();
                _unitOfWork.Villa.Add(obj);
                _unitOfWork.Save();

                TempData["success"] = "The villa has been created successfully";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Update(int villaId)
        {
            //Villa? obj = _db.villas.FirstOrDefault(u => u.Id == villaId);


            //Villa? obj = _db.villas.Find(villaId);
            //var VillaList = _db.villas.Where(u => u.Price > 50 && u.Occupancy > 0);

            //Villa? obj = _villaRepo.Get(u => u.Id == villaId);
            Villa? obj = _unitOfWork.Villa.Get(u => u.Id == villaId);

            if (obj == null)
            {
                //return NotFound()
                return RedirectToAction("Error","Home");
            }
            return View(obj);
        }

        [HttpPost]
        public IActionResult Update(Villa obj) 
        {

            if (ModelState.IsValid && obj.Id>0)
            {
                //    _db.villas.Update(obj);
                //    _db.SaveChanges();
                //_villaRepo.Update(obj);
                // _villaRepo.Save();
                _unitOfWork.Villa.Update(obj);
                _unitOfWork.Save();

                TempData["success"] = "The villa has been updated.";
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }

        public IActionResult Delete(int villaid)
        {
            //Villa? obj = _db.villas.FirstOrDefault(u=> u.Id == villaid);
            //Villa? obj = _villaRepo.Get(u => u.Id == villaid);
            Villa? obj = _unitOfWork.Villa.Get(u => u.Id == villaid);
            if (obj is  null)
            {
                return RedirectToAction("Error","Home");
            }
            return View(obj);
        }
        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
            //Villa? objFromDb = _db.villas.FirstOrDefault(u => u.Id == obj.Id);
            //if(ModelState.IsValid && obj.Id > 0)
            //{
            //    _db.villas.Remove(objFromDb);
            //    _db.SaveChanges();
            //    return RedirectToAction("Index");
            //}
            //Villa? objFromDb = _villaRepo.Get(u => u.Id == obj.Id);
            Villa? objFromDb = _unitOfWork.Villa.Get(u => u.Id == obj.Id);
            if (objFromDb is not null)
            {
                //_db.villas.Remove(objFromDb);
                //_db.SaveChanges();
                //_villaRepo.Remove(objFromDb);
                //_villaRepo.Save();
                _unitOfWork.Villa.Remove(objFromDb);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa could not be deleted.";
            return View();
        }
    }
}
