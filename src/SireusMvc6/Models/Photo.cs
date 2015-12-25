namespace SireusMvc6.Models
{
    public class Photo
    {
        private readonly int _albumid;

        private readonly string _caption;

        private readonly int _id;

        public Photo(int id, int albumid, string caption)
        {
            _id = id;
            _albumid = albumid;
            _caption = caption;
        }

        public int PhotoID
        {
            get { return _id; }
        }

        public int AlbumID
        {
            get { return _albumid; }
        }

        public string Caption
        {
            get { return _caption; }
        }
    }
}