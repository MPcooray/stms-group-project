FROM node:20-alpine AS build
WORKDIR /app

# Accept Vite API base at build time (Vite embeds vars prefixed with VITE_)
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}

# install dependencies
# The Dockerfile lives in the Frontend directory and the build context will be set to ./Frontend
COPY package.json package-lock.json* ./
COPY pnpm-lock.yaml ./
# Copy all frontend source files into the build stage. Ensure the entrypoint is
# included so later stages can copy it from this stage instead of the build
# context (avoids issues where the final stage can't see the file directly).
COPY . .

# Make sure the entrypoint exists in the build stage
RUN [ -f ./docker-entrypoint.sh ] || (echo "docker-entrypoint.sh missing in build context" && false)

RUN npm ci --legacy-peer-deps || npm install
RUN npm run build

FROM nginx:stable-alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
# Copy the entrypoint from the build stage (guaranteed to be present there)
COPY --from=build /app/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh
EXPOSE 80
ENTRYPOINT ["/docker-entrypoint.sh"]
