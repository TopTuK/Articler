# Articler

An AI-powered article writing platform that leverages Retrieval Augmented Generation (RAG) to help you create high-quality content. Articler allows you to build knowledge bases from various data sources (text documents, PDFs) and generate articles using AI chat assistance.

## Features

- 🤖 **AI-Powered Content Generation**: Generate full articles, outlines, or specific sections with AI assistance
- 📚 **Knowledge Stacking**: Build layered knowledge bases for different projects by combining multiple data sources
- 📄 **Multi-Format Data Sources**: Add text documents and PDFs (via URL) to your project's knowledge base
- 💬 **Interactive AI Chat**: Chat with AI agents that understand your project context and data sources
- 🎯 **Project Management**: Organize your writing projects with descriptions and metadata
- 🔐 **Secure Authentication**: JWT-based authentication with OpenID Connect support
- ⚡ **Distributed Architecture**: Built on Microsoft Orleans for scalable, distributed processing
- 🔍 **Vector Search**: Powered by Qdrant for efficient semantic search across your documents

## Architecture

Articler follows a microservices architecture using Microsoft Orleans:

- **Frontend**: Vue.js 3 SPA with Vite, TailwindCSS, and Vuestic UI
- **Backend API**: ASP.NET Core Web API with JWT authentication
- **Orleans Silo**: Distributed actor framework for state management and processing
- **Database**: PostgreSQL for persistent storage
- **Vector Store**: Qdrant for document embeddings and semantic search
- **AI Integration**: OpenAI, DeepSeek, and OpenRouter support

## Tech Stack

### Backend
- .NET (ASP.NET Core)
- Microsoft Orleans (distributed actor framework)
- PostgreSQL
- Qdrant (vector database)
- Semantic Kernel
- Serilog (logging)

### Frontend
- Vue.js 3
- Vite
- TailwindCSS
- Vuestic UI
- Vue Router
- Pinia (state management)
- Vue I18n (internationalization)
- Axios

## Prerequisites

- .NET SDK (latest version)
- Node.js 20.19.0+ or 22.12.0+
- PostgreSQL 9.5+
- Qdrant (vector database)
- OpenAI API key (or DeepSeek/OpenRouter)

## Project Structure

```
Articler/
├── Articler.AppDomain/          # Domain models and business logic
├── Articler.AppHost/            # Application host
├── Articler.GrainClasses/       # Orleans grain implementations
│   ├── Chat/                    # Chat agent grains
│   ├── Document/                # Document storage grains
│   ├── Project/                 # Project management grains
│   └── User/                    # User management grains
├── Articler.GrainInterfaces/    # Orleans grain interfaces
├── Articler.ServiceDefaults/    # Shared service configurations
├── Articler.SiloHost/           # Orleans silo host
├── Articler.WebApi/             # REST API controllers and services
│   ├── Controllers/             # API endpoints
│   ├── Middlewares/             # Authentication and user middleware
│   └── Services/                # Business services
├── Articler.TestApp/            # Test application
├── webapp/                      # Vue.js frontend application
│   ├── src/
│   │   ├── components/         # Vue components
│   │   ├── services/           # API service clients
│   │   ├── stores/             # Pinia stores
│   │   └── views/              # Page views
│   └── package.json
├── PostgreSQL-Main.sql          # Main database schema
├── PostgreSQL-Clustering.sql    # Orleans clustering tables
├── PostgreSQL-Persistence.sql   # Orleans persistence tables
└── Articler.sln                 # Solution file
```

## Getting Started

### 1. Database Setup

Create a PostgreSQL database and run the SQL scripts in order:

```bash
# Create the database
createdb articler

# Run the SQL scripts
psql -d articler -f PostgreSQL-Main.sql
psql -d articler -f PostgreSQL-Clustering.sql
psql -d articler -f PostgreSQL-Persistence.sql
```

### 2. Configuration

#### Backend Configuration

Update `Articler.WebApi/appsettings.json` and `Articler.SiloHost/appsettings.json` with your configuration:

```json
{
  "Auth": {
    "Jwt": {
      "Key": "your-secret-key",
      "ValidIssuer": "Articler",
      "ValidAudience": "Articler"
    },
    "Oidc": {
      "Authority": "your-oidc-authority",
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret",
      "CallbackUrl": "/auth/callback"
    }
  },
  "Qdrant": {
    "Host": "http://localhost:6333",
    "ApiKey": "your-qdrant-api-key"
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "BaseUrl": "https://api.openai.com/v1"
  }
}
```

#### Frontend Configuration

Update `webapp/src/config.js` with your API endpoint:

```javascript
export const API_BASE_URL = 'http://localhost:5000/api'
```

### 3. Install Dependencies

#### Backend
```bash
dotnet restore
```

#### Frontend
```bash
cd webapp
npm install
```

### 4. Run the Application

#### Development Mode

1. **Start the Orleans Silo Host**:
   ```bash
   cd Articler.SiloHost
   dotnet run
   ```

2. **Start the Web API** (in a new terminal):
   ```bash
   cd Articler.WebApi
   dotnet run
   ```

3. **Start the Frontend** (in a new terminal):
   ```bash
   cd webapp
   npm run dev
   ```

The application will be available at:
- Frontend: http://localhost:5173
- API: http://localhost:5000 (or as configured)

#### Production Mode

Build and run the applications:

```bash
# Build backend
dotnet build --configuration Release

# Build frontend
cd webapp
npm run build
```

## Usage

1. **Sign In**: Use the login page to authenticate with your PM Moscow Club account (or configured OIDC provider)

2. **Create a Project**: 
   - Navigate to "Projects" and click "Create New Project"
   - Enter a title and description for your project

3. **Add Data Sources**:
   - Open your project
   - In the "Data Sources" section, add text documents or PDF URLs
   - These documents will be processed and added to your project's knowledge base

4. **Chat with AI**:
   - Use the chat interface to interact with AI agents
   - The AI will use your project's data sources to provide context-aware responses
   - Generate articles, outlines, or get writing assistance

5. **Manage Content**:
   - Edit generated content using the built-in editor
   - Export to various formats (Markdown, HTML, Word)

## Development

### Building

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build Articler.WebApi/Articler.WebApi.csproj
```

### Testing

```bash
# Run tests (if available)
dotnet test
```

### Linting

```bash
# Frontend linting
cd webapp
npm run lint
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Author

Sergey Sidorov - 2025

## Acknowledgments

- Microsoft Orleans for distributed computing
- Semantic Kernel for AI integration
- Qdrant for vector search capabilities
- Vue.js and the Vue ecosystem
