{% import 'docker/file.jinja2' as df with context -%}

{{ df.minimal_init(
  'rust:latest',
  syslog_interceptor.container_user,
  1000,
  syslog_interceptor.container_home
) }}

{{ df.install([ 'netcat' ]) }}

{% set app_dir = syslog_interceptor.container_app_dir %}

RUN mkdir -p {{ app_dir }}
WORKDIR {{ app_dir }}
COPY . .

{{ df.run_as_root('chown ' + syslog_interceptor.container_user + ':users . -R') }}

RUN rustup install nightly \
      && rustup default nightly \
      && cargo build --release \
      && cp target/release/syslog-interceptor . \
      && rm -rf target

EXPOSE 6514/udp
EXPOSE 8080/tcp

CMD ["./syslog-interceptor"]
