using System.Text.Json;
using FluentValidation.Results;
using Shared.SharedKernel;

namespace Core.Validation;

public static class ValidationExtension
{
    public static Errors ToList(this ValidationResult validationResult)
    {
        var validationErrors = validationResult.Errors;

        var errors = from validationError in validationErrors
            let errorMessage = validationError.ErrorMessage
            let error = JsonSerializer.Deserialize<Error>(errorMessage)
            select Error.Validation(error.Code, errorMessage, validationError.PropertyName);

        return errors.ToList();
    }
}