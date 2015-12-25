namespace SireusMvc6.Models
{
    public class Album
    {
        private readonly string _caption;
        private readonly int _count;

        private readonly int _id;

        private readonly bool _ispublic;

        public Album(int id, int count, string caption, bool ispublic)
        {
            _id = id;
            _count = count;
            _caption = caption;
            _ispublic = ispublic;
        }

        public int AlbumID
        {
            get { return _id; }
        }

        public int Count
        {
            get { return _count; }
        }

        public string Caption
        {
            get { return _caption; }
        }

        public bool IsPublic
        {
            get { return _ispublic; }
        }
    }

    public class VacationGadget
    {
        private readonly int _obscurecount;

        public VacationGadget(int obscurecount)
        {
            _obscurecount = obscurecount;
        }

        public int ObscureCount
        {
            get { return _obscurecount; }
        }
    }
}