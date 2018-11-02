namespace SpecFlow.Assist.Dynamic.PropertyNameMapping
{
    public class DefaultPropertyNameMapper : IPropertyNameMapper
    {
        public string Map(string header)
        {
            var words = header.Split(' ');
            var propName = words[0]; // leave the first word as is, since it might be correct cased...

            for (var i = 1; i < words.Length; i++)
            {
                var s = words[i];
                if (s.Length > 0)
                {
                    propName += s[0].ToString().ToUpperInvariant() +
                            s.Substring(1).ToLowerInvariant();
                }
            }

            return propName;
        }
    }
}
