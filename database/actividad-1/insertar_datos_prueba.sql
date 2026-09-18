SET search_path TO torres;

INSERT INTO turn_layout (player_count, round_number, turn_count) VALUES
    (2, 1, 4), (2, 2, 4), (2, 3, 4),
    (3, 1, 4), (3, 2, 3), (3, 3, 3),
    (4, 1, 4), (4, 2, 3), (4, 3, 3);

INSERT INTO build_allowance (player_count, round_number, turn_number, building_count) VALUES
    (2, 1, 1, 3), (2, 1, 2, 3), (2, 1, 3, 3), (2, 1, 4, 3),
    (2, 2, 1, 3), (2, 2, 2, 3), (2, 2, 3, 3), (2, 2, 4, 3),
    (2, 3, 1, 3), (2, 3, 2, 3), (2, 3, 3, 3), (2, 3, 4, 3),
    (3, 1, 1, 3), (3, 1, 2, 3), (3, 1, 3, 2), (3, 1, 4, 2),
    (3, 2, 1, 3), (3, 2, 2, 3), (3, 2, 3, 2),
    (3, 3, 1, 3), (3, 3, 2, 3), (3, 3, 3, 2),
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

INSERT INTO sanction_level (level, ban_duration) VALUES
    (1, interval '5 hours'),
    (2, interval '1 day'),
    (3, interval '3 days');

INSERT INTO system_parameter (parameter_code, parameter_value) VALUES
    ('report_threshold_occasions',  '5'),
    ('chat_message_max_length',     '200'),
    ('chat_messages_kept_per_room', '200'),
    ('room_inactivity_minutes',     '3'),
    ('match_resume_minutes',        '3'),
    ('turn_seconds',                '90'),
    ('recovery_code_minutes',       '5'),
    ('password_hash_cost',          '11');

INSERT INTO player (username, email, password_hash, avatar_reference, created_at) VALUES
    ('Ana_G',   'ana.gonzalez@example.com',  '$2a$11$nRwEHKsFZCJ1zGcJ0FNaFwEaDPjzQNlVLWtKGFY90m54skdVdIk9p', 'avatars/ana_g.png',   TIMESTAMPTZ '2026-08-20 10:15:00-05'),
    ('Bruno_M', 'bruno.mendez@example.com',  '$2a$11$3iHNzTpR8zDHmpq94GJg6GFl3ivqA5rTM9FZiOdww9IT3xhP1hzru', 'avatars/bruno_m.jpg', TIMESTAMPTZ '2026-08-22 19:40:00-05'),
    ('Carla_R', 'carla.rivas@example.com',   '$2a$11$bRIURbb/8Vfi.QztmOE4wwxwL7xFWGY2SMpEL.RKsBHYuReqs6NM8', NULL,                  TIMESTAMPTZ '2026-08-27 08:05:00-05'),
    ('Diego_S', 'diego.salas@example.com',   '$2a$11$577lIQLpf7SAYsQBkJfsTraoaWcxbX9rBBh6fWq3qsIaLb6XpY7.7', 'avatars/diego_s.png', TIMESTAMPTZ '2026-09-01 21:30:00-05'),
    ('Elena_T', 'elena.tapia@example.com',   '$2a$11$qINvX7U1oJw5xISTOBR5Q6qROA/LP1WZBeZjcnfzOFr4zORA2V.RU', NULL,                  TIMESTAMPTZ '2026-09-03 16:20:00-05'),
    ('Fabio_L', 'fabio.luna@example.com',    '$2a$11$Q6NFn7LFdWhDK3BG2nXh37dfX3PzNw2mHc0HZkNRsQeP5aKw8SaS1', 'avatars/fabio_l.png', TIMESTAMPTZ '2026-09-05 12:00:00-05');

INSERT INTO friendship (requester_id, addressee_id, status, requested_at, responded_at) VALUES
    ((SELECT player_id FROM player WHERE username = 'Ana_G'),
     (SELECT player_id FROM player WHERE username = 'Bruno_M'),
     'accepted', TIMESTAMPTZ '2026-08-23 11:00:00-05', TIMESTAMPTZ '2026-08-23 18:12:00-05'),
    ((SELECT player_id FROM player WHERE username = 'Ana_G'),
     (SELECT player_id FROM player WHERE username = 'Carla_R'),
     'accepted', TIMESTAMPTZ '2026-08-28 09:30:00-05', TIMESTAMPTZ '2026-08-28 10:02:00-05'),
    ((SELECT player_id FROM player WHERE username = 'Carla_R'),
     (SELECT player_id FROM player WHERE username = 'Elena_T'),
     'accepted', TIMESTAMPTZ '2026-09-04 20:45:00-05', TIMESTAMPTZ '2026-09-04 21:00:00-05'),
    ((SELECT player_id FROM player WHERE username = 'Diego_S'),
     (SELECT player_id FROM player WHERE username = 'Ana_G'),
     'pending',  TIMESTAMPTZ '2026-09-10 22:10:00-05', NULL);

INSERT INTO room (code, visibility, created_at, closed_at) VALUES
    ('K7QM', 'public',  TIMESTAMPTZ '2026-09-06 18:00:00-05', TIMESTAMPTZ '2026-09-06 21:30:00-05'),
    ('B3XZ', 'private', TIMESTAMPTZ '2026-09-12 16:30:00-05', TIMESTAMPTZ '2026-09-12 17:35:00-05'),
    ('M9TD', 'public',  TIMESTAMPTZ '2026-09-12 18:00:00-05', NULL),
    ('K7QM', 'private', TIMESTAMPTZ '2026-09-12 19:10:00-05', NULL);

INSERT INTO room_invitation (room_id, invited_player_id, inviter_player_id, channel, created_at) VALUES
    ((SELECT room_id FROM room WHERE code = 'M9TD' AND closed_at IS NULL),
     (SELECT player_id FROM player WHERE username = 'Carla_R'),
     (SELECT player_id FROM player WHERE username = 'Ana_G'),
     'in_game', TIMESTAMPTZ '2026-09-12 18:06:00-05'),
    ((SELECT room_id FROM room WHERE code = 'M9TD' AND closed_at IS NULL),
     (SELECT player_id FROM player WHERE username = 'Elena_T'),
     (SELECT player_id FROM player WHERE username = 'Ana_G'),
     'email',   TIMESTAMPTZ '2026-09-12 18:08:00-05'),
    ((SELECT room_id FROM room WHERE code = 'K7QM' AND closed_at IS NULL),
     (SELECT player_id FROM player WHERE username = 'Fabio_L'),
     (SELECT player_id FROM player WHERE username = 'Bruno_M'),
     'in_game', TIMESTAMPTZ '2026-09-12 19:14:00-05');

INSERT INTO match (room_id, status, player_count, started_at, finished_at, end_reason) VALUES
    ((SELECT room_id FROM room WHERE code = 'K7QM' AND closed_at IS NOT NULL),
     'finished', 3, TIMESTAMPTZ '2026-09-06 18:20:00-05', TIMESTAMPTZ '2026-09-06 19:05:00-05', 'completed'),
    ((SELECT room_id FROM room WHERE code = 'K7QM' AND closed_at IS NOT NULL),
     'finished', 2, TIMESTAMPTZ '2026-09-06 19:20:00-05', TIMESTAMPTZ '2026-09-06 19:50:00-05', 'abandoned'),
    ((SELECT room_id FROM room WHERE code = 'B3XZ'),
     'finished', 4, TIMESTAMPTZ '2026-09-12 16:50:00-05', TIMESTAMPTZ '2026-09-12 17:33:00-05', 'interrupted'),
    ((SELECT room_id FROM room WHERE code = 'M9TD'),
     'in_progress', 3, TIMESTAMPTZ '2026-09-12 18:25:00-05', NULL, NULL);

INSERT INTO match_participant (match_id, seat_number, player_id, final_score, final_position, was_withdrawn) VALUES
    ((SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-06 18:20:00-05'), 1,
     (SELECT player_id FROM player WHERE username = 'Ana_G'),   42, 2, false),
    ((SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-06 18:20:00-05'), 2,
     (SELECT player_id FROM player WHERE username = 'Bruno_M'), 51, 1, false),
    ((SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-06 18:20:00-05'), 3,
     (SELECT player_id FROM player WHERE username = 'Carla_R'), 30, 3, false),
    ((SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-06 19:20:00-05'), 1,
     (SELECT player_id FROM player WHERE username = 'Elena_T'), 27, 1, false),
    ((SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-06 19:20:00-05'), 2,
     (SELECT player_id FROM player WHERE username = 'Diego_S'), 15, 2, true),
    ((SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-12 16:50:00-05'), 1,
     (SELECT player_id FROM player WHERE username = 'Bruno_M'), NULL, NULL, false),
    ((SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-12 16:50:00-05'), 2,
     (SELECT player_id FROM player WHERE username = 'Carla_R'), NULL, NULL, false),
    ((SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-12 16:50:00-05'), 3,
     (SELECT player_id FROM player WHERE username = 'Fabio_L'), NULL, NULL, false),
    ((SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-12 18:25:00-05'), 1,
     (SELECT player_id FROM player WHERE username = 'Ana_G'),   NULL, NULL, false),
    ((SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-12 18:25:00-05'), 2,
     (SELECT player_id FROM player WHERE username = 'Fabio_L'), NULL, NULL, false);

INSERT INTO match_state (match_id, round_number, turn_number, active_seat_number, state_document, saved_at) VALUES
    ((SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-12 18:25:00-05'), 2, 1, 3,
     '{
        "seats": [
          {"seat_number": 1, "kind": "account", "color": "rojo",  "accumulated_score": 24, "cards_in_hand": [4, 6]},
          {"seat_number": 2, "kind": "account", "color": "azul",  "accumulated_score": 31, "cards_in_hand": []},
          {"seat_number": 3, "kind": "guest", "alias": "Manolo", "seat_ticket": "T4-9F2C-71AD", "color": "verde", "accumulated_score": 18, "cards_in_hand": [1]}
        ],
        "board": {
          "towers": [
            {"column": 2, "row": 2, "castle": 1, "level": 2},
            {"column": 2, "row": 3, "castle": 1, "level": 1},
            {"column": 3, "row": 2, "castle": 1, "level": 3},
            {"column": 4, "row": 4, "castle": 2, "level": 2},
            {"column": 5, "row": 4, "castle": 2, "level": 2},
            {"column": 5, "row": 5, "castle": 2, "level": 3},
            {"column": 6, "row": 5, "castle": 2, "level": 1},
            {"column": 7, "row": 7, "castle": 3, "level": 2},
            {"column": 7, "row": 8, "castle": 3, "level": 1}
          ],
          "knights": [
            {"seat_number": 1, "column": 2, "row": 2, "level": 2},
            {"seat_number": 1, "column": 5, "row": 4, "level": 2},
            {"seat_number": 1, "column": 7, "row": 7, "level": 2},
            {"seat_number": 2, "column": 2, "row": 3, "level": 1},
            {"seat_number": 2, "column": 5, "row": 5, "level": 3},
            {"seat_number": 3, "column": 3, "row": 2, "level": 3},
            {"seat_number": 3, "column": 6, "row": 5, "level": 1},
            {"seat_number": 3, "column": 7, "row": 8, "level": 1}
          ],
          "king": {"column": 4, "row": 4, "castle": 2, "level": 2}
        },
        "cards": {"deck_remaining": 3, "used": [2, 3]}
      }'::jsonb,
     TIMESTAMPTZ '2026-09-12 18:41:00-05');

INSERT INTO password_recovery (player_id, code, created_at) VALUES
    ((SELECT player_id FROM player WHERE username = 'Carla_R'), '4821', TIMESTAMPTZ '2026-09-12 19:05:00-05');

INSERT INTO report (reporter_player_id, reported_player_id, room_id, match_id, reported_text, created_at) VALUES
    ((SELECT player_id FROM player WHERE username = 'Ana_G'),
     (SELECT player_id FROM player WHERE username = 'Diego_S'),
     (SELECT room_id FROM room WHERE code = 'K7QM' AND closed_at IS NOT NULL),
     (SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-06 18:20:00-05'),
     'Juegas como un inutil, largate del jardin', TIMESTAMPTZ '2026-09-06 18:35:00-05'),
    ((SELECT player_id FROM player WHERE username = 'Bruno_M'),
     (SELECT player_id FROM player WHERE username = 'Diego_S'),
     (SELECT room_id FROM room WHERE code = 'K7QM' AND closed_at IS NOT NULL),
     (SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-06 18:20:00-05'),
     'Callate ya, nadie te quiere aqui', TIMESTAMPTZ '2026-09-06 18:41:00-05'),
    ((SELECT player_id FROM player WHERE username = 'Carla_R'),
     (SELECT player_id FROM player WHERE username = 'Diego_S'),
     (SELECT room_id FROM room WHERE code = 'K7QM' AND closed_at IS NOT NULL),
     (SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-06 19:20:00-05'),
     'Ojala te quedes sin orugas, basura', TIMESTAMPTZ '2026-09-06 19:33:00-05'),
    ((SELECT player_id FROM player WHERE username = 'Ana_G'),
     (SELECT player_id FROM player WHERE username = 'Diego_S'),
     (SELECT room_id FROM room WHERE code = 'K7QM' AND closed_at IS NOT NULL),
     NULL,
     'Sigue llorando en la sala, perdedor', TIMESTAMPTZ '2026-09-06 19:58:00-05'),
    ((SELECT player_id FROM player WHERE username = 'Elena_T'),
     (SELECT player_id FROM player WHERE username = 'Diego_S'),
     (SELECT room_id FROM room WHERE code = 'B3XZ'),
     (SELECT match_id FROM match WHERE started_at = TIMESTAMPTZ '2026-09-12 16:50:00-05'),
     'Eres lo peor de esta partida, idiota', TIMESTAMPTZ '2026-09-12 17:10:00-05'),
    ((SELECT player_id FROM player WHERE username = 'Bruno_M'),
     (SELECT player_id FROM player WHERE username = 'Diego_S'),
     (SELECT room_id FROM room WHERE code = 'M9TD'),
     NULL,
     'Otra vez insultando a todos en la sala', TIMESTAMPTZ '2026-09-12 18:15:00-05');

INSERT INTO sanction (player_id, level, is_permanent, threshold_at, effective_from, effective_until) VALUES
    ((SELECT player_id FROM player WHERE username = 'Diego_S'), 1, false,
     TIMESTAMPTZ '2026-09-12 18:15:00-05',
     TIMESTAMPTZ '2026-09-12 18:15:00-05',
     TIMESTAMPTZ '2026-09-12 23:15:00-05');
