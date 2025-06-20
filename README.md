Ссылка на документацию, результаты и планы развития:

https://docs.google.com/document/d/1YpENCvGWa9eeBJWsodfSVQbPLFPO8jdewIH4s-7KJyI/edit?usp=sharing

В данной таблице собраны данные по камерам:

https://docs.google.com/spreadsheets/d/17yxX-prSVFULPCx3y657XHxPm4NMsxRh5AtvTdvg4Kk/edit?usp=sharing



# # <img src="https://raw.githubusercontent.com/Tarikul-Islam-Anik/Animated-Fluent-Emojis/master/Emojis/Objects/Camera%20with%20Flash.png" alt="Camera with Flash" width="40" height="40" /> CameraTester



---

## <img src="https://raw.githubusercontent.com/Tarikul-Islam-Anik/Animated-Fluent-Emojis/master/Emojis/Objects/Bar%20Chart.png" alt="Bar Chart" width="25" height="25" /> Что анализирует программа?

- **Резкость** (sharpness, дисперсия, blur)
- **PSNR** (относительный уровень шума)
- **Детекция цветных полос (красных, зелёных) и анализ их структуры**
- **Графики яркости вдоль верхней и нижней линии**
- **Метрики для каждой линии**:
  - Сумма градиентов (резкость линии)
  - Среднее значение яркости
  - Стандартное отклонение (шум)
  - Контрастность
  - Длины чёрных и белых полос (равномерность)
  - Коэффициент корреляции яркости по линии
- **Визуализация**: построение графиков и наложение результатов на изображение

---

## <img src="https://raw.githubusercontent.com/Tarikul-Islam-Anik/Animated-Fluent-Emojis/master/Emojis/Objects/Hammer%20and%20Wrench.png" alt="Hammer and Wrench" width="25" height="25" /> Зависимости

| Библиотека       | Назначение                                    |
|------------------|-----------------------------------------------|
| OpenCvSharp      | Работа с изображениями, обработка и анализ    |
| System.Numerics  | Математические расчёты                        |
| Windows Forms    | Графический интерфейс                         |
| System.Windows.Forms.DataVisualization.Charting | Графики        |

---

## ✅ Как использовать

1. **Откройте проект в Visual Studio** (или MonoDevelop на Linux).
2. **Установите необходимые NuGet-пакеты**:  
   - OpenCvSharp  
   - System.Windows.Forms.DataVisualization  
3. **Запустите проект**.
4. **Загрузите тестовое изображение с цветными полосами** через интерфейс.
5. **Нажмите кнопку анализа** — программа вычислит метрики, нарисует линии, построит графики яркости, отобразит коэффициенты и визуализирует результаты.
6. **Изучите отчёт и графики** — результаты появятся в текстовом поле и в виде графиков.

---

## 📌 Описание метрик

- **Резкость** (sharpness): насколько чётко видны детали.
- **Дисперсия**: разброс значений яркости (характеризует шум).
- **Blur (Laplacian)**: показатель "размытия".
- **PSNR**: объективная оценка уровня шума (чем больше — тем чище изображение).
- **Сумма градиентов по линии**: суммарное изменение яркости — резкость переходов.
- **Среднее значение яркости и стандартное отклонение**: яркость и её стабильность на линии.
- **Контрастность**: разница между самыми светлыми и самыми тёмными точками.
- **Длины чёрных/белых полос**: отражают равномерность и регулярность структуры.
- **Коэффициент корреляции**: показывает наличие тренда или "наклона" яркости вдоль линии.
- **Коэффициент крутости**: комплексный показатель, учитывающий шум, резкость и контраст.

---

## 👨‍💻 Разработчики

* Трушин Владисла
* Пересторонин Аким
* Синяков Вячеслав

---

## Пример типового вывода
![Результат 1](https://github.com/JasonScavenger/CameraTester/blob/Results/%D0%A0%D0%B5%D0%B7%D1%83%D0%BB%D1%8C%D1%82%D0%B0%D1%82/%D0%A0%D0%B5%D0%B7%D1%83%D0%BB%D1%8C%D1%82%D0%B0%D1%82%20(1).jpg)



