FROM ubuntu:24.04

RUN apt-get update && apt-get install -y \
  jq unoconv \
  && rm -rf /var/lib/apt/lists/*

COPY out/ /opt/ozma-report-generator
COPY docker/entrypoint.sh /usr/local/bin/docker-entrypoint.sh

VOLUME ["/root/.aspnet/DataProtection-Keys"]
CMD ["/usr/local/bin/docker-entrypoint.sh"]
