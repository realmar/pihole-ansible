FROM nginx:latest

ENV HOME /tmp
WORKDIR $HOME/app

COPY ./realmar $HOME/app

RUN apt-get update && apt-get install -y nodejs npm
RUN mkdir -p $HOME/app

RUN npm install
RUN npm run build-prod
RUN cp -r $HOME/app/dist/realmar/* /usr/share/nginx/html/.
