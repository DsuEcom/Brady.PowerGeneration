namespace Brady.PowerGeneration.Core.Interfaces
{
    /// <summary>
    /// Defines a contract for validating XML data models.
    /// This interface separates validation logic from data access concerns.
    /// </summary>
    public interface IXmlDataValidator<T>
    {
        /// <summary>
        /// Validates the provided data according to business rules.
        /// </summary>
        /// <param name="data">The data to validate</param>
        /// <returns>A task representing the asynchronous validation operation</returns>
        /// <throws>InvalidOperationException when validation fails</throws>
        Task ValidateAsync(T data);
    }
}
