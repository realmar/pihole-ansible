mod duplicati;

use std::net::ToSocketAddrs;

use anyhow::{anyhow, Result};
use warp::Filter;

use crate::app_config::{RetryPolicy, WebConfig};

pub async fn run(config: WebConfig, retry_policy: RetryPolicy) -> Result<()> {
  let server_addr = config
    .server_address
    .to_socket_addrs()?
    .next()
    .ok_or(anyhow!(format!(
      "Cannot parse server address: {}",
      config.server_address
    )))?;

  let routes = warp::path("duplicati")
    .and(warp::path::end())
    .and(warp::post())
    .and(warp::body::json())
    .and(warp::any().map(move || config.duplicati.clone()))
    .and(warp::any().map(move || retry_policy.clone()))
    .and_then(duplicati::handler);

  warp::serve(routes).run(server_addr).await;

  Ok(())
}
