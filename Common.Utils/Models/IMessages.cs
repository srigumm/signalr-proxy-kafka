using System.Threading.Tasks;

namespace Api.Models{
    public interface IMessages
    {
        Task InformNewOrderToMix(OrderRequest o);
    }
}