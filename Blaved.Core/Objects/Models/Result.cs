namespace Blaved.Core.Objects.Models
{
    public class Result<T>(bool status, T? data)
    {
        public bool Status { get; set; } = status;
        public T? Data { get; set; } = data;
    }
}
