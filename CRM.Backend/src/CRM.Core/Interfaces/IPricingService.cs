using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces
{
    public interface IPricingService
    {
        Task<IEnumerable<PriceBook>> GetAllPriceBooksAsync(CancellationToken cancellationToken = default);
        Task<PriceBook?> GetPriceBookByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PriceBook> CreatePriceBookAsync(PriceBook priceBook, CancellationToken cancellationToken = default);
        Task<PriceBook> UpdatePriceBookAsync(PriceBook priceBook, CancellationToken cancellationToken = default);
        Task<bool> DeletePriceBookAsync(int id, CancellationToken cancellationToken = default);

        Task<decimal> CalculatePriceAsync(int productId, int? priceBookId = null, int quantity = 1, CancellationToken cancellationToken = default);
    }
}
