using System;
using System.Collections.Generic;

namespace Bewit.Extensions.HotChocolate.Tests.Integration
{
    public class QueryRequest
    {
        public string NamedQuery { get; }

        public string Query { get; }

        public string OperationName { get; }

        public Dictionary<string, object> Variables { get; }

        public QueryRequest(
            string namedQuery,
            string query,
            string operationName,
            Dictionary<string, object> variables)
        {
            if (namedQuery == null)
            {
                throw new ArgumentNullException(nameof(namedQuery));
            }

            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (operationName == null)
            {
                throw new ArgumentNullException(nameof(operationName));
            }

            if (variables == null)
            {
                throw new ArgumentNullException(nameof(variables));
            }

            NamedQuery = namedQuery;
            Query = query;
            OperationName = operationName;
            Variables = variables;
        }
    }
}
