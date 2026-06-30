using System.Net;

namespace MinimalAPI.Models
{
    public class APIResponse
    {
        public APIResponse()
        {
            ErrorMessages = [];
        }

        public bool IsSuccess { get; set; } = true;
        public object? Result { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<string> ErrorMessages { get; set; }
    }
}
