import paho.mqtt.client as mqtt
import cv2
import numpy as np
import time

address="ECE180D/team2/position"
FaceCascade = cv2.CascadeClassifier('C:/Users/yuson/anaconda3/Library/etc/haarcascades/haarcascade_frontalface_default.xml')
cap = cv2.VideoCapture(0)

def on_connect(client, userdata, flags, rc):
    print("Connection returned result: "+str(rc))

def on_disconnect(client, userdata, rc):
    if rc != 0:
        print('Unexpected Disconnect')
    else:
        print('Expected Disconnect')

def on_message(client, userdata, message):
    print('Received message: "' + str(message.payload) + '" on topic "' +
        message.topic + '" with QoS ' + str(message.qos))

client = mqtt.Client()
client.on_connect = on_connect
client.on_disconnect = on_disconnect
client.on_message = on_message
client.connect_async("test.mosquitto.org")
client.loop_start()

while(True):
    ret, frame = cap.read()
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)

    faces = FaceCascade.detectMultiScale(
        gray,
        scaleFactor=1.1,
        minNeighbors=5
    )

    if len(faces)==0:
        msg="N"
    elif faces[0][0]<120:
        msg="R"
    elif faces[0][0]>320:
        msg="L"
    else:
        msg="M"
    # print(faces)

    for face in faces:
        (x, y, w, h) = face
        cv2.rectangle(frame, (x, y), (x+w, y+h), (0, 255, 0), 4)
    
    cv2.namedWindow('Face',flags=cv2.WINDOW_NORMAL | cv2.WINDOW_KEEPRATIO | cv2.WINDOW_GUI_EXPANDED)
    cv2.imshow('Face', frame)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break


    print('Publishing Position: '+msg)
    client.publish(address, msg, qos=1)
    time.sleep(0.15)

client.loop_stop()
client.disconnect()
cap.release()
cv2.destroyAllWindows() 


