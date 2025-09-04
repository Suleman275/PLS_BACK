using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Models;

namespace UserManagement.API.Endpoints.ClientUsers.Documents {
    public class DeleteDocumentRequest {
        [FromClaim(ClaimNames.SubjectId)]
        public Guid SubjectId { get; set; }

        [FromClaim(ClaimNames.Permissions)]
        public List<UserPermission> SubjectPermissions { get; set; } = [];

        [FromRoute]
        public Guid ClientId { get; set; }

        [FromRoute]
        public Guid DocumentId { get; set; }
    }
    public class DeleteDocument(AppDbContext dbContext, ILogger<DeleteDocument> logger) : Endpoint<DeleteDocumentRequest> {
        public override void Configure() {
            Delete("client-users/{ClientId}/documents/{DocumentId}");
            Version(1);
            Permissions(nameof(UserPermission.Documents_Delete), nameof(UserPermission.Students_Own_Documents_Create), nameof(UserPermission.ImmigrationClients_Own_Documents_Create), nameof(UserPermission.Documents_Own_Delete));
        }

        public override async Task HandleAsync(DeleteDocumentRequest req, CancellationToken ct) {
            var client = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == req.ClientId, ct);
            if (client is null) {
                AddError(req => req.ClientId, "The specified client does not exist.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var documentToDelete = await dbContext.Documents
                .FirstOrDefaultAsync(d => d.Id == req.DocumentId && d.OwnerId == req.ClientId, ct);
            if (documentToDelete is null) {
                logger.LogWarning("Document {DocumentId} not found for client {ClientId} or does not belong to client.", req.DocumentId, req.ClientId);
                await SendNotFoundAsync(ct); // 404 if document doesn't exist or doesn't belong to client
                return;
            }

            var isRequestingOwnDocuments = req.ClientId == req.SubjectId;
            var hasOwnDocumentsPermission = req.SubjectPermissions.Contains(UserPermission.Documents_Own_Read);
            if (isRequestingOwnDocuments && hasOwnDocumentsPermission) {

            }
            else {
                var hasDocumentsDeletePermission = req.SubjectPermissions.Contains(UserPermission.Documents_Create);
                var hasAssignedDocumentsDeletePermission =
                    req.SubjectPermissions.Contains(UserPermission.Students_Own_Documents_Create) ||
                    req.SubjectPermissions.Contains(UserPermission.ImmigrationClients_Own_Documents_Create);

                if (!hasDocumentsDeletePermission && hasAssignedDocumentsDeletePermission) {
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

            documentToDelete.DeletedById = req.SubjectId;
            documentToDelete.DeletedOn = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(ct);

            await SendNoContentAsync(ct);
        }
    }
}
