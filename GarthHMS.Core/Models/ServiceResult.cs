using GarthHMS.Core.Entities;

namespace GarthHMS.Core.Models
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ServiceResult<T> Success(T data, string message = "Operación exitosa")
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static ServiceResult<T> Failure(string message)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Message = message,
                Data = default
            };
        }
    }
}
