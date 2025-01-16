using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
/*
Есть несколько заводов, каждый из которых выпускает продукцию одного типа.
Например, завод A выпускает продукт "a", завод B - продукт "b" и т.д.
Каждый завод выпускает разное количество своей продукции.
Завод А - n единиц в час, B - 1.1n единиц в 1 час, С - 1.2n единиц продукции в час и т.д.
Размерами продукции пренебрегаем и предполагаем одинаковыми для всех фабрик, однако каждый продукт имеет уникальные свойства: название, вес, тип упаковки.

Необходимо организовать эффективное складирование продукции заводов, а так же доставку в торговые сети из расчёта, что склад может вмещать
M*(сумму продукции всех фабрик в час).
По заполнению склада не менее чем на 95% склад должен начинать освобождаться от продукции следующим образом.
Продукцию со склада забирает грузовой транспорт различной вместимости (не менее двух видов грузовиков).
Грузовик может забирать продукцию разных типов. 
М - может быть переменным, но не менее 100. Число заводов может быть переменным, но не менее трёх. n может быть переменным, но не менее 50 

Необходимо вывести следующие результаты работы алгоритма: 
-последовательность поступления продукции на склад (фабрика, продукт, число единиц) 
-необходимо подсчитать статистику, сколько продукции и какого состава в среднем перевозят грузовики. 
Приложение предлагается реализовать многопоточным. 
В случае успешного выполнения пришлите ссылку на гит с реализацией задания на akretov@rustest.ru. И в скором времени мы с вами свяжемся.


*/


// к сожалению, от нехватки комерческого опыта не придумал куда добавить многопоточноть в данном решении.

namespace Test
{
    class MainClass
    {
        async public static Task Main(string[] args)
        {
            Work work = new Work();

            work.WorkStorege();
        }
    }

    class Work
    {
        private float _sleepCount = 1000;
        private int _countProductOne = 0;
        private int _countProductTwo = 0;
        private int _countProductThree = 0;
        private int _countAllProduct = 0;
        private int _countFactory = 0;

        Storage _storage = new Storage();
        private Truck _tempTruck = null;
        private Product _tempProduct = null;

        private List<Product> _productsSamples = new List<Product>();

        public void WorkStorege()
        {
            _countFactory = _storage.TakeCountFactory();

            for (int i = 0; i < _countFactory; i++)
            {
                _productsSamples.Add(_storage.TakeSampleProduct(i));
            }

            while (true)
            {
                _storage.FillingStorege();

                if (_storage.CheckCapacity())
                {
                    if(_tempTruck != null && _tempTruck.InWay == false)
                    {
                        _tempTruck = _storage.GetTransportForLoading();
                    }
                    else
                    {
                        _storage.CreateRoute();
                        _tempTruck = _storage.GetTransportForLoading();
                    }

                    for (int i = 0; i < _tempTruck.SizeCargo; i++)
                    {
                        _tempProduct = _storage.TakeProduct();

                        if(_tempProduct != null)
                        {
                            if (_tempProduct == _productsSamples[0])
                            {
                                _countProductOne++;
                                _tempTruck.LoadingPruduct(_tempProduct);
                            }
                            else if (_tempProduct == _productsSamples[1])
                            {
                                _countProductTwo++;
                                _tempTruck.LoadingPruduct(_tempProduct);
                            }
                            else if (_tempProduct == _productsSamples[2])
                            {
                                _countProductThree++;
                                _tempTruck.LoadingPruduct(_tempProduct);
                            }

                            _countAllProduct++;
                        }
                    }

                    _tempTruck.Send();
                }

                Console.Clear();

                if(_tempTruck != null)
                {
                    ShowStatistics();
                    Console.WriteLine();
                    _storage.ShowRoute();
                }

                Console.WriteLine();

                _storage.ShowInfoFactory();

                Console.WriteLine($"шаг операции {_sleepCount/1000} секунда");
                Thread.Sleep(Convert.ToInt32(_sleepCount));
            }
        }

        private void ShowStatistics()
        {
            Console.WriteLine($"стасистика перевозки:" +
                $"\nпервый продукт - {_countProductOne}({TransportationPercentage(_countAllProduct, _countProductOne)})" +
                $"\nвторой продуки - {_countProductTwo}({TransportationPercentage(_countAllProduct, _countProductTwo)})" +
                $"\nтретий продукт - {_countProductThree}({TransportationPercentage(_countAllProduct, _countProductThree)})");
        }

        private int TransportationPercentage(int allCount, int thisProduct)
        {
            return ((100 * thisProduct) / allCount);
        }
    }

    class Storage
    {
        private List<Factory> _factories = new List<Factory>();
        private Queue<Product> _products = new Queue<Product>();
        private List<Truck> _carPark = new List<Truck>();
        private List<Truck> _trucks = new List<Truck>();

        private int _maxStore = 0;
        private int _currentStoreCapacity = 0;
        private int _minCounter = 100;
        private int _maxCounter = 500;
        private int _storageLimit;

        public Storage()
        {
            _carPark.Add(new TruckA());
            _carPark.Add(new TruckB());
            _carPark.Add(new TruckC());

            _factories.Add(new FactoryA());
            _factories.Add(new FactoryB());
            _factories.Add(new FactoryC());

            CountMaxStore();
        }

        public void Countdown()
        {
            foreach (Truck truck in _trucks)
            {
                if(truck.InWay == true)
                {
                    truck.NextDay();
                }
            }
        }

        public int TakeCountFactory()
        {
            return _factories.Count;
        }

        public Product TakeSampleProduct(int index)
        {
            return _factories[index].Product;
        }

        public bool CheckCapacity()
        {
            return _currentStoreCapacity > _storageLimit;
        }

        public void FillingStorege() 
        {
            foreach (Factory factory in _factories)
            {
                for (int i = 0; i < factory.ProductPerHour; i++)
                {
                    _products.Enqueue(factory.Product);
                }
            }
        }

        public Product TakeProduct()
        {
            if(_products.Peek() != null)
            {
                return _products.Dequeue();
            }
            else
            {
                return null;
            }
        }

        public void CreateRoute()
        {
            _trucks.Add(_carPark[UserUtils.GenerateRandomNumber(0, _carPark.Count)].Clone());
        }

        public Truck GetTransportForLoading()
        {
            for (int i = 0; i < _trucks.Count; i++)
            {
                if(_trucks[i].InWay == false)
                {
                    return _trucks[i];
                }
            }

            return null;
        }

        public void ShowRoute()
        {
            foreach (Truck truck in _trucks)
            {
                if(truck.InWay == true)
                {
                    truck.ShowInfo();
                    truck.NextDay();
                }
            }
        }

        public void ShowInfoFactory()
        {
            _currentStoreCapacity = _products.Count;

            foreach (Factory factory in _factories)
            {
                Console.WriteLine($"{factory.Name} - поставляет на склад {factory.Product.Name} в количестве {factory.ProductPerHour} шт/час");
            }

            Console.WriteLine($"\nмаксимальная вместительность склада - {_maxStore}");
            Console.WriteLine($"предел в 95% - {_storageLimit}");
            Console.WriteLine($"текущее количество товаров на складе - {_currentStoreCapacity}");
        }

        private void CountMaxStore()
        {
            foreach (Factory factory in _factories)
            {
                _maxStore += factory.ProductPerHour;
            }

            _maxStore = _maxStore * UserUtils.GenerateRandomNumber(_minCounter, _maxCounter);
            _storageLimit = _maxStore * 95 / 100;
        }
    }

    abstract class Factory
    {
        public string Name { get; private set; }
        public Product Product { get; private set; }
        public int ProductPerHour { get; private set; }

        public Factory(Product product, int productPerHour, string name)
        {
            Name = name;
            Product = product;
            ProductPerHour = productPerHour;
        }
    }

    abstract class Product
    {
        public string Name { get; private set; }
        public int Weight { get; private set; }
        public string Type { get; private set; }

        public Product(string name, int weight, string type)
        {
            Name = name;
            Weight = weight;
            Type = type;
        }

        public void ShowInfo()
        {
            Console.WriteLine($"{Name} - {Type} - {Weight}");
        }
    }

    abstract class Truck
    {
        private int _currentDayInWay = 0;
        private Queue<Product> _products = new Queue<Product>();

        public string Name { get; private set; }
        public int  SizeCargo { get; private set; }
        public int TimeTravel  { get; private set; }
        public bool InWay { get; private set; }

        public Truck(string name, int sizeCargo, int timeTravel, bool inWay)
        {
            Name = name;
            SizeCargo = sizeCargo;
            TimeTravel = timeTravel;
            InWay = inWay;
        }

        public void LoadingPruduct(Product product)
        {
            _products.Enqueue(product);
        }

        public void NextDay()
        {
            _currentDayInWay++;

            if(_currentDayInWay == TimeTravel)
            {
                InWay = false;
                _currentDayInWay = 0;
            }
        }

        public void Send()
        {
            InWay = true;
        }

        public void ShowInfo()
        {
            Console.WriteLine($"{Name} - {SizeCargo} - {_currentDayInWay}/{TimeTravel} - {InWay}");
        }

        public abstract Truck Clone();

    }

    class FactoryA : Factory
    {
        public FactoryA() : base(new ProductA(), UserUtils.GenerateRandomProductPesHour(), UserUtils.TakeNameFactory())
        {

        }
    }

    class FactoryB : Factory
    {
        public FactoryB() : base(new ProductB(), UserUtils.GenerateRandomProductPesHour(), UserUtils.TakeNameFactory())
        {

        }
    }
    
    class FactoryC : Factory
    {
        public FactoryC() : base(new ProductC(), UserUtils.GenerateRandomProductPesHour(), UserUtils.TakeNameFactory())
        {

        }
    }

    class ProductA : Product
    {
        public ProductA() : base(UserUtils.TakeNameProduct(), UserUtils.GenerateRandomWight(), UserUtils.TakeTypeProduct())
        {

        }
    }

    class ProductB : Product
    {
        public ProductB() : base(UserUtils.TakeNameProduct(), UserUtils.GenerateRandomWight(), UserUtils.TakeTypeProduct())
        {

        }
    }

    class ProductC : Product
    {
        public ProductC() : base(UserUtils.TakeNameProduct(), UserUtils.GenerateRandomWight(), UserUtils.TakeTypeProduct())
        {

        }
    }

    class TruckA : Truck
    {
        public TruckA() : base(UserUtils.TakeNameTruck(), 750, UserUtils.GenerateRandomNumber(3, 10), false)
        {

        }

        public override Truck Clone()
        {
            return new TruckA();
        }
    }

    class TruckB : Truck
    {
        public TruckB() : base(UserUtils.TakeNameTruck(), 1500, UserUtils.GenerateRandomNumber(3, 10), false)
        {

        }

        public override Truck Clone()
        {
            return new TruckB();
        }
    }

    class TruckC : Truck
    {
        public TruckC() : base(UserUtils.TakeNameTruck(), 2500, UserUtils.GenerateRandomNumber(3, 10), false)
        {

        }

        public override Truck Clone()
        {
            return new TruckC();
        }
    }

    class UserUtils
    {
        private static Random s_random = new Random();
        private static string[] s_names = new string[] {"продукт_1", "продукт_2", "продукт_3", "продукт_4", "продукт_5", "продукт_6", "продукт_7" };
        private static string[] s_types = new string[] { "тип_1", "тип_2", "тип_3", "тип_4", "тип_5", "тип_6", "тип_7" };
        private static int s_minWight = 1;
        private static int s_maxWight = 5;
        private static int s_minProductPesHour = 100;
        private static int s_maxProductPesHour = 1000;
        private static int s_indexFactory = 1;
        private static int s_indexTruck = 1;
        private static int s_minSizeCargo = 500;
        private static int s_maxSizeCargo = 2000;
        private static string s_nameFactory = "фабрика_";
        private static string s_nameTruck = "маршрут_";

        public static int GenerateRandomNumber(int minRandomNumber, int maxRandomNumber)
        {
            return s_random.Next(minRandomNumber, maxRandomNumber);
        }

        public static int GenerateRandomProductPesHour()
        {
            return s_random.Next(s_minProductPesHour, s_maxProductPesHour);
        }

        public static int GenerateRandomWight()
        {
            return s_random.Next(s_minWight, s_maxWight);
        }

        public static string TakeNameProduct()
        {
            return s_names[GenerateRandomNumber(0, s_names.Length)];
        }

        public static string TakeTypeProduct()
        {
            return s_types[GenerateRandomNumber(0, s_types.Length)];
        }

        public static string TakeNameFactory()
        {
            return s_nameFactory + s_indexFactory++;
        }

        public static string TakeNameTruck()
        {
            return s_nameTruck + s_indexTruck++;
        }

        public static int GetSizeCargo()
        {
            return s_random.Next(s_minSizeCargo, s_maxSizeCargo);
        }
    }
}
