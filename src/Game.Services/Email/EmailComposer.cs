using System;
using System.Globalization;

namespace Game.Services
{
    public static class EmailComposer
    {
        private const string RegistrationSubject = "Torres: tu código para crear la cuenta / your code to create the account";
        private const string RegistrationBody = "Hola:\n\nTu código para crear tu cuenta de Torres es {0}. Escríbelo en el juego antes de {1} minutos.\n\nSi no pediste crear una cuenta, ignora este correo.\n\n---\n\nHello:\n\nYour code to create your Torres account is {0}. Type it in the game within {1} minutes.\n\nIf you did not ask to create an account, ignore this email.";
        private const string RecoverySubject = "Torres: tu código para recuperar el acceso / your code to recover access";
        private const string RecoveryBody = "Hola:\n\nTu código para recuperar el acceso a tu cuenta de Torres es {0}. Escríbelo en el juego antes de {1} minutos.\n\nSi no lo pediste, ignora este correo: tu contraseña no cambia.\n\n---\n\nHello:\n\nYour code to recover access to your Torres account is {0}. Type it in the game within {1} minutes.\n\nIf you did not ask for it, ignore this email: your password does not change.";
        private const string InvitationSubject = "Torres: {0} te invita a su sala / {0} invites you to their room";
        private const string InvitationBody = "Hola:\n\n{0} te invita a su sala de Torres. El código de la sala es {1}.\n\nEntra al juego y responde la invitación desde «Invitaciones», o escribe el código en «Salas».\n\n---\n\nHello:\n\n{0} invites you to their Torres room. The room code is {1}.\n\nOpen the game and answer the invitation from \"Invitations\", or type the code in \"Rooms\".";

        public static EmailMessage ComposeRegistrationCode(string recipientAddress, string code, int validMinutes)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(recipientAddress);
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(validMinutes);

            string body = string.Format(CultureInfo.InvariantCulture, RegistrationBody, code, validMinutes);
            var registrationEmail = new EmailMessage(recipientAddress, RegistrationSubject, body);

            return registrationEmail;
        }

        public static EmailMessage ComposeRecoveryCode(string recipientAddress, string code, int validMinutes)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(recipientAddress);
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(validMinutes);

            string body = string.Format(CultureInfo.InvariantCulture, RecoveryBody, code, validMinutes);
            var recoveryEmail = new EmailMessage(recipientAddress, RecoverySubject, body);

            return recoveryEmail;
        }

        public static EmailMessage ComposeRoomInvitation(string recipientAddress, string roomCode, string inviterUsername)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(recipientAddress);
            ArgumentException.ThrowIfNullOrWhiteSpace(roomCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(inviterUsername);

            string subject = string.Format(CultureInfo.InvariantCulture, InvitationSubject, inviterUsername);
            string body = string.Format(CultureInfo.InvariantCulture, InvitationBody, inviterUsername, roomCode);
            var invitationEmail = new EmailMessage(recipientAddress, subject, body);

            return invitationEmail;
        }
    }
}
