using SharedKernel.Models;

namespace UserManagement.API.Models;

public class Expense : EntityBase {
    public decimal Amount { get; set; }
    public Guid CurrencyId { get; set; } 
    public Currency Currency { get; set; } = default!; 
    public DateTime Date { get; set; }

    public string? Description { get; set; }

    public Guid ExpenseTypeId { get; set; }
    public ExpenseType ExpenseType { get; set; } = default!; 
}