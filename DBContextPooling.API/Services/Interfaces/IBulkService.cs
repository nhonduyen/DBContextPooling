using DBContextPooling.API.Models;
using System.Data;

namespace DBContextPooling.API.Services.Interfaces
{
    public interface IBulkService
    {
        Task ExecuteBulkCopyAsync(DataTable dataTable, object destinationTable, CancellationToken cancellationToken = default);
        Task<int> ExecuteBulkUpdateAsync(DataTable dataTable, object destinationTable, CancellationToken cancellationToken = default);
        DataTable ConvertListToDatatable(List<Customer> list);
    }
}
