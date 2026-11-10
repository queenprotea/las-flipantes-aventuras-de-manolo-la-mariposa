using System;
using System.Collections.Generic;

using Game.Contracts;

namespace Torres.Client.Session
{
    internal sealed class RoomInvitationInbox
    {
        private readonly List<RoomInvitation> _invitations = new List<RoomInvitation>();

        internal int Revision { get; private set; }

        internal IReadOnlyList<RoomInvitation> Invitations => _invitations;

        internal void Load(PendingInvitationsResponse pendingInvitations)
        {
            ArgumentNullException.ThrowIfNull(pendingInvitations);

            _invitations.Clear();
            _invitations.AddRange(pendingInvitations.Invitations);
            Revision++;
        }

        internal void Add(RoomInvitation invitation)
        {
            ArgumentNullException.ThrowIfNull(invitation);

            _invitations.RemoveAll(keptInvitation => keptInvitation.RoomId == invitation.RoomId);
            _invitations.Insert(0, invitation);
            Revision++;
        }

        internal void Remove(int roomId)
        {
            int removedCount = _invitations.RemoveAll(keptInvitation => keptInvitation.RoomId == roomId);
            if (removedCount > 0)
            {
                Revision++;
            }
        }

        internal void Clear()
        {
            _invitations.Clear();
            Revision++;
        }
    }
}
