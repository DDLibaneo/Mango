namespace Mango.Web.Utility
{
    /// <summary>
    /// Static Details
    /// </summary>
    public static class SD
    {
        public static string CouponAPIBase { get; set; }

        public static string AuthAPIBase { get; set; }

        public static string RoleAdmin { get; set; } = "ADMIN";

        public static string RoleCustomer { get; set; } = "CUSTOMER";

        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
    }
}
