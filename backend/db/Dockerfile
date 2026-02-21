# Filename: Dockerfile

# Use MySQL 8.0 for robust JSON support
FROM mysql:8.0

# Copy the initialization script into the container
# MySQL automatically executes scripts in this directory on startup
COPY init.sql /docker-entrypoint-initdb.d/

# (Optional) specific config if needed, though defaults are usually fine for dev
# COPY my-custom.cnf /etc/mysql/conf.d/