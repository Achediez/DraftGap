using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace DraftGapBackend.Application.Users
{
    /// Clase estática para validar los datos de usuario antes de su registro o actualización.
    public static class UserValidator
    {
        /// Valida el nombre de usuario: obligatorio, sin espacios y solo letras, números o guiones bajos.
        public static void ValidateUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("El nombre de usuario es obligatorio.");

            // No se permiten espacios en el nombre de usuario
            if (userName.Any(char.IsWhiteSpace))
                throw new ArgumentException("El nombre de usuario no puede contener espacios.");

            // Solo letras, números y guiones bajos
            if (!Regex.IsMatch(userName, @"^[a-zA-Z0-9_]+$"))
                throw new ArgumentException("El nombre de usuario solo puede contener letras, números y guiones bajos.");
        }

        /// Valida el email: obligatorio y formato básico de email.
        public static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email es obligatorio.");

            // Validación básica de formato de email
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("El email no es válido.");
        }

        /// Valida la contraseña: obligatoria y mínimo 6 caracteres.
        public static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                throw new ArgumentException("La contraseña debe tener al menos 6 caracteres.");
            // Puedes añadir más reglas aquí (mayúsculas, símbolos, etc.)
        }
    }
}
