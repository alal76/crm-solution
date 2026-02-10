using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces
{
    public interface IProductBundleService
    {
        Task<IEnumerable<ProductBundle>> GetAllBundlesAsync(CancellationToken cancellationToken = default);
        Task<ProductBundle?> GetBundleByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ProductBundle> CreateBundleAsync(ProductBundle bundle, CancellationToken cancellationToken = default);
        Task<ProductBundle> UpdateBundleAsync(ProductBundle bundle, CancellationToken cancellationToken = default);
        Task<bool> DeleteBundleAsync(int id, CancellationToken cancellationToken = default);

        Task<decimal> CalculateBundlePriceAsync(int bundleId, CancellationToken cancellationToken = default);
    }
}
