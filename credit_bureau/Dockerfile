# Base stage: shared PHP image and extensions
FROM serversideup/php:8.5-fpm-nginx-alpine

USER root

# Save the build arguments as a variable
ARG USER_ID
ARG GROUP_ID

# Use the build arguments to change the UID
# and GID of www-data while also changing
# the file permissions for NGINX
RUN docker-php-serversideup-set-id www-data $USER_ID:$GROUP_ID && \
    \
    # Update the file permissions to match the new UID/GID
    docker-php-serversideup-set-file-permissions --owner $USER_ID:$GROUP_ID --service nginx

# Serversideup/php Laravel automations
ENV PHP_OPCACHE_ENABLE=1 \
    HEALTHCHECK_PATH=/up \
    AUTORUN_ENABLED=false \
    AUTORUN_DEBUG=true

# Run entrypoint-dev.sh to install dependencies and run migrations
COPY --chmod=755 docker/php/entrypoint-dev.sh /etc/entrypoint.d/entrypoint-dev.sh

# Run as www-data (with host UID/GID) so volume files have correct ownership
USER www-data

EXPOSE 8080
