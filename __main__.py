import speech_recognition as sr
import paho.mqtt.client as mqtt
import time
import string

def on_connect(client, userata, flags, rc):
    print("Connection returned result: " + str(rc))
    client.subscribe("ECE180d/team2")

def on_disconnect(client, userdata, rc):
    if rc != 0:
        print("Unexpected disconnect")
    else:
        print("Expected disconnect")

def on_message(client, user_data, message):
    print('Received message: "' + str(message.payload) + '" on topic "' + message.topic + '" with QoS ' + str(message.qos))

# create/setup client instance
client = mqtt.Client()
client = mqtt.Client()
client.on_connect = on_connect
client.on_disconnect = on_disconnect
client.on_message = on_message

client.connect_async("test.mosquitto.org")
client.subscribe('ECE180D/team2')

r = sr.Recognizer()
m = sr.Microphone()

heallist=['heal','ideal','meal','deal','steel', 'wheel','feel', 'steal','heel','peel','zeal','kneel','teal','real','hill','pele']
dlist=['commend','defend','befriend','command','depend']

# set flag --> flag value will depend on unity
flag = 0

try:
    print("A moment of silence, please...")
    with m as source: r.adjust_for_ambient_noise(source,duration=5)
    print("Set minimum energy threshold to {}".format(r.energy_threshold))
    while True:
        # keyboard input to trigger listening (temp)
        inp = input("\nType 'Start' to begin listening:\n")
        inp = inp.lower()
        if (inp == "start"):
            print("Say something!")
            with m as source: audio = r.listen(source)
            print("Got it! Now to recognize it...")
            try:
                # recognize speech using Google Speech Recognition
                value = r.recognize_google(audio)
                value=str(value).split(" ",)
                value=value[0].lower()
                if str(value) in heallist:
                    value='heal'
                    flag = 1
                elif str(value) in dlist:
                    value='defend'
                    flag = 2
                else:
                    print(str(value))
                    print('try again, that word is not part of the game')
                    flag = 0
                client.publish('ECE180D/team2', flag, qos=1)
            
            # we need some special handling here to correctly print unicode characters to standard output
            #    if str is bytes:  # this version of Python uses bytes for strings (Python 2)
            #       print("You said {}".format(value).encode("utf-8"))
            #   else:  # this version of Python uses unicode for strings (Python 3+)
            print("You said {}".format(value))
            except sr.UnknownValueError:
                print("Oops! Didn't catch that")
            except sr.RequestError as e:
                print("Uh oh! Couldn't request results from Google Speech Recognition service; {0}".format(e))
        else:
            print("Unrecognized input, try again")
            continue
            
except KeyboardInterrupt:
    # disconnect
    client.loop_stop()
    client.disconnect()
    pass
