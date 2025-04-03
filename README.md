<div align="center">
<h3>Social network for UI/UX designers</h3>
<p>

[![Vue.js](https://img.shields.io/badge/Vue.js-4FC08D?logo=vuedotjs&logoColor=fff)](#) [![.NET](https://img.shields.io/badge/Asp.NET%20Core-512BD4?logo=dotnet&logoColor=fff)](#) [![Postgres](https://img.shields.io/badge/Postgres-%23316192.svg?logo=postgresql&logoColor=white)](#)

</p>
</div>
A modern web platform for UI/UX designers to showcase their work, interact with clients, and manage their portfolio.

## ✨ Features

- 📸 Portfolio Management – Easily create and manage a personal portfolio to showcase design projects.
- 💬 Real-time Chat – Communicate with clients and other designers instantly via built-in messaging.
- 🪙 In-app Currency System – A built-in currency for premium features and user transactions.
- 🔒 Protected & Private Posts – Control the visibility of posts with advanced privacy settings.
- 👥 User Subscriptions – Follow and subscribe to designers for updates and exclusive content.
- 📱 Responsive Design – Optimized for seamless use across different devices and screen sizes.

## 🎥 Demo

### Portfolio View
<img 
  src="picturesForReadMe/portfolio-demo.png" 
  alt="Portfolio Demo" 
  width="600"
/>

### Post Upload
<img 
  src="picturesForReadMe/upload-demo.png" 
  alt="Portfolio Demo" 
  width="600"
/>


<!--
### Chat Interface

![Chat Demo](picturesForReadMe/chat-demo.gif)
-->

## 🛠️ Tech Stack

### Frontend:

[![Vue.js](https://img.shields.io/badge/Vue.js-4FC08D?logo=vuedotjs&logoColor=fff)](#) [![TailwindCSS](https://img.shields.io/badge/Tailwind%20CSS-%2338B2AC.svg?logo=tailwind-css&logoColor=white)](#) [![Vite](https://img.shields.io/badge/Vite-646CFF?logo=vite&logoColor=fff)](#) [![JavaScript](https://img.shields.io/badge/JavaScript-F9A03C?logo=javascript&logoColor=fff)](#)

### Backend & Database:

[![.NET](https://img.shields.io/badge/.NET%20Core-512BD4?logo=dotnet&logoColor=fff)](#) [![Postgres](https://img.shields.io/badge/Postgres-%23316192.svg?logo=postgresql&logoColor=white)](#) [![Swagger](https://img.shields.io/badge/Swagger-%2385EA2D.svg?logo=swagger&logoColor=black)](#)

### Other:

[![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=fff)](#) [![SignalR](https://img.shields.io/badge/SignalR-6006A9?logo=microsoft&logoColor=fff)](#) [![WebSocket](https://img.shields.io/badge/WebSocket-4A154B?logo=websocket&logoColor=fff)](#)

## 🚀 Getting Started

1. Clone the repository:

```bash
git clone https://github.com/krasnw/designerSocialNetwork.git
```

2. Go to the project’s root directory. Create .env file. Here is the template:

```env
JWT_SECRET_KEY=Secret_key_for_JWT_token_that_is_at_least_32_characters_long

DATABASE=api_database
DATABASE_USER=api_user
DATABASE_PASSWORD=api_user_password
```

> [!IMPORTANT]
> Make sure that JWT_SECRET_KEY is at least 32 characters long. System will not work if it is shorter.

> [!NOTE]
> Database related variables are used to connect to the database, so they can be used to externally connect to the database.

3. Run the following command.

```bash
docker-compose up
```

### 🔌 Ports

- Backend: 8080
- Backend Admin: 8082
- Database: 5433
- Frontend: 8084
- Frontend Admin: 8085
