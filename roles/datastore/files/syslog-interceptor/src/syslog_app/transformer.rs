mod patterns;

use std::str::{self, FromStr};

use anyhow::anyhow;
use anyhow::Result;
use tokio::net::UdpSocket;
use tracing::{debug, error, trace};

use patterns::*;

use super::error::SyslogError;
use super::sender::LogSender;
use crate::app_config::SyslogConfig;

// https://en.wikipedia.org/wiki/User_Datagram_Protocol
const BUFFER_SIZE: usize = 65507;

pub struct Transformer {
  listener: UdpSocket,
  sender: LogSender,
  buffer: [u8; BUFFER_SIZE],
  patterns: Patterns,
}

impl Transformer {
  pub async fn new(config: &SyslogConfig) -> Result<Transformer> {
    let listener = UdpSocket::bind(&config.server_address).await?;

    Ok(Transformer {
      listener,
      sender: LogSender::new(config).await?,
      buffer: [0u8; BUFFER_SIZE],
      patterns: Patterns::new(config)?,
    })
  }

  pub async fn receive_messages(&mut self) -> Result<()> {
    loop {
      match self.listener.recv_from(&mut self.buffer).await {
        Ok((data_read, _)) => {
          if let Err(err) = self.parse(data_read).await {
            error!("Failed to process message: {}", err);
          }
        }
        Err(err) => error!("recv_from failed: {}", err),
      }
    }
  }

  async fn parse(&mut self, data_read: usize) -> Result<()> {
    let data = &self.buffer[0..data_read];
    let message = str::from_utf8(data)?;

    let cap = PRIORITY_REGEX.captures(message);
    match cap {
      Some(captures) => {
        let raw = &captures["n"];
        let priority = u8::from_str(raw)?;
        let new_priority = self.get_new_priority(priority, message);
        let remainder = &message[raw.len() + 2..message.len()];
        let new_message = format!("<{}>{}", new_priority, remainder);

        debug!("{}", remainder);

        if priority != new_priority {
          debug!("Rewrite {} --> {}", priority, new_priority);
        }

        trace!("{}", new_message);

        self.sender.send(new_message.as_bytes()).await?;
      }
      None => return Err(anyhow!(SyslogError::InvalidFormat(String::from(message)))),
    }

    Ok(())
  }

  fn get_new_priority(&self, priority: u8, message: &str) -> u8 {
    let facility = priority / 8;
    let severity = priority - (facility * 8);

    for (regex, severity) in self.patterns.get().iter() {
      if regex.is_match(message) {
        return facility * 8 + *severity;
      }
    }

    // if its an error then default to info
    if severity == 3 {
      facility * 8 + 6
    } else {
      priority
    }
  }
}
