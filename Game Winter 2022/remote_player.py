import paho.mqtt.client as mqtt
import numpy as np

prompt = "ECE180D/team2/prompt"
reply = "ECE180D/team2/reply"

# 0. define callbacks - functions that run when events happen.
# The callback for when the client receives a CONNACK response from the server.
def on_connect(client, userdata, flags, rc):
    print("Connection returned result: " + str(rc))

    # Subscribing in on_connect() means that if we lose the connection and
    # reconnect then subscriptions will be renewed.
    client.subscribe(prompt, qos=1)

# The callback of the client when it disconnects.
def on_disconnect(client, userdata, rc):
    if rc != 0:
        print('Unexpected Disconnect')
    else:
        print('Expected Disconnect')

# The default message callback
# (you can create separate callbacks per subscribed topic)
def on_message(client, userdata, message):
    message.payload = message.payload.decode("utf-8")
    print(str(message.payload))


# 1. create a client instance
client = mqtt.Client()
# add additional client options (security, certifications, etc.)
# many default options should be good to start off.
# add callbacks to client
client.on_connect = on_connect
client.on_disconnect = on_disconnect
client.on_message = on_message

# 2. connect to a broker using one of the connect*() functions.
client.connect_async("test.mosquitto.org")

# 3. call one of the loop*() functions to maintain network traffic flow with the broker.
client.loop_start()

# 4. use subscribe() to subscribe to a topic and receive messages

# 5. use publish() to publish messages to the broker.
# payload must be a string, bytearray, int, float or None.
#print('Publishing...')
while True:
    # print("Type your message below:")
    # inp = input("Type 'q' to exit\n")
    inp = input("")

    # if (inp == 'q'):
    #     break
    # else:
    client.publish(reply, inp, qos=1)

# 6. use disconnect() to disconnect from the broker.
client.loop_stop()
client.disconnect()