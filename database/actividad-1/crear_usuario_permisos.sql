CREATE ROLE torres_app WITH LOGIN PASSWORD 'definir_en_el_despliegue';

GRANT CONNECT ON DATABASE torres TO torres_app;

GRANT USAGE ON SCHEMA torres TO torres_app;

REVOKE CREATE ON SCHEMA torres FROM torres_app;

GRANT SELECT, INSERT, UPDATE, DELETE ON
    torres.player,
    torres.friendship,
    torres.match,
    torres.match_state
TO torres_app;

GRANT SELECT, INSERT, UPDATE ON
    torres.room,
    torres.match_participant
TO torres_app;

GRANT SELECT, INSERT, DELETE ON
    torres.room_invitation,
    torres.password_recovery
TO torres_app;

GRANT SELECT, INSERT ON
    torres.report,
    torres.sanction
TO torres_app;

GRANT SELECT ON
    torres.turn_layout,
    torres.build_allowance,
    torres.action_cost,
    torres.sanction_level,
    torres.system_parameter,
    torres.player_ranking
TO torres_app;

GRANT USAGE ON ALL SEQUENCES IN SCHEMA torres TO torres_app;
