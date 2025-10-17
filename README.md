# 🚀 DataServer Automation System (Demo Version)

⚙️ This is a demo version of a project originally developed for internal use at TOTVS.
Sensitive data and proprietary components have been removed.

## 📋 About The Project

DataServer Automation System is a web-based tool designed to simplify API testing and integration processes. It provides a user-friendly interface for developers and analysts to work with REST APIs, manage request templates, and compare JSON data structures.

### 🎯 Key Features

- 🔍 **Template Library**: Pre-configured request templates
- 🚀 **API Testing**: Simplified GET/POST request execution
- 📊 **JSON Comparison**: Side-by-side JSON structure comparison
- 📋 **Documentation**: Integrated tips and documentation
- 👥 **User Management**: Role-based access control
- 📁 **Data Export**: Export capabilities to CSV format

## 🛠️ Built With

- **Backend**: ASP.NET Core
- **Frontend**: HTML5, CSS3, JavaScript
- **Database**: SQL Server
- **Authentication**: Custom auth system
- **Documentation**: Swagger/OpenAPI

## 🔧 Installation

1. Clone the repository
```bash
git clone https://github.com/AnaChris154/Automacao-de-Dataservers.git
```

2. Update the connection strings in `appsettings.json` with your database details

3. Run the database migrations
```bash
dotnet ef database update
```

4. Run the application
```bash
dotnet run
```

## 💡 Usage

### Example Request
```json
GET /api/dataserver/example
{
    "filter": "ACTIVE = 1",
    "fields": ["ID", "NAME", "STATUS"]
}
```

### Example Response
```json
{
    "success": true,
    "data": [
        {
            "id": 1,
            "name": "Example Item",
            "status": "Active"
        }
    ]
}
```

## 👥 User Types

1. **Regular Users**
   - Execute API requests
   - Use template library
   - Compare JSON structures

2. **Administrators**
   - All regular user features
   - User management
   - System configuration
   - Access to logs

## ⚡ Quick Start Guide

1. Register/Login to the system
2. Navigate to the API Testing section
3. Choose a pre-configured template or create a new request
4. Execute the request and view results
5. Use the JSON comparison tool to analyze responses

## 📚 Documentation

Full documentation available in the [Wiki](https://github.com/AnaChris154/Automacao-de-Dataservers/wiki)

## 🤝 Contributing

This is a demo version for portfolio purposes. However, suggestions and improvements are welcome:

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This demo version is distributed under the MIT License. See `LICENSE` for more information.

## 📧 Contact

Ana Christine Ferreira Costa - ana.christine@totvs.com.br

Project Link: [https://github.com/AnaChris154/Automacao-de-Dataservers](https://github.com/AnaChris154/Automacao-de-Dataservers)

---

⚠️ **Note**: This is a demonstration version. Real connection strings, tokens, and credentials have been removed and replaced with example data.
