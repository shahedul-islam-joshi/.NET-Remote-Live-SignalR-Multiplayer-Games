# NeonGrid: Real-Time Multiplayer Tic-Tac-Toe

NeonGrid is a high-performance, real-time multiplayer gaming platform built with **ASP.NET Core** and **SignalR**. Designed for zero-friction gameplay, it allows users to jump into global sessions using nothing but a "codename"—no registration, no passwords, just instant competition.

##  Key Features

### 1. Zero-Friction Entry
* **Anonymous Identity:** Users enter a "Codename" to join the global lobby immediately.
* **Session Persistence:** Uses in-memory state management to track active players without the overhead of a database for high-speed performance.

### 2. Real-Time Multiplayer Engine
* **SignalR Hub Architecture:** Powered by WebSockets for bi-directional, low-latency communication.
* **Global Lobby:** A dynamic list of "Active Zones" that updates in real-time as users create or close game sessions.
* **Optimistic UI Updates:** The game board provides immediate feedback, ensuring a "snappy" feel even on high-latency networks.

### 3. "Neon-Glass" Aesthetic
* **Modern UI/UX:** A sleek "Cyberpunk" interface utilizing Glassmorphism (backdrop-blur filters) and CSS neon glows.
* **Responsive Design:** Fully playable across mobile, tablet, and desktop browsers.
* **State-Driven Transitions:** Smooth transitions between the Login, Lobby, and Game Arena views.

### 4. Global Leaderboard & Stats
* **Performance Tracking:** Tracks Wins, Losses, and Draws per codename.
* **Resilient Persistence:** Features a fail-safe JSON-based storage system designed to handle restricted file systems in cloud environments (like Render or Azure).
##  Technical Stack
* **Backend:** ASP.NET Core 8.0/9.0
* **Real-Time:** SignalR (WebSockets with Long Polling fallback)
* **Frontend:** HTML5, CSS3, JavaScript (Vanilla ES6+), Bootstrap 5
* **State Management:** Singleton Service with Concurrent Collections (Thread-safe)
### Cloud Hosting (Render/Azure)
The application is pre-configured for cloud deployment:
* **CORS Enabled:** Permits connections from remote origins.
* **Stateless Compatibility:** Designed to run in ephemeral environments.
* **Path Correction:** Automatically detects the best directory for stats storage.

##  Roadmap
- [ ] **Reconnection Logic:** Ability to rejoin a session if the browser is refreshed.
- [ ] **Global Chat:** A lobby-wide chat system for players to challenge each other.
- [ ] **SQLite Integration:** Moving from JSON to a lightweight database for better data integrity.
##  Folder Structure

The project follows a standard ASP.NET Core MVC pattern, augmented with a dedicated Hubs and Services layer for real-time multiplayer logic.

```text
NeonGrid/
├── Controllers/                 # Standard MVC Controllers
│   ├── HomeController.cs        # Main entry and View routing
│   └── StatsController.cs       # API for leaderboard and player statistics
├── Hubs/                        # Real-time Communication
│   └── GameHub.cs               # SignalR Hub for handling game events
├── Models/                      # Data Structures
│   ├── Domain Models/           # Core Logic Entities
│   │   ├── GameSession.cs       # Logic for individual game instances
│   │   ├── Player.cs            # Player session data
│   │   └── UserStat.cs          # Persistent statistics structure
│   └── ErrorViewModel.cs        # Standard error handling model
├── Services/                    # Business Logic Layer
│   └── GameManager.cs           # Singleton service managing global game state
├── Views/                       # UI Templates (Razor)
│   ├── Home/
│   │   ├── Index.cshtml         # Main Game Client & Neon UI
│   │   └── Privacy.cshtml       # Standard privacy page
│   └── Shared/                  # Global Layouts & Partials
│       ├── _Layout.cshtml       # Main HTML wrapper with SignalR scripts
│       └── Error.cshtml         # Error display page
├── wwwroot/                     # Static Assets (CSS, JS, Images)
├── player_stats.json            # Local JSON storage for leaderboards
├── Dockerfile                   # Containerization configuration
├── Program.cs                   # Application startup and DI configuration
└── README.md                    # Project documentation