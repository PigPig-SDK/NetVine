
using Core;
using Infrastructure;
using Infrastructure.Networking.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UI.ViewModels
{
    public class MockNetUser
    {
        private readonly List<MockDevice> _devices = new();

        public MockNetUser(params string[] deviceNames)
        {
            foreach (var name in deviceNames)
                _devices.Add(new MockDevice(name));
        }

        public async Task Start()
        {
            foreach (var device in _devices)
                await device.Start();
        }

        public void Stop()
        {
            foreach (var device in _devices)
                device.Stop();
        }

        private class MockDevice
        {
            private Timer? _timer;
            private double _mockTime;
            private readonly string _deviceName;
            private readonly Random _random = new();

            private readonly double _phaseOffset;

            public MockDevice(string deviceName)
            {
                _deviceName = deviceName;
                _phaseOffset = new Random(deviceName.GetHashCode()).NextDouble() * Math.PI * 2;
            }

            public async Task Start()
            {
                InjectData();
                await AddMockUser();
                _timer = new Timer(_ => InjectData(),
                    null,
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1));
            }

            public void Stop()
            {
                _timer?.Dispose();
                _timer = null;
            }

            private async Task AddMockUser()
            {
                ConnectedUserInfo.AddUserData(Guid.NewGuid(), _deviceName);

                using var context = new DBInteract();
                context.AddUser(new User(_deviceName));
                await context.SaveChangesAsync();
            }

            private void InjectData()
            {
                _mockTime += 0.1;
                var random = new Random();

                var data = new List<ProgramData>
    {
        new ProgramData
        {
            SystemName   = _deviceName,
            ProcessName  = "MockChrome",
            CpuUsage     = (float)(20 + 15 * Math.Sin(_mockTime)),
            MemoryUsage  = (float)(800 + 200 * Math.Sin(_mockTime * 0.5)),
            DiskUsage    = (float)(Math.Max(0, 5 * Math.Sin(_mockTime * 2))),
            NetworkUsage = (float)(Math.Max(0, 3 * Math.Sin(_mockTime * 3))),
            Date         = DateTime.Now,  // required for DB primary key
            Timespan     = 1f
        },
        new ProgramData
        {
            SystemName   = _deviceName,
            ProcessName  = "MockDiscord",
            CpuUsage     = (float)(10 + 8 * Math.Cos(_mockTime * 1.3)),
            MemoryUsage  = (float)(400 + 100 * Math.Cos(_mockTime * 0.7)),
            DiskUsage    = (float)(Math.Max(0, 2 * Math.Cos(_mockTime * 1.5))),
            NetworkUsage = (float)(Math.Max(0, 5 * Math.Cos(_mockTime * 0.9))),
            Date         = DateTime.Now,
            Timespan     = 1f
        },
        new ProgramData
        {
            SystemName   = _deviceName,
            ProcessName  = "MockVSCode",
            CpuUsage     = (float)(5 + 20 * Math.Abs(Math.Sin(_mockTime * 0.3))),
            MemoryUsage  = (float)(600 + 50 * random.NextDouble()),
            DiskUsage    = (float)(Math.Max(0, 8 * Math.Abs(Math.Sin(_mockTime)))),
            NetworkUsage = 0,
            Date         = DateTime.Now,
            Timespan     = 1f
        },
        new ProgramData
        {
            SystemName   = _deviceName,
            ProcessName  = "MockSystem",
            CpuUsage     = (float)(3 + 2 * random.NextDouble()),
            MemoryUsage  = (float)(200 + 20 * random.NextDouble()),
            DiskUsage    = (float)(1 + random.NextDouble()),
            NetworkUsage = (float)(0.5 + random.NextDouble()),
            Date         = DateTime.Now,
            Timespan     = 1f
        },
    };


                Task.Run(() => DBInteract.Store(data, false));

                ResourceService.Instance.OnSnapshot(_deviceName, data.Cast<IProgramData>().ToList());
            }

        }
    }
}