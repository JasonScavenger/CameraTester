# -*- coding: utf-8 -*-
# строчка выше нужна чтоб питон жрал кириллицу (utf-8) 

from tkinter import *
from PIL import ImageTk, Image
from tkinter import filedialog
import os

root = Tk()     # создаем корневой объект - окно
# Влад Аким Никита Камилла Слава
root.title("VANKS Camera Tester")     # устанавливаем заголовок окна
root.geometry("400x350")    # устанавливаем размеры окна
 
label = Label(text="здарова бандиты") # создаем текстовую метку
label.pack(anchor=NW)    # размещаем метку в окне

panel = Label(root)

ImageToAnalyze = None

def openfn():
    filename = filedialog.askopenfilename(title='open')
    return filename

def open_img():
    global panel
    global ImageToAnalyze

    filename = openfn()
    ImageToAnalyze = Image.open(filename)
    scale = ImageToAnalyze.height / ImageToAnalyze.width
    if (scale > 1):
        height = 256
        width = (int)(height / scale)
    else:
        width = 256
        height = (int)(width * scale)
    img = ImageToAnalyze.resize((width, height), Image.Resampling.BILINEAR)
    img = ImageTk.PhotoImage(img)
    panel.destroy()
    panel = Label(root, image=img)
    panel.image = img
    panel.pack(anchor=NW)

btn = Button(root, text='Открыть картинку для анализа', command=open_img).pack(anchor=NW)

def analyze():
    return 0

btn2 = Button(root, text='Анализ', command=analyze)

btn2.place(x=256, y=20)

root.update()

btn2.place(x=256 - btn2.winfo_width(), y=20)
 
root.mainloop()
