﻿// ReSharper disable InconsistentNaming
namespace DreamFactory.Model.User
{
    /// <summary>
    /// PasswordResponse.
    /// </summary>
    public class PasswordResponse
    {
        /// <summary>
        /// User's security question, returned on reset request when no email confirmation required.
        /// </summary>
        public string security_question { get; set; }

        /// <summary>
        /// True if password updated or reset request granted via email confirmation.
        /// </summary>
        public bool? success { get; set; }
    }
}
