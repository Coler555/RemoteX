﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Xamarin.Forms;
using RemoteX.Sensor;
using RemoteXDataLibary;

namespace RemoteX.Controller
{
    public class SensorTestPage : ControllerPage
    {
        IConnectionManager _ConnectionManager;
        float[] latestAccelerometerData;
        float[] latestMagneticFieldData;
        public SensorTestPage() : base()
        {
            latestAccelerometerData = null;
            latestMagneticFieldData = null;

            ISensorManager sensorManager = DependencyService.Get<ISensorManager>();
            _ConnectionManager = DependencyService.Get<IConnectionManager>();
            ISensor gyroSensor = sensorManager[SensorType.Gyroscope];

            gyroSensor.Activate();
            gyroSensor.OnSensorDataUpdated += _SensorDataHandler;
            ISensor accelerometerSensor = sensorManager[SensorType.Accelerometer];
            accelerometerSensor.Activate();
            accelerometerSensor.OnSensorDataUpdated += _SensorDataHandler;
            ISensor magneticField = sensorManager[SensorType.MagneticField];
            magneticField.Activate();
            magneticField.OnSensorDataUpdated += _SensorDataHandler;


            ControllerContentView = new StackLayout
            {
                Children = {
                    new Label { Text = "Sensor Testttt" }
                }
            };
        }


        void _SensorDataHandler(ISensor sensor, float[] data)
        {
            IConnection connection = _ConnectionManager.ControllerConnection;
            if (sensor.SensorType == SensorType.Gyroscope)
            {
                
                if (connection != null)
                {
                    connection.SendAsync(Data.encodeSensorData(new Data((int)DataType.SensorGyroscope, data)));
                }
            }
            if (sensor.SensorType == SensorType.Accelerometer || sensor.SensorType == SensorType.MagneticField)
            {
                if (sensor.SensorType == SensorType.Accelerometer)
                {
                    if(latestAccelerometerData ==null)
                    {
                        latestAccelerometerData = new float[3];
                    }
                    latestAccelerometerData[0] = data[0];
                    latestAccelerometerData[1] = data[1];
                    latestAccelerometerData[2] = data[2];
                    
                }
                else if (sensor.SensorType == SensorType.MagneticField)
                {
                    if (latestMagneticFieldData == null)
                    {
                        latestMagneticFieldData = new float[3];
                    }
                    latestMagneticFieldData[0] = data[0];
                    latestMagneticFieldData[1] = data[1];
                    latestMagneticFieldData[2] = data[2];
                }
                if (latestAccelerometerData != null && latestMagneticFieldData != null)
                {
                    float[] orientation = DependencyService.Get<ISensorManager>().GetOrientation(latestAccelerometerData, latestMagneticFieldData);
                    if(connection != null)
                    {
                        connection.SendAsync(Data.encodeSensorData(new Data((int)DataType.OrientationAngle, orientation)));
                    }
                }
            }

        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DependencyService.Get<ISensorManager>()[SensorType.Gyroscope].OnSensorDataUpdated -= _SensorDataHandler;
        }
        private void printArray(float[] array)
        {
            string s = "";
            for(int i = 0;i<array.Length;i++)
            {
                s += array[i];
                if(i == array.Length-1)
                {
                    
                }
                else
                {
                    s += ", ";
                }
            }
            Debug.WriteLine(s);
        }

    }
}