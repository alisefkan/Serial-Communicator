using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Threading;
using System.Globalization;
using System.Data;
using System.ComponentModel;
using System.Windows.Forms;

namespace serialcomfinal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int MaxNumberOfItemsAllowed = 1000;
        SerialPort mainport = new SerialPort();


        public MainWindow()
        {
            InitializeComponent();
            GetSerialDevices();
            mainport.DataReceived += Mainport_DataReceived;

        }

        private void GetSerialDevices()
        {
            foreach (string deviceport in SerialPort.GetPortNames())
            {
                serialports.Items.Add(deviceport);
            }
        }

        private void WriteReceived(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                received.Items.Add(text);
                if (received.Items.Count >= MaxNumberOfItemsAllowed)
                {
                    received.Items.Clear();
                }
            }));
        }
        private void Mainport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (mainport.IsOpen)
            {
                try
                {
                    if (mainport.BytesToRead >= 0)
                    {

                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = false;
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
                            {

                                int dataLength = mainport.BytesToRead;
                                byte[] data = new byte[dataLength];
                                mainport.Read(data, 0, dataLength);

                                string cikis = System.Text.Encoding.UTF8.GetString(data);
                                Thread.Sleep(100);
                                received.Items.Add("RX >" + cikis);




                            }, null);
                        }).Start();
                    }
                    Thread.Sleep(100);
                }
                catch (Exception g)
                {
                    System.Windows.MessageBox.Show(g.Message, "Seri portta Main'de hata var");
                }
            }
        }

        private void WriteSent(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                send.Items.Add(text);
                if (send.Items.Count >= MaxNumberOfItemsAllowed)
                {
                    send.Items.Clear();
                }
            }), DispatcherPriority.ContextIdle);
        }

        string textadres;
        private void dosyasec_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog openfile = new OpenFileDialog() { Filter = "Text Dosyaları|*txt", Title = "Text Dosyaları" })
            {
                if (openfile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textadres = openfile.FileName;
                    sendtextbox.Text = openfile.FileName;
                }

            }
        }
        string line = "";
        string line2 = "";

        private void sendmessage_Click(object sender, RoutedEventArgs e)
        {
            if (mainport.IsOpen)
            {
                if (!string.IsNullOrEmpty(sendtextbox.Text))
                {
                    try
                    {

                        StreamReader sr = new StreamReader(textadres);
                        StreamReader sr2 = new StreamReader(textadres);
                        int linenumber = 0;
                        while (line2 != null)
                        {
                            line2 = sr2.ReadLine();
                            linenumber++;
                        }
                        int i = 0;
                        string[] towrite = new string[linenumber - 1];
                        while (line != null)
                        {
                            line = sr.ReadLine();
                            if (line != null)
                            {
                                towrite[i] = line;
                                i++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        for (int j = 0; j < i; j++)
                        {
                            mainport.Write(towrite[j]);
                            Thread.Sleep(100);
                            send.Items.Add("TX > " + towrite[j]);
                        }


                    }
                    catch (Exception g)
                    {
                        System.Windows.MessageBox.Show(g.Message, "Seri porttan alınan veride hata var send");
                    }
                }


            }
        }

        private void sendtextbox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void connectbutton_Click(object sender, RoutedEventArgs e)
        {

            if (!mainport.IsOpen)
            {
                try
                {
                    mainport.BaudRate = 921600;
                    mainport.DataBits = 8;
                    mainport.Handshake = Handshake.None;
                    mainport.Parity = Parity.None;
                    mainport.StopBits = StopBits.One;
                }
                catch (Exception ggg)
                {
                    System.Windows.MessageBox.Show(ggg.Message, "Seri port değerlerinde hata var");
                    return;
                }
                try
                {
                    if (serialports.SelectedItem != null)
                    {
                        mainport.PortName = serialports.SelectedItem.ToString();
                    }
                }
                catch (Exception ggg)
                {
                    System.Windows.MessageBox.Show(ggg.Message, "Port adında hata var");
                    return;
                }

                try
                {
                    if (!mainport.IsOpen)
                    {
                        try
                        {
                            mainport.Open();
                            connectbutton.Content = "Disconnect";
                        }
                        catch (Exception dfjfd)
                        {
                            System.Windows.MessageBox.Show(dfjfd.Message); return;
                        }
                    }
                    else
                    {
                        try
                        {
                            mainport.Close();
                            connectbutton.Content = "Connect";
                        }
                        catch (Exception haric)
                        {
                            System.Windows.MessageBox.Show(haric.Message);
                        }
                    }
                }
                catch (Exception gg)
                {
                    System.Windows.MessageBox.Show(gg.Message);
                }
            }
            else
            {
                try
                {
                    mainport.Close();
                    connectbutton.Content = "Connect";
                }
                catch (Exception harici)
                {
                    System.Windows.MessageBox.Show(harici.Message);
                }
            }
        }

        private void received_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainport.IsOpen)
            {

                try
                {
                    int dataLength = mainport.BytesToRead;
                    byte[] data = new byte[dataLength];
                    int nbrDataRead = mainport.Read(data, 0, dataLength);

                    if (mainport.BytesToRead >= 0)
                    {

                        //received.Items.Add(mainport.BytesToRead.ToString(sendtextbox.Text));
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Sıkıntı var");
                    }

                    Thread.Sleep(100);
                }
                catch (Exception g)
                {
                    System.Windows.MessageBox.Show(g.Message, "Seri porttan alınan veride hata var (received)");
                }
            }


        }
        //Gelen verileri temizlemek amacıyla yazılmıştır.
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            received.Items.Clear();
            send.Items.Clear();

        }

        private void send_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}

