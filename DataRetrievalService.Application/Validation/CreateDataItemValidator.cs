using DataRetrievalService.Application.DTOs;
using FluentValidation;

namespace DataRetrievalService.Application.Validation
{
    public class CreateDataItemValidator : AbstractValidator<CreateDataItemDto>
    {
        public CreateDataItemValidator()
        {
            RuleFor(x => x.Value).NotEmpty().MaximumLength(1000);
        }
    }
}