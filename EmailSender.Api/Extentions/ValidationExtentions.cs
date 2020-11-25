using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmailSender.Api.Extentions
{
    public static class ValidationExtentions
    {
        /// <summary>
        /// Checks if the requested instance is valid based on the model attributes.
        /// </summary>
        /// <typeparam name="T">Any type that we want to validate.</typeparam>
        /// <param name="object">The object we with to validate.</param>
        /// <param name="validationResults">a variable that can be used to expose the failed validation results.</param>
        /// <returns>If the requested object is valid.</returns>
        public static bool IsModelValid<T>(this T @object, out IEnumerable<ValidationResult> validationResults) where T : class
        {
            var failedValidationResults = new List<ValidationResult>();
            var isModelValid = false;

            if (@object != null)
                isModelValid = Validator.TryValidateObject(@object, new ValidationContext(@object), failedValidationResults);

            validationResults = failedValidationResults;
            return isModelValid;
        }
    }
}
