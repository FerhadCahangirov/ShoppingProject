namespace ShoppingMvc.Helpers
{
        public class PathConstants
        {
            public static string Product => Path.Combine("Assets", "assets", "products");
            public static string Users => Path.Combine("Assets", "assets", "users");
            public static string RootPath { get; set; }
        }
}
