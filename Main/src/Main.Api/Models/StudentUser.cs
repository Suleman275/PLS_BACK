using UserManagement.API.Enums;

namespace UserManagement.API.Models;

public class StudentUser : User {
    public override UserRole Role { get; set; } = UserRole.Student;

    public Guid? LocationId { get; set; }
    public Location? Location { get; set; }

    public Guid? NationalityId { get; set; }
    public Nationality? Nationality { get; set; }

    public Guid? AdmissionAssociateId { get; set; }
    public EmployeeUser? AdmissionAssociate { get; set; }

    public Guid? CounselorId { get; set; }
    public EmployeeUser? Counselor {  get; set; }

    public Guid? SopWriterId { get; set; }
    public EmployeeUser? SopWriter { get; set; }

    public Guid? RegisteredById { get; set; }
    public EmployeeUser? RegisteredBy { get; set; }
    public DateTime? RegistrationDate { get; set; }

    public Guid? ClientSourceId { get; set; }
    public ClientSource? ClientSource { get; set; }

    public long StorageLimit { get; set; } = 100 * 1024 * 1024; // 100 MB in bytes
    public long StorageUsage { get; set; } = 0;
}