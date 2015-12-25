using System;
using Microsoft.AspNet.Mvc;
using SireusMvc6.Models;

namespace SireusMvc6.Controllers
{
    public class DetailsController : Controller
    {
        //
        // GET: /Details/

        public ActionResult Index(string arg1, string arg2)
        {
            ViewData["ShowPage"] = "Home";
            ViewData["PhotoID"] = arg1;
            ViewData["AlbumID"] = arg2;
            var photolist = PhotoManager.GetPhotos(Convert.ToInt32(ViewData["AlbumID"]));
            ViewBag.Page = PhotoManager.GetPageFromPhotoIdAlbumId(Convert.ToInt32(ViewData["PhotoID"]),
                Convert.ToInt32(ViewData["AlbumID"]));
            if ((photolist.Count == 0) | (ViewBag.Page == -1))
            {
                return Redirect("/");
            }
            ViewBag.List = photolist;
            ViewBag.Page = PhotoManager.GetPageFromPhotoIdAlbumId(Convert.ToInt32(ViewData["PhotoID"]),
                Convert.ToInt32(ViewData["AlbumID"]));
            return View();
        }

        public ActionResult SetDetails(string arg1, string arg2)
        {
            ViewData["ShowPage"] = "Home";
            ViewData["PhotoID"] = arg1;
            ViewData["AlbumID"] = arg2;
            var photolist = PhotoManager.GetPhotos(Convert.ToInt32(ViewData["AlbumID"]));
            ViewBag.Page = PhotoManager.GetPageFromPhotoIdAlbumId(Convert.ToInt32(ViewData["PhotoID"]),
                Convert.ToInt32(ViewData["AlbumID"]));
            if ((photolist.Count == 0) | (ViewBag.Page == -1))
            {
                return Redirect("/");
            }
            ViewBag.List = photolist;
            ViewBag.Page = PhotoManager.GetPageFromPhotoIdAlbumId(Convert.ToInt32(ViewData["PhotoID"]),
                Convert.ToInt32(ViewData["AlbumID"]));
            return View("Index");
        }
    }
}