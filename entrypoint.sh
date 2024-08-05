#!/bin/sh

# Start SQL Server in the background
/opt/mssql/bin/sqlservr &

# Wait for SQL Server to start up (this time may need to be adjusted)
echo "Waiting for SQL Server to start..."
sleep 30s

# Run the SQL initialization script if it exists
if [ -f /docker-entrypoint-initdb.d/init-database.sql ]; then
    echo "Running initialization script..."
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -i /docker-entrypoint-initdb.d/init-database.sql
fi

# Keep the container running
wait
