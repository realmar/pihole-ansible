use thiserror::Error;

#[derive(Error, Debug)]
pub enum SyslogError {
  #[error("Wrong format: {0}")]
  InvalidFormat(String),
}
