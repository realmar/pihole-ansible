use anyhow::Result;

use crate::app_config::SyslogConfig;
use tokio::net::UdpSocket;

pub struct LogSender {
  socket: UdpSocket,
}

impl LogSender {
  pub async fn new(config: &SyslogConfig) -> Result<LogSender> {
    let sender = UdpSocket::bind("0.0.0.0:0").await?;
    sender.connect(&config.forward_address).await?;

    Ok(LogSender { socket: sender })
  }

  pub async fn send(&mut self, data: &[u8]) -> Result<()> {
    self.socket.send(data).await?;
    Ok(())
  }
}
