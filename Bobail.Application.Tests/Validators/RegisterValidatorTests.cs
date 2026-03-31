using FluentValidation.TestHelper;

namespace Bobail.Application.Tests.Validators
{
    public class RegisterValidatorTests
    {
        private readonly RegisterValidator _validator = new();

        [Fact]
        public void Should_Fail_When_Email_Invalid()
        {
            var result = _validator.TestValidate((
             Email: "invalid",
             Password: "123456",
             Nickname: "mihai"
             ));

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Fail_When_Email_Empty()
        {
            var result = _validator.TestValidate((Email: "", Password: "123456", Nickname: "mihai"));

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Fail_When_Password_Too_Short()
        {
            var result = _validator.TestValidate((Email: "test@mail.com", Password: "123", Nickname: "mihai"));

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Fail_When_Nickname_Too_Short()
        {
            var result = _validator.TestValidate((Email: "test@mail.com", Password: "123456", Nickname: "mi"));

            result.ShouldHaveValidationErrorFor(x => x.Nickname);
        }

        [Fact]
        public void Should_Pass_When_All_Valid()
        {
            var result = _validator.TestValidate(("test@mail.com", "123456", "mihai"));

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}