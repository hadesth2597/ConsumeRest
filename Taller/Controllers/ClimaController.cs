using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Taller.RestModel;

namespace Taller.Controllers
{
    public class ClimaController : Controller
    {

        private TallerModel rest = new TallerModel();
        // GET: Clima
        public ActionResult Index()
        {
            var api = "https://query.yahooapis.com/v1/public/yql?q=select%20item.condition%20from%20weather.forecast%20where%20woeid%20%3D%202487889&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";
            var model = rest.GetClima(api); 
            return View(model);
        }
    }
}