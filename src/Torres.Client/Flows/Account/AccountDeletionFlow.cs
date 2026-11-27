using System;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Localization;
using Torres.Client.Threading;

namespace Torres.Client.Flows
{
    internal sealed class AccountDeletionFlow
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PendingRequest<DeleteAccountResult> _deletionRequest = new PendingRequest<DeleteAccountResult>();

        internal AccountDeletionFlow(GameSessionConnection gameSessionConnection)
        {
            _gameSessionConnection = gameSessionConnection;
        }

        internal bool IsDeleting => _deletionRequest.IsWaiting;

        internal void Open()
        {
            _deletionRequest.Forget();
        }

        internal void DeleteAccount()
        {
            if (!_deletionRequest.IsWaiting)
            {
                _deletionRequest.Start(_gameSessionConnection.DeleteAccountAsync());
            }
        }

        internal AccountDeletionOutcome? TakeOutcome()
        {
            FinishedRequest<DeleteAccountResult>? finishedDeletion = _deletionRequest.TakeFinished();
            AccountDeletionOutcome? outcome = null;
            if ((finishedDeletion is not null) && finishedDeletion.IsSuccessful)
            {
                outcome = ToOutcome(finishedDeletion.Reply);
            }
            else if (finishedDeletion is not null)
            {
                outcome = new AccountDeletionOutcome(TextKeys.Common.ServerErrorTitle, false);
            }

            return outcome;
        }

        private static AccountDeletionOutcome ToOutcome(DeleteAccountResult deletionResult)
        {
            AccountDeletionOutcome outcome = deletionResult switch
            {
                DeleteAccountResult.Deleted => new AccountDeletionOutcome(null, true),
                DeleteAccountResult.MatchInProgress => new AccountDeletionOutcome(TextKeys.AccountSettings.MatchInProgress, false),
                DeleteAccountResult.DatabaseUnavailable => new AccountDeletionOutcome(TextKeys.Common.ServerErrorTitle, false),
                _ => throw new ArgumentOutOfRangeException(nameof(deletionResult)),
            };

            return outcome;
        }
    }
}
