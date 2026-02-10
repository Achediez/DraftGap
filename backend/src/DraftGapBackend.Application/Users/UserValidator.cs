using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace DraftGapBackend.Application.Users
{
    /// <summary>
    /// Clase estática para validar los datos de usuario antes de su registro o actualización.
    /// </summary>
    public static class UserValidator
    {
        /// <summary>
        /// Valida todos los campos y devuelve una lista de errores encontrados.
        /// </summary>
        public static List<string> ValidateAll(string userName, string email, string password)
        {
            var errors = new List<string>();

            // Validación de nombre de usuario
            if (string.IsNullOrWhiteSpace(userName))
                errors.Add("El nombre de usuario es obligatorio.");
            else
            {
                if (userName.Any(char.IsWhiteSpace))
                    errors.Add("El nombre de usuario no puede contener espacios.");
                if (!Regex.IsMatch(userName, @"^[a-zA-Z0-9_]+$"))
                    errors.Add("El nombre de usuario solo puede contener letras, números y guiones bajos.");
            }

            // Validación de email
            if (string.IsNullOrWhiteSpace(email))
                errors.Add("El email es obligatorio.");
            else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                errors.Add("El email no es válido.");

            // Validación de contraseña
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                errors.Add("La contraseña debe tener al menos 6 caracteres.");

            return errors;
        }
    }
}
