using PiNetwork.Blazor.Sdk.Dto.Auth;
using System.Threading.Tasks;

namespace PiNetwork.Blazor.Sdk
{
    public interface IPiNetworkClientBlazor
    {
        Task Authenticate(string redirectUri);

        Task AuthenticateOnErrorCallBack(string error, string redirectUri);

        Task AuthenticateOnSuccessCallBack(AuthResultDto auth, string redirectUri);

        Task CreatePayment(decimal amount, string memo, int orderId, int retries = 0);

        Task CreatePaymentOnReadyForServerApprovalCallBack(string paymentId, int retries = 0);

        Task CreatePaymentOnReadyForServerCompletionCallBack(string paymentId, string txid, int retries = 0);

        Task CreatePaymentOnCancelCallBack(string paymentId);

        Task CreatePaymentOnErrorCallBack(string paymentId, string txid);

        Task OpenShareDialog(string title, string messsage);

        Task IsPiNetworkBrowser();
    }
}