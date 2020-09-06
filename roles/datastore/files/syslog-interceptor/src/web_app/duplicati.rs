use std::collections::HashMap;

use reqwest;
use serde_json::Value;
use tracing::{debug, error, info, trace};
use warp::{http::StatusCode, reject, Rejection};

use crate::app_config::WebConfig;

pub async fn handler(
  data: HashMap<String, Value>,
  config: WebConfig,
) -> std::result::Result<StatusCode, Rejection> {
  trace!("{:?}", data);

  if let Some(value) = data.get("Data") {
    let mut processed = HashMap::<String, Value>::new();
    processed.insert("Duplicati".into(), value.clone());

    debug!("{:?}", processed);

    match serde_json::to_string(&processed) {
      Ok(json) => {
        let forward_address = config.duplicati.forward_address.as_str();
        if let Err(err) = reqwest::Client::new()
          .post(forward_address)
          .body(json)
          .send()
          .await
        {
          error!("Failed to forward data to {}: {}", forward_address, err);
          Err(reject::reject())
        } else {
          info!("Forwarded to {}", forward_address);
          Ok(StatusCode::OK)
        }
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
