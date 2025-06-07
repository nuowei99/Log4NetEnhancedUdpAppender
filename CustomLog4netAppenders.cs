using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using log4net.Appender;
using log4net.Core;
using log4net.Util;

namespace CustomLog4netAppenders
{
    public class EnhancedUdpAppender : UdpAppender
    {
        // 自定义协议版本号
        private const byte PROTOCOL_VERSION = 0x01;
        
        // 配置属性：本地IP（自动获取）
        public string LocalIP { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                // 1. 构建自定义协议数据包
                byte[] packet = BuildCustomPacket(loggingEvent);
                // 2. 通过UDP发送数据
                base.SendPacket(packet);
            }
            catch (Exception ex)
            {
                ErrorHandler.Error("UDP发送失败: " + ex.Message);
            }
        }

        private byte[] BuildCustomPacket(LoggingEvent loggingEvent)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                // 协议头
                ms.WriteByte(PROTOCOL_VERSION);   // 版本号
                ms.WriteByte(0x00);               // 保留位

                // 时间戳（Unix时间戳，8字节）
                long timestamp = (long)(loggingEvent.TimeStamp - new DateTime(1970, 1, 1)).TotalMilliseconds;
                byte[] timeBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(timestamp));
                ms.Write(timeBytes, 0, 8);

                // 日志等级（1字节）
                ms.WriteByte((byte)loggingEvent.Level.Value);

                // 事件ID（4字节）
                int eventId = Convert.ToInt32(loggingEvent.Properties["EventID"] ?? 0);
                byte[] eventIdBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(eventId));
                ms.Write(eventIdBytes, 0, 4);

                // 本地IP（4字节）
                IPAddress ip = GetLocalIP();
                ms.Write(ip.GetAddressBytes(), 0, 4);

                // 程序集名称
                string assemblyName = loggingEvent.LocationInformation?.AssemblyName ?? "";
                byte[] assemblyBytes = Encoding.UTF8.GetBytes(assemblyName);
                ms.WriteByte((byte)assemblyBytes.Length);  // 长度前缀
                ms.Write(assemblyBytes, 0, assemblyBytes.Length);

                // 日志消息
                string message = loggingEvent.RenderedMessage;
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                byte[] msgLen = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(msgBytes.Length));
                ms.Write(msgLen, 0, 4);  // 4字节长度前缀
                ms.Write(msgBytes, 0, msgBytes.Length);

                return ms.ToArray();
            }
        }

        private IPAddress GetLocalIP()
        {
            // 优先使用配置的IP
            if (!string.IsNullOrEmpty(LocalIP) && IPAddress.TryParse(LocalIP, out var configIp))
                return configIp;

            // 自动获取IPv4地址
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip;
                }
            }
            catch { /* 忽略错误 */ }

            return IPAddress.Loopback; // 默认返回回环地址
        }
    }
}