using System;

namespace Stock.Application.Exceptions
{
    public class StockNotFoundException : Exception
    {
        public StockNotFoundException(string message) : base(message) { }
    }

    public class StockAlreadyExistsException : Exception
    {
        public StockAlreadyExistsException(string message, Exception? innerException)
    : base(message, innerException)
        {
        }
    }

    public class StockValidationException : Exception
    {
        public StockValidationException(string message) : base(message) { }
    }
}
