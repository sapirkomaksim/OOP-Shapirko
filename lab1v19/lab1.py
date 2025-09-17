class Weather:
    def __init__(self, date):
        self.date = date
        print("Конструктор Weather Викликаний")
    
    def __del__(self):
        print("Деструктор Weather Викликаний")

    def Area(self):
        return self.date
    
    def GetWeather(self):
        return f"Погода на дату: {self.date}"


f = Weather(25)
print(f.date)          
print(f.GetWeather())  
