using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AssetManagementWeb.Models;
using AssetManagementWeb.Utilities;
using Newtonsoft.Json;

namespace AssetManagementWeb.Controllers
{
    public class AssetController : Controller
    {
        // GET: Asset
        public ActionResult Index()
        {
            return View();
        }

  public ActionResult Test()
        {
            List<LocatedAssetsViewModel> model = new List<LocatedAssetsViewModel>();

            AssetsEntities entities = new AssetsEntities();
            try
            {
                List<AssetLocations> assets = entities.AssetLocations.ToList();

                CultureInfo fiFi = new CultureInfo("fi-FI");

                // muodostetaan näkymämalli tietokannan rivien pohjalta
                foreach (AssetLocations asset in assets)
                {
                    LocatedAssetsViewModel view = new LocatedAssetsViewModel();
                    view.Id = asset.Id;
                    view.LocationCode = asset.AssetLocation.Code;
                    view.LocationName = asset.AssetLocation.Name;
                    view.LocationAdress = asset.AssetLocation.Adress;
                    view.AssetCode = asset.Assets.Code;
                    view.AssetName = asset.Assets.Type + ": " + asset.Assets.Model;
                    view.LastSeen = asset.LastSeen.Value.ToString(fiFi);
                    //view.LastSeen = asset.LastSeen;

                    model.Add(view);
                }
            }
            finally
            {
                entities.Dispose();
            }

            return View(model);
        }



        // GET: Asset/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Asset/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AssignLocation()
        {
            string json = Request.InputStream.ReadToEnd();
            AssignLocationModel inputData =
                JsonConvert.DeserializeObject<AssignLocationModel>(json);
            
            bool success = false;
            string error = "";
            AssetsEntities entities = new AssetsEntities();

            try
            {
                // haetaan ensin paikan id-numero koodin perusteella
                int locationId = (from l in entities.AssetLocation
                                  where l.Code == inputData.LocationCode
                                  select l.Id).FirstOrDefault();

                // haetaan laitteen id-numero koodin perusteella
                int assetId = (from a in entities.Assets
                               where a.Code == inputData.AssetCode
                               select a.Id).FirstOrDefault();

                if ((locationId > 0) && (assetId > 0))
                {
                    // tallennetaan uusi rivi aikaleiman kanssa kantaan
                    AssetLocations newEntry = new AssetLocations();
                    newEntry.LocationId = locationId;
                    newEntry.AssetId = assetId;
                    newEntry.LastSeen = DateTime.Now;

                    entities.AssetLocations.Add(newEntry);
                    entities.SaveChanges();

                    success = true;
                }
            }
            catch (Exception ex)
            {
                error = ex.GetType().Name + ": " + ex.Message;
            }
            finally
            {
                entities.Dispose();
            }

            // palautetaan JSON-muotoinen tulos kutsujalle
            var result = new { success = success, error = error };
            return Json(result);
        }
    

    }
}
