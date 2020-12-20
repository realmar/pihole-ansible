import os
import sys

BASEDIR = os.path.abspath(os.path.dirname(__file__))
sys.path.append(BASEDIR)

if __name__ == "__main__":
    import socket
    from random import randint
    from time import sleep
    import mqttconfig
    from config import get_config

    hostname = socket.gethostname()
    config = get_config()
    mqtt_client = mqttconfig.setup_mqtt_client(config["local_ip"])

    try:
        while True:
            mqtt_client.publish(config["topic"] + "/" + hostname,
                                str(randint(
                                    config["value_range"]["min"],
                                    config["value_range"]["max"])),
                                mqttconfig.QUALITY_OF_SERVICE,
                                False)

            sleep(2)
    finally:
        mqtt_client.disconnect()
