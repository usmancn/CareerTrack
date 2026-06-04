namespace CareerTrack.Models.ViewModels
{
    public class UserRoleManagementViewModel
    {
        public List<UserRoleItemViewModel> Users { get; set; } = new();
        public List<UserCompanyOptionViewModel> Companies { get; set; } = new();
    }

    public class UserRoleItemViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string CurrentRole { get; set; } = string.Empty;
        public string SelectedRole { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public bool IsEmployerPendingApproval { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    public class UserCompanyOptionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
    }
}
