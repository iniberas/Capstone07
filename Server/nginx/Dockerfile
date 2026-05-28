FROM nginx:1.29.5-alpine

RUN apk add --no-cache apache2-utils

ARG DASHBOARD_USERNAME
ARG DASHBOARD_PASSWORD

RUN htpasswd -bc /etc/nginx/.htpasswd ${DASHBOARD_USERNAME} ${DASHBOARD_PASSWORD}