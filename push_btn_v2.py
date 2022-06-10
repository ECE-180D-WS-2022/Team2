#!/usr/bin/env python3

import signal                   
import sys
import RPi.GPIO as GPIO
import speech_recognition as sr
import paho.mqtt.client as mqtt
import time
import string

import subprocess
from pathlib import Path

BUTTON_GPIO = 16
LED_GPIO = 26

# swap out PyAudio with subprocess bash call
path = Path(__file__).parent.absolute()
file_path = str(path) + "/bash1.wav"

#print(file_path)

# upon CTRL+C
def signal_handler(sig, frame):
    # clean up GPIO
    GPIO.cleanup()
    # clean up mqtt client
    client.loop_stop()
    client.disconnect()
    sys.exit(0)

#  to speech input
def button_callback(channel):
    if not GPIO.input(BUTTON_GPIO):
      print("Button pressed")

      # initiate some kind of ACK handshake with client
      client.publish('ECE180D/team2/commands', 'wait')

      # rest of handshake: pressed --> received --> recording
      # should happen in on_message

        # cmdline cmd here
        #subprocess.run(["arecord", "-D", "plughw:1", "-c1", "-r", "48000",
        #  "-f", "S32_LE", "-t", "wav", "-V", "mono", "-v", "bash1.wav",
        #  "-d2"])
     
        # open
        #f = open("bash1.wav", "rb")
        #audio_stream = f.read()
        #f.close()
        #payload = bytearray(audio_stream)

        #client.publish('ECE180D/team2/commands', payload, qos=1)
      
    else:
      print("Button not pressed")


# mqtt client connect/disconnect functions
def on_connect(client, userdata, flags, rc):
  print("Connection returned result: " + str(rc))
  client.subscribe("ECE180D/team2/commands")

def on_disconnect(client, userdata, rc):
  if rc != 0:
    print("Unexpected disconnect")
  else:
    print("Expected disconnect")

def on_message(client, userdata, message):
  if len(str(message.payload)) == 384044:
    print('Received message: "' + len(str(message.payload)) + '" on topic "'
        + message.topic + '" with QoS ' + str(message.qos))
  else:
    print('Received message: "' + str(message.payload) + '" on topic "'
        + message.topic + '" with QoS ' + str(message.qos))


  # check for client ACK
  temp = str(message.payload)
  if temp == "b'ACK'":
    print('Proceed with voice command')

    # light up indication LED
    GPIO.output(LED_GPIO, GPIO.HIGH)

    # cmdline subprocess call here
    
    subprocess.run(["arecord", "-D", "plughw:1", "-c1", "-r", "48000",
      "-f", "S32_LE", "-t", "wav", "-V", "mono", "-v", file_path,
      "-d2"])

    GPIO.output(LED_GPIO, GPIO.LOW)

    # publish wav file to topic
    f = open(file_path, "rb")
    audio_stream = f.read()
    f.close()
    payload = bytearray(audio_stream)

    client.publish('ECE180D/team2/commands', payload, qos=1)



# mqtt client setup
client = mqtt.Client()
client.on_connect = on_connect
client.on_disconnect = on_disconnect
client.on_message = on_message

client.connect_async("test.mosquitto.org")
client.subscribe("ECE180D/team2/commands");

# list out word dictionaries
heallist = ['heal','ideal','meal','deal','steel','wheel','feel','steal','heel','peel',
    'zeal','kneel','teal','real','hill','pele','eel','you','kiel','recover','recorder']
dlist = ['commend','defend','befriend','command','depend','fend','define','defer',
    'defeat','the','offend']

client.loop_start()

if __name__ == '__main__':
    
    #path = Path(__file__).parent.absolute()
    #print(path)
    
    GPIO.setmode(GPIO.BCM)
    GPIO.setup(BUTTON_GPIO, GPIO.IN, pull_up_down=GPIO.PUD_UP)
    GPIO.setup(LED_GPIO, GPIO.OUT)

    GPIO.add_event_detect(BUTTON_GPIO, GPIO.BOTH, 
            callback=button_callback, bouncetime=50)
    
    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)
    signal.pause()
