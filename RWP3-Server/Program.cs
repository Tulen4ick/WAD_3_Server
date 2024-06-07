using RWP3;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SocketSever
{
    public class Program
    {
        public static CarModel Car_ModelToCarModel(Car_Model cm)
        {
            return (CarModel)cm;
        }
        public static TruckModel Car_ModelToTruckModel(Car_Model cm)
        {
            return (TruckModel)cm;
        }
        public static TankModel Car_ModelToTankModel(Car_Model cm)
        {
            return (TankModel)cm;
        }

        

        static Dictionary<CarBrand, List<Car_Model>> cars = new Dictionary<CarBrand, List<Car_Model>>();
        static /*async*/ void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);
            try
            {
                Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);
                while (true)
                {
                    Console.WriteLine("Ожидаем соединение через порт ", ipEndPoint);
                    /*var sClient = await sListener.AcceptAsync();
                    Task.Run(async () => await GenerateCars(sClient));*/
                    byte[] bytes = new byte[1024];
                    Socket handler = sListener.Accept();
                    int bytesRec = handler.Receive(bytes);
                    /*int bytesRec = await handler.ReceiveAsync(bytes, SocketFlags.None);*/
                    string data = null;
                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                    XmlDocument xmlbrand = new XmlDocument();
                    xmlbrand.LoadXml(data);
                    StringReader stringReader = new StringReader(xmlbrand.OuterXml);
                    XmlSerializer xmlSerializerBrands = new XmlSerializer(typeof(CarBrand));
                    CarBrand cb = xmlSerializerBrands.Deserialize(stringReader) as CarBrand;
                    Console.WriteLine("Полученная машина: " + cb.Name + " " + cb.Model);
                    cars.ContainsKey(cb);
                    if (!cars.ContainsKey(cb))
                    {
                        cars.Add(cb, new List<Car_Model>());
                        Random rnd = new Random();
                        int count = rnd.Next(10, 20);
                        for (int i = 0; i < count; ++i)
                        {
                            if (cb.Type == "Легковой")
                            {
                                string[] MultyMedia = { "Pioneer", "Alpine", "Sony", "Prology" };
                                CarModel cm = new CarModel(rnd.Next(100000, 999999), MultyMedia[rnd.Next(0, 3)], rnd.Next(1, 3));
                                cars[cb].Add(cm);
                            }
                            else if (cb.Type == "Грузовой")
                            {
                                TruckModel tm = new TruckModel(rnd.Next(100000, 999999), rnd.Next(4, 20), rnd.Next(100, 1000));
                                cars[cb].Add(tm);
                            }
                            else if (cb.Type == "Танк")
                            {
                                string[] Ammo = { "Cumulative", "Armor-piercing", "Fragmentation" };
                                TankModel tm = new TankModel(rnd.Next(100000, 999999), rnd.Next(3, 8), Ammo[rnd.Next(0, 2)]);
                                cars[cb].Add(tm);
                            }
                            Thread.Sleep(rnd.Next(0, 500));
                        }
                    }
                    if (cb.Type == "Легковой")
                    {
                        XmlSerializer xmlSerializerCars = new XmlSerializer(typeof(List<CarModel>));
                        XmlDocument xmlDocument = new XmlDocument();
                        List<CarModel> cm = cars[cb].ConvertAll(new Converter<Car_Model, CarModel>(Car_ModelToCarModel));
                        using (MemoryStream ms = new MemoryStream())
                        {
                            xmlSerializerCars.Serialize(ms, cm);

                            ms.Position = 0;
                            xmlDocument.Load(ms);
                        }
                        string xmlData;
                        using (StringWriter stringWriter = new StringWriter())
                        {
                            xmlDocument.Save(stringWriter);
                            xmlData = stringWriter.ToString();
                        }
                        byte[] msg = Encoding.ASCII.GetBytes(xmlData);
                        int bytesSent = handler.Send(msg);
                        /*int bytesSent = await handler.SendAsync(msg, SocketFlags.None);*/
                        Console.WriteLine("Список легковых отправлен");
                    }
                    else if (cb.Type == "Грузовой")
                    {
                        XmlSerializer xmlSerializerTrucks = new XmlSerializer(typeof(List<TruckModel>));
                        XmlDocument xmlDocument = new XmlDocument();
                        List<TruckModel> cm = cars[cb].ConvertAll(new Converter<Car_Model, TruckModel>(Car_ModelToTruckModel));
                        using (MemoryStream ms = new MemoryStream())
                        {
                            xmlSerializerTrucks.Serialize(ms, cm);

                            ms.Position = 0;
                            xmlDocument.Load(ms);
                        }
                        string xmlData;
                        using (StringWriter stringWriter = new StringWriter())
                        {
                            xmlDocument.Save(stringWriter);
                            xmlData = stringWriter.ToString();
                        }
                        byte[] msg = Encoding.ASCII.GetBytes(xmlData);
                        int bytesSent = handler.Send(msg);
                        Console.WriteLine("Список грузовых отправлен");
                    }
                    else if (cb.Type == "Танк")
                    {
                        XmlSerializer xmlSerializerTanks = new XmlSerializer(typeof(List<TankModel>));
                        XmlDocument xmlDocument = new XmlDocument();
                        List<TankModel> cm = cars[cb].ConvertAll(new Converter<Car_Model, TankModel>(Car_ModelToTankModel));
                        using (MemoryStream ms = new MemoryStream())
                        {
                            xmlSerializerTanks.Serialize(ms, cm);

                            ms.Position = 0;
                            xmlDocument.Load(ms);
                        }
                        string xmlData;
                        using (StringWriter stringWriter = new StringWriter())
                        {
                            xmlDocument.Save(stringWriter);
                            xmlData = stringWriter.ToString();
                        }
                        byte[] msg = Encoding.ASCII.GetBytes(xmlData);
                        int bytesSent = handler.Send(msg);
                        Console.WriteLine("Список танков отправлен");
                    }
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    /*Socket handler = sListener.Accept();*/
                    /*XmlDocument listofbrands = new XmlDocument();
                    using (XmlWriter writer = listofbrands.CreateNavigator().AppendChild())
                    {
                        new XmlSerializer(cars.GetType()).Serialize(writer, cars);
                    }*/

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
