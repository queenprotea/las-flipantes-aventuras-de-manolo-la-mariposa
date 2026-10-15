using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract(CallbackContract = typeof(IChatCallback))]
    public interface IChatService
    {
        [OperationContract(Name = "SendChatMessage")]
        Task<SendChatMessageResult> SendChatMessageAsync(string text);
    }
    
    [ServiceContract]
    public interface IChatCallback
    {
        [OperationContract(Name = "OnChatMessage", IsOneWay = true)]
        void OnChatMessage(ChatMessageContract message);
    }
}
