-- =============================================================================
-- Torres - persistence schema (PostgreSQL 14+)
--
-- Derived from Documento-Base-Consolidada-Torres.md and the use case descriptions,
-- version 4 (10 sep 2026).
-- Ten entities plus one derived view and five read-only rule catalogues.
--
-- What is deliberately NOT here, and why (see the analysis for the full reasoning):
--   * Room membership, room host and room ranking: they include guests, of whom
--     nothing is stored, and rooms are not resumed after a restart. Server memory.
--   * Guests: no table and no row. They exist only inside the live match state.
--   * Move history, sessions, tokens, per-round score breakdown: nobody consumes them.
--   * The room ranking: it cannot be produced by a query because guests take part
--     in it and guests are not stored.
--
-- Identifiers are in English (coding standard, rule 1) and use snake_case, which is
-- the PostgreSQL convention and avoids quoted identifiers.
-- =============================================================================

DROP SCHEMA IF EXISTS torres CASCADE;
CREATE SCHEMA torres;
SET search_path TO torres;

-- =============================================================================
-- 1. PLAYER
-- =============================================================================

-- Persistent player identity. It also holds the profile, which turned out to be
-- the username (already needed to log in and to search players) plus the avatar.
CREATE TABLE player
(
    player_id        integer      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    username         varchar(20)  NOT NULL,
    email            varchar(254) NOT NULL,
    password_hash    varchar(255) NOT NULL,
    avatar_reference varchar(255) NULL,
    created_at       timestamptz  NOT NULL DEFAULT now(),

    CONSTRAINT ck_player_username_format CHECK (username ~ '^[A-Za-z0-9_]{3,20}$'),
    CONSTRAINT ck_player_email_shape     CHECK (position('@' in email) > 1)
);

COMMENT ON TABLE  player                  IS 'Persistent player identity and profile. Guests are never stored.';
COMMENT ON COLUMN player.username         IS 'Log in credential, displayed name, and the only way to search a player.';
COMMENT ON COLUMN player.password_hash    IS 'BCrypt hash (60 chars, salt included). The plain password is never stored nor logged.';
COMMENT ON COLUMN player.avatar_reference IS 'Where the server stored the uploaded image. The file itself lives outside the database.';

-- Case-insensitive uniqueness: "Ana" and "ana" must not be two accounts.
CREATE UNIQUE INDEX ux_player_username_lower ON player (lower(username));
CREATE UNIQUE INDEX ux_player_email_lower    ON player (lower(email));

-- =============================================================================
-- 2. FRIENDSHIP
-- =============================================================================

-- One row per pair, never two. The direction only matters while the request is
-- pending; a rejected request is deleted, and there is no blocking.
CREATE TABLE friendship
(
    requester_id integer     NOT NULL,
    addressee_id integer     NOT NULL,
    status       varchar(8)  NOT NULL,
    requested_at timestamptz NOT NULL DEFAULT now(),
    responded_at timestamptz NULL,

    CONSTRAINT pk_friendship PRIMARY KEY (requester_id, addressee_id),

    CONSTRAINT fk_friendship_requester
        FOREIGN KEY (requester_id) REFERENCES player (player_id) ON DELETE CASCADE,
    CONSTRAINT fk_friendship_addressee
        FOREIGN KEY (addressee_id) REFERENCES player (player_id) ON DELETE CASCADE,

    CONSTRAINT ck_friendship_status CHECK (status IN ('pending', 'accepted')),
    CONSTRAINT ck_friendship_self   CHECK (requester_id <> addressee_id),
    CONSTRAINT ck_friendship_answer CHECK ((status = 'pending'  AND responded_at IS NULL)
                                        OR (status = 'accepted' AND responded_at IS NOT NULL))
);

COMMENT ON TABLE friendship IS 'Friend request and accepted friendship. A rejected request is deleted, not kept.';

-- The reversed pair is the same pair: A->B and B->A cannot coexist.
CREATE UNIQUE INDEX ux_friendship_pair
    ON friendship (least(requester_id, addressee_id), greatest(requester_id, addressee_id));

-- Serves "friend requests waiting for me" and "my friends".
CREATE INDEX ix_friendship_addressee ON friendship (addressee_id);

-- =============================================================================
-- 3. ROOM
-- =============================================================================

-- A room is where players gather. It stays alive while somebody is inside; who is
-- inside is server memory, because guests can be members and even hosts.
-- An empty room is closed, not deleted: its matches point at it.
CREATE TABLE room
(
    room_id    integer     GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code       varchar(4)  NOT NULL,
    visibility varchar(7)  NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    closed_at  timestamptz NULL,

    -- Four alphanumeric characters, with the confusable ones left out of the
    -- alphabet: no O, I or L, and no 0 or 1. The code is typed from an email.
    CONSTRAINT ck_room_code_format CHECK (code ~ '^[ABCDEFGHJKMNPQRSTUVWXYZ2-9]{4}$'),
    CONSTRAINT ck_room_visibility  CHECK (visibility IN ('public', 'private')),
    CONSTRAINT ck_room_closed_at   CHECK (closed_at IS NULL OR closed_at >= created_at)
);

COMMENT ON TABLE  room           IS 'Meeting place. Closed when it runs out of players, and on server start up.';
COMMENT ON COLUMN room.code      IS 'Join code: four alphanumeric characters, confusable ones excluded. Unique only among open rooms.';
COMMENT ON COLUMN room.visibility IS 'Decides one single thing: whether the room shows up in the listing. Fixed when the room is created and never changed.';
COMMENT ON COLUMN room.closed_at  IS 'Set when the room is closed. A closed room admits nobody and releases its code.';

-- The code is unique only among open rooms, so it can be short and reused.
CREATE UNIQUE INDEX ux_room_code_open ON room (code) WHERE closed_at IS NULL;

-- =============================================================================
-- 4. ROOM INVITATION
-- =============================================================================

-- Only pending invitations exist: answering one deletes it, and that is what makes
-- an invitation single use. Always addressed to an account, so the email carries no
-- secret of its own: it carries the room code, and the redemption checks that whoever
-- presents it is the addressed account.
CREATE TABLE room_invitation
(
    room_id            integer     NOT NULL,
    inviter_player_id  integer     NOT NULL,
    invited_player_id  integer     NOT NULL,
    channel            varchar(8)  NOT NULL,
    created_at         timestamptz NOT NULL DEFAULT now(),

    CONSTRAINT fk_room_invitation_room
        FOREIGN KEY (room_id) REFERENCES room (room_id) ON DELETE CASCADE,
    CONSTRAINT fk_room_invitation_inviter
        FOREIGN KEY (inviter_player_id) REFERENCES player (player_id) ON DELETE CASCADE,
    CONSTRAINT fk_room_invitation_invited
        FOREIGN KEY (invited_player_id) REFERENCES player (player_id) ON DELETE CASCADE,

    CONSTRAINT ck_room_invitation_channel CHECK (channel IN ('email', 'in_game')),
    CONSTRAINT ck_room_invitation_self    CHECK (inviter_player_id <> invited_player_id),

    -- The pair IS the identity: only one pending invitation per room and invitee.
    -- No surrogate key, because nothing references an invitation.
    CONSTRAINT pk_room_invitation PRIMARY KEY (room_id, invited_player_id)
);

COMMENT ON TABLE  room_invitation         IS 'Pending invitation to a room, announced by email or inside the game. Deleted when answered or when the room closes.';
COMMENT ON COLUMN room_invitation.channel IS 'How the invitee was told. The email carries the room code; the invitation itself is in the invitee inbox either way.';

-- Serves "invitations waiting for me", including those received while offline.
CREATE INDEX ix_room_invitation_invited ON room_invitation (invited_player_id);

-- =============================================================================
-- 5. MATCH
-- =============================================================================

-- A match, both while it is played and after it is archived. Every match is born
-- inside a room. Deleting a room is not allowed while it has matches: rooms are
-- closed, never deleted.
CREATE TABLE match
(
    match_id     integer     GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    room_id      integer     NOT NULL,
    status       varchar(12) NOT NULL,
    player_count smallint    NOT NULL,
    started_at   timestamptz NOT NULL DEFAULT now(),
    finished_at  timestamptz NULL,
    end_reason   varchar(12) NULL,

    CONSTRAINT fk_match_room
        FOREIGN KEY (room_id) REFERENCES room (room_id) ON DELETE RESTRICT,

    CONSTRAINT ck_match_status       CHECK (status IN ('in_progress', 'finished')),
    CONSTRAINT ck_match_player_count CHECK (player_count BETWEEN 2 AND 4),
    CONSTRAINT ck_match_end_reason   CHECK (end_reason IS NULL
                                         OR end_reason IN ('completed', 'abandoned', 'interrupted')),
    CONSTRAINT ck_match_interval     CHECK (finished_at IS NULL OR finished_at >= started_at),

    -- A match in progress has no ending; a finished one has both ending and reason.
    CONSTRAINT ck_match_ending CHECK ((status = 'in_progress' AND finished_at IS NULL     AND end_reason IS NULL)
                                   OR (status = 'finished'    AND finished_at IS NOT NULL AND end_reason IS NOT NULL))
);

COMMENT ON TABLE  match              IS 'A match, in progress or archived. The sequence of moves is never stored.';
COMMENT ON COLUMN match.player_count IS 'How many players sat down, 2 to 4. Not derivable: guests leave no row and a deleted account takes its own away.';
COMMENT ON COLUMN match.end_reason   IS 'completed = three rounds played; abandoned = fewer than two players left; interrupted = nobody came back after a server crash.';

-- Serves "which matches were left in progress" after a restart.
CREATE INDEX ix_match_in_progress ON match (match_id) WHERE status = 'in_progress';

-- Serves the history, which is ordered by date.
CREATE INDEX ix_match_finished_at ON match (finished_at DESC) WHERE status = 'finished';

-- =============================================================================
-- 6. MATCH PARTICIPANT
-- =============================================================================

-- One player with an account inside one match. Guests never get a row, so seat
-- numbers and final positions may have gaps: if a guest won, no row holds place 1.
CREATE TABLE match_participant
(
    match_id       integer  NOT NULL,
    seat_number    smallint NOT NULL,
    player_id     integer  NOT NULL,
    final_score    integer  NULL,
    final_position smallint NULL,
    was_withdrawn  boolean  NOT NULL DEFAULT false,

    CONSTRAINT pk_match_participant PRIMARY KEY (match_id, seat_number),

    CONSTRAINT fk_match_participant_match
        FOREIGN KEY (match_id) REFERENCES match (match_id) ON DELETE CASCADE,
    CONSTRAINT fk_match_participant_player
        FOREIGN KEY (player_id) REFERENCES player (player_id) ON DELETE CASCADE,

    CONSTRAINT ck_match_participant_seat     CHECK (seat_number BETWEEN 1 AND 4),
    CONSTRAINT ck_match_participant_position CHECK (final_position IS NULL OR final_position BETWEEN 1 AND 4),
    CONSTRAINT ck_match_participant_score    CHECK (final_score IS NULL OR final_score >= 0),

    -- Either the match is still being played, or both results are already written.
    CONSTRAINT ck_match_participant_result CHECK ((final_score IS NULL     AND final_position IS NULL)
                                               OR (final_score IS NOT NULL AND final_position IS NOT NULL)),

    CONSTRAINT uq_match_participant_player  UNIQUE (match_id, player_id),
    CONSTRAINT uq_match_participant_position UNIQUE (match_id, final_position)
);

COMMENT ON TABLE  match_participant                IS 'A player with an account inside a match, and that player result.';
COMMENT ON COLUMN match_participant.seat_number    IS 'Player identifier inside the match. The only player id written to the event log.';
COMMENT ON COLUMN match_participant.final_position IS '1 = winner. Empty until the match ends. Stored, not derived: the tie break is resolved by the domain.';
COMMENT ON COLUMN match_participant.was_withdrawn  IS 'Left the match before it ended: exceeded the reconnection window (rules 5.2) or surrendered. Same effect either way, and the match still counts for the ranking.';

-- Serves "history of player X".
CREATE INDEX ix_match_participant_player ON match_participant (player_id);

-- =============================================================================
-- 7. MATCH STATE
-- =============================================================================

-- The live state of a match in progress, written when each turn closes and deleted
-- when the match ends. It exists only so that a match can be resumed after a crash.
--
-- Only the server reads state_document, and it reads it whole: no query ever filters
-- or joins on the board, so normalising it would buy nothing and would spread the
-- rules of the game across the schema. The board belongs to the domain layer.
--
-- "State only exists while the match is in progress" is enforced by the operation
-- that ends the match, not by the engine: it deletes this row in the same transaction.
CREATE TABLE match_state
(
    match_id           integer     PRIMARY KEY,
    round_number       smallint    NOT NULL,
    turn_number        smallint    NOT NULL,
    active_seat_number smallint    NOT NULL,
    state_document     jsonb       NOT NULL,
    saved_at           timestamptz NOT NULL DEFAULT now(),

    CONSTRAINT fk_match_state_match
        FOREIGN KEY (match_id) REFERENCES match (match_id) ON DELETE CASCADE,

    CONSTRAINT ck_match_state_round CHECK (round_number BETWEEN 1 AND 3),
    CONSTRAINT ck_match_state_turn  CHECK (turn_number > 0),
    CONSTRAINT ck_match_state_seat  CHECK (active_seat_number BETWEEN 1 AND 4)
);

COMMENT ON TABLE  match_state                IS 'Live state of a match in progress. Written at every turn close, deleted when the match ends.';
COMMENT ON COLUMN match_state.round_number   IS 'Round the saved state is about to play.';
COMMENT ON COLUMN match_state.state_document IS 'Towers and castles, knights, king, cards drawn and used, accumulated score, and the seats of the guests playing, with their alias and seat ticket.';
COMMENT ON COLUMN match_state.saved_at       IS 'Last turn close. The turn clock is not stored: a resumed turn starts again with its full 90 seconds.';

-- =============================================================================
-- 8. PASSWORD RECOVERY
-- =============================================================================

-- Proof that whoever asks to change a password owns the account email. The system
-- emails a code, the user types it inside the application and sets a new password.
-- There is no link: the client is a MonoGame desktop application with no web stack,
-- so a link would have nothing to redeem it.
CREATE TABLE password_recovery
(
    player_id integer     PRIMARY KEY,
    code       varchar(16) NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),

    CONSTRAINT fk_password_recovery_player
        FOREIGN KEY (player_id) REFERENCES player (player_id) ON DELETE CASCADE
);

COMMENT ON TABLE  password_recovery            IS 'Single use recovery code emailed to the account owner. Deleted when used.';
COMMENT ON COLUMN password_recovery.code       IS 'Never written to the event log: it is a credential (coding standard, log4net rules).';
COMMENT ON COLUMN password_recovery.created_at IS 'The code is valid for five minutes from this instant. Expiry is checked by the operation, not by the engine.';

-- =============================================================================
-- 9. REPORT
-- =============================================================================

-- A record that somebody wrote an offensive chat message. The chat itself is never
-- stored, so without the copy of the text the report would have no substance.
-- Only accounts report, and only accounts are reported: guests are outside the
-- whole regime, because nothing of a guest is stored.
--
-- What accumulates towards a sanction is OCCASIONS, not individual reports: every
-- distinct match in which a player is reported counts one, and all the reports made
-- in one room outside any match count one as well. That is why room_id is mandatory
-- and match_id is not: the pair is what identifies the occasion.
CREATE TABLE report
(
    report_id           integer     GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    reporter_player_id integer     NOT NULL,
    reported_player_id integer     NOT NULL,
    room_id             integer     NOT NULL,
    match_id            integer     NULL,
    reported_text       varchar(200) NOT NULL,
    created_at          timestamptz NOT NULL DEFAULT now(),

    CONSTRAINT fk_report_reporter
        FOREIGN KEY (reporter_player_id) REFERENCES player (player_id) ON DELETE CASCADE,
    CONSTRAINT fk_report_reported
        FOREIGN KEY (reported_player_id) REFERENCES player (player_id) ON DELETE CASCADE,
    CONSTRAINT fk_report_room
        FOREIGN KEY (room_id) REFERENCES room (room_id) ON DELETE CASCADE,
    CONSTRAINT fk_report_match
        FOREIGN KEY (match_id) REFERENCES match (match_id) ON DELETE CASCADE,

    CONSTRAINT ck_report_self CHECK (reporter_player_id <> reported_player_id)
);

COMMENT ON TABLE  report               IS 'An offensive chat message reported by one account against another. Deleted with either account.';
COMMENT ON COLUMN report.reported_text IS 'Copy of the message, at most 200 characters, the chat limit. The chat is not stored, so this copy is the only evidence.';
COMMENT ON COLUMN report.match_id      IS 'Empty when the message was written in the room outside any match. Room plus match is what identifies an occasion.';

-- Serves "how many occasions does this account accumulate".
CREATE INDEX ix_report_reported ON report (reported_player_id);

-- =============================================================================
-- 10. SANCTION
-- =============================================================================

-- A ban on an account, current or past. Sanctions are automatic and reached by
-- threshold: there is no administration role and nobody reviews or lifts them.
--
-- Five occasions cross the threshold. The ladder is level 1, 2 and 3, whose
-- durations live in sanction_level; the fourth crossing is permanent. The count of
-- previous sanctions is not stored: it is counted over the rows of the account.
CREATE TABLE sanction
(
    sanction_id     integer     GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    player_id      integer     NOT NULL,
    level           smallint    NULL,
    is_permanent    boolean     NOT NULL DEFAULT false,
    threshold_at    timestamptz NOT NULL,
    effective_from  timestamptz NOT NULL,
    effective_until timestamptz NULL,

    CONSTRAINT fk_sanction_player
        FOREIGN KEY (player_id) REFERENCES player (player_id) ON DELETE CASCADE,

    CONSTRAINT ck_sanction_effective CHECK (effective_from >= threshold_at),
    CONSTRAINT ck_sanction_interval  CHECK (effective_until IS NULL OR effective_until > effective_from),

    -- A temporary sanction has a level and an end; a permanent one has neither.
    CONSTRAINT ck_sanction_kind CHECK ((is_permanent = false AND level IS NOT NULL AND effective_until IS NOT NULL)
                                    OR (is_permanent = true  AND level IS NULL     AND effective_until IS NULL))
);

COMMENT ON TABLE  sanction                 IS 'Ban on an account. Automatic, by threshold of reported occasions. Deleted with the account.';
COMMENT ON COLUMN sanction.threshold_at    IS 'When the fifth occasion was reached.';
COMMENT ON COLUMN sanction.effective_from  IS 'When the ban starts to apply, and when its duration starts counting. Later than threshold_at when the threshold was crossed during a match: the player finishes it first.';
COMMENT ON COLUMN sanction.effective_until IS 'Empty when permanent. A temporary ban stops applying on its own, because nobody can lift it.';

-- Serves "does this account have a sanction in force right now".
CREATE INDEX ix_sanction_player_current ON sanction (player_id, effective_until);

-- =============================================================================
-- 11. GLOBAL RANKING (derived)
-- =============================================================================

-- The global ranking stores nothing of its own: every number comes from matches
-- already archived. Matches played with guests count; guests themselves cannot
-- appear because they are not stored. Interrupted matches have no result.
--
-- The room ranking has no view here on purpose: it includes guests, so no query
-- could produce it, and it dies with the room. The server keeps it in memory.
CREATE VIEW player_ranking AS
SELECT a.player_id,
       a.username,
       count(*)                                     AS matches_played,
       count(*) FILTER (WHERE p.final_position = 1) AS wins,
       sum(p.final_score)                           AS total_score,
       max(p.final_score)                           AS best_score,
       max(m.finished_at)                           AS last_match_at
FROM player a
         JOIN match_participant p ON p.player_id = a.player_id
         JOIN match m ON m.match_id = p.match_id
WHERE m.status = 'finished'
  AND m.end_reason <> 'interrupted'
  AND p.final_position IS NOT NULL
GROUP BY a.player_id, a.username;

COMMENT ON VIEW player_ranking IS 'Global ranking, ordered by wins in the query: ORDER BY wins DESC, total_score DESC, matches_played ASC.';

-- =============================================================================
-- 12. RULE PARAMETERS (read only catalogues)
--
-- The only numbers the rules document itself declares as variable. Kept here so
-- they can change without touching Game.Domain or recompiling. They are read by
-- the services layer and passed to the domain as arguments (coding standard 2.2).
-- Values are loaded by seed-configuration.sql.
-- =============================================================================

-- Rules 1.3, first summary card table: turns per round.
CREATE TABLE turn_layout
(
    player_count smallint NOT NULL,
    round_number smallint NOT NULL,
    turn_count   smallint NOT NULL,

    CONSTRAINT pk_turn_layout PRIMARY KEY (player_count, round_number),
    CONSTRAINT ck_turn_layout_players CHECK (player_count BETWEEN 2 AND 4),
    CONSTRAINT ck_turn_layout_round   CHECK (round_number BETWEEN 1 AND 3),
    CONSTRAINT ck_turn_layout_turns   CHECK (turn_count > 0)
);

-- Rules 1.3, second summary card table: buildings handed to each player.
CREATE TABLE build_allowance
(
    player_count   smallint NOT NULL,
    round_number   smallint NOT NULL,
    turn_number    smallint NOT NULL,
    building_count smallint NOT NULL,

    CONSTRAINT pk_build_allowance PRIMARY KEY (player_count, round_number, turn_number),
    CONSTRAINT ck_build_allowance_turn      CHECK (turn_number > 0),
    CONSTRAINT ck_build_allowance_buildings CHECK (building_count >= 0),
    CONSTRAINT fk_build_allowance_layout
        FOREIGN KEY (player_count, round_number) REFERENCES turn_layout (player_count, round_number)
);

-- Rules 3.2: action point cost of each action.
CREATE TABLE action_cost
(
    action_code       varchar(32) PRIMARY KEY,
    action_point_cost smallint    NOT NULL,

    CONSTRAINT ck_action_cost_value CHECK (action_point_cost >= 0)
);

-- Duration of each step of the ban ladder. The fourth crossing of the threshold is
-- permanent and has no row: a permanent ban has no duration.
CREATE TABLE sanction_level
(
    level        smallint PRIMARY KEY,
    ban_duration interval NOT NULL,

    CONSTRAINT ck_sanction_level_value    CHECK (level BETWEEN 1 AND 3),
    CONSTRAINT ck_sanction_level_duration CHECK (ban_duration > interval '0')
);

COMMENT ON TABLE sanction_level IS 'Ladder of temporary bans: 5 hours, 1 day, 3 days. The fourth crossing is permanent and is not a level.';

-- The remaining numbers the system must be able to change without recompiling.
CREATE TABLE system_parameter
(
    parameter_code  varchar(48) PRIMARY KEY,
    parameter_value varchar(32) NOT NULL
);

COMMENT ON TABLE system_parameter IS 'Read only configuration: report threshold, chat limits, heartbeat, room inactivity and recovery code lifetime.';

-- Declared here because sanction is created before its catalogue.
ALTER TABLE sanction
    ADD CONSTRAINT fk_sanction_level FOREIGN KEY (level) REFERENCES sanction_level (level);
