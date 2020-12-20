import pathlib
import yaml
from os import path

PATH = str(pathlib.Path(__file__).parent.absolute())


def get_config():
    with open(path.join(PATH, "sensor.yml"), 'r') as file:
        return yaml.load(file)
