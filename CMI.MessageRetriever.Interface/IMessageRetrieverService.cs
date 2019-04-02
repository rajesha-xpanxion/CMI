using CMI.MessageRetriever.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMI.MessageRetriever.Interface
{
    public interface IMessageRetrieverService
    {
        Task<IEnumerable<MessageBodyResponse>> Execute();
    }
}
