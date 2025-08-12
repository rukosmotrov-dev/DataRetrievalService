using DataRetrievalService.Api.Contracts.Data;
using FluentValidation;

namespace DataRetrievalService.Api.Validation.Data
{
    public class CreateDataItemRequestValidator : AbstractValidator<CreateDataItemRequest>
    {
        public CreateDataItemRequestValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Value is required.")
                .MaximumLength(1000).WithMessage("Value must be at most 1000 characters.");
        }
    }
}
