using System;

namespace SpecFlow.Assist.Dynamic.PropertyValueParser
{
    public class DefaultPropertyValueParser : IPropertyValueParser
    {
        public object Parse(string value)
        {
            int i;
            if (int.TryParse(value, out i))
                return i;

            double db;
            if (double.TryParse(value, out db))
            {
                decimal d;
                if (decimal.TryParse(value, out d) && d.Equals((decimal)db))
                {
                    return db;
                }
                return d;
            }

            bool b;
            if (bool.TryParse(value, out b))
                return b;

            DateTime dt;
            if (DateTime.TryParse(value, out dt))
                return dt;

            return value;
        }
    }
}
