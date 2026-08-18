FROM serversideup/php:8.4-fpm-nginx

ARG USER_ID=1000
ARG GROUP_ID=1000

USER root

RUN docker-php-serversideup-set-id www-data ${USER_ID}:${GROUP_ID} && \
    docker-php-serversideup-set-file-permissions --owner ${USER_ID}:${GROUP_ID} --service nginx

WORKDIR /var/www/html

# Copy composer files first for layer caching optimization
COPY --chown=www-data:www-data composer.json composer.lock ./

# Run composer install as the www-data user to avoid permission issues with vendor files
USER www-data
RUN composer install --no-dev --optimize-autoloader --no-scripts

# Copy the rest of your application code
COPY --chown=www-data:www-data . .
# ----------------------------------------

USER www-data
