#!/usr/bin/env python3

import speech_recognition as sr
import paho.mqtt.client as mqtt
import numpy as np
import time
import string

# for word "heal" --> switched to less ambiguous "recover"
heallist=['heal','ideal','meal','deal','steel', 'wheel','feel', 'steal','heel','peel','zeal','kneel','teal','real','hill','pele','eel','you','kiel','recover','recorder']

# for word "defend"
dlist=['commend','defend','befriend','command','depend','fend','define','defer','defeat','the','offend']

def on_connect(client, userdata, flags, rc):
  print("Connection returned result: "+str(rc))

  # Subscribing in on_connect() means that if we lose the connection and
  # reconnect then subscriptions will be renewed.
  client.subscribe("ECE180D/team2/commands")

# The callback of the client when it disconnects. 
def on_disconnect(client, userdata, rc): 
  if rc != 0: 
    print('Unexpected Disconnect')
  else:
    print('Expected Disconnect')

# The default message callback. 
# (won't be used if only publishing, but can still exist)
def on_message(client, userdata, message): 
  print('Received message: "' + str(len(message.payload)) + '" on topic "' + 
        message.topic + '" with QoS ' + str(message.qos))

  temp = str(message.payload)
  # print(temp)
  # check payload contents/length
  if str(message.payload) == "b'pressed'":
      client.publish('ECE180D/team2/commands', 'ACK', qos=1)

  if len(message.payload) == 384044:
    # save byte stream to wav
    byte_array = message.payload
    byte_data = bytes(byte_array)
    with open("from_pi1.wav", "wb") as f:
      f.write(byte_data)
      f.close()

    # transcribe audio upon receiving .wav file
    from os import path
    AUDIO_FILE = path.join(path.dirname(path.realpath(__file__)), "from_pi1.wav")
    r = sr.Recognizer()
    with sr.AudioFile(AUDIO_FILE) as source:
        audio = r.record(source)

    try:
        value = r.recognize_google(audio)
        value = str(value).split(" ",)
        value = value[0].lower()
        if str(value) in heallist:
            value = 'heal'
            client.publish('ECE180D/team2/commands', value, qos=1)
        elif str(value) in dlist:
            value = 'defend'
            client.publish('ECE180D/team2/commands', value, qos=1)
        print("Google Speech Recognition thinks you said " + str(value))
    except sr.UnknownValueError:
        print("Google Speech Recognition could not understand audio")
    except sr.RequestError as e:
        print("Could not request results from Google Speech Recognition service; {0}".format(e))


# create/setup client instance
client = mqtt.Client()
client = mqtt.Client()
client.on_connect = on_connect
client.on_disconnect = on_disconnect
client.on_message = on_message

client.connect_async("test.mosquitto.org")
client.subscribe('ECE180D/team2/commands')

# start client loop
client.loop_start()

try:
    while True:
        print("waiting for new speech command")
        
        # obtain path to .wav file in the same folder as this script
        #from os import path
        #AUDIO_FILE = path.join(path.dirname(path.realpath(__file__)), "from_pi1.wav")
        # AUDIO_FILE = path.join(path.dirname(path.realpath(__file__)), "french.aiff")
        # AUDIO_FILE = path.join(path.dirname(path.realpath(__file__)), "chinese.flac")

        # use the audio file as the audio source
        #r = sr.Recognizer()
        #with sr.AudioFile(AUDIO_FILE) as source:
        #    audio = r.record(source)  # read the entire audio file

        # recognize speech using Google Speech Recognition
        #try:
            # for testing purposes, we're just using the default API key
            # to use another API key, use `r.recognize_google(audio, key="GOOGLE_SPEECH_RECOGNITION_API_KEY")`
            # instead of `r.recognize_google(audio)`
        #    value = r.recognize_google(audio)
        #    value = str(value).split(" ",)
        #    value = value[0].lower()
        #    if str(value) in heallist:
        #        value = 'heal'
        #        client.publish('ECE180D/team2/commands', value, qos=1)
        #    elif str(value) in dlist:
        #        value = 'defend'
        #        client.publish('ECE180D/team2/commands', value, qos=1)
        #    print("Google Speech Recognition thinks you said " + str(value))
        #except sr.UnknownValueError:
        #    print("Google Speech Recognition could not understand audio")
        #except sr.RequestError as e:
        #    print("Could not request results from Google Speech Recognition service; {0}".format(e))

except KeyboardInterrupt:
    # force disconnect
    client.loop_stop()
    client.disconnect()
    pass




