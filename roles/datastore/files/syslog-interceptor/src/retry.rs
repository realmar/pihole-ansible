use std::time::Duration;

use anyhow::Result;
use tokio::time::delay_for;
use tracing::{error, warn};

use crate::app_config::RetryPolicy;

pub async fn retry<Future, Ok>(retry_policy: RetryPolicy, mut f: impl FnMut() -> Future)
where
    Future: std::future::Future<Output = Result<Ok>>,
{
    let mut wait = retry_policy.initial_delay;
    let mut try_num = 0;

    while let Err(err) = f().await {
        warn!("{:?}", err);
        delay_for(Duration::from_millis(wait)).await;

        // exponential backoff
        wait *= 2;
        try_num += 1;
        if try_num >= retry_policy.max {
            error!("Retry limit reached error: {}", err);
            break;
        }
    }
}
