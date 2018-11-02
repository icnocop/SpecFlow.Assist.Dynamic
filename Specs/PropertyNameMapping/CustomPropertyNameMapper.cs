using SpecFlow.Assist.Dynamic.PropertyNameMapping;

namespace Specs.PropertyNameMapping
{
    public class CustomPropertyNameMapper : IPropertyNameMapper
    {
        public string Map(string header)
        {
            return header.Replace(" ", string.Empty);
        }
    }
}
