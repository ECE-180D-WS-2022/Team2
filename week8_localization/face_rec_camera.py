import cv2
import numpy as np
#480 x 640 resolution 
#ver x hor

FaceCascade = cv2.CascadeClassifier('C:/Users/yuson/anaconda3/Library/etc/haarcascades/haarcascade_frontalface_default.xml')
cap = cv2.VideoCapture(0)

while(True):
    
    ret, frame = cap.read()
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)

    faces = FaceCascade.detectMultiScale(
        gray,
        scaleFactor=1.1,
        minNeighbors=5
    )

    if len(faces)==0:
        print("Player Undetected!")
    elif faces[0][0]<120:
        print("Right Lane")
    elif faces[0][0]>320:
        print("Left Lane")
    else:
        print("Middle Lane")
    # print(faces)

    for face in faces:
        (x, y, w, h) = face
        cv2.rectangle(frame, (x, y), (x+w, y+h), (0, 255, 0), 4)
    
    cv2.namedWindow('Face',flags=cv2.WINDOW_NORMAL | cv2.WINDOW_KEEPRATIO | cv2.WINDOW_GUI_EXPANDED)
    cv2.imshow('Face', frame)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break
    
cap.release()
cv2.destroyAllWindows() 