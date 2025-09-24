FROM node:20-alpine AS build
WORKDIR /app

# Accept Vite API base at build time (Vite embeds vars prefixed with VITE_)
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}

# install dependencies
# The Dockerfile lives in the Frontend directory and the build context will be set to ./Frontend
COPY package.json package-lock.json* ./
COPY pnpm-lock.yaml ./
COPY . .

RUN npm ci --legacy-peer-deps || npm install
RUN npm run build

FROM nginx:stable-alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh
EXPOSE 80
ENTRYPOINT ["/docker-entrypoint.sh"]
