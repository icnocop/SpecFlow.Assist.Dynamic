using SpecFlow.Assist.Dynamic.PropertyNameMapping;
using SpecFlow.Assist.Dynamic.PropertyValueParser;

namespace SpecFlow.Assist.Dynamic
{
    public class Options
    {
        public Options()
        {
            this.DoTypeConversion = true;
            this.PropertyNameMapper = new DefaultPropertyNameMapper();
            this.PropertyValueParser = new DefaultPropertyValueParser();
        }

        /// <summary>
        /// Gets or sets a value indicating whether types should be converted
        /// according to conventions described in https://github.com/marcusoftnet/SpecFlow.Assist.Dynamic/wiki/Conversion-conventions#property-type-conversions
        /// </summary>
        /// <value><c>true</c> if types should be converted; otherwise, <c>false</c>.</value>
        public bool DoTypeConversion { get; set; }

        public IPropertyNameMapper PropertyNameMapper { get; set; }

        public IPropertyValueParser PropertyValueParser { get; set; }
    }
}
