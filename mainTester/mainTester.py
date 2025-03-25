# -*- coding: utf-8 -*-
# строчка выше нужна чтоб питон жрал кириллицу (utf-8) 

from tkinter import *
from PIL import ImageTk, Image
from tkinter import filedialog
import os

def openfn():
    filename = filedialog.askopenfilename(title='open')
    return filename

def open_img():
    filename = openfn()
    img = Image.open(filename)
    #img = img.resize((250, 250), Image.)
    img = ImageTk.PhotoImage(img)
    panel = Label(root, image=img)
    panel.image = img
    panel.pack()
 
root = Tk()     # создаем корневой объект - окно
root.title("camera tester")     # устанавливаем заголовок окна
root.geometry("300x250")    # устанавливаем размеры окна
 
label = Label(text="здарова бандиты") # создаем текстовую метку
label.pack()    # размещаем метку в окне

btn = Button(root, text='open image', command=open_img).pack()
 
root.mainloop()
