FROM postgres:latest

ENV POSTGRES_USER=api_user
ENV POSTGRES_PASSWORD=api_user_password
ENV POSTGRES_DB=api_database

COPY ./scripts/01-init.sql /docker-entrypoint-initdb.d/
COPY ./scripts/02-dataToDatabase.sql /docker-entrypoint-initdb.d/

EXPOSE 5432