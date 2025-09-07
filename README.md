# AspireDemoGrpc - .NET Aspire Microservices Demo

A comprehensive demo project demonstrating .NET Aspire with gRPC, MQTT, MongoDB, and Angular Frontend. This project showcases how modern microservices architectures can be orchestrated and managed with .NET Aspire.

## 🏗️ Architecture Overview

The project consists of several interconnected services:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Angular       │    │   WebApi        │    │   Backend       │
│   Frontend      │◄──►│   (Gateway)     │◄──►│   (gRPC)        │
│   (Port 38297)  │    │   (Port 7054)   │    │   (Port 5000)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   MQTT          │    │   MongoDB       │    │   PostgreSQL    │
│   Publisher     │    │   Service        │    │   Database      │
│   (Port 5001)   │    │   (Port 5003)   │    │   (Container)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   MQTT          │    │   MongoDB       │    │   Redis         │
│   Subscriber    │    │   Database      │    │   Cache         │
│   (Port 5002)   │    │   (Container)   │    │   (Container)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │
         ▼
┌─────────────────┐
│   MQTT Broker   │
│   (Mosquitto)   │
│   (Port 1883)   │
└─────────────────┘
```

## 🧩 Component Details

### Frontend (Angular)
- **Technology**: Angular 17+ with Standalone Components
- **Port**: 38297 (dynamically assigned)
- **Features**: 
  - gRPC Service Tests (Ping, Add Numbers, Database Query)
  - MQTT Messaging (Publish, Subscribe, Clear Messages)
  - REST API Integration

### WebApi (Gateway Service)
- **Technology**: ASP.NET Core Web API
- **Port**: 7054 (dynamically assigned)
- **Features**:
  - REST API Endpoints for Frontend
  - gRPC Client for Backend Communication
  - Calculator Service Implementation
  - PostgreSQL Integration
  - Redis Caching

### Backend (gRPC Service)
- **Technology**: ASP.NET Core gRPC Server
- **Port**: 5000 (dynamically assigned)
- **Features**:
  - Data Service (gRPC)
  - Test Data Provision (X, Y values)

### MQTT Services
- **Publisher**: Publishes messages to MQTT Broker
- **Subscriber**: Subscribes to messages from MQTT Broker
- **Broker**: Local Mosquitto MQTT Broker

### Databases
- **PostgreSQL**: Main database for persistent data
- **MongoDB**: NoSQL database for document storage
- **Redis**: In-Memory cache for performance optimization

## 🚀 Setup and Installation

### Prerequisites

Ensure the following software is installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

### Step 1: Clone Repository

```bash
git clone <repository-url>
cd AspireDemoGrpc
```

### Step 2: Restore .NET Dependencies

```bash
dotnet restore
```

### Step 3: Install Angular Dependencies

```bash
cd AngularFrontend
npm install
cd ..
```

### Step 4: Configure MQTT Broker

The project uses a local Mosquitto MQTT broker. Install and configure it:

```bash
# Ubuntu/Debian
sudo apt update
sudo apt install mosquitto mosquitto-clients

# macOS (with Homebrew)
brew install mosquitto

# Windows (with Chocolatey)
choco install mosquitto
```

**Mosquitto Configuration:**

Create or edit the configuration file `/etc/mosquitto/mosquitto.conf`:

```conf
# Mosquitto configuration for Aspire Demo
allow_anonymous true

# Listen on all interfaces
listener 1883

# Logging
log_dest file /var/log/mosquitto/mosquitto.log
log_type error
log_type warning
log_type notice
log_type information
```

**Start Mosquitto Service:**

```bash
# Linux/macOS
sudo systemctl start mosquitto
sudo systemctl enable mosquitto

# macOS (with Homebrew)
brew services start mosquitto

# Windows
net start mosquitto
```

### Step 5: Start Project

```bash
dotnet run --project AspireWalkthrough.AppHost
```

The Aspire Dashboard will be available at `https://localhost:17107`.

## 🔧 Development

### Project Structure

```
AspireDemoGrpc/
├── AngularFrontend/           # Angular Frontend
├── AspireWalkthrough.AppHost/ # Aspire Orchestrator
├── AspireWalkthrough.ServiceDefaults/ # Shared Service Configuration
├── Backend/                   # gRPC Backend Service
├── WebApi/                    # Web API Gateway
├── MqttPublisher/             # MQTT Publisher Service
├── MqttSubscriber/            # MQTT Subscriber Service
├── MongoService/              # MongoDB Service
├── ExternalGrpcClient/        # External gRPC Client
├── Shared/                    # Shared Protobuf Definitions
└── README.md
```

### Local Development

1. **Start All Services:**
   ```bash
   dotnet run --project AspireWalkthrough.AppHost
   ```

2. **Develop Individual Services:**
   ```bash
   # Backend Service
   dotnet run --project Backend
   
   # WebApi Service
   dotnet run --project WebApi
   
   # MQTT Publisher
   dotnet run --project MqttPublisher
   
   # MQTT Subscriber
   dotnet run --project MqttSubscriber
   
   # MongoDB Service
   dotnet run --project MongoService
   ```

3. **Frontend Development:**
   ```bash
   cd AngularFrontend
   npm start
   ```

### Debugging

- Use the **Aspire Dashboard** (`https://localhost:17107`) for service monitoring
- **Structured Logs** show detailed logs of all services
- **Resources** shows the status of all services and containers
- **Traces** shows distributed tracing between services

## 🧪 Testing

### Test gRPC Services

1. Open the frontend
2. Click **"Ping"** → Tests the connection
3. Click **"Add Numbers"** → Tests gRPC communication
4. Click **"Database Query"** → Tests database access

### Test MQTT Messaging

1. Click **"Publish Message"** → Enter a message
2. Click **"Get Messages"** → Shows received messages
3. Click **"Clear Messages"** → Clears all messages

### Test API Endpoints

```bash
# WebApi Endpoints
curl https://localhost:7054/ping
curl https://localhost:7054/add
curl https://localhost:7054/answer-from-db

# MQTT Publisher
curl http://localhost:5001/publish/test-message

# MQTT Subscriber
curl http://localhost:5002/messages
curl http://localhost:5002/clear

# MongoDB Service
curl http://localhost:5003/users
```

## 🔍 Troubleshooting

### Common Issues

1. **MQTT Broker won't start:**
   ```bash
   sudo systemctl status mosquitto
   sudo journalctl -xeu mosquitto.service
   ```

2. **Services can't connect:**
   - Check if all services are running: `ps aux | grep dotnet`
   - Check ports: `ss -tlnp | grep -E "(5000|5001|5002|5003|7054)"`

3. **Frontend can't reach backend:**
   - Check the URL in `AngularFrontend/src/app/app.component.ts`
   - Ensure WebApi is running

4. **Docker containers won't start:**
   ```bash
   docker ps -a
   docker logs <container-name>
   ```

### Check Logs

```bash
# Aspire Logs
dotnet run --project AspireWalkthrough.AppHost --verbosity detailed

# MQTT Broker Logs
sudo tail -f /var/log/mosquitto/mosquitto.log

# Service-specific Logs
dotnet run --project Backend --verbosity detailed
```

## 📚 Technologies

- **.NET Aspire 9.4.1**: Service Orchestration and Monitoring
- **ASP.NET Core 9.0**: Web API and gRPC Services
- **gRPC**: Inter-Service Communication
- **Protocol Buffers**: Data Serialization
- **Angular 17+**: Frontend Framework
- **MQTT**: Message Queuing (Mosquitto)
- **PostgreSQL**: Relational Database
- **MongoDB**: NoSQL Database
- **Redis**: In-Memory Cache
- **Docker**: Container Orchestration

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License. See `LICENSE` for details.

## 🆘 Support

For questions or issues:

1. Check the [Troubleshooting](#-troubleshooting) section
2. Look at the [Issues](https://github.com/your-repo/issues)
3. Create a new issue with detailed description

---

**Happy coding with .NET Aspire! 🚀**
