# Database Management

## Backup and Restore

- **backup** database:

```bash
pg_dump --username=postgres -f c:/users/dfusi/desktop/dump.sql pythia
```

- **restore** database from a dump like the above (first you must _create_ the database):

```bash
psql -U postgres -d pythia -f c:/users/dfusi/desktop/dump.sql
```

- **bulk write** for API:

```bash
./pythia bulk-write c:/users/dfusi/desktop/dump
```
