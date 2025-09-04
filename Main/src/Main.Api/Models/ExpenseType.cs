using SharedKernel.Models;

public class ExpenseType : EntityBase {
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}