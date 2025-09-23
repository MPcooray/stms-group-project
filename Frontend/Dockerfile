FROM node:20-alpine AS build
WORKDIR /app

# Accept Vite API base at build time (Vite embeds vars prefixed with VITE_)
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}

# install dependencies
COPY Frontend/package.json Frontend/package-lock.json* ./
COPY Frontend/pnpm-lock.yaml ./
COPY Frontend/ .

RUN npm ci --legacy-peer-deps || npm install
RUN npm run build

FROM nginx:stable-alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY Frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY Frontend/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh
EXPOSE 80
ENTRYPOINT ["/docker-entrypoint.sh"]
