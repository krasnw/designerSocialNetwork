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
  async approveTransaction(transactionHash) {
    const response = await axios.post(
      `${API_URL}/Chat/transaction/${transactionHash}/approve`,
      { transactionHash },
      {
        headers: getAuthHeaders(),
      }
    );
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

  async sendTransactionMessage(formData) {
    const response = await axios.post(`${API_URL}/Chat/transaction`, formData, {
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
      },
    });
    return response.data;
  }

  onMessage(callback) {
    const handlers = [
      "ReceiveMessage",
      "ReceiveTransactionMessage",
      "ReceiveTransactionApproval",
    ];

    const cleanupFunctions = handlers.map((eventName) => {
      this.connection.on(eventName, callback);
      return () => this.connection.off(eventName, callback);
    });

    // Return cleanup function that removes all handlers
    return () => cleanupFunctions.forEach((cleanup) => cleanup());
  }
}

export const chatMessagesService = new ChatMessagesService();
