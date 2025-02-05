import { API_URL } from "./constants";
import { getAuthHeaders } from "./auth";
import { WS_URL } from "./constants";
import axios from "axios";
import * as signalR from "@microsoft/signalr";

export const chatService = {
  async getUserChats() {
    const response = await axios.get(`${API_URL}/Chat/users`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  },
};

class ChatMessagesService {
  constructor() {
    this.connection = null;
    this.messageCallbacks = new Set();
  }

  createConnection() {
    return new signalR.HubConnectionBuilder()
      .withUrl(WS_URL, {
        accessTokenFactory: () => localStorage.getItem("JWT"),
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
        headers: getAuthHeaders(),
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Debug)
      .build();
  }

  async connect() {
    this.connection = this.createConnection();

    this.connection.on("ReceiveMessage", (message) => {
      console.log("Message received:", message);
    });

    this.connection.on("TestResponse", (message) => {
      console.log("Test response:", message);
    });

    this.connection.on("ReceiveTransactionMessage", (message) => {
      console.log("Transaction message:", message);
    });

    this.connection.on("ReceiveTransactionApproval", (approval) => {
      console.log("Transaction approval:", approval);
    });

    this.connection.on("ReceiveEndRequestMessage", (request) => {
      console.log("End request:", request);
    });

    this.connection.onreconnected((connectionId) => {
      console.log("Reconnected.", connectionId);
    });

    this.connection.onclose(() => {
      console.log("Disconnected from chat");
    });
    await this.connection.start();
  }

  disconnect() {
    if (this.connection) {
      this.connection.stop();
    }
  }

  async getConversationMessages(username) {
    const response = await axios.get(
      `${API_URL}/Chat/conversations/${username}`,
      {
        headers: getAuthHeaders(),
      }
    );
    return response.data;
  }

  async sendMessage(formData) {
    const response = await axios.post(`${API_URL}/Chat/messages`, formData, {
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "multipart/form-data",
      },
    });
    return response.data;
  }

  onMessage(callback) {
    this.connection.on("ReceiveMessage", callback);
    return () => {
      this.connection.off("ReceiveMessage", callback);
    };
  }
}

export const chatMessagesService = new ChatMessagesService();
