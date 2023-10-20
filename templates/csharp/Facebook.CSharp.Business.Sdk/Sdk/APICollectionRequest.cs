using System.Collections.Generic;
using System.Threading.Tasks;

namespace Facebook.Csharp.Business.Sdk.Sdk
{
    public abstract class APICollectionRequest<T> : APIRequest<T> where T : APINode
    {
        protected APICollectionRequest(APIContext context, string nodeId, string endpoint, string method, IEnumerable<string> paramNames)
            : base(context, nodeId, endpoint, method, paramNames)
        {
        }

        public abstract Task<APINodeList<T>> ExecuteAsync(CancellationToken cancellationToken = default);

        public abstract Task<APINodeList<T>> ExecuteAsync(Dictionary<string, object> extraParams, CancellationToken cancellationToken = default);
    }
}