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
                throw new ValidationApiException(context.ModelState.GetErrorMessage());
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }
    }
}
