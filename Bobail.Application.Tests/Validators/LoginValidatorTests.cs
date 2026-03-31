using FluentValidation.TestHelper;

namespace Bobail.Application.Tests.Validators
{
    public class LoginValidatorTests
    {
        private readonly LoginValidator _validator = new();

        [Fact]
        public void Should_Fail_When_Email_Invalid()
        {
            var result = _validator.TestValidate((Email:"invalid",Password:"123456"));

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Fail_When_Password_Empty()
        {
            var result = _validator.TestValidate((Email:"test@mail.com",Password: ""));

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Pass_When_Valid()
        {
            var result = _validator.TestValidate(("test@mail.com", "123456"));

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}