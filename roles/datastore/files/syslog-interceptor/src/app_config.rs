use config;
use serde::{Deserialize, Serialize};
use log::LevelFilter;

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PatternConfig {
  pub regex: String,
  pub severity: u8,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SyslogConfig {
  pub server_address: String,
  pub forward_address: String,

  pub pattern_prefix: String,
  pub pattern_postfix: String,
  pub patterns: Vec<PatternConfig>,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct DuplicatiWebConfig {
  pub forward_address: String,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct WebConfig {
  pub server_address: String,
  pub duplicati: DuplicatiWebConfig,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct InternalConfig {
  pub log_level: LevelFilter,
  pub log_socket: String,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Config {
  pub internal: InternalConfig,
  pub syslog: SyslogConfig,
  pub web: WebConfig,
}

impl Default for Config {
  fn default() -> Config {
    Config {
      internal: InternalConfig {
        log_level: LevelFilter::Warn,
        log_socket: "/dev/log".into(),
      },
      syslog: SyslogConfig {
        server_address: "0.0.0.0:6514".into(),
        forward_address: "localhost:6518".into(),

        pattern_prefix: r"(=|:|\s)(?i)".into(),
        pattern_postfix: r"(\W|^\d)".into(),
        patterns: vec![
          PatternConfig {
            regex: "emerg".into(),
            severity: 0,
          },
          PatternConfig {
            regex: "alert".into(),
            severity: 1,
          },
          PatternConfig {
            regex: "crit".into(),
            severity: 2,
          },
          PatternConfig {
            regex: "(error|(?-i)err)".into(),
            severity: 3,
          },
          PatternConfig {
            regex: "(warn|warning)".into(),
            severity: 4,
          },
          PatternConfig {
            regex: "notice".into(),
            severity: 5,
          },
          PatternConfig {
            regex: "info".into(),
            severity: 6,
          },
          PatternConfig {
            regex: "debug".into(),
            severity: 7,
          },
        ],
      },

      web: WebConfig {
        server_address: "localhost:8080".into(),
        duplicati: DuplicatiWebConfig {
          forward_address: "http://localhost:4444".into(),
        },
      },
    }
  }
}

pub fn load() -> Config {
  let mut settings = config::Config::default();

  // settings.merge(config::File::with_name("/etc/syslog-interceptor")).ok();
  settings.merge(config::File::with_name("config")).ok();

  let config = match settings.try_into::<Config>() {
    Ok(c) => c,
    Err(err) => {
      eprintln!("Failed to load config: {}", err);
      eprintln!("Using default config");

      Config::default()
    }
  };

  println!("Config loaded: {:#?}", config);

  config
}
