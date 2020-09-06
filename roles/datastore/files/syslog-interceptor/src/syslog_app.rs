mod error;
mod sender;
mod transformer;

use anyhow::Result;

use crate::app_config::SyslogConfig;
use transformer::Transformer;

pub async fn run(config: SyslogConfig) -> Result<()> {
  let mut transformer = Transformer::new(&config).await?;
  transformer.receive_messages().await?;

  Ok(())
}
