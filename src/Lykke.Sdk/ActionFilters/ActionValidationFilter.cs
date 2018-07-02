using Lykke.Common.ApiLibrary.Contract;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.ApiLibrary.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lykke.Sdk.ActionFilters
{
    public class ActionValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errorResponse = ErrorResponseFactory.Create(context.ModelState);
                errorResponse.ErrorMessage = context.ModelState.GetErrorMessage();
                throw new ValidationApiException(errorResponse);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }
    }
}
