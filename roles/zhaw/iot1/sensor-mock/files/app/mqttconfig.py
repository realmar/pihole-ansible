#!/usr/bin/env python3

'''

                    ___           ___           ___
        ___        /\__\         /\  \         /\  \
       /\  \      /::|  |       /::\  \       /::\  \
       \:\  \    /:|:|  |      /:/\:\  \     /:/\ \  \
       /::\__\  /:/|:|  |__   /::\~\:\  \   _\:\~\ \  \
    __/:/\/__/ /:/ |:| /\__\ /:/\:\ \:\__\ /\ \:\ \ \__\
   /\/:/  /    \/__|:|/:/  / \:\~\:\ \/__/ \:\ \:\ \/__/
   \::/__/         |:/:/  /   \:\ \:\__\    \:\ \:\__\
    \:\__\         |::/  /     \:\ \/__/     \:\/:/  /
     \/__/         /:/  /       \:\__\        \::/  /
                   \/__/         \/__/         \/__/


    File:          mqttconfig.py


    Purpose:       Globally used MQTT stuff
                   is placed here.


    Remarks:       - This application
                     uses the paho-mqtt-module.
                     Be sure it is installed
                     on the device you want
                     to run this application.


    Author:        P. Leibundgut <leiu@zhaw.ch>


    Date:          10/2016

'''

import sys
import log
import paho.mqtt.client as mqtt
from config import get_config


# logging setup
logger = log.setup_custom_logger( "mqtt_thing_mqttconfig" )

# MQTT related globals
BROKER_IP            = get_config()["broker_address"]
BROKER_PORT          = int( 1883 )
CONNECTION_KEEPALIVE = int(   60 ) # unit is seconds
QUALITY_OF_SERVICE   = int(    0 )

# Every pulblisher/subscriber requires
# a mqtt client instance.
def setup_mqtt_client( local_ip ):

  mqtt_client = mqtt.Client()
  mqtt_client.on_connect = on_mqtt_connect
  mqtt_client.on_publish = on_mqtt_publish
  mqtt_client.on_disconnect = on_mqtt_disconnect

  try:
    mqtt_client.connect( BROKER_IP,
                         BROKER_PORT,
                         CONNECTION_KEEPALIVE,
                         local_ip )
  except:
    logger.debug( "could not establish connection to broker with ip : " + BROKER_IP )
    sys.exit( 1 )

  mqtt_client.loop_start()

  return mqtt_client


# functions where the instance of the mqtt client points on
def on_mqtt_publish( client, userdata, mid ):
  logger.debug( "MQTT client successfully published a message to broker " + BROKER_IP )


def on_mqtt_connect( client, userdata, flags, rc ):
  logger.debug( "connected to broker with result code: " + str( rc ) )


def on_mqtt_disconnect( client, userdata, rc ):
  if rc != 0:
    logger.debug( "Unexpected disconnection." )
  else:
    logger.debug( "MQTT client disconnected without errors." )
