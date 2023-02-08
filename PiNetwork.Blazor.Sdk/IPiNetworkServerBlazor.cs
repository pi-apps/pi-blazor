using PiNetwork.Blazor.Sdk.Dto.Payment;
using System.Threading.Tasks;

namespace PiNetwork.Blazor.Sdk
{
    public interface IPiNetworkServerBlazor
    {
        Task<PaymentDto> PaymentApprove(string id);

        Task<PaymentDto> PaymentComplete(string id, string txid);

        Task<PaymentDto> PaymentGet(string id);
    }
}