using SharedKernel.Models;
using UserManagement.API.Enums;

namespace UserManagement.API.Models {
    public class UniversityApplication : EntityBase {
        public Guid ApplicantId { get; set; }
        public User Applicant { get; set; } = default!;

        public Guid UniversityProgramId { get; set; }
        public UniversityProgram UniversityProgram { get; set; } = default!;

        public ApplicationStatus ApplicationStatus { get; set; } = ApplicationStatus.UnderReview;

        public DateTime ApplyDate { get; set; }
        public DateTime? ReviewSuccessDate { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public DateTime? ResultDate { get; set; }
    }
}
