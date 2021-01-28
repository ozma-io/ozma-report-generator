namespace ReportGenerator.FunDbApi
{
    public class FunDbQuery
    {
        public string Name { get; private set; }
        public string QueryTextWithoutParameterValues { get; private set; }

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

        public void SetResult(dynamic? result)
        {
            if (result != null) IsLoaded = true;
            _result = result;
        }

        //public async Task LoadDataAsync(Dictionary<string, object> queryParametersWithValues, string instanceName, TokenProcessor tokenProcessor)
        //{
        //    var queryTextToRun = QueryTextWithoutParameterValues;
        //    dynamic? result = null;
        //    var apiConnector = new FunDbApiConnector(instanceName, tokenProcessor);
        //    if (queryTextToRun.StartsWith("/views/"))
        //        result = await apiConnector.LoadQueryNamed(queryTextToRun, queryParametersWithValues);
        //    else
        //        result = await apiConnector.LoadQueryAnonymous(queryTextToRun, queryParametersWithValues);
        //    if (result != null) IsLoaded = true;
        //    _result = result;
        //}
    }
}
