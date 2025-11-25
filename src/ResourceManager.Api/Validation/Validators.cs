using FluentValidation;
using ResourceManager.Api.Contracts;
using ResourceManager.Domain;

namespace ResourceManager.Api.Validation;

public class CreateResourceRequestValidator : AbstractValidator<CreateResourceRequest>
{
    public CreateResourceRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty().MaximumLength(120);
        RuleFor(r => r.Location).NotEmpty().MaximumLength(120);
        RuleFor(r => r.Type).IsInEnum();
        RuleFor(r => r.Capacity).GreaterThan(0).LessThanOrEqualTo(500);
    }
}

public class UpdateResourceRequestValidator : AbstractValidator<UpdateResourceRequest>
{
    public UpdateResourceRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty().MaximumLength(120);
        RuleFor(r => r.Location).NotEmpty().MaximumLength(120);
        RuleFor(r => r.Type).IsInEnum();
        RuleFor(r => r.Capacity).GreaterThan(0).LessThanOrEqualTo(500);
    }
}

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(r => r.ResourceId).NotEmpty();
        RuleFor(r => r.RequesterId).NotEmpty();
        RuleFor(r => r.Purpose).NotEmpty().MaximumLength(200);

        RuleFor(r => r.EndsAt)
            .GreaterThan(r => r.StartsAt)
            .WithMessage("A booking has to end after it starts.");

        RuleFor(r => r)
            .Must(r => BookingRules.IsValidRange(r.StartsAt, r.EndsAt))
            .WithMessage($"A booking cannot be longer than {BookingRules.MaxHours} hours.")
            .When(r => r.EndsAt > r.StartsAt);
    }
}

public class CancelBookingRequestValidator : AbstractValidator<CancelBookingRequest>
{
    public CancelBookingRequestValidator()
    {
        RuleFor(r => r.CancelledBy).NotEmpty();
        RuleFor(r => r.Reason).NotEmpty().MaximumLength(200);
    }
}
