@"
# Enhanced UDP Appender for log4net

A custom log4net UDP appender that extends the standard UdpAppender with enhanced protocol support and automatic IP detection capabilities.

## Features

- Custom binary protocol with version control (0x01)
- Automatic local IP detection with fallback to loopback
- Configurable local IP address override
- Rich metadata support in UDP packets
- UTF-8 encoding for text data

## Installation

Add the assembly reference to your project and configure log4net:

\`\`\`xml
<log4net>
  <appender name="UdpLogger" type="CustomLog4netAppenders.EnhancedUdpAppender">
    <remoteAddress value="192.168.1.100" />
    <remotePort value="514" />
    <localIP value="192.168.1.50" />  <!-- Optional -->
  </appender>

  <root>
    <level value="ALL" />
    <appender-ref ref="UdpLogger" />
  </root>
</log4net>
\`\`\`

## Protocol Specification

UDP packet structure:

| Field | Size (bytes) | Description |
|-------|-------------|-------------|
| Version | 1 | Protocol version (0x01) |
| Reserved | 1 | Reserved for future use |
| Timestamp | 8 | Unix timestamp (ms, network byte order) |
| Log Level | 1 | log4net level value |
| Event ID | 4 | Event identifier (network byte order) |
| IP Address | 4 | Source IP address (IPv4) |
| Assembly Name Length | 1 | Length of assembly name |
| Assembly Name | variable | UTF-8 encoded assembly name |
| Message Length | 4 | Message length (network byte order) |
| Message | variable | UTF-8 encoded log message |

## Usage Example

\`\`\`csharp
using log4net;

ILog logger = LogManager.GetLogger(typeof(Program));

// Basic logging
logger.Info("Application started");

// Logging with Event ID
log4net.ThreadContext.Properties["EventID"] = 1001;
logger.Error("System error occurred");
\`\`\`

## Requirements

- .NET Framework 4.5+
- log4net 2.0.0+

## License

This project is licensed under the MIT License.
"@ | Out-File -FilePath "README.md" -Encoding UTF8