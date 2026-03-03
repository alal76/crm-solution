using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Tests.Helpers
{
    /// <summary>
    /// Base class for validator tests providing common assertion helpers.
    /// Reduces boilerplate in validator test classes.
    /// </summary>
    /// <typeparam name="TValidator">The validator class type</typeparam>
    public abstract class ValidatorTestFixtureBase<TValidator> where TValidator : class
    {
        protected TValidator Validator { get; }

        protected ValidatorTestFixtureBase()
        {
            Validator = CreateValidator();
        }

        /// <summary>
        /// Override to instantiate the validator under test.
        /// </summary>
        protected abstract TValidator CreateValidator();

        /// <summary>
        /// Asserts that validation passes for the given model.
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="model">Model instance to validate</param>
        /// <param name="setup">Optional setup action to modify model before validation</param>
        protected void AssertValid<T>(T model, System.Action<T>? setup = null)
        {
            setup?.Invoke(model);
            var result = ValidateModel(model);
            Xunit.Assert.Empty(result); // No validation errors
        }

        /// <summary>
        /// Asserts that validation fails with a specific error message containing the given key.
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="model">Model instance to validate</param>
        /// <param name="expectedErrorKey">Expected substring in error message</param>
        /// <param name="setup">Optional setup action to modify model before validation</param>
        protected void AssertInvalid<T>(T model, string expectedErrorKey, System.Action<T>? setup = null)
        {
            setup?.Invoke(model);
            var result = ValidateModel(model);
            Xunit.Assert.NotEmpty(result);
            Xunit.Assert.Contains(result, e => 
                e.ErrorMessage != null && e.ErrorMessage.Contains(expectedErrorKey, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Asserts that validation fails with an error for a specific property.
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="model">Model instance to validate</param>
        /// <param name="propertyName">Name of the property expected to have an error</param>
        /// <param name="setup">Optional setup action</param>
        protected void AssertPropertyInvalid<T>(T model, string propertyName, System.Action<T>? setup = null)
        {
            setup?.Invoke(model);
            var result = ValidateModel(model);
            Xunit.Assert.NotEmpty(result);
            Xunit.Assert.Contains(result, e => 
                e.MemberNames != null && System.Linq.Enumerable.Contains(e.MemberNames, propertyName));
        }

        /// <summary>
        /// Override to provide custom validation logic.
        /// Default implementation uses DataAnnotations validation.
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="model">Model to validate</param>
        /// <returns>Collection of validation results (empty if valid)</returns>
        protected virtual IEnumerable<ValidationResult> ValidateModel<T>(T model)
        {
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        /// <summary>
        /// Validates a single property of a model.
        /// </summary>
        protected IEnumerable<ValidationResult> ValidateProperty<T>(T model, string propertyName)
        {
            var context = new ValidationContext(model, serviceProvider: null, items: null)
            {
                MemberName = propertyName
            };
            var results = new List<ValidationResult>();
            var propertyValue = typeof(T).GetProperty(propertyName)?.GetValue(model);
            System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(propertyValue, context, results);
            return results;
        }
    }
}
