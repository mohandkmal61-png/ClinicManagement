namespace ClinicManagement.Application.Exceptions
{
    public class AuthValidationException : Exception
    {
        public AuthValidationException(IEnumerable<string> errors)
            : base("The registration request is invalid.")
        {
            Errors = errors.ToArray();
        }

        public IReadOnlyList<string> Errors { get; }
    }
}
