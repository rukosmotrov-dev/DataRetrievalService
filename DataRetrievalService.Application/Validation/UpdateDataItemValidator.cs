using DataRetrievalService.Application.DTOs;
using FluentValidation;

namespace DataRetrievalService.Application.Validation
{
    public class UpdateDataItemValidator : AbstractValidator<UpdateDataItemDto>
    {
        public UpdateDataItemValidator()
        {
            RuleFor(x => x.Value).NotEmpty().MaximumLength(1000);
        }
    }
}
