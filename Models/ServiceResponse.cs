using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ServiceResponse<T>
    {
        public T Data { get; private set; }
        public Exception Exception { get; private set; }
        public string Message { get; private set; }
        public bool Ok { get; private set; }

        // Private constructor to enforce factory pattern usage
        private ServiceResponse(T data, bool ok, string message, Exception exception)
        {
            Data = data;
            Ok = ok;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Creates a successful response with data.
        /// </summary>
        public static ServiceResponse<T> Success(T data, string message = "Operation Successful")
        {
            return new ServiceResponse<T>(data, true, message, null);
        }

        /// <summary>
        /// Creates a failed response with an exception.
        /// </summary>
        public static ServiceResponse<T> Fail(string message, Exception exception = null)
        {
            return new ServiceResponse<T>(default, false, message, exception);
        }
    }

}
