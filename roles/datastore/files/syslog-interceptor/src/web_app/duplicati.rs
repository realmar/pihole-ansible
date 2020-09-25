use std::collections::HashMap;

use anyhow::{anyhow, Result};
use reqwest;
use serde_json::Value;
use tokio;
use tracing::{debug, error, info, trace};
use warp::{http::StatusCode, reject, Rejection};

use crate::app_config::{DuplicatiWebConfig, RetryPolicy};
use crate::retry::retry;

async fn send(forward_address: String, json: String) -> Result<()> {
  if let Err(err) = reqwest::Client::new()
    .post(forward_address.as_str())
    .body(json)
    .send()
    .await
  {
    Err(anyhow!(
      "Failed to forward data to {}: {}",
      forward_address,
      err
    ))
  } else {
    info!("Forwarded to {}", forward_address);
    Ok(())
  }
}

pub async fn handler(
  data: HashMap<String, Value>,
  config: DuplicatiWebConfig,
  retry_policy: RetryPolicy,
) -> std::result::Result<StatusCode, Rejection> {
  trace!("{:?}", data);

  if let Some(value) = data.get("Data") {
    let mut processed = HashMap::<String, Value>::new();
    processed.insert("Duplicati".into(), value.clone());

    debug!("{:?}", processed);

    match serde_json::to_string(&processed) {
      Ok(json) => {
        tokio::spawn(retry(retry_policy, move || {
          send(config.forward_address.clone(), json.clone())
        }));

        Ok(StatusCode::OK)
      }
      Err(err) => {
        error!("Failed serialize data: {}", err);
        Err(reject::reject())
      }
    }
  } else {
    error!("Cannot find key 'Data' in duplicati stats");
    Err(reject::reject())
  }
}
