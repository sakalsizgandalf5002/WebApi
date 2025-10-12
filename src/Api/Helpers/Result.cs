using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Helpers
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
       
       public static Result<T> Ok(T data, string message = null) => new() { Success = true, Data = data, Message = message };
       public static Result<T> Fail(string message) => new() { Success = false, Message = message };

       
    }
}