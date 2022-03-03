import speech_recognition as sr
import time
import string

r = sr.Recognizer()
m = sr.Microphone()
heallist=['heal','ideal','meal','deal','steel', 'wheel','feel', 'steal','heel','peel','zeal','kneel','teal','real','hill']
dlist=['commend','defend','befriend','command','depend']
try:
    print("A moment of silence, please...")
    with m as source: r.adjust_for_ambient_noise(source,duration=5)
    print("Set minimum energy threshold to {}".format(r.energy_threshold))
    while True:
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
            elif str(value) in dlist:
                value='defend'
            else:
                print(str(value))
                print('try again, that word is not part of the game')
             
            
            # we need some special handling here to correctly print unicode characters to standard output
        #    if str is bytes:  # this version of Python uses bytes for strings (Python 2)
         #       print("You said {}".format(value).encode("utf-8"))
         #   else:  # this version of Python uses unicode for strings (Python 3+)
            print("You said {}".format(value))
        except sr.UnknownValueError:
            print("Oops! Didn't catch that")
        except sr.RequestError as e:
            print("Uh oh! Couldn't request results from Google Speech Recognition service; {0}".format(e))
except KeyboardInterrupt:
    pass
