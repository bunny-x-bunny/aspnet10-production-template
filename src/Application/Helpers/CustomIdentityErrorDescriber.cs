using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helpers {
  public class CustomIdentityErrorDescriber : IdentityErrorDescriber {

    public CustomIdentityErrorDescriber() {

    }

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new IdentityError { Code = nameof(PasswordRequiresUniqueChars), Description = $"Пароль должен содержать минимум {uniqueChars} специальных символов" };
    public override IdentityError RecoveryCodeRedemptionFailed() => new IdentityError { Code = nameof(RecoveryCodeRedemptionFailed), Description = $"Код восснатовления был использован некорректно" };
    public override IdentityError UserNotInRole(string role) => new IdentityError() { Code = nameof(UserNotInRole), Description = $"Пользователь не имеет роль {role}." };
    public override IdentityError DefaultError() => new IdentityError { Code = nameof(DefaultError), Description = $"Неизвестная ошибка" };
    public override IdentityError ConcurrencyFailure() => new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Ошибка оптимистичного параллельного выполнения, объект был изменен." };
    public override IdentityError PasswordMismatch() => new IdentityError { Code = nameof(PasswordMismatch), Description = "Неверный пароль." };
    public override IdentityError InvalidToken() => new IdentityError { Code = nameof(InvalidToken), Description = "Неверный токен доступа" };
    public override IdentityError LoginAlreadyAssociated() => new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "Пользователь с такими учетными данными уже существует." };
    public override IdentityError InvalidEmail(string? email) => new IdentityError { Code = nameof(InvalidEmail), Description = $"Некорректный email: '{email}'." };
    public override IdentityError InvalidRoleName(string? role) => new IdentityError { Code = nameof(InvalidRoleName), Description = $"Роль '{role}' не существует." };
    public override IdentityError InvalidUserName(string? userName) => new IdentityError { Code = nameof(InvalidUserName), Description = $"Недопустимый логин: '{userName}', разрешённые символы: 'a-z 'A-Z'." };
    public override IdentityError DuplicateUserName(string userName) => new IdentityError { Code = nameof(DuplicateUserName), Description = $"Логин '{userName}' уже занят." };
    public override IdentityError DuplicateEmail(string email) => new IdentityError { Code = nameof(DuplicateEmail), Description = $"Email '{email}' уже занят." };
    public override IdentityError DuplicateRoleName(string role) => new IdentityError { Code = nameof(DuplicateRoleName), Description = $"Роль '{role}' уже существует." };
    public override IdentityError UserAlreadyHasPassword() => new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "У пользователя уже имеется пароль." };
    public override IdentityError UserLockoutNotEnabled() => new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "Блокировка для этого пользователя отключена." };
    public override IdentityError UserAlreadyInRole(string role) => new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"Пользователь уже имеет роль '{role}'." };
    public override IdentityError PasswordTooShort(int length) => new IdentityError { Code = nameof(PasswordTooShort), Description = $"Длина паролья должна быть минимум {length} символов." };
    public override IdentityError PasswordRequiresNonAlphanumeric() => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Пароль должен содержать минимум один специальный символ." };
    public override IdentityError PasswordRequiresDigit() => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Пароль дожден содержать минимум 1 цифру ('0'-'9')." };
    public override IdentityError PasswordRequiresLower() => new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Пароль дожден содержать минимум 1 прописную букву ('a'-'z')." };
    public override IdentityError PasswordRequiresUpper() => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Пароль дожден содержать минимум 1 заглавную букву ('A'-'Z')." };
  }
}
