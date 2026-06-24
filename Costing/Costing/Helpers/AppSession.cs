namespace Costing.Helpers
{
    public static class AppSession
    {
        public static Models.LoginUser CurrentUser { get; set; }

        public static bool IsAdmin => CurrentUser?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;
    }
}