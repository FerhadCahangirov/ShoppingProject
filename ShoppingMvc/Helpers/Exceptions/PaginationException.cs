using System.Runtime.Serialization;

namespace ShoppingMvc.Helpers.Exceptions
{
    public class PaginationException : Exception
    {
        public PaginationException() : base("Pagination Problem")
        {
        }

        public PaginationException(string? message) : base(message)
        {
        }
    }
}