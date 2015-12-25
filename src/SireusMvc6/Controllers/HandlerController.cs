using SireusMvc6.Models;
using System;
using System.IO;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;

namespace SireusMvc6.Controllers
{
    public class HandlerController : Controller
    {
        //
        // GET: /Images/

        public ActionResult Index(string arg1, string arg2)
        {
            PhotoSize size;
            switch (arg2.Replace("Size=", ""))
            {
                case "S":
                    size = PhotoSize.Small;
                    break;
                case "M":
                    size = PhotoSize.Medium;
                    break;
                case "L":
                    size = PhotoSize.Large;
                    break;
                default:
                    size = PhotoSize.Original;
                    break;
            }

            if (!Startup.Session.ContainsKey("PhotoID"))
            {
                Startup.Session.Add("PhotoID", arg1.Replace("PhotoID=", ""));
            }
            else {
                Startup.Session["PhotoID"]=arg1.Replace("PhotoID=", "");
            }

            if (arg1 == "PhotoID=0")
            {
                var tmpPhotoId = PhotoManager.GetRandomPhotoId(PhotoManager.GetRandomAlbumId());
                arg1 = "PhotoID=" + tmpPhotoId;
                Startup.Session["PhotoID"] = tmpPhotoId.ToString();
            }
            // Setup the PhotoID Parameter
            var id = 1;
            var stream = new MemoryStream();

            if (arg1.Substring(0, 7) == "PhotoID")
            {
                id = Convert.ToInt32(arg1.Replace("PhotoID=", ""));
                PhotoManager.GetPhoto(id, size).CopyTo(stream);
            }
            else
            {
                id = Convert.ToInt32(arg1.Replace("AlbumID=", ""));
                PhotoManager.GetFirstPhoto(id, size).CopyTo(stream);
            }

            return File(stream.GetBuffer(), "image/png");
        }

        public ActionResult Download(string arg1, string arg2)
        {
            if (Startup.Session.ContainsKey("PhotoID"))
            {
                ViewData["PhotoID"] = Startup.Session["PhotoID"];
            }
            else
            {
                ViewData["PhotoID"] = PhotoManager.GetRandomPhotoId(PhotoManager.GetRandomAlbumId()).ToString();
            }
            ViewData["Size"] = "L";
            return View();
        }
    }
}