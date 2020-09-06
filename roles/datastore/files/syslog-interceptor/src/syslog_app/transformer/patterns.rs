use std::vec::Vec;

use anyhow::Result;
use lazy_static::lazy_static;
use regex::Regex;

use crate::app_config::SyslogConfig;

lazy_static! {
  pub(super) static ref PRIORITY_REGEX: Regex = Regex::new(r"^<(?P<n>\d{1,3})>").unwrap();
}

pub struct Patterns {
  patterns: Vec<(Regex, u8)>,
}

impl Patterns {
  pub fn new(config: &SyslogConfig) -> Result<Patterns> {
    let mut patterns = Vec::<(Regex, u8)>::new();

    for p in config.patterns.iter() {
      patterns.push((
        Regex::new(format!(
          "{}{}{}",
          config.pattern_prefix, p.regex, config.pattern_postfix
        ).as_str())?,
        p.severity,
      ))
    }

    Ok(Patterns { patterns })
  }

  pub fn get(&self) -> &Vec<(Regex, u8)> {
    &self.patterns
  }
}
