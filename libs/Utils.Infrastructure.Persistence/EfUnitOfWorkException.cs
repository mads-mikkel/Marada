using System.Runtime.Serialization;

namespace Marada.Utils.Infrastructure.Persistence
{
    [Serializable]
    internal class EfUnitOfWorkException: Exception
    {
        public EfUnitOfWorkException()
        {
        }

        public EfUnitOfWorkException(string? message) : base(message)
        {
        }

        public EfUnitOfWorkException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EfUnitOfWorkException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}