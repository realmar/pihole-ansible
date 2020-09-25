#![feature(async_closure)]

use std::vec::Vec;

use anyhow::{anyhow, Result};
use bincode;
use clap::Clap;
use serde::{Deserialize, Serialize};
use shiplift;
use tokio::prelude::*;
use tokio::{self, fs::File};
use tracing::{error, Level};
use tracing_subscriber::FmtSubscriber;

const FILENAME: &'static str = "state.bin";

#[derive(Clap, Debug)]
enum SubCommand {
    #[clap(about = "Prepare for backup")]
    Stop {
        #[clap(short, long, about = "Stop following containers (by name)")]
        exclude: Vec<String>,
    },
    #[clap(about = "Restore containers after backup")]
    Start,
}

#[derive(Clap)]
#[clap(
    version = "1.0",
    author = "Anastassios Martakos <anastassios.martakos@outlook.com>",
    rename_all = "kebab-case"
)]
struct Opts {
    #[clap(subcommand)]
    subcmd: SubCommand,
}

#[derive(Serialize, Deserialize, PartialEq, Debug)]
struct StoppedContainers(Vec<String>);

async fn stop<'a>(id: String, containers: &shiplift::Containers<'a>) -> Result<()> {
    let container = containers.get(id.as_str());
    if let Err(err) = container.stop(None).await {
        error!("CRITICAL Failed to stop container {} error: {}", id, err);
        Err(anyhow!(err))
    } else {
        Ok(())
    }
}

async fn stop_command(exclude: &Vec<String>) -> Result<()> {
    let docker = shiplift::Docker::new();
    let containers = docker.containers();

    // note: this will only return started containers
    // which is actually what we want here
    let reps = containers.list(&Default::default()).await?;
    let mut stopped_containers = StoppedContainers(vec![]);
    let mut tasks = Vec::<_>::new();

    if reps.len() > 0 {
        for rep in reps {
            let excluded = {
                let mut result = false;

                'outter: for e in exclude.iter() {
                    for name in rep.names.iter() {
                        if name.contains(e) {
                            result = true;
                            break 'outter;
                        }
                    }
                }

                result
            };

            if excluded == false {
                let id = rep.id;
                stopped_containers.0.push(id.clone());
                let future = stop(id.clone(), &containers);
                let pin = Box::pin(future);
                tasks.push(pin);
            }
        }

        let data = bincode::serialize(&stopped_containers)?;

        futures::future::join_all(tasks).await;

        let mut file = File::create(FILENAME).await?;
        file.write_all(&data).await?;
    }

    Ok(())
}

async fn start<'a>(id: String, containers: &shiplift::Containers<'a>) -> Result<()> {
    let container = containers.get(id.as_str());
    if let Err(err) = container.start().await {
        error!("CRITICAL Failed to start container {} error: {}", id, err);
        Err(anyhow!(err))
    } else {
        Ok(())
    }
}

async fn start_command() -> Result<()> {
    let docker = shiplift::Docker::new();
    let containers = docker.containers();

    let mut file = File::open(FILENAME).await?;
    let mut contents = vec![];

    file.read_to_end(&mut contents).await?;

    let stopped_containers: StoppedContainers = bincode::deserialize(&contents)?;
    let mut tasks = Vec::<_>::new();

    for id in stopped_containers.0.iter() {
        let future = start(id.clone(), &containers);
        let pin = Box::pin(future);
        tasks.push(pin);
    }

    futures::future::join_all(tasks).await;

    Ok(())
}

#[tokio::main]
async fn main() -> Result<()> {
    let subscriber = FmtSubscriber::builder()
        .with_max_level(Level::INFO)
        .with_ansi(false)
        .without_time()
        .finish();

    if let Err(err) = tracing::subscriber::set_global_default(subscriber) {
        eprintln!("Failed to init logging: {}", err);
    }

    let opts: Opts = Opts::parse();

    if let Err(err) = match opts.subcmd {
        SubCommand::Start => start_command().await,
        SubCommand::Stop { ref exclude } => stop_command(exclude).await,
    } {
        error!("CRITICAL Failed {:#?} error: {}", opts.subcmd, err);
        Err(anyhow!(err))
    } else {
        Ok(())
    }
}
