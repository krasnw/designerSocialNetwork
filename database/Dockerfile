FROM postgres:latest

ENV POSTGRES_USER=api_user
ENV POSTGRES_PASSWORD=api_user_password
ENV POSTGRES_DB=api_database

COPY scripts/* /docker-entrypoint-initdb.d/

EXPOSE 5432