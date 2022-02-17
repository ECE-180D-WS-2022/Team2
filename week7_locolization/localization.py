import cv2 as cv
import numpy as np
#480 x 640 resolution 
#ver x hor

cap = cv.VideoCapture(0)
while(1):
    # Take each frame
    _, frame = cap.read()
    # Convert BGR to HSV
    hsv = cv.cvtColor(frame, cv.COLOR_BGR2HSV)

    lower_bound1 = np.array([0,50,50])
    upper_bound1 = np.array([10,255,255])
    mask1 = cv.inRange(hsv, lower_bound1, upper_bound1)

    lower_bound2 = np.array([170,50,50])
    upper_bound2 = np.array([180,255,255])
    mask2 = cv.inRange(hsv, lower_bound2, upper_bound2)

    mask=mask1+mask2
    # Bitwise-AND mask and original image
    res = cv.bitwise_and(frame,frame, mask= mask)




    # my edition
    sum=0
    total_num=0
    for col in range(0,480):
        num=0
        for row in range(0,640):
            if mask[col][row]!=0:
                num=num+1
        total_num=total_num+num
        sum=sum+col*num
    if total_num!=0:
        ver_center=round(sum/total_num)
    else:
        ver_center=0

    sum=0
    total_num=0
    for row in range(0,640):
        num=0
        for col in range(0,480):
            if mask[col][row]!=0:
                num=num+1
        total_num=total_num+num
        sum=sum+row*num
    if total_num!=0:
        hor_center=round(sum/total_num)
    else:
        hor_center=0

    print(ver_center)
    print(hor_center)

    if hor_center<200:
        print("you are in the right lane")
    elif hor_center>440:
        print("you are in the left lane")
    else:
        print("you are in the middle lane")

    if (ver_center!=0)&(hor_center!=0):
        left_edge=hor_center-75
        right_edge=hor_center+75
        up_edge=ver_center-75
        down_edge=ver_center+75

        if left_edge<0:
            left_edge=0
        if right_edge>639:
            right_edge=639
        if up_edge<0:
            up_edge=0
        if down_edge>479:
            down_edge=479

        frame[up_edge:down_edge,left_edge]=[0,255,255]
        frame[up_edge:down_edge,right_edge]=[0,255,255]
        frame[up_edge,left_edge:right_edge]=[0,255,255]
        frame[down_edge,left_edge:right_edge]=[0,255,255]

    #end of my edition


    cv.imshow('frame',frame)
    cv.imshow('mask',mask)
    cv.imshow('res',res)


    k = cv.waitKey(5) & 0xFF
    if k == 27:
        break
cap.release()
cv.destroyAllWindows() 