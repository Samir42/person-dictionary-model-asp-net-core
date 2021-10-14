using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PersonDictionaryModel.Core.Model.API;
using PersonDictionaryModel.Core.Model.Models.Person;
using PersonDictionaryModel.Resources;
using PersonDictionaryModel.Resources.Resources;
using PersonDictionaryModel.Web.Framework.Results;
using System.IO;
using System.Linq;
using System.Net;
using static PersonDictionaryModel.Core.Model.Constants.PersonModelConstants;

namespace PersonDictionaryModel.Web.Filters
{
    public class ValidatePersonModelAttribute : ActionFilterAttribute
    {
        public IStringLocalizer<ValidationErrorsResource> _localizer { get; }
        private ILogger<ValidatePersonModelAttribute> _logger;

        public readonly string[] _extensions = new string[] { ".jpg", ".png" };


        public ValidatePersonModelAttribute(
            IStringLocalizer<ValidationErrorsResource> localizer, 
            ILogger<ValidatePersonModelAttribute> logger)
        {
            _localizer = localizer;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            int statusCode = (int)HttpStatusCode.InternalServerError;
            var errorCode = ApiErrorCodeKeys.E10001;
            var response = new ValidationErrorResponse();

            var argumentValue = context.ActionArguments.FirstOrDefault().Value;

            var isCreatePersonDto = argumentValue is IPersonDto;

            if (isCreatePersonDto)
            {
                var personDtoToValidate = argumentValue as IPersonDto;

                var errorMessageOrDefault = ValidatePersonDto(personDtoToValidate);

                if (!string.IsNullOrEmpty(errorMessageOrDefault))
                {
                    errorCode = ApiErrorCodeKeys.E10005;
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response.UserMessage = errorMessageOrDefault;
                    response.ErrorCode = errorCode;

                    _logger.LogError("ERROR CODE: {0} - {1}", errorCode, response.UserMessage);

                    context.Result = new CustomObjectResult(statusCode, response);
                }

            }

        }

        public string ValidatePersonDto(IPersonDto personDtoToValidate)
        {
            if (personDtoToValidate.FirstName is null ||
                personDtoToValidate.FirstName.Length < MinFirstNameLength ||
                personDtoToValidate.FirstName.Length > MaxFirstNameLength)
            {
                return _localizer
                    .GetString(ValidationErrorKeys.InacceptableLengthForProperty,
                    FirstName,
                    MinFirstNameLength,
                    MaxFirstNameLength);
            }

            if (personDtoToValidate.LastName is null ||
                personDtoToValidate.LastName.Length < MinLastNameLength ||
                personDtoToValidate.LastName.Length > MaxLastNameLength)
            {
                return _localizer
                    .GetString(ValidationErrorKeys.InacceptableLengthForProperty,
                    LastName,
                    MinLastNameLength,
                    MaxLastNameLength);
            }

            if (personDtoToValidate.PersonalNumber is null ||
                personDtoToValidate.PersonalNumber.Length != MaxPersonalPhoneNumberLength)
            {
                return _localizer
                    .GetString(ValidationErrorKeys.InacceptableLengthForNumber,
                    MaxPersonalPhoneNumberLength);
            }

            var lastValidationStepResult = ValidatePhoto(personDtoToValidate.Photo);

            return lastValidationStepResult;
        }

        private string ValidatePhoto(IFormFile formFile)
        {
            if (formFile is null)
                return string.Empty;
            else 
            {
                var extension = Path.GetExtension(formFile.FileName);
                if (!_extensions.Contains(extension.ToLower()))
                {
                    return _localizer
                    .GetString(ValidationErrorKeys.UnallowedFileType,
                    extension);
                }
            }

            if (formFile.Length > 4194304)
            {
                return _localizer
                    .GetString(ValidationErrorKeys.UnallowedFileSize,
                    4);
            }

            return string.Empty;
        }

    }
}
