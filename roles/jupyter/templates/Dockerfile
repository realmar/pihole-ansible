{% set apt_install = 'apt-get install -y -q --no-install-recommends' %}

{% macro install(packages) %}
USER root
RUN {{ apt_install }} {{ packages|join(' ') }}
USER $USER
{% endmacro %}

################################################################################
# INIT
################################################################################

FROM ubuntu:20.04

ENV HOME {{ jupyter.home }}
ENV UID {{ jupyter.uid }}
ENV USER {{ jupyter.container_user }}
ENV TZ {{ timezone }}

USER root

RUN rm /bin/sh && ln -s /bin/bash /bin/sh
RUN echo 'debconf debconf/frontend select Noninteractive' | debconf-set-selections

RUN apt-get update
{{ install([
  'apt-transport-https',
  'build-essential',
  'ca-certificates',
  'curl',
  'git',
  'libssl-dev',
  'wget',
  'sudo'
]) }}

USER root

RUN echo "pi ALL=(ALL:ALL) NOPASSWD: /usr/bin/apt" >> /etc/sudoers
RUN useradd -m -s /bin/bash -N -u $UID $USER
RUN mkdir -p $HOME \
      && touch $HOME/.bashrc \
      && mkdir -p $HOME/data

RUN chown $USER $HOME -R

################################################################################
# JUPYTERLAB
################################################################################

{{ install([
    'python3',
    'python3-dev',
    'python3-pip',
    'llvm',
    'clang',
    'cmake'
]) }}

USER root

RUN pip3 install jupyterlab

USER $USER

################################################################################
# DOTNET CORE - C# - F# - POWERSHELL
################################################################################

RUN mkdir -p $HOME/dotnet-sdk
WORKDIR $HOME/dotnet-sdk

ENV PATH        $HOME/dotnet-sdk:$HOME/.dotnet/tools:$PATH
ENV DOTNET_ROOT $HOME/dotnet-sdk
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT 1

{% if is_dev %}
RUN wget https://download.visualstudio.microsoft.com/download/pr/8db2b522-7fa2-4903-97ec-d6d04d297a01/f467006b9098c2de256e40d2e2f36fea/dotnet-sdk-3.1.301-linux-x64.tar.gz -O sdk.tar.gz
{% else %}
RUN wget https://download.visualstudio.microsoft.com/download/pr/fe5c0663-3ed1-4a93-95e1-fd068b89215b/14d1caad8fd2859d5f3514745a9bf6b3/dotnet-sdk-3.1.301-linux-arm64.tar.gz -O sdk.tar.gz
{% endif %}

RUN tar xf sdk.tar.gz

RUN dotnet tool install --global Microsoft.dotnet-interactive
RUN dotnet interactive jupyter install

################################################################################
# NODEJS - JAVASCRIPT - TYPESCRIPT
################################################################################

WORKDIR $HOME

{{ install([ 'nodejs', 'npm' ]) }}

ENV NPM_PACKAGES "$HOME/.npm-packages"
ENV PATH $NPM_PACKAGES/bin:$PATH
ENV MANPATH $NPM_PACKAGES/share/man:$MANPATH

RUN mkdir "$NPM_PACKAGES"
RUN npm config set prefix "$NPM_PACKAGES"
RUN npm install -g tslab
RUN tslab install --python=python3

################################################################################
# RUST
################################################################################

RUN curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh -s -- -y
ENV PATH $HOME/.cargo/bin:$PATH
RUN cargo install evcxr_jupyter
RUN evcxr_jupyter --install

################################################################################
# JAVA
################################################################################

{{ install([ 'openjdk-14-jdk-headless', 'unzip' ]) }}

RUN mkdir IJava
WORKDIR $HOME/IJava
RUN wget https://github.com/SpencerPark/IJava/releases/download/v1.3.0/ijava-1.3.0.zip
RUN unzip ijava-1.3.0.zip

USER root
RUN python3 install.py --sys-prefix
USER $USER

{#
################################################################################
# HASKELL
################################################################################

WORKDIR $HOME

{{ install([
    'python3-pip',
    'git',
    'libtinfo-dev',
    'libzmq3-dev',
    'libcairo2-dev',
    'libpango1.0-dev',
    'libmagic-dev',
    'libblas-dev',
    'liblapack-dev'
 ]) }}

USER root

RUN curl -sSL https://get.haskellstack.org/ | sh
ENV PATH /home/pi/.local/bin:$PATH

USER $USER

RUN git clone https://github.com/gibiansky/IHaskell

WORKDIR $HOME/IHaskell

RUN pip3 install -r requirements.txt
RUN stack install --fast
RUN ihaskell install --stack

# USER root
# RUN jupyter labextension install jupyterlab-ihaskell
#}

################################################################################
# FINISH UP
################################################################################

USER $USER
WORKDIR $HOME/data
VOLUME [ "$HOME/data" ]

EXPOSE 8888
ENTRYPOINT [ "jupyter", "notebook" ]
