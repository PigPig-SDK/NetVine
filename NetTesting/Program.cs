///
/// This file is used to test networking accuracy
///

using Infrastructure.Networking;
using System.Net;

Client? client = null;
Host? host = null;

void PrintHelp()
{ 
    Console.WriteLine("Type 'connect <Ip> <port>' to connect");
    Console.WriteLine("Type 'host <port>' to host");
    Console.WriteLine("Type 'myip' for your local addresses");
    Console.WriteLine("Type 'probe' to probe your connection axis");
    Console.WriteLine("Type 'clear' clear screen");
    Console.WriteLine("Type 'send <byte count>' to send garbage of that size");
}

PrintHelp();



while (true)
{
    string line = Console.ReadLine();
    string[] split = line.Split(' ');

    switch (split[0])
    {
        case "host":
            {
                if (host != null) return;

                if (split.Length != 2)
                {
                    Console.WriteLine("Invalid input, try again");
                    continue;
                }
                if ( !int.TryParse(split[1], out int port))
                {
                    Console.WriteLine($"Invalid port address: {split[1]}");
                    continue;
                }

                var ip = IPAddress.Any;
                Console.WriteLine($"Attempting host under: {ip} : {port}");
                host = new(ip, port);
                host.Start();
                break;
            }
        case "connect":
            {
                if (client != null) return;

                if (split.Length != 3)
                {
                    Console.WriteLine("Invalid input, try again");
                    continue;
                }
                IPAddress.TryParse(split[1], out IPAddress? ip);
                if (ip == null || !int.TryParse(split[2], out int port))
                {
                    Console.WriteLine($"Invalid IP address: {split[1]}");
                    continue;
                }

                Console.WriteLine($"Attempting connect to: {ip} {port}");
                client = new(ip, port);
                client.ConnectAsync();
                break;
            }
        case "disconnect":
            {
                
                if (client != null)
                {
                    Console.WriteLine("Disconnecting client");
                    client.DisconnectShutdown();
                    client.Dispose();
                    client = null;
                }
                if (host != null)
                {
                    Console.WriteLine("Disconnecting host");
                    host.DisconnectAll();
                    host.Stop();
                    host.Dispose();
                    host = null;
                }
                break;
            }
        case "probe":
            {
                if (client != null)
                {
                    Console.WriteLine($"Client says:" +
                        $"\n{nameof(client.IsConnected)} : {client.IsConnected}" +
                        $"\n{nameof(client.IsConnecting)} : {client.IsConnecting}");
                    
                }
                if (host != null)
                {
                    Console.WriteLine($"Host says:" +
                    $"\n{nameof(host.IsStarted)} : {host.IsStarted}" +
                    $"\n{nameof(host.IsSocketDisposed)} : {host.IsSocketDisposed}");
                }
                if (host == null && client == null)
                {
                    Console.WriteLine("Nothing to probe");
                }
                break;
            }
        case "send":
            {
                if (split.Length != 2)
                {
                    Console.WriteLine("Please supply an ammount of bytes to send.");
                    continue;
                }
                if(!int.TryParse(split[1], out int result))
                {
                    Console.WriteLine($"{split[1]} is not a valid number.");
                    continue;
                }

                byte[] message = new byte[result];//Allocate a ton of useless shit.
                for(int i = 0; i <  message.Length; i++)
                {
                    message[i] = (byte)(i % Byte.MaxValue);
                }
                Console.WriteLine($"Created package of {result} bytes");
                if (host != null)
                {
                    host.Multicast(message);
                    continue;
                }
                if(client != null)
                {
                    client.Send(message);
                }

                if(client == null && host == null)
                {
                    Console.WriteLine("Cant send! Not host or client.");
                }

                break;
            }
        case "myip":
            {
                ///https://www.geeksforgeeks.org/c-sharp/c-sharp-program-to-find-the-ip-address-of-the-machine/
                // Get the Name of HOST  
                string hostName = Dns.GetHostName();
                Console.WriteLine(hostName);
                // Get the IP from GetHostByName method of dns class.
                foreach(var ip in Dns.GetHostEntry(hostName).AddressList)
                {
                    Console.WriteLine($"Your IP: {ip.ToString()}");
                }
                
                break;
            }
        case "clear":
            {
                Console.Clear();
                PrintHelp();
                break;
            }
        default:
            {
                Console.WriteLine($"I don't understand {line}");
                break;
            }
    }
}
    