using FastEndpoints;
using FluentValidation;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.ClientUsers.Documents {
    public class ListUserDocumentsRequest {
        [FromRoute]
        public Guid ClientId { get; set; }

        [FromClaim(ClaimNames.SubjectId)]
        public Guid SubjectId { get; set; }

        [FromClaim(ClaimNames.Permissions)]
        public List<UserPermission> SubjectPermissions { get; set; } = [];
    }

    public class ListUserDocumentsResponse {
        public List<DocumentDto> Documents { get; set; } = [];
    }

    public class DocumentDto {
        public Guid Id { get; set; }
        public string FileName { get; set; } = default!;
        public string? Description { get; set; }
        public long FileSize { get; set; }
        public DocumentStatus DocumentStatus { get; set; }
        public Guid DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = default!;
        public DateTime CreatedOn { get; set; }
    }

    public class ListUserDocumentsRequestValidator : Validator<ListUserDocumentsRequest> {
        public ListUserDocumentsRequestValidator() {
            RuleFor(x => x.ClientId)
              .NotEmpty().WithMessage("Client ID is required.");
        }
    }

    public class ListUserDocuments(AppDbContext dbContext) : Endpoint<ListUserDocumentsRequest, ListUserDocumentsResponse> {
        public override void Configure() {
            Get("client-users/{ClientId}/documents");
            Version(1);
            Permissions(nameof(UserPermission.Documents_Read), nameof(UserPermission.Students_Own_Documents_Read), nameof(UserPermission.ImmigrationClients_Own_Documents_Read), nameof(UserPermission.Documents_Own_Read));
        }

        public override async Task HandleAsync(ListUserDocumentsRequest req, CancellationToken ct) {
            var client = await dbContext.Users
              .SingleOrDefaultAsync(u => u.Id == req.ClientId, ct);

            if (client is null) {
                AddError(req => req.ClientId, "This User does not exist.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var isRequestingOwnDocuments = req.ClientId == req.SubjectId;
            var hasOwnDocumentsPermission = req.SubjectPermissions.Contains(UserPermission.Documents_Own_Read);
            if (isRequestingOwnDocuments && hasOwnDocumentsPermission) {

            }
            else {
                var hasDocumentsViewAllPermission = req.SubjectPermissions.Contains(UserPermission.Documents_Read);
                var hasAssignedDocumentsViewPermission =
                    req.SubjectPermissions.Contains(UserPermission.Students_Own_Documents_Read) ||
                    req.SubjectPermissions.Contains(UserPermission.ImmigrationClients_Own_Documents_Read);

                if (!hasDocumentsViewAllPermission && hasAssignedDocumentsViewPermission) {
                    var isAssigned = false;

                    if (client is StudentUser student) {
                        isAssigned = student.AdmissionAssociateId == req.SubjectId ||
                          student.CounselorId == req.SubjectId ||
                          student.SopWriterId == req.SubjectId;
                    }
                    else if (client is ImmigrationClientUser immigrationClient) {
                        isAssigned = immigrationClient.AdmissionAssociateId == req.SubjectId ||
                          immigrationClient.CounselorId == req.SubjectId ||
                          immigrationClient.SopWriterId == req.SubjectId;
                    }

                    if (!isAssigned) {
                        await SendForbiddenAsync(ct);
                        return;
                    }
                }
            }

            var documents = await dbContext.Documents
              .AsNoTracking()
              .Include(d => d.DocumentType)
              .Where(d => d.OwnerId == req.ClientId)
              .Select(d => new DocumentDto {
                  Id = d.Id,
                  FileName = d.FileName,
                  FileSize = d.FileSize,
                  DocumentStatus = d.DocumentStatus,
                  DocumentTypeId = d.DocumentTypeId,
                  DocumentTypeName = d.DocumentType.Name,
                  Description = d.Description,
                  CreatedOn = d.CreatedOn,
              })
              .ToListAsync();

            var res = new ListUserDocumentsResponse {
                Documents = documents,
            };

            await SendOkAsync(res, ct);
            return;
        }
    }
}