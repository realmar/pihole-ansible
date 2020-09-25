#![feature(proc_macro_hygiene, decl_macro, async_closure)]

mod app_config;
mod retry;
mod syslog_app;
mod web_app;

use anyhow::{anyhow, Result};
use tokio;

use log::LevelFilter;
use tracing::Level;
use tracing_subscriber::FmtSubscriber;

fn map_log_level(level: LevelFilter) -> Level {
  match level {
    LevelFilter::Trace => Level::TRACE,
    LevelFilter::Debug => Level::DEBUG,
    LevelFilter::Info => Level::INFO,
    LevelFilter::Warn | LevelFilter::Off => Level::WARN,
    LevelFilter::Error => Level::ERROR,
  }
}

#[tokio::main]
async fn main() -> Result<()> {
  let config = app_config::load();
  let subscriber = FmtSubscriber::builder()
    .with_max_level(map_log_level(config.internal.log_level))
    .with_ansi(false)
    .without_time()
    .finish();

  if let Err(err) = tracing::subscriber::set_global_default(subscriber) {
    eprintln!("Failed to init logging: {}", err);
  }

  let result = tokio::try_join!(
    tokio::spawn(syslog_app::run(config.syslog.clone())),
    tokio::spawn(web_app::run(
      config.web.clone(),
      config.retry_policy.clone()
    ))
  );
  match result {
    Ok(..) => Ok(()),
    Err(err) => Err(anyhow!(err)),
  }
}
