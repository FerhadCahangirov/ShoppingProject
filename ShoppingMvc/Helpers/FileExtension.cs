using Microsoft.AspNetCore.Authentication.OAuth;

namespace ShoppingMvc.Helpers
{
    public static class FileExtension
    {
        public static bool IsValidSize(this IFormFile file, int kb = 20)
            => file.Length <= kb * 1024;
        public static bool IsCorrectType(this IFormFile file, string contentType = "image")
            => file.ContentType.Contains(contentType);
    }
}
