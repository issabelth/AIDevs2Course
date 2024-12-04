using System;

namespace Utilities
{
    public class MyException : Exception
    {
        public MyException(string message)
            : base(message)
        {
        }
    }
}
