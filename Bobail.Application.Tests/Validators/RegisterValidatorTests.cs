using Bobail.Application.Validators;
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
                Password: "StrongPass1",
                Nickname: "mihai"));

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Fail_When_Email_Empty()
        {
            var result = _validator.TestValidate((
                Email: "",
                Password: "StrongPass1",
                Nickname: "mihai"));

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Fail_When_Password_Is_Too_Short()
        {
            var result = _validator.TestValidate((
                Email: "test@mail.com",
                Password: "Aa1bc",
                Nickname: "mihai"));

            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage(PasswordPolicy.PasswordRequirementsMessage);
        }

        [Fact]
        public void Should_Fail_When_Password_Has_No_Uppercase_Letter()
        {
            var result = _validator.TestValidate((
                Email: "test@mail.com",
                Password: "strongpass1",
                Nickname: "mihai"));

            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage(PasswordPolicy.PasswordRequirementsMessage);
        }

        [Fact]
        public void Should_Fail_When_Password_Has_No_Lowercase_Letter()
        {
            var result = _validator.TestValidate((
                Email: "test@mail.com",
                Password: "STRONGPASS1",
                Nickname: "mihai"));

            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage(PasswordPolicy.PasswordRequirementsMessage);
        }

        [Fact]
        public void Should_Fail_When_Password_Has_No_Digit()
        {
            var result = _validator.TestValidate((
                Email: "test@mail.com",
                Password: "StrongPass",
                Nickname: "mihai"));

            result.ShouldHaveValidationErrorFor(x => x.Password)
                .WithErrorMessage(PasswordPolicy.PasswordRequirementsMessage);
        }

        [Fact]
        public void Should_Fail_When_Nickname_Too_Short()
        {
            var result = _validator.TestValidate((
                Email: "test@mail.com",
                Password: "StrongPass1",
                Nickname: "mi"));

            result.ShouldHaveValidationErrorFor(x => x.Nickname);
        }

        [Fact]
        public void Should_Pass_When_All_Valid()
        {
            var result = _validator.TestValidate((
                Email: "test@mail.com",
                Password: "StrongPass1",
                Nickname: "mihai"));

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
