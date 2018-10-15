using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace SlittersWPF
{

  /*Socket: This operation is used to create a new socket end point, allocating operating system resources that can then be used to execute an incoming and outgoing communication.

    Bind: Servers must bind a socket to an address to establish a local name. A bind enables external communication to connect to and send messages through new end points, enable the socket to read messages off of it and send its own.

    Listen: The socket shows its interest in message queues participation by notifying the OS by executing an optional listen operation, where message queues consume incoming requests for further processing.

    Accept: A socket accepts an incoming request by leaving a new socket, that can be used to read and write to the client.

    Connect: Used to connect to a server-side network end point. Once the target end point accepts the request, the socket can be used to read from and write data to the socket.

    Send: once a socket is connected to an end point, sending data initiates a message that the other point is then able to receive.  non Blocking method

    Receive: after the end point to which a socket is connected sends data, a socket may consume that data by receiving it.  non Blocking method

    Close: when a connection has finished its session, a socket must be closed that releases all the resources held by the socket. */
    class TCPServSocket
    {
        public static bool SortRequest = false;
        public static String wraptext = "";
        public IPAddress LocalIPAddr;
        public static String CmptrIPAddr = "";
        
        //Constructor
        public TCPServSocket()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                LocalIPAddr = null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            LocalIPAddr = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            CmptrIPAddr = LocalIPAddr.ToString();
        }   

        public static void StartListening()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];
            IPAddress ipAddress = IPAddress.Parse(CmptrIPAddr); 
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 1001);
            
            // Create a TCP/IP socket. IPV4 Stream TCP  
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        
            try
            {
                // Bind the socket to the local endpoint and listen for incoming connections. 
                listener.Bind(localEndPoint);
                listener.Listen(100);

                // Start an asynchronous socket to listen for connections.  
                    
                listener.BeginAccept(new AsyncCallback(AcceptCallback),listener);
                //MessageBox.Show("Listening");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            // Get the socket that handles the client request
            Socket handler = listener.EndAccept(ar);


            // Create the state object.  
            StateObject state = new StateObject();
            {

            }
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            
            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            String contentret = String.Empty;
            contentret = "@0002#;";


            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read   
                // more data.  
                content = state.sb.ToString();

                if (content.IndexOf("RE1") > -1)
                {
                    // All the data has been read from the   
                    // client. Display it on the console.
                    // wraptext goes to SortData object
                    wraptext = content;
                    //MessageBox.Show("Read  bytes from socket. \n Data : " + content.Length + content);
                    // Echo the data back to the client.  
                    Send(handler, contentret);
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
            SortRequest = true;
            

        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                //MessageBox.Show("Sent bytes to client." + bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        //Deconstructor
        ~TCPServSocket()
        {
            
        }
    }
}
