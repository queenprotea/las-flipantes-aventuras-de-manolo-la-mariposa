DROP SCHEMA IF EXISTS torres CASCADE;

CREATE SCHEMA torres;

SET search_path TO torres;

CREATE TABLE player
(
    player_id        integer      GENERATED ALWAYS AS IDENTITY,
    username         varchar(20)  NOT NULL,
    email            varchar(254) NOT NULL,
    password_hash    varchar(255) NOT NULL,
    avatar_reference varchar(255) NULL,
    created_at       timestamptz  NOT NULL DEFAULT now(),

    CONSTRAINT pk_player PRIMARY KEY (player_id),
    CONSTRAINT ck_player_username_format CHECK (username ~ '^[A-Za-z0-9_]{3,20}$'),
    CONSTRAINT ck_player_email_shape     CHECK (position('@' in email) > 1)
);

CREATE UNIQUE INDEX ux_player_username_lower ON player (lower(username));

CREATE UNIQUE INDEX ux_player_email_lower ON player (lower(email));

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

CREATE UNIQUE INDEX ux_friendship_pair
    ON friendship (least(requester_id, addressee_id), greatest(requester_id, addressee_id));

CREATE INDEX ix_friendship_addressee ON friendship (addressee_id);

CREATE TABLE room
(
    room_id    integer     GENERATED ALWAYS AS IDENTITY,
    code       varchar(4)  NOT NULL,
    visibility varchar(7)  NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    closed_at  timestamptz NULL,

    CONSTRAINT pk_room PRIMARY KEY (room_id),
    CONSTRAINT ck_room_code_format CHECK (code ~ '^[ABCDEFGHJKMNPQRSTUVWXYZ2-9]{4}$'),
    CONSTRAINT ck_room_visibility  CHECK (visibility IN ('public', 'private')),
    CONSTRAINT ck_room_closed_at   CHECK (closed_at IS NULL OR closed_at >= created_at)
);

CREATE UNIQUE INDEX ux_room_code_open ON room (code) WHERE closed_at IS NULL;

CREATE TABLE room_invitation
(
    room_id           integer     NOT NULL,
    invited_player_id integer     NOT NULL,
    inviter_player_id integer     NOT NULL,
    channel           varchar(8)  NOT NULL,
    created_at        timestamptz NOT NULL DEFAULT now(),

    CONSTRAINT pk_room_invitation PRIMARY KEY (room_id, invited_player_id),
    CONSTRAINT fk_room_invitation_room
        FOREIGN KEY (room_id) REFERENCES room (room_id) ON DELETE CASCADE,
    CONSTRAINT fk_room_invitation_invited
        FOREIGN KEY (invited_player_id) REFERENCES player (player_id) ON DELETE CASCADE,
    CONSTRAINT fk_room_invitation_inviter
        FOREIGN KEY (inviter_player_id) REFERENCES player (player_id) ON DELETE CASCADE,
    CONSTRAINT ck_room_invitation_channel CHECK (channel IN ('email', 'in_game')),
    CONSTRAINT ck_room_invitation_self    CHECK (inviter_player_id <> invited_player_id)
);

CREATE INDEX ix_room_invitation_invited ON room_invitation (invited_player_id);

CREATE TABLE match
(
    match_id     integer     GENERATED ALWAYS AS IDENTITY,
    room_id      integer     NOT NULL,
    status       varchar(12) NOT NULL,
    player_count smallint    NOT NULL,
    started_at   timestamptz NOT NULL DEFAULT now(),
    finished_at  timestamptz NULL,
    end_reason   varchar(12) NULL,

    CONSTRAINT pk_match PRIMARY KEY (match_id),
    CONSTRAINT fk_match_room
        FOREIGN KEY (room_id) REFERENCES room (room_id) ON DELETE RESTRICT,
    CONSTRAINT ck_match_status       CHECK (status IN ('in_progress', 'finished')),
    CONSTRAINT ck_match_player_count CHECK (player_count BETWEEN 2 AND 4),
    CONSTRAINT ck_match_end_reason   CHECK (end_reason IS NULL
                                         OR end_reason IN ('completed', 'abandoned', 'interrupted')),
    CONSTRAINT ck_match_interval     CHECK (finished_at IS NULL OR finished_at >= started_at),
    CONSTRAINT ck_match_ending CHECK ((status = 'in_progress' AND finished_at IS NULL     AND end_reason IS NULL)
                                   OR (status = 'finished'    AND finished_at IS NOT NULL AND end_reason IS NOT NULL))
);

CREATE INDEX ix_match_in_progress ON match (match_id) WHERE status = 'in_progress';

CREATE INDEX ix_match_finished_at ON match (finished_at DESC) WHERE status = 'finished';

CREATE TABLE match_participant
(
    match_id       integer  NOT NULL,
    seat_number    smallint NOT NULL,
    player_id      integer  NOT NULL,
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
    CONSTRAINT ck_match_participant_result CHECK ((final_score IS NULL     AND final_position IS NULL)
                                               OR (final_score IS NOT NULL AND final_position IS NOT NULL)),
    CONSTRAINT uq_match_participant_player   UNIQUE (match_id, player_id),
    CONSTRAINT uq_match_participant_position UNIQUE (match_id, final_position)
);

CREATE INDEX ix_match_participant_player ON match_participant (player_id);

CREATE TABLE match_state
(
    match_id           integer     NOT NULL,
    round_number       smallint    NOT NULL,
    turn_number        smallint    NOT NULL,
    active_seat_number smallint    NOT NULL,
    state_document     jsonb       NOT NULL,
    saved_at           timestamptz NOT NULL DEFAULT now(),

    CONSTRAINT pk_match_state PRIMARY KEY (match_id),
    CONSTRAINT fk_match_state_match
        FOREIGN KEY (match_id) REFERENCES match (match_id) ON DELETE CASCADE,
    CONSTRAINT ck_match_state_round CHECK (round_number BETWEEN 1 AND 3),
    CONSTRAINT ck_match_state_turn  CHECK (turn_number > 0),
    CONSTRAINT ck_match_state_seat  CHECK (active_seat_number BETWEEN 1 AND 4)
);

CREATE TABLE password_recovery
(
    player_id  integer     NOT NULL,
    code       varchar(16) NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),

    CONSTRAINT pk_password_recovery PRIMARY KEY (player_id),
    CONSTRAINT fk_password_recovery_player
        FOREIGN KEY (player_id) REFERENCES player (player_id) ON DELETE CASCADE
);

CREATE TABLE report
(
    report_id          integer      GENERATED ALWAYS AS IDENTITY,
    reporter_player_id integer      NOT NULL,
    reported_player_id integer      NOT NULL,
    room_id            integer      NOT NULL,
    match_id           integer      NULL,
    reported_text      varchar(200) NOT NULL,
    created_at         timestamptz  NOT NULL DEFAULT now(),

    CONSTRAINT pk_report PRIMARY KEY (report_id),
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

CREATE INDEX ix_report_reported ON report (reported_player_id);

CREATE TABLE sanction_level
(
    level        smallint NOT NULL,
    ban_duration interval NOT NULL,

    CONSTRAINT pk_sanction_level PRIMARY KEY (level),
    CONSTRAINT ck_sanction_level_value    CHECK (level BETWEEN 1 AND 3),
    CONSTRAINT ck_sanction_level_duration CHECK (ban_duration > interval '0')
);

CREATE TABLE sanction
(
    sanction_id     integer     GENERATED ALWAYS AS IDENTITY,
    player_id       integer     NOT NULL,
    level           smallint    NULL,
    is_permanent    boolean     NOT NULL DEFAULT false,
    threshold_at    timestamptz NOT NULL,
    effective_from  timestamptz NOT NULL,
    effective_until timestamptz NULL,

    CONSTRAINT pk_sanction PRIMARY KEY (sanction_id),
    CONSTRAINT fk_sanction_player
        FOREIGN KEY (player_id) REFERENCES player (player_id) ON DELETE CASCADE,
    CONSTRAINT fk_sanction_level
        FOREIGN KEY (level) REFERENCES sanction_level (level),
    CONSTRAINT ck_sanction_effective CHECK (effective_from >= threshold_at),
    CONSTRAINT ck_sanction_interval  CHECK (effective_until IS NULL OR effective_until > effective_from),
    CONSTRAINT ck_sanction_kind CHECK ((is_permanent = false AND level IS NOT NULL AND effective_until IS NOT NULL)
                                    OR (is_permanent = true  AND level IS NULL     AND effective_until IS NULL))
);

CREATE INDEX ix_sanction_player_current ON sanction (player_id, effective_until);

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

CREATE TABLE build_allowance
(
    player_count   smallint NOT NULL,
    round_number   smallint NOT NULL,
    turn_number    smallint NOT NULL,
    building_count smallint NOT NULL,

    CONSTRAINT pk_build_allowance PRIMARY KEY (player_count, round_number, turn_number),
    CONSTRAINT fk_build_allowance_layout
        FOREIGN KEY (player_count, round_number) REFERENCES turn_layout (player_count, round_number),
    CONSTRAINT ck_build_allowance_turn      CHECK (turn_number > 0),
    CONSTRAINT ck_build_allowance_buildings CHECK (building_count >= 0)
);

CREATE TABLE action_cost
(
    action_code       varchar(32) NOT NULL,
    action_point_cost smallint    NOT NULL,

    CONSTRAINT pk_action_cost PRIMARY KEY (action_code),
    CONSTRAINT ck_action_cost_value CHECK (action_point_cost >= 0)
);

CREATE TABLE system_parameter
(
    parameter_code  varchar(48) NOT NULL,
    parameter_value varchar(32) NOT NULL,

    CONSTRAINT pk_system_parameter PRIMARY KEY (parameter_code)
);

CREATE VIEW player_ranking AS
SELECT p.player_id,
       p.username,
       count(*)                                      AS matches_played,
       count(*) FILTER (WHERE mp.final_position = 1) AS wins,
       sum(mp.final_score)                           AS total_score,
       max(mp.final_score)                           AS best_score,
       max(m.finished_at)                            AS last_match_at
FROM player p
         JOIN match_participant mp ON mp.player_id = p.player_id
         JOIN match m ON m.match_id = mp.match_id
WHERE m.status = 'finished'
  AND m.end_reason <> 'interrupted'
  AND mp.final_position IS NOT NULL
GROUP BY p.player_id, p.username;
