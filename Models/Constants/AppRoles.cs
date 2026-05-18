namespace CareerTrack.Models.Constants
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string School = "School";
        public const string Student = "Student";
        public const string Employer = "Employer";

        public static readonly string[] All = { Admin, School, Student, Employer };

        public static string DisplayName(string role)
        {
            return role switch
            {
                Admin => "Admin",
                School => "Okul",
                Student => "Öğrenci",
                Employer => "İşveren",
                _ => role
            };
        }
    }
}
