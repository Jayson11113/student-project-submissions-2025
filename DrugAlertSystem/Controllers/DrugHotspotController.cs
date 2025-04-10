using Microsoft.AspNetCore.Mvc;
using DrugAlertSystem.Models;
using DrugAlertSystem.Services;

namespace DrugAlertSystem.Controllers
{
    public class DrugHotspotController : Controller
    {
        private readonly DrugHotspotPredictionService _predictionService;

        public DrugHotspotController(DrugHotspotPredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        public IActionResult Predict()
        {
            return View(new DrugHotspotPredictionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Predict(DrugHotspotPredictionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var data = new DrugHotspotData
            {
                Location = model.Location,
                PeopleLoitering = model.PeopleLoitering,
                DrugWrappersFound = model.DrugWrappersFound,
                StrongSmell = model.StrongSmell,
                LoudNoiseOrMusic = model.LoudNoiseOrMusic,
                ShoeHangingOnWire = model.ShoeHangingOnWire,
                PeopleInAndOut = model.PeopleInAndOut
            };

            var (isHotspot, probability, score) = _predictionService.GetPredictionResult(data);

            var result = new DrugHotspotPredictionResultViewModel
            {
                Input = model,
                IsHotspot = isHotspot,
                Probability = probability,
                Score = score
            };

            return View("Result", result);
        }

        public IActionResult Result(DrugHotspotPredictionResultViewModel model)
        {
            return View(model);
        }
    }
}