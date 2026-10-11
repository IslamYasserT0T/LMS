# PostgreSQL Integration Results

Docker CLI is installed, but its Linux engine is unavailable: the Docker Desktop named pipe `//./pipe/dockerDesktopLinuxEngine` does not exist. The required `lms-integration-postgres` container therefore could not be inspected or created.

PostgreSQL processes are running locally and PID 8388 listens on `0.0.0.0:5432` and `::5432`. Its executable path, command line, data directory, database identity, and ownership were unavailable to this user context. It is not labelled or otherwise evidenced as the dedicated integration fixture, so it was deliberately not connected to or inspected at the database level. Port 55432 has no verified fixture.

No migration, schema write, report query, definition persistence operation, or database fixture ran. `LMSLocal` was not touched.

Required next environment: either start Docker Desktop's Linux engine so a new `postgres:16` container named `lms-integration-postgres` can be created with `127.0.0.1:55432:5432`, database `LMSIntegrationTest`, and user `lms_integration`; or provide another clearly identified disposable PostgreSQL fixture with equivalent restricted credentials. Configure it through `ConnectionStrings__Default` for the isolated Host process only.
