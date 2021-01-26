using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReportGenerator.FunDbApi
{
    public class FunDbQuery
    {
        public string Name { get; private set; }
        public string QueryTextWithoutParameterValues { get; private set; }
        //public List<string> ParameterNames { get; private set; }

        public bool IsLoaded { get; private set; }
        private object? _result;

        public object? Result
        {
            get
            {
                if (!IsLoaded) return null;
                return _result;
            }
        }

        public FunDbQuery(string name, string queryTextWithoutParameterValues)
        {
            Name = name;
            QueryTextWithoutParameterValues = queryTextWithoutParameterValues;
        }

        public async Task LoadDataAsync(Dictionary<string, object> queryParametersWithValues, string instanceName, string token)
        {
            var queryTextToRun = QueryTextWithoutParameterValues;
            dynamic? result = null;
            var apiConnector = new FunDbApiConnector(instanceName, token);
            if (queryTextToRun.StartsWith("/views/"))
                result = await apiConnector.LoadQueryNamed(queryTextToRun, queryParametersWithValues);
            else
                result = await apiConnector.LoadQueryAnonymous(queryTextToRun, queryParametersWithValues);
            if (result != null) IsLoaded = true;
            _result = result;
        }
    }
}
