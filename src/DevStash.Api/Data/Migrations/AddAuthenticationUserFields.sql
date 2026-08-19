START TRANSACTION;
DROP INDEX devstash_dotnet.ix_application_users_normalized_email;

ALTER TABLE devstash_dotnet.application_users ADD display_name character varying(100) NOT NULL DEFAULT '';

CREATE UNIQUE INDEX ux_application_users_normalized_email ON devstash_dotnet.application_users (normalized_email);

INSERT INTO devstash_dotnet.__ef_migrations_history ("MigrationId", "ProductVersion")
VALUES ('20260819133230_AddAuthenticationUserFields', '10.0.11');

COMMIT;
