using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.ClientUsers.Documents {

    public class UpdateDocumentRequest {
        [FromClaim(ClaimNames.SubjectId)]
        public Guid SubjectId { get; set; }

        [FromClaim(ClaimNames.Permissions)]
        public List<UserPermission> SubjectPermissions { get; set; } = [];

        [FromRoute]
        public Guid ClientId { get; set; }

        [FromRoute]
        public Guid DocumentId { get; set; }
        public DocumentStatus? DocumentStatus { get; set; }
        public Guid? DocumentTypeId { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateDocumentResponse {
        public Guid DocumentId { get; set; }
        public string Message { get; set; } = default!;
        public DocumentStatus CurrentStatus { get; set; }
        public string? CurrentDocumentTypeName { get; set; }
    }

    public class UpdateDocument(AppDbContext dbContext) : Endpoint<UpdateDocumentRequest, UpdateDocumentResponse> {
        public override void Configure() {
            Put("client-users/{ClientId}/documents/{DocumentId}");
            Version(1);
            Permissions(nameof(UserPermission.Documents_Update), nameof(UserPermission.Students_Own_Documents_Update), nameof(UserPermission.ImmigrationClients_Own_Documents_Update));
        }

        public override async Task HandleAsync(UpdateDocumentRequest req, CancellationToken ct) {
            var client = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == req.ClientId, ct);
            if (client is null) {
                AddError(req => req.ClientId, "The specified client does not exist.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var documentToUpdate = await dbContext.Documents
                .FirstOrDefaultAsync(d => d.Id == req.DocumentId && d.OwnerId == req.ClientId, ct);
            if (documentToUpdate is null) {
                await SendNotFoundAsync(ct);
                return;
            }

            var hasDocumentsUpdatePermission = req.SubjectPermissions.Contains(UserPermission.Documents_Create);
            var hasAssignedDocumentsUpdatePermission =
                req.SubjectPermissions.Contains(UserPermission.Students_Own_Documents_Create) ||
                req.SubjectPermissions.Contains(UserPermission.ImmigrationClients_Own_Documents_Create);

            if (!hasDocumentsUpdatePermission && hasAssignedDocumentsUpdatePermission) {
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

            if (req.DocumentTypeId.HasValue) {
                var documentType = await dbContext.DocumentTypes.AsNoTracking().FirstOrDefaultAsync(dt => dt.Id == req.DocumentTypeId, ct);
                if (documentType is null) {
                    AddError(req => req.DocumentTypeId, "The specified document type does not exist.");
                    await SendErrorsAsync(400, ct);
                    return;
                }
            }

            documentToUpdate.DocumentStatus = req.DocumentStatus ?? documentToUpdate.DocumentStatus;
            documentToUpdate.DocumentTypeId = req.DocumentTypeId ?? documentToUpdate.DocumentTypeId;
            documentToUpdate.Description = req.Description;
            documentToUpdate.LastModifiedById = req.SubjectId;
            documentToUpdate.LastModifiedOn = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            var response = new UpdateDocumentResponse {
                DocumentId = documentToUpdate.Id,
                Message = "Document metadata updated successfully.",
            };

            await SendOkAsync(response, ct);
            return;
        }
    }
}