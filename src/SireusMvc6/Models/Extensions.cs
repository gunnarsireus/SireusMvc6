using System.Reflection;

namespace SireusMvc6.Models
{
    public static class Extensions
    {
        public static T Clone<T>(this T obj) where T :class, new()
        {
            T ReturnValue = new T();
            PropertyInfo[] sourceProperties = obj.GetType().GetProperties();

            foreach (PropertyInfo sourceProp in sourceProperties)
            {
                if (sourceProp.CanWrite)
                {
                    sourceProp.SetValue(ReturnValue, sourceProp.GetValue(obj, null), null);
                }
            }
            return ReturnValue;
        }
    }
}
