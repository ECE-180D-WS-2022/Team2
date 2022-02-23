import cv2
import numpy as np
#480 x 640 resolution 
#ver x hor

img_path = 'test.jpg'
img = cv2.imread(img_path)
if img is None:
    print("ERROR")
    exit(1)
gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)


FaceCascade = cv2.CascadeClassifier('C:/Users/yuson/anaconda3/Library/etc/haarcascades/haarcascade_frontalface_default.xml')

faces = FaceCascade.detectMultiScale(
    gray,
    scaleFactor=1.1,
    minNeighbors=5
)

for face in faces:
    (x, y, w, h) = face
    cv2.rectangle(img, (x, y), (x+w, y+h), (0, 255, 0), 4)

cv2.namedWindow('Face',flags=cv2.WINDOW_NORMAL | cv2.WINDOW_KEEPRATIO | cv2.WINDOW_GUI_EXPANDED)
 
cv2.imshow('Face', img)
cv2.waitKey(0)
cv2.destroyAllWindows()