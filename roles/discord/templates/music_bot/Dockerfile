{% import 'docker/file.jinja2' as df with context -%}

{{ df.init(
  'ubuntu:20.04',
  music_bot.container_user,
  music_bot.uid,
  music_bot.home,
) }}

{{ df.install([ 'openjdk-8-jdk-headless', 'maven', 'git' ]) }}

{% set jar_file = 'JMusicBot.jar' %}

WORKDIR $HOME
# RUN wget https://github.com/jagrosh/MusicBot/releases/download/0.2.9/JMusicBot-0.2.9-Linux.jar -O {{ jar_file }}
RUN git clone https://github.com/Whew-Inc/MusicBot.git
WORKDIR $HOME/MusicBot
RUN mvn clean package
RUN cp target/JMusicBot-Snapshot-All.jar ../{{ jar_file }}

WORKDIR $HOME
ENTRYPOINT [ "java", "-Dnogui=true", "-jar", "{{ jar_file }}" ]
