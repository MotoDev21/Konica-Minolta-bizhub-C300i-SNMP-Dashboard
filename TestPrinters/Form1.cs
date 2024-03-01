using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.Net.NetworkInformation;
using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using System.IO;
using Newtonsoft.Json.Bson;
using Guna.Charts.WinForms;
using Guna.Charts.Interfaces;

namespace TestPrinters
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Konica_Minolta_bizhub_C300i();
        } 

        void GraphsWrite(GunaLineDataset DS, string date, int value)
        {
            
            DS.DataPoints.Add(date, value);

        }

        void JsonWrite(string time, string value, string filePathData)
        {
            // Создаем объект для записи в JSON
            var data = new
            {
                Time = time,
                Value = value
            };

            // Преобразуем объект в JSON
            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);

            // Указываем путь к файлу JSON
            string filePath = filePathData;

            try
            {
                // Проверяем, существует ли файл и не пустой ли он
                if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
                {
                    // Если файла нет или он пустой, создаем его и добавляем начальный символ [
                    File.WriteAllText(filePath, "[" + Environment.NewLine + jsonData);
                }
                else
                {
                    // Добавляем запятую в конец файла перед добавлением нового объекта
                    var allText = File.ReadAllText(filePath);
                    if (!allText.EndsWith("[" + Environment.NewLine))
                    {
                        // Удаляем закрывающую скобку ]
                        allText = allText.TrimEnd(']', '\n', '\r') + "," + Environment.NewLine;
                    }

                    // Добавляем новый объект JSON
                    allText += jsonData;

                    // Перезаписываем файл с новым содержимым
                    File.WriteAllText(filePath, allText);
                }

                // Добавляем закрывающую скобку ] в конец файла
                File.AppendAllText(filePath, Environment.NewLine + "]");

                Console.WriteLine("Данные успешно дописаны в файл JSON.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи в файл: {ex.Message}");
            }
        }

        class Person
        {
            public string Time { get; set; }
            public string Value { get; set; }
        }

        void JsonRead(GunaLineDataset DS, string filePathData)
        {
            // Указываем путь к файлу JSON
            string filePath = filePathData;

            try
            {
                // Считываем содержимое файла JSON
                string jsonData = File.ReadAllText(filePath);

                // Десериализуем JSON в список объектов типа Person
                var people = JsonConvert.DeserializeObject<List<Person>>(jsonData);

                // Выводим данные на экран
                Console.WriteLine("Данные из файла JSON:");
                foreach (var person in people)
                {
                    GraphsWrite(DS, person.Time, Convert.ToInt32(person.Value));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файла: {ex.Message}");
            }
        }

        bool IpCheckPing(string ip)
        {
            string ipAddress = ip; // Замените на нужный IP-адрес

            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(ipAddress);

            if (reply.Status == IPStatus.Success)
            {
                return true;
            }
            else
            {
                return false;      
            }
        }

        void Konica_Minolta_bizhub_C300i()
        {
            if (IpCheckPing("10.0.0.102")) //IP вашего принтера
            {
                guna2Shapes1.FillColor = Color.Green;
                guna2Shapes1.BorderColor = Color.Green;

                guna2HtmlLabel3.Text = SNMP_Data("1.3.6.1.4.1.18334.1.1.1.5.7.2.1.1.0"); //Всего страниц
                guna2HtmlLabel14.Text = SNMP_Data("1.3.6.1.4.1.18334.1.1.1.5.7.2.1.3.0");//Всего, двусторонняя печать

                double poctar = Convert.ToDouble(SNMP_Data("1.3.6.1.4.1.18334.1.1.1.5.7.2.1.9.0")) / 100;
                double poctar_result= Convert.ToDouble(SNMP_Data("1.3.6.1.4.1.18334.1.1.1.5.7.2.1.3.0")) / poctar;
                guna2HtmlLabel15.Text = poctar_result.ToString() + " %"; //Тариф на двустороннюю печать


                guna2HtmlLabel16.Text = SNMP_Data("1.3.6.1.4.1.18334.1.1.1.5.7.2.1.8.0");//Количество оригиналов
                guna2HtmlLabel17.Text = SNMP_Data("1.3.6.1.4.1.18334.1.1.1.5.7.2.1.9.0");//Счетчик бумаги
                guna2HtmlLabel18.Text = SNMP_Data("1.3.6.1.4.1.18334.1.1.1.5.7.2.1.10.0");//Кол-во всех отпечатанных листов

                guna2RadialGauge3.Value = Convert.ToInt32(SNMP_Data("1.3.6.1.2.1.43.11.1.1.9.1.1"));//Голубой граффик %
                guna2HtmlLabel6.Text = SNMP_Data("1.3.6.1.2.1.43.11.1.1.9.1.1") + "%"; //Голубой %
                JsonWrite(DateTime.Now.ToString(), SNMP_Data("1.3.6.1.2.1.43.11.1.1.9.1.1"), "data/Konica_Minolta_bizhub_C300i/Cyan.json");//Голубой  в json
                JsonRead(gunaLineDataset3, "data/Konica_Minolta_bizhub_C300i/Cyan.json");

                guna2RadialGauge2.Value = Convert.ToInt32(SNMP_Data("1.3.6.1.2.1.43.11.1.1.9.1.2"));//Фиолетовый граффик %
                guna2HtmlLabel5.Text = SNMP_Data("1.3.6.1.2.1.43.11.1.1.9.1.2") + "%"; //Фиолетовый %
                JsonWrite(DateTime.Now.ToString(), SNMP_Data("1.3.6.1.2.1.43.11.1.1.9.1.2"), "data/Konica_Minolta_bizhub_C300i/Magenta.json");//Фиолетовый в json
                JsonRead(gunaLineDataset2, "data/Konica_Minolta_bizhub_C300i/Magenta.json");

                guna2RadialGauge4.Value = Convert.ToInt32(SNMP_Data("1.3.6.1.2.1.43.11.1.1.9.1.4"));//Черный граффик %
                guna2HtmlLabel7.Text = SNMP_Data("1.3.6.1.2.1.43.11.1.1.9.1.4") + "%"; //Черный %
                JsonWrite(DateTime.Now.ToString(), SNMP_Data("1.3.6.1.2.1.43.11.1.1.9.1.4"), "data/Konica_Minolta_bizhub_C300i/Black.json");//Черный  в json
                JsonRead(gunaLineDataset4, "data/Konica_Minolta_bizhub_C300i/Black.json");

                guna2RadialGauge1.Value = Convert.ToInt32(SNMP_Data("1.3.6.1.2.1.43.11.1.1.9.1.3"));//Желтый граффик %
                guna2HtmlLabel4.Text = SNMP_Data("1.3.6.1.2.1.43.11.1.1.9.1.3") + "%"; //Желтый %
                JsonWrite(DateTime.Now.ToString(), SNMP_Data("1.3.6.1.2.1.43.11.1.1.9.1.3"), "data/Konica_Minolta_bizhub_C300i/Yellow.json");//Желтый в json
                JsonRead(gunaLineDataset1, "data/Konica_Minolta_bizhub_C300i/Yellow.json");
            }
            else
            {
                MessageBox.Show("Принтер Konica Minolta bizhub C300i не доступен по IP: 10.0.0.102");
            }
           
        }

        string SNMP_Data(string OID)
        {
            // IP адрес или имя хоста устройства SNMP
            var host = "10.0.0.102";

            // SNMP community string
            var community = "public";

            // OID (Object Identifier), который вы хотите запросить, например sysDescr
            var oid = new ObjectIdentifier(OID);

            try
            {
                // SNMP запрос
                var result = Messenger.Get(VersionCode.V2,
                                            new IPEndPoint(IPAddress.Parse(host), 161),
                                            new OctetString(community),
                                            new List<Variable> { new Variable(oid) },
                                            10000);

                string resultSNMP = string.Empty; // Инициализация переменной вне цикла

                foreach (var variable in result)
                {
                    // Здесь может быть логика обработки
                    resultSNMP = variable.Data.ToString();

                    // Если нужно вернуть значение сразу при его нахождении, раскомментируйте следующую строку:
                    // return resultSNMP;
                }

                // Возвращаем результат после выполнения цикла
                return resultSNMP;


            }
            catch (Exception ex)
            {
                string error = ($"Ошибка при выполнении SNMP запроса: {ex.Message}");
                return error;
            }

        }

        private void gunaChart1_Load(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            Application.Restart();
            Environment.Exit(0); ;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            //zabbix link
        }
    }
}
