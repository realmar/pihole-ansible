FROM rust:latest

WORKDIR /root

COPY . .

RUN rustup install nightly \
      && rustup default nightly \
      && cargo build --release \
      && cp target/release/bh . \
      && rm -rf target
