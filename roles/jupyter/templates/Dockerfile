{% import 'docker/file.jinja2' as df with context -%}

################################################################################
# INIT
################################################################################

{{ df.init(
  'ubuntu:20.04',
  jupyter.container_user,
  jupyter.uid,
  jupyter.home
) }}

RUN mkdir -p $HOME/data

{{ df.install([
  'build-essential',
  'libssl-dev'
]) }}

{{ df.allow_sudo('/usr/bin/apt') }}

################################################################################
# JUPYTERLAB
################################################################################

{{ df.install([
    'python3',
    'python3-dev',
    'python3-pip',
    'llvm',
    'clang',
    'cmake'
]) }}

{{ df.run_as_root('pip3 install jupyterlab') }}

################################################################################
# DOTNET CORE - C# - F# - POWERSHELL
################################################################################

RUN mkdir -p $HOME/dotnet-sdk
WORKDIR $HOME/dotnet-sdk

{{ df.add_to_path('$HOME/dotnet-sdk:$HOME/.dotnet/tools') }}
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

{{ df.install_nodejs() }}

RUN npm install -g tslab
RUN tslab install --python=python3

################################################################################
# RUST
################################################################################

RUN curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh -s -- -y
{{ df.add_to_path('$HOME/.cargo/bin') }}
RUN cargo install evcxr_jupyter
RUN evcxr_jupyter --install

################################################################################
# JAVA
################################################################################

{{ df.install([ 'openjdk-14-jdk-headless', 'unzip' ]) }}

RUN mkdir IJava
WORKDIR $HOME/IJava
RUN wget https://github.com/SpencerPark/IJava/releases/download/v1.3.0/ijava-1.3.0.zip
RUN unzip ijava-1.3.0.zip

{{ df.run_as_root('python3 install.py --sys-prefix') }}

{#
################################################################################
# HASKELL
################################################################################

WORKDIR $HOME

{{ df.install([
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

{{ df.run_as_root('curl -sSL https://get.haskellstack.org/ | sh') }}
{{ df.add_to_path('/home/pi/.local/bin') }}

RUN git clone https://github.com/gibiansky/IHaskell

WORKDIR $HOME/IHaskell

RUN pip3 install -r requirements.txt
RUN stack install --fast
RUN ihaskell install --stack

# {{ df.run_as_root('jupyter labextension install jupyterlab-ihaskell') }}
#}

################################################################################
# FINISH UP
################################################################################

USER $USER
WORKDIR $HOME/data
VOLUME [ "$HOME/data" ]

EXPOSE 8888
ENTRYPOINT [ "jupyter", "notebook" ]
