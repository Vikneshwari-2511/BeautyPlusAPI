namespace BeautyPlusParlour.Constants;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
    public const string Customer = "Customer";

    // Policy names (used in [Authorize(Policy = ...)])
    public const string AdminOnly = "AdminOnly";
    public const string StaffOrAdmin = "StaffOrAdmin";
    public const string CustomerOnly = "CustomerOnly";
}