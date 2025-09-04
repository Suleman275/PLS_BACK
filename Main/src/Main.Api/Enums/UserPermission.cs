namespace SharedKernel.Enums;

public enum UserPermission {
    // Locations Permissions
    Locations_Create = 1,
    Locations_Read = 2,
    Locations_Update = 3,
    Locations_Delete = 4,

    // Nationalities Permissions
    Nationalities_Create = 5,
    Nationalities_Read = 6,
    Nationalities_Update = 7,
    Nationalities_Delete = 8,

    // Income Types Permissions
    IncomeTypes_Create = 9,
    IncomeTypes_Read = 10,
    IncomeTypes_Update = 11,
    IncomeTypes_Delete = 12,

    // Expense Types Permissions
    ExpenseTypes_Create = 13,
    ExpenseTypes_Read = 14,
    ExpenseTypes_Update = 15,
    ExpenseTypes_Delete = 16,

    // Incomes Permissions
    Incomes_Create = 17,
    Incomes_Read = 18,
    Incomes_Update = 19,
    Incomes_Delete = 20,

    // Expenses Permissions
    Expenses_Create = 21,
    Expenses_Read = 22,
    Expenses_Update = 23,
    Expenses_Delete = 24,

    // University Types Permissions
    UniversityTypes_Create = 25,
    UniversityTypes_Read = 26,
    UniversityTypes_Update = 27,
    UniversityTypes_Delete = 28,

    // Program Types Permissions
    ProgramTypes_Create = 29,
    ProgramTypes_Read = 30,
    ProgramTypes_Update = 31,
    ProgramTypes_Delete = 32,

    // Document Types Permissions
    DocumentTypes_Create = 33,
    DocumentTypes_Read = 34,
    DocumentTypes_Update = 35,
    DocumentTypes_Delete = 36,

    // Employees Permissions
    Employees_Create = 37,
    Employees_Read = 38,
    Employees_Update = 39,
    Employees_Delete = 40,

    // Currencies Permissions
    Currencies_Create = 41,
    Currencies_Read = 42,
    Currencies_Update = 43,
    Currencies_Delete = 44,

    // Students Permissions
    Students_Create = 45,
    Students_Read = 46,
    Students_Update = 47,
    Students_Delete = 48,

    Students_Own_Read = 49,
    Students_Own_Update = 50,

    Students_Own_Documents_Create = 51,
    Students_Own_Documents_Read = 52,
    Students_Own_Documents_Update = 53,
    Students_Own_Documents_Delete = 54,

    Students_Own_UniversityApplications_Create = 55,
    Students_Own_UniversityApplications_Read = 56,
    Students_Own_UniversityApplications_Update = 57,
    Students_Own_UniversityApplications_Delete = 58,

    Students_Own_VisaApplications_Create = 59,
    Students_Own_VisaApplications_Read = 60,
    Students_Own_VisaApplications_Update = 61,
    Students_Own_VisaApplications_Delete = 62,

    // Partners Permissions
    Partners_Create = 63,
    Partners_Read = 64,
    Partners_Update = 65,
    Partners_Delete = 66,

    // Immigration Clients Permissions
    ImmigrationClients_Create = 67,
    ImmigrationClients_Read = 68,
    ImmigrationClients_Update = 69,
    ImmigrationClients_Delete = 70,

    ImmigrationClients_Own_Read = 71,
    ImmigrationClients_Own_Update = 72,

    ImmigrationClients_Own_Documents_Create = 73,
    ImmigrationClients_Own_Documents_Read = 74,
    ImmigrationClients_Own_Documents_Update = 75,
    ImmigrationClients_Own_Documents_Delete = 76,

    ImmigrationClients_Own_VisaApplications_Create = 77,
    ImmigrationClients_Own_VisaApplications_Read = 78,
    ImmigrationClients_Own_VisaApplications_Update = 79,
    ImmigrationClients_Own_VisaApplications_Delete = 80,

    // Universities Permissions
    Universities_Create = 81,
    Universities_Read = 82,
    Universities_Update = 83,
    Universities_Delete = 84,

    // University Programs Permissions
    UniversityPrograms_Create = 85,
    UniversityPrograms_Read = 86,
    UniversityPrograms_Update = 87,
    UniversityPrograms_Delete = 88,

    // Documents Permissions
    Documents_Create = 89,
    Documents_Read = 90,
    Documents_Update = 91,
    Documents_Delete = 92,

    Documents_Own_Create = 93,
    Documents_Own_Read = 94,
    Documents_Own_Update = 95,
    Documents_Own_Delete = 96,

    // University Applications Permissions
    UniversityApplications_Create = 97,
    UniversityApplications_Read = 98,
    UniversityApplications_Update = 99,
    UniversityApplications_Delete = 100,

    UniversityApplications_Own_Create = 101,
    UniversityApplications_Own_Read = 102,

    // Visa Applications Permissions
    VisaApplications_Create = 103,
    VisaApplications_Read = 104,
    VisaApplications_Update = 105,
    VisaApplications_Delete = 106,

    VisaApplications_Own_Read = 107,

    // Visa Application Types Permissions
    VisaApplicationTypes_Create = 108,
    VisaApplicationTypes_Read = 109,
    VisaApplicationTypes_Update = 110,
    VisaApplicationTypes_Delete = 111,

    // Client Sources Permissions
    ClientSources_Create = 112,
    ClientSources_Read = 113,
    ClientSources_Update = 114,
    ClientSources_Delete = 115,

    // Dashboard Permissions
    Portal_Overview = 116,
    Users_Overview = 117,
    Finances_Overview = 118,

    //Community Permissions
    Communities_Create,
    Communities_Read,
    Communities_Update,
    Communities_Delete,

    Posts_Create,
    Posts_Read,
    Posts_Update,
    Posts_Delete,

    Posts_Own_Update,
    Posts_Own_Delete,

    Comments_Create,
    Comments_Read,
    Comments_Update,
    Comments_Delete,

    Comments_Own_Update,
    Comments_Own_Delete
}
