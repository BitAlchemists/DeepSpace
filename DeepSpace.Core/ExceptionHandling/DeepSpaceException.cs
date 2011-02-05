using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepSpace.Core.ExceptionHandling
{
    /// <summary>
    /// Summary description for AxiomException.
    /// </summary>
    public class DeepSpaceException : ApplicationException
    {
        public DeepSpaceException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }
        public DeepSpaceException(string message, Exception innerException, params object[] args)
            : base(string.Format(message, args), innerException)
        {
        }
    }
}
