using SpecFlow.Assist.Dynamic.PropertyValueParser;

namespace Specs.PropertyValueParser
{
    public class CustomPropertyValueParser : IPropertyValueParser
    {
        public object Parse(string value)
        {
            return 42;
        }
    }
}
