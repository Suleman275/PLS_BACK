using FastEndpoints;
using Main.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Enums;
using UserManagement.API.Enums;
using UserManagement.API.Models;

namespace Main.Api.Endpoints.ClientUsers.Documents {

    public class CreateDocumentRequest {
        [FromClaim(ClaimNames.SubjectId)]
        public Guid SubjectId { get; set; }

        [FromClaim(ClaimNames.Permissions)]
        public List<UserPermission> SubjectPermissions { get; set; } = [];

        public IFormFile File { get; set; } = default!;

        [FromRoute]
        public Guid ClientId { get; set; }

        public Guid DocumentTypeId { get; set; }
        public string? Description { get; set; }
    }

    public class CreateDocumentResponse {
        public Guid DocumentId { get; set; }
        public string Message { get; set; } = default!;
    }
    public class CreateDocument(AppDbContext dbContext) : Endpoint<CreateDocumentRequest, CreateDocumentResponse> {
        public override void Configure() {
            Post("client-users/{ClientId}/documents");
            AllowFileUploads();
            Version(1);
            Permissions(nameof(UserPermission.Documents_Create), nameof(UserPermission.Students_Own_Documents_Create), nameof(UserPermission.ImmigrationClients_Own_Documents_Create), nameof(UserPermission.Documents_Own_Create));
        }

        public override async Task HandleAsync(CreateDocumentRequest req, CancellationToken ct) {
            var client = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == req.ClientId, ct);
            if (client is null) {
                AddError(req => req.ClientId, "The specified client does not exist.");
                await SendErrorsAsync(400, ct);
                return;
            }


            var isRequestingOwnDocuments = req.ClientId == req.SubjectId;
            var hasOwnDocumentsPermission = req.SubjectPermissions.Contains(UserPermission.Documents_Own_Read);
            if (isRequestingOwnDocuments && hasOwnDocumentsPermission) {

            }
            else {
                var hasDocumentsCreatePermission = req.SubjectPermissions.Contains(UserPermission.Documents_Create);
                var hasAssignedDocumentsCreatePermission =
                    req.SubjectPermissions.Contains(UserPermission.Students_Own_Documents_Create) ||
                    req.SubjectPermissions.Contains(UserPermission.ImmigrationClients_Own_Documents_Create);

                if (!hasDocumentsCreatePermission && hasAssignedDocumentsCreatePermission) {
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

            var documentType = await dbContext.DocumentTypes.AsNoTracking().FirstOrDefaultAsync(dt => dt.Id == req.DocumentTypeId, ct);
            if (documentType is null) {
                AddError(req => req.DocumentTypeId, "The specified document type does not exist.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var documentId = Guid.CreateVersion7();
            var s3Key = $"v2/documents/{req.ClientId}/{documentId}-{req.File.FileName}";

            try {
                var document = new Document {
                    FileName = req.File.FileName,
                    S3Key = s3Key,
                    FileSize = req.File.Length,
                    DocumentStatus = DocumentStatus.UnderReview,
                    OwnerId = req.ClientId,
                    DocumentTypeId = req.DocumentTypeId,
                    Description = req.Description,
                    CreatedById = req.SubjectId
                };

                dbContext.Documents.Add(document);
                await dbContext.SaveChangesAsync(ct);

                var response = new CreateDocumentResponse {
                    DocumentId = document.Id,
                    Message = "Document uploaded successfully."
                };

                await SendOkAsync(response, cancellation: ct);
                return;
            }
            catch (Exception) {
                AddError("An unexpected error occurred while processing your request.");
                await SendErrorsAsync(500, ct);
            }
        }
    }
}