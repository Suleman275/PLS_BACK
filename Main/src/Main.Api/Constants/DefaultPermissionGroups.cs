using SharedKernel.Enums;

namespace UserManagement.API.Constants;

public static class DefaultPermissionGroups {
    public static readonly ICollection<UserPermission> AdminPermissions = Enum.GetValues<UserPermission>();

    public static readonly ICollection<UserPermission> StudentPermissions = [
        UserPermission.Locations_Read,
        UserPermission.Nationalities_Read,
        UserPermission.Universities_Read,
        UserPermission.UniversityPrograms_Read,
        UserPermission.UniversityTypes_Read,
        UserPermission.ProgramTypes_Read,

        UserPermission.Documents_Own_Create,
        UserPermission.Documents_Own_Read,
        UserPermission.Documents_Own_Update,
        UserPermission.Documents_Own_Delete,

        UserPermission.UniversityApplications_Own_Create,
        UserPermission.UniversityApplications_Own_Read,
        UserPermission.VisaApplications_Own_Read,

        UserPermission.Communities_Read,
        UserPermission.Posts_Create,
        UserPermission.Posts_Read,
        UserPermission.Posts_Own_Update,
        UserPermission.Posts_Own_Delete,
        UserPermission.Comments_Create,
        UserPermission.Comments_Read,
        UserPermission.Comments_Own_Update,
        UserPermission.Comments_Own_Delete,
    ];

    public static readonly ICollection<UserPermission> EmployeePermissions = [
        UserPermission.Locations_Read,
        UserPermission.Nationalities_Read,
        UserPermission.Universities_Read,
        UserPermission.UniversityPrograms_Read,
        UserPermission.UniversityTypes_Read,
        UserPermission.ProgramTypes_Read,
        UserPermission.ClientSources_Read,
        UserPermission.DocumentTypes_Read,
        UserPermission.VisaApplicationTypes_Read,
        UserPermission.Employees_Read,
        UserPermission.ImmigrationClients_Own_Read,
        UserPermission.Students_Own_Read,
        UserPermission.Communities_Read,
        UserPermission.Posts_Create,
        UserPermission.Posts_Read,
        UserPermission.Posts_Own_Update,
        UserPermission.Posts_Own_Delete,
        UserPermission.Comments_Create,
        UserPermission.Comments_Read,
        UserPermission.Comments_Own_Update,
        UserPermission.Comments_Own_Delete,
    ];

    public static readonly ICollection<UserPermission> PartnerPermissions = [
        UserPermission.Communities_Read,
        UserPermission.Posts_Create,
        UserPermission.Posts_Read,
        UserPermission.Posts_Own_Update,
        UserPermission.Posts_Own_Delete,
        UserPermission.Comments_Create,
        UserPermission.Comments_Read,
        UserPermission.Comments_Own_Update,
        UserPermission.Comments_Own_Delete,
    ];

    public static readonly ICollection<UserPermission> ImmigrationClientPermissions = [
        UserPermission.Locations_Read,
        UserPermission.Nationalities_Read,
        UserPermission.Documents_Own_Create,
        UserPermission.Documents_Own_Read,
        UserPermission.Documents_Own_Update,
        UserPermission.Documents_Own_Delete,
        UserPermission.VisaApplications_Own_Read,
        UserPermission.Communities_Read,
        UserPermission.Posts_Create,
        UserPermission.Posts_Read,
        UserPermission.Posts_Own_Update,
        UserPermission.Posts_Own_Delete,
        UserPermission.Comments_Create,
        UserPermission.Comments_Read,
        UserPermission.Comments_Own_Update,
        UserPermission.Comments_Own_Delete,
    ];
}