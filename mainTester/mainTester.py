# -*- coding: utf-8 -*-
# строчка выше нужна чтоб питон жрал кириллицу (utf-8) 

from tkinter import *
from PIL import ImageTk, Image
from tkinter import filedialog
import os

import cv2

import cv2 as cv
import numpy as np
from matplotlib import pyplot as plt

root = Tk()     # создаем корневой объект - окно
# Влад Аким Никита Камилла Слава
root.title("VANKS Camera Tester")     # устанавливаем заголовок окна
root.geometry("400x350")    # устанавливаем размеры окна
 
label = Label(text="здарова бандиты") # создаем текстовую метку
label.pack(anchor=NW)    # размещаем метку в окне

panel = Label(root)

ImageToAnalyzePath = ""

def openfn():
    filename = filedialog.askopenfilename(title='open')
    return filename

def open_img():
    global panel
    global ImageToAnalyzePath

    filename = openfn()
    ImageToAnalyzePath = filename
    img = Image.open(filename)
    scale = img.height / img.width
    if (scale > 1):
        height = 256
        width = (int)(height / scale)
    else:
        width = 256
        height = (int)(width * scale)
    img = img.resize((width, height), Image.Resampling.BILINEAR)
    img = ImageTk.PhotoImage(img)
    panel.destroy()
    panel = Label(root, image=img)
    panel.image = img
    panel.pack(anchor=NW)

def calculate_noise_metrics(image_path):
    # Load the image
    image = cv2.imread(image_path)

    # Convert the image to grayscale
    gray_image = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    # Apply Gaussian blur to the grayscale image
    blurred_image = cv2.GaussianBlur(gray_image, (5, 5), 0)

    # Calculate the noise by subtracting the blurred image from the original grayscale image
    noise = gray_image - blurred_image

    # Calculate the mean and standard deviation of the noise
    mean_noise = np.mean(noise)
    std_noise = np.std(noise)

    return mean_noise, std_noise

def analyze():
    global ImageToAnalyzePath
    result = calculate_noise_metrics(ImageToAnalyzePath)
    print("mean noise: " + str(result.__getitem__(0)) + "; std noise: " + str(result.__getitem__(1)))

img_rgb = cv.imread('photo.png')
assert img_rgb is not None, "file could not be read, check with os.path.exists()"
img_gray = cv.cvtColor(img_rgb, cv.COLOR_BGR2GRAY)
template = cv.imread('black.png', cv.IMREAD_GRAYSCALE)
assert template is not None, "file could not be read, check with os.path.exists()"
w, h = template.shape[::-1]
 
res = cv.matchTemplate(img_gray,template,cv.TM_CCOEFF_NORMED)
threshold = 0.8
loc = np.where( res >= threshold)
for pt in zip(*loc[::-1]):
    cv.rectangle(img_rgb, pt, (pt[0] + w, pt[1] + h), (0,0,255), 2)
 
cv.imwrite('res.png',img_rgb)

btn = Button(root, text='Открыть картинку для анализа', command=open_img).pack(anchor=NW)
btn2 = Button(root, text='Анализ', command=analyze)
btn2.place(x=256, y=20)
root.update()
btn2.place(x=256 - btn2.winfo_width(), y=20)
root.mainloop()
