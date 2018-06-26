﻿using System;
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

using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Diagnostics;
using System.Threading;

namespace Bluetooth_Mouse_Controller_Receiver
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        
        ControllerManager controllerManager;
        private Dictionary<Guid, ControllerManager> controllerManagers;
        public MainWindow()
        {
            controllerManagers = new Dictionary<Guid, ControllerManager>();
            InitializeComponent();
        }

        //这个站且用来测试鼠标移动，以下就是他妈的移动方法。
        private void Button_BluetoothInitialize_Click(object sender, RoutedEventArgs e)
        { 
            //Initialize();
            BTTaskManager btTaskManager = BTTaskManager.instance;
            BTTask btTask = btTaskManager.newTask();
            btTask.onReceiveMessage += onReceiveData;
            Debug.WriteLine("UI::"+Thread.CurrentThread.ManagedThreadId);
            btTask.startAdvertising();
            ControllerManager controllerManager = new ControllerManager();
            controllerManagers.Add(btTask.taskId, controllerManager);
            
            //btTask.onReceiveMessage += Receive();
        }
        private void onReceiveData(BTTask btTask, byte[] message)
        {
            RemoteXDataLibary.Data[] datas = null;
            try
            {
                datas = RemoteXDataLibary.Data.fromBytes(message);
            }
            catch(Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("XJ2::" + exception.Message);
            }
            ControllerManager controllerManager = controllerManagers[btTask.taskId];
            if (datas != null)
            {
                foreach (var data in datas)
                {
                    controllerManager.addData(data);
                    Debug.WriteLine(data);
                }
            }
        }
        private void Button_MoveCursor_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Point point = new System.Drawing.Point();
            point.X = Convert.ToInt32(TextBox_SetMousePositionX.Text);
            point.Y = Convert.ToInt32(TextBox_SetMousePositionY.Text);
            MoveCursor moveCursor = new MoveCursor();
            moveCursor.MoveTo(point);
        }

        private void tbMouseRateX_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach(var pair in controllerManagers)
            {
                var controllerManager = pair.Value;
                float tryRate;
                if (float.TryParse(tbMouseRateX.Text, out tryRate))
                {
                    controllerManager.gyroscopeMouseManager.mouseSpeedFactor.x = tryRate;
                }
                
            }
        }

        private void tbMouseRateY_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (var pair in controllerManagers)
            {
                var controllerManager = pair.Value;
                float tryRate;
                if (float.TryParse(tbMouseRateY.Text, out tryRate))
                {
                    controllerManager.gyroscopeMouseManager.mouseSpeedFactor.y = tryRate;
                }

            }
        }
    }
}
