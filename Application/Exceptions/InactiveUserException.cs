using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class InactiveUserException : Exception
    {
        public InactiveUserException(string message) : base(message)
        {
        }
        public InactiveUserException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
