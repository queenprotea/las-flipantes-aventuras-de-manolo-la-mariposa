-- =============================================================================
-- Torres - seed data for ZONE B (rule parameters).
-- Values transcribed from the rules document, version 3.0:
--   turn_layout     <- rules 1.3, "Turnos por ronda"
--   build_allowance <- rules 1.3, "Construcciones que recibe cada jugador en cada turno"
--   action_cost     <- rules 3.2, "Costes de las acciones"
-- Values fixed by the project, in Documento-Base-Consolidada-Torres.md:
--   sanction_level   <- ban ladder
--   system_parameter <- thresholds, limits and deadlines
-- Run after schema.sql.
-- =============================================================================

SET search_path TO torres;

INSERT INTO turn_layout (player_count, round_number, turn_count) VALUES
    (2, 1, 4), (2, 2, 4), (2, 3, 4),
    (3, 1, 4), (3, 2, 3), (3, 3, 3),
    (4, 1, 4), (4, 2, 3), (4, 3, 3);

INSERT INTO build_allowance (player_count, round_number, turn_number, building_count) VALUES
    -- 2 players: 3 buildings in every turn of every round.
    (2, 1, 1, 3), (2, 1, 2, 3), (2, 1, 3, 3), (2, 1, 4, 3),
    (2, 2, 1, 3), (2, 2, 2, 3), (2, 2, 3, 3), (2, 2, 4, 3),
    (2, 3, 1, 3), (2, 3, 2, 3), (2, 3, 3, 3), (2, 3, 4, 3),
    -- 3 players: 3 buildings in the first two turns, 2 in the remaining ones.
    (3, 1, 1, 3), (3, 1, 2, 3), (3, 1, 3, 2), (3, 1, 4, 2),
    (3, 2, 1, 3), (3, 2, 2, 3), (3, 2, 3, 2),
    (3, 3, 1, 3), (3, 3, 2, 3), (3, 3, 3, 2),
    -- 4 players: 1 building in every turn.
    (4, 1, 1, 1), (4, 1, 2, 1), (4, 1, 3, 1), (4, 1, 4, 1),
    (4, 2, 1, 1), (4, 2, 2, 1), (4, 2, 3, 1),
    (4, 3, 1, 1), (4, 3, 2, 1), (4, 3, 3, 1);

INSERT INTO action_cost (action_code, action_point_cost) VALUES
    ('place_knight',       2),
    ('move_knight',        1),
    ('raise_knight_level', 1),
    ('place_building',     1),
    ('draw_action_card',   1),
    ('play_action_card',   0);

-- Ban ladder. The fourth crossing of the threshold is permanent and has no row.
INSERT INTO sanction_level (level, ban_duration) VALUES
    (1, interval '5 hours'),
    (2, interval '1 day'),
    (3, interval '3 days');

-- Everything else that must be changeable without recompiling. Stored as text so
-- that one table holds counts, sizes and durations alike; the services layer parses
-- them and passes them to the domain as arguments.
INSERT INTO system_parameter (parameter_code, parameter_value) VALUES
    -- Five distinct occasions with a report cross the threshold.
    ('report_threshold_occasions',   '5'),
    -- Chat: a sliding window, never stored in the database.
    ('chat_message_max_length',      '200'),
    ('chat_messages_kept_per_room',  '200'),
    -- A player marked inactive keeps his seat in the room until this runs out.
    -- Does not apply to a player inside a match, who is ruled by rules 5.2.
    ('room_inactivity_minutes',      '3'),
    -- A match left in progress waits this long from server start up.
    ('match_resume_minutes',         '3'),
    -- Turn clock and, by the same rule, the clock of each initial placement.
    ('turn_seconds',                 '90'),
    -- The emailed recovery code stops being valid after this.
    ('recovery_code_minutes',        '5'),
    -- BCrypt work factor.
    ('password_hash_cost',           '11');

-- Deliberately NOT seeded: 'heartbeat_period_seconds' and 'heartbeat_misses_to_drop'.
-- The project declares that connection loss is detected by heartbeat, with a declared
-- period and a declared number of misses, but it has not fixed those two numbers yet.
-- Add them here when the team does; inventing them would put an undecided value into
-- the database.
