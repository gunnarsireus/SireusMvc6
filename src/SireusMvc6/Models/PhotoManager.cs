using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SireusMvc6.Models
{
    public class IdList
    {
        public IdList(int a)
        {
            AlbumId = a;
            Photolist = new List<Photo>();
        }

        public int AlbumId;
        public List<Photo> Photolist;
    }

    public class PhotoManager
    {
        public static List<IdList> GlobalAlbumList = new List<IdList>();
        public static Stream GetPhoto(int photoid, PhotoSize size)
        {
            using (var connection = new SqlConnection(Startup.DbConnection))
            {
                using (var command = new SqlCommand("GetPhoto", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PhotoID", photoid));
                    command.Parameters.Add(new SqlParameter("@Size", Convert.ToInt32(size)));
                    //var filter = !(HttpContext.Current.User.IsInRole("Friends") | HttpContext.Current.User.IsInRole("Administrators"));
                    command.Parameters.Add(new SqlParameter("@IsPublic", true));
                    connection.Open();
                    var result = (byte[])command.ExecuteScalar();
                    try
                    {
                        return new MemoryStream(result);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        public static Stream GetPhoto(PhotoSize size)
        {
            var rootPath = AppDomain.CurrentDomain.BaseDirectory;
            var path = rootPath + "//White/Images/";
            switch (size)
            {
                case PhotoSize.Small:
                    path = (path + "placeholder-100.jpg");
                    break;
                case PhotoSize.Medium:
                    path = (path + "placeholder-200.jpg");
                    break;
                case PhotoSize.Large:
                    path = (path + "placeholder-600.jpg");
                    break;
                default:
                    path = (path + "placeholder-600.jpg");
                    break;
            }
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public static Stream GetFirstPhoto(int albumid, PhotoSize size)
        {
            using (var connection = new SqlConnection(Startup.DbConnection))
            {
                using (var command = new SqlCommand("GetFirstPhoto", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@AlbumID", albumid));
                    command.Parameters.Add(new SqlParameter("@Size", Convert.ToInt32(size)));
                    //var filter = !(HttpContext.Current.User.IsInRole("Friends") | HttpContext.Current.User.IsInRole("Administrators"));
                    command.Parameters.Add(new SqlParameter("@IsPublic", true));
                    connection.Open();
                    var result = (byte[])command.ExecuteScalar();
                    try
                    {
                        return new MemoryStream(result);
                    }
                    catch (ArgumentNullException e)
                    {
                        return null;
                    }
                }
            }
        }

        public static List<Photo> GetPhotos(int albumId)
        {

            foreach (var a in GlobalAlbumList.Where(a => a.AlbumId == albumId))
            {
                return a.Photolist;
            }

            var album = new IdList(albumId); GlobalAlbumList.Add(album);

            using (var connection = new SqlConnection(Startup.DbConnection))
            {
                using (var command = new SqlCommand("GetPhotos", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@AlbumID", albumId));
                    //var filter = !(HttpContext.Current.User.IsInRole("Friends") | HttpContext.Current.User.IsInRole("Administrators"));
                    command.Parameters.Add(new SqlParameter("@IsPublic", true));
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while ((reader.Read()))
                        {
                            var temp = new Photo(Convert.ToInt32(reader["PhotoID"]), Convert.ToInt32(reader["AlbumID"]), Convert.ToString(reader["Caption"]));
                            //list.Add(temp);
                            album.Photolist.Add(temp);
                        }
                    }
                    return album.Photolist;
                }
            }
        }

        public static List<Photo> GetPhotos()
        {
            return GetPhotos(GetRandomAlbumId());
        }

        // Album-Related Methods

        public static void AddPhoto(int albumId, string caption, byte[] bytesOriginal)
        {
            using (var connection = new SqlConnection(Startup.DbConnection))
            {
                using (var command = new SqlCommand("AddPhoto", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@AlbumID", albumId));
                    command.Parameters.Add(new SqlParameter("@Caption", caption));
                    command.Parameters.Add(new SqlParameter("@BytesOriginal", bytesOriginal));
                    command.Parameters.Add(new SqlParameter("@BytesFull", ResizeImageFile(bytesOriginal, 600)));
                    command.Parameters.Add(new SqlParameter("@BytesPoster", ResizeImageFile(bytesOriginal, 198)));
                    command.Parameters.Add(new SqlParameter("@BytesThumb", ResizeImageFile(bytesOriginal, 100)));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void RemovePhoto(int photoId)
        {
            using (var connection = new SqlConnection(Startup.DbConnection))
            {
                using (var command = new SqlCommand("RemovePhoto", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@PhotoID", photoId));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void EditPhoto(string caption, int photoId)
        {
            using (var connection = new SqlConnection(Startup.DbConnection))
            {
                using (var command = new SqlCommand("EditPhoto", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Caption", caption));
                    command.Parameters.Add(new SqlParameter("@PhotoID", photoId));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<Album> GetAlbums()
        {
            using (var connection = new SqlConnection(Startup.DbConnection))
            {
                using (var command = new SqlCommand("GetAlbums", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    //var filter = !(HttpContext.Current.User.IsInRole("Friends") | HttpContext.Current.User.IsInRole("Administrators"));
                    command.Parameters.Add(new SqlParameter("@IsPublic", true));
                    connection.Open();
                    List<Album> list = new List<Album>();
                    using (var reader = command.ExecuteReader())
                    {
                        while ((reader.Read()))
                        {
                            var temp = new Album(Convert.ToInt32(reader["AlbumID"]), Convert.ToInt32(reader["NumberOfPhotos"]), Convert.ToString(reader["Caption"]), Convert.ToBoolean(reader["IsPublic"]));
                            list.Add(temp);
                        }
                    }
                    return list;
                }
            }
        }

        public static void AddAlbum(string caption, bool isPublic)
        {
            using (var connection = new SqlConnection(Startup.DbConnection))
            {
                using (var command = new SqlCommand("AddAlbum", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Caption", caption));
                    command.Parameters.Add(new SqlParameter("@IsPublic", isPublic));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void RemoveAlbum(int albumId)
        {
            using (var connection = new SqlConnection(Startup.DbConnection))
            {
                using (var command = new SqlCommand("RemoveAlbum", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@AlbumID", albumId));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void EditAlbum(string caption, bool isPublic, int albumId)
        {
            using (var connection = new SqlConnection(Startup.DbConnection))
            {
                using (var command = new SqlCommand("EditAlbum", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Caption", caption));
                    command.Parameters.Add(new SqlParameter("@IsPublic", isPublic));
                    command.Parameters.Add(new SqlParameter("@AlbumID", albumId));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static int GetRandomAlbumId()
        {
            using (var connection = new SqlConnection(Startup.DbConnection))
            {
                using (var command = new SqlCommand("GetNonEmptyAlbums", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    var list = new List<Album>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var temp = new Album(Convert.ToInt32(reader["AlbumID"]), 0, "", false);
                            //list.Add(temp);
                            var plist = GetPhotos(temp.AlbumID);
                            list.AddRange(plist.Select(p => temp));
                        }
                    }
                    try
                    {
                        var r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                        return list[r.Next(list.Count)].AlbumID;
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        return -1;
                    }
                }
            }
        }
        public static int GetRandomPhotoId(int albumId)
        {
            var list = GetPhotos(albumId);
            try
            {
                var r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                return list[r.Next(list.Count)].PhotoID;
            }
            catch (ArgumentOutOfRangeException e)
            {
                return -1;
            }
        }

        public static int GetPageFromPhotoIdAlbumId(int pid, int aid)
        {
            var list = GetPhotos(aid);
            var cnt = 0;
            var pageId = -2;
            while (cnt < list.Count)
            {
                if (list[cnt].PhotoID == pid) { pageId = cnt; cnt = list.Count; }
                cnt++;
            }
            return pageId + 1;
        }

        // Auxiliary Functions

        private static byte[] ResizeImageFile(byte[] imageFile, int targetSize)
        {
            using (System.Drawing.Image oldImage = System.Drawing.Image.FromStream(new MemoryStream(imageFile)))
            {
                var newSize = CalculateDimensions(oldImage.Size, targetSize);
                using (var newImage = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format24bppRgb))
                {
                    using (var canvas = Graphics.FromImage(newImage))
                    {
                        canvas.SmoothingMode = SmoothingMode.AntiAlias;
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        canvas.DrawImage(oldImage, new Rectangle(new Point(0, 0), newSize));
                        var m = new MemoryStream();
                        newImage.Save(m, ImageFormat.Jpeg);
                        return m.GetBuffer();
                    }
                }
            }
        }

        private static Size CalculateDimensions(Size oldSize, int targetSize)
        {
            var newSize = default(Size);
            if ((oldSize.Height > oldSize.Width))
            {
                newSize.Width = Convert.ToInt32((oldSize.Width * Convert.ToSingle((targetSize / Convert.ToSingle(oldSize.Height)))));
                newSize.Height = targetSize;
            }
            else
            {
                newSize.Width = targetSize;
                newSize.Height = Convert.ToInt32((oldSize.Height * Convert.ToSingle((targetSize / Convert.ToSingle(oldSize.Width)))));
            }
            return newSize;
        }

        //public static ICollection ListUploadDirectory()
        //{
        //    var d = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/Upload"));
        //    return d.GetFileSystemInfos("*.jpg");
        //}

    }
}