using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using ImpromptuInterface;
using SpecFlow.Assist.Dynamic;

namespace TechTalk.SpecFlow.Assist
{
    public static class DynamicTableHelpers
    {
        private const string ERRORMESS_PROPERTY_DIFF_SET = "Properties differs between the table and the set";
        private const string ERRORMESS_INSTANCETABLE_FORMAT = "Can only create instances of tables with one row, or exactly 2 columns and several rows";
        private const string ERRORMESS_NOT_ON_TABLE = "The '{0}' value not present in the table, but on the instance";
        private const string ERRORMESS_NOT_ON_INSTANCE = "The '{0}' value not present on the instance, but in the table";
        private const string ERRORMESS_VALUE_DIFFERS =
            "The '{0}' value differs from table and instance.\n\tInstance:\t'{1}'(type: {2}).\n\tTable:\t\t'{3}'(type: {4})";

        private const string ERRORMESS_NUMBER_OF_ROWS_DIFFERS =
            "Number of rows for table ({0} rows) and set ({1} rows) differs";

        private const string ERRORMESS_SET_VALUES_DIFFERS =
            "A difference was found on row '{0}' for column '{1}' (property '{2}').\n\tInstance:\t'{3}'(type: {4}).\n\tTable:\t\t'{5}'(type: {6})";

        /// <summary>
        /// Create a dynamic object from the headers and values of the <paramref name="table" />
        /// </summary>
        /// <param name="table">the table to create a dynamic object from</param>
        /// <param name="options">The options.</param>
        /// <returns>the created object</returns>
        public static ExpandoObject CreateDynamicInstance(this Table table, Options options = null)
        {
            if (options == null)
            {
                options = new Options();
            }

            if (table.Header.Count == 2 && table.RowCount > 1)
            {
                var horizontalTable = CreateHorizontalTable(table);
                return CreateDynamicInstance(horizontalTable.Rows[0], options);
            }

            if (table.RowCount == 1)
            {
                return CreateDynamicInstance(table.Rows[0], options);
            }

            throw new DynamicInstanceFromTableException(ERRORMESS_INSTANCETABLE_FORMAT);
        }

        /// <summary>
        /// Creates a set of dynamic objects based of the <paramref name="table" /> headers and values
        /// </summary>
        /// <param name="table">the table to create a set of dynamics from</param>
        /// <param name="options">The options.</param>
        /// <returns>a set of dynamics</returns>
        public static IEnumerable<dynamic> CreateDynamicSet(this Table table, Options options = null)
        {
            if (options == null)
            {
                options = new Options();
            }

            return from r in table.Rows
                   select CreateDynamicInstance(r, options);
        }

        /// <summary>
        /// Validates if a dynamic instance <paramref name="instance" /> matches the <paramref name="table" />
        /// Throws descriptive exception if not
        /// </summary>
        /// <param name="table">the table to compare the instance against</param>
        /// <param name="instance">the instance to compare the table against</param>
        /// <param name="options">The options.</param>
        public static void CompareToDynamicInstance(this Table table, dynamic instance, Options options = null)
        {
            if (options == null)
            {
                options = new Options();
            }

            IList<string> propDiffs = GetPropertyDifferences(table, instance, options);
            if (propDiffs.Any())
                throw new DynamicInstanceComparisonException(propDiffs);

            AssertValuesOfRowDifference(table.Rows[0], instance, options);
        }

        /// <summary>
        /// Validates that the dynamic set <paramref name="set" /> matches the <paramref name="table" />
        /// Throws descriptive exception if not
        /// </summary>
        /// <param name="table">the table to compare the set against</param>
        /// <param name="set">the set to compare the table against</param>
        /// <param name="options">The options.</param>
        public static void CompareToDynamicSet(this Table table, IList<dynamic> set, Options options = null)
        {
            if (options == null)
            {
                options = new Options();
            }

            AssertEqualNumberOfRows(table, set);

            IList<string> propDiffs = GetPropertyDifferences(table, set[0], options);
            if (propDiffs.Any())
            {
                throw new DynamicSetComparisonException(ERRORMESS_PROPERTY_DIFF_SET, propDiffs);
            }

            // Now we know that the table and the list has the same number of rows and properties

            var valueDifference = GetSetValueDifferences(table, set, options);

            if (valueDifference.Any())
            {
                throw new DynamicSetComparisonException(ERRORMESS_PROPERTY_DIFF_SET, valueDifference);
            }
        }

        private static List<string> GetSetValueDifferences(Table table, IList<object> set, Options options)
        {
            var memberNames = Impromptu.GetMemberNames(set[0]);
            var valueDifference = new List<string>();

            for (var i = 0; i < set.Count; i++)
            {
                foreach (var memberName in memberNames)
                {
                    var currentHeader = string.Empty;
                    var rowValue = GetRowValue(i, table, memberName, out currentHeader, options);

                    if ((rowValue.GetType() == typeof(string)) && ((string)rowValue == "NULL"))
                    {
                        rowValue = null;
                    }

                    var instanceValue = Impromptu.InvokeGet(set[i], memberName);

                    if (((instanceValue != null) || (rowValue != null))
                        && !instanceValue.Equals((dynamic)rowValue))
                    {
                        var rowType = rowValue.GetType().Name;
                        var instanceType = instanceValue.GetType().Name;

                        var difference = string.Format(ERRORMESS_SET_VALUES_DIFFERS,
                                                       i + 1,
                                                       currentHeader,
                                                       memberName,
                                                       instanceValue,
                                                       instanceType,
                                                       rowValue,
                                                       rowType);

                        valueDifference.Add(difference);
                    }
                }
            }
            return valueDifference;
        }

        private static object GetRowValue(int rowIndex, Table table, string memberName, out string currentHeader, Options options)
        {
            object rowValue = null;
            currentHeader = string.Empty;
            foreach (var header in table.Header)
            {
                if (CreatePropertyName(header, options) == memberName)
                {
                    currentHeader = header;
                    rowValue = CreateTypedValue(table.Rows[rowIndex][header], options);
                    break;
                }
            }
            return rowValue;
        }

        private static void AssertValuesOfRowDifference(TableRow tableRow, dynamic instance, Options options)
        {
            IList<string> valueDiffs = ValidateValuesOfRow(tableRow, instance, options);
            if (valueDiffs.Any())
                throw new DynamicInstanceComparisonException(valueDiffs);
        }

        private static IList<string> GetPropertyDifferences(Table table, dynamic instance, Options options)
        {
            var tableHeadersAsPropertyNames = table.Header.Select(x => CreatePropertyName(x, options));
            IEnumerable<string> instanceMembers = Impromptu.GetMemberNames(instance);

            return GetPropertyNameDifferences(tableHeadersAsPropertyNames, instanceMembers);
        }

        private static void AssertEqualNumberOfRows(Table table, IList<object> set)
        {
            if (table.RowCount != set.Count)
            {
                var mess = string.Format(ERRORMESS_NUMBER_OF_ROWS_DIFFERS, table.RowCount, set.Count);
                throw new DynamicSetComparisonException(mess);
            }
        }

        private static IList<string> ValidateValuesOfRow(TableRow tableRow, dynamic instance, Options options)
        {
            var valueDiffs = new List<string>();

            foreach (var header in tableRow.Keys)
            {
                var propertyName = CreatePropertyName(header, options);
                var valueFromInstance = Impromptu.InvokeGet(instance, propertyName);
                var typeFromInstance = valueFromInstance.GetType().Name;
                var valueFromTable = CreateTypedValue(tableRow[header], options);
                var typeFromTable = valueFromTable.GetType().Name;

                if (!valueFromInstance.Equals(valueFromTable))
                {
                    var mess = string.Format(ERRORMESS_VALUE_DIFFERS, propertyName, valueFromInstance, typeFromInstance, valueFromTable, typeFromTable);
                    valueDiffs.Add(mess);
                }
            }
            return valueDiffs;
        }

        private static IList<string> GetPropertyNameDifferences(IEnumerable<string> tableHeadersAsPropertyNames, IEnumerable<string> instanceMembers)
        {
            var allMembersInTableButNotInInstance = tableHeadersAsPropertyNames.Except(instanceMembers);
            var allMembersInInstanceButNotInTable = instanceMembers.Except(tableHeadersAsPropertyNames);

            var diffs = new List<string>();

            diffs.AddRange(
                allMembersInInstanceButNotInTable.Select(
                    m => string.Format(ERRORMESS_NOT_ON_TABLE, m)));

            diffs.AddRange(
                allMembersInTableButNotInInstance.Select(
                    m => string.Format(ERRORMESS_NOT_ON_INSTANCE, m)));

            return diffs;
        }

        private static Table CreateHorizontalTable(Table verticalFieldValueTable)
        {
            var dic = verticalFieldValueTable.
                            Rows.ToDictionary(row => row[0], row => row[1]);

            var horizontalTable = new Table(dic.Keys.ToArray());
            horizontalTable.AddRow(dic);
            return horizontalTable;
        }

        private static ExpandoObject CreateDynamicInstance(TableRow tablerow, Options options)
        {
            dynamic expando = new ExpandoObject();
            var dicExpando = expando as IDictionary<string, object>;

            foreach (var header in tablerow.Keys)
            {
                var propName = CreatePropertyName(header, options);
                var propValue = CreateTypedValue(tablerow[header], options);
                dicExpando.Add(propName, propValue);
            }

            return expando;
        }

        private static object CreateTypedValue(string valueFromTable, Options options)
        {
            if (!options.DoTypeConversion)
                return valueFromTable;

            return options.PropertyValueParser.Parse(valueFromTable);
        }

        private static string CreatePropertyName(string header, Options options)
        {
            var cleanedHeader = RemoveReservedChars(header);
            var propName = options.PropertyNameMapper.Map(cleanedHeader);

            // Throw if no chars in string
            if (propName.Length != 0) return propName;

            var mess = string.Format("Property '{0}' only contains reserved C# characters", header);
            throw new DynamicInstanceFromTableException(mess);
        }

        private static string RemoveReservedChars(string orgPropertyName)
        {
            const string pattern = @"[^\w\s]";
            const string replacement = "";
            return Regex.Replace(orgPropertyName, pattern, replacement);
        }
    }
}
