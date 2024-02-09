using Microsoft.AspNetCore.Authentication.OAuth;

namespace ShoppingMvc.Helpers
{
    public static class FileExtension
    {
        public static bool IsValidSize(this IFormFile file, int kb = 20)
            => file.Length <= kb * 1024;
        public static bool IsCorrectType(this IFormFile file, string contentType = "image")
            => file.ContentType.Contains(contentType);
        public static async Task<string> SaveAsync(this IFormFile file, string path)
        {
            string extension = Path.GetExtension(path);
            string filename = Path.GetFileNameWithoutExtension(file.FileName).Length > 32 ?
                file.FileName.Substring(0, 32) :
                Path.GetFileName(file.FileName);

            filename = Path.Combine(path, Path.GetRandomFileName() + filename + extension);

            using (FileStream fs = File.Create(Path.Combine(PathConstants.RootPath, filename)))
            {
                await file.CopyToAsync(fs);
            }
            return filename;
        }
    }
}
