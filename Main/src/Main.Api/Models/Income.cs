using SharedKernel.Models;
using UserManagement.API.Enums;

namespace UserManagement.API.Models;

public class Income : EntityBase {
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; } 
    public Currency Currency { get; set; } = default!; 
    public DateTime Date { get; set; }
    public IncomeStatus IncomeStatus { get; set; }
    public string? Description { get; set; }
    public Guid IncomeTypeId { get; set; }
    public IncomeType IncomeType { get; set; } = default!;
}