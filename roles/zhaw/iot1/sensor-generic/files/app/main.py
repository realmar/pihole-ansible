import os
import sys
from sensor_type import *

BASEDIR = os.path.abspath(os.path.dirname(__file__))
sys.path.append(BASEDIR)


def read(pin, type):
    if type == ANALOG:
        return grovepi.analogRead(pin)
    elif type == DIGITAL:
        return grovepi.digitalRead(pin)
    elif type == ULTRASONIC:
        return grovepi.ultrasonicRead(pin)


if __name__ == "__main__":
    import socket
    from random import randint
    from time import sleep
    import mqttconfig
    from config import get_config
    import grovepi
    import json

    h = socket.gethostname()
    try:
        hostname = h.split("-")[1]
    except:
        hostname = h

    config = get_config()
    mqtt_client = mqttconfig.setup_mqtt_client(config["local_ip"])

    sensors = config["sensors"]

    for sensor in sensors:
        grovepi.pinMode(sensor["grovepi_pin"], "INPUT")

    sleep(1)

    try:
        while True:
            for sensor in sensors:
                mqtt_client.publish("iot1/" + sensor["name"] + "/" + hostname,
                                    json.dumps(
                                        {
                                            "value":  read(sensor["grovepi_pin"], sensor["grovepi_type"])
                                        }
                ),
                    sensor["mqtt_qos"],
                    False)
            sleep(config["send_interval"])
    finally:
        mqtt_client.disconnect()
