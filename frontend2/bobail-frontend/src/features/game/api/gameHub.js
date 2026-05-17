import * as signalR from "@microsoft/signalr";
import { getStoredToken } from "../../auth/utils/authStorage";

const HUB_URL = "https://localhost:7006/hubs/game";

export function createGameHubConnection() {
  return new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => getStoredToken() || "",
    })
    .withAutomaticReconnect()
    .build();
}
