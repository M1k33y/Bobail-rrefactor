import * as signalR from "@microsoft/signalr";
import { getStoredToken } from "../utils/authStorage";

const HUB_URL = "https://localhost:7006/hubs/auth";

export function createAuthHubConnection() {
  return new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => getStoredToken() || "",
    })
    .withAutomaticReconnect()
    .build();
}
