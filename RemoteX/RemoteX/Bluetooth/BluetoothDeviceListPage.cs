﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Xamarin.Forms;
using RemoteX.Data;
using RemoteX.Core;

namespace RemoteX.Bluetooth
{
    public class BluetoothDeviceListPage : ContentPage
    {
        IBluetoothManager bluetoothManager;
        ObservableCollection<IBluetoothDevice> bluetoothDeviceList;
        Button scanDevicesButton;
        Button qrCodeConnectButton;
        public BluetoothDeviceListPage()
        {

            bluetoothManager = DependencyService.Get<IManagerManager>().BluetoothManager;
            bluetoothDeviceList = new ObservableCollection<IBluetoothDevice>();
            resetDeviceList();
            ListView bluetoothDeviceListView = new ListView();
            bluetoothDeviceListView.ItemSelected += onDeviceSelected;
            bluetoothDeviceListView.ItemsSource = bluetoothDeviceList;
            bluetoothDeviceListView.ItemTemplate = new DataTemplate(typeof(BluetoothDeviceCell));

            qrCodeConnectButton = new Button();
            qrCodeConnectButton.Text = "QRCode";
            qrCodeConnectButton.Clicked += onQRCodeConnectClicked;

            scanDevicesButton = new Button();
            scanDevicesButton.Clicked += (object sender, EventArgs e) =>
            {
                resetDeviceList();
                bluetoothManager.SearchForBlutoothDevices();

            };
            bluetoothManager.OnDiscoveryStarted += onDiscoveryStarted;
            bluetoothManager.OnDevicesFound += onDevicesFound;
            bluetoothManager.OnDiscoveryFinished += onDiscoveryFinished;
            scanDevicesButton.Text = "Scan Devices";
            Content = new StackLayout
            {
                Children = {
                    qrCodeConnectButton,
                    scanDevicesButton,
                    bluetoothDeviceListView
                }
            };

        }
        private void onDevicesFound(IBluetoothManager IBluetoothManager, IBluetoothDevice[] devices)
        {
            foreach (IBluetoothDevice device in devices)
            {
                bool isFounded = false;
                foreach (IBluetoothDevice foundedDevice in this.bluetoothDeviceList)
                {
                    if (foundedDevice.Address == device.Address)
                    {
                        isFounded = true;
                        break;
                    }
                }
                if (isFounded)
                {
                    break;
                }
                this.bluetoothDeviceList.Add(device);
            }
        }
        private void resetDeviceList()
        {
            bluetoothDeviceList.Clear();
            IBluetoothDevice[] pairedDevices = bluetoothManager.PairedDevices;
            foreach (IBluetoothDevice device in pairedDevices)
            {
                bluetoothDeviceList.Add(device);
            }
        }
        private void onDiscoveryStarted(IBluetoothManager bluetoothManager) { scanDevicesButton.IsEnabled = false; scanDevicesButton.Text = "Discovering"; }
        private void onDiscoveryFinished(IBluetoothManager bluetoothManager)
        {
            scanDevicesButton.IsEnabled = true;
            scanDevicesButton.Text = "Scan Devices";
        }
        private async void onDeviceSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }
            IBluetoothDevice device = e.SelectedItem as IBluetoothDevice;
            await Navigation.PushAsync(new BluetoothDeviceInfoPage(device));
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            bluetoothManager.OnDiscoveryStarted -= onDiscoveryStarted;
            bluetoothManager.OnDevicesFound -= onDevicesFound;
            bluetoothManager.OnDiscoveryFinished -= onDiscoveryFinished;
        }
        private async void onQRCodeConnectClicked(object sender, EventArgs e)
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
            var scanResult = await scanner.Scan();
            if(scanResult != null && scanResult.Text!="")
            {
                var connectInfo = Connection.DecodeBluetoothConnection(scanResult.Text);
                Guid guid = connectInfo.Guid;
                IBluetoothDevice bluetoothDevice = DependencyService.Get<IManagerManager>().BluetoothManager.GetBluetoothDevice(connectInfo.DeviceAddress);


                IConnectionManager connectionManager = DependencyService.Get<IConnectionManager>();
                IClientConnection currentConnection = connectionManager.ControllerConnection as IClientConnection;
                ConnectionEstablishState connectState = ConnectionEstablishState.Failed;
                IClientConnection connection = DependencyService.Get<IManagerManager>().BluetoothManager.CreateRfcommClientConnection(bluetoothDevice, guid);
                if (connectionManager.ControllerConnection != null)
                {
                    bool result = await DisplayAlert("Connect", "Stop Current Connection ?", "Yes", "No");
                    if (result)
                    {
                        if (currentConnection.ConnectionEstablishState == ConnectionEstablishState.Connecting)
                        {
                            currentConnection.AbortConnecting();
                        }
                    }
                }
                connectionManager.ControllerConnection = connection;
                connectState = await connection.ConnectAsync();

            }
        }




    }
}