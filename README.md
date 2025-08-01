# unity-lobby-auth-system

Metaverse Cloud Services
Overview 
This document provides details regarding an implementation of an Unity lobby authentication system. The system implements a complete multiplayer lobby system with user authentication, lobby management, and game session coordination using Unity's cloud services.

Features
The Unity lobby authentication system is a multiplayer game system that provides:

User Authentication: Sign-in/sign-up functionality using Unity Authentication service
Lobby Management: Create, search, join, and manage multiplayer game lobbies
UI Management: Panel-based interface system for different game states
Network Integration: Unity Netcode for GameObjects with Relay service support

Components
Core Management Classes
Component
File
Purpose
MenuManager
Assets/Scripts/Menu/MenuManager.cs
Central controller for authentication and service initialization
PanelManager
Assets/Scripts/Tools/PanelManager.cs
Singleton managing UI panel transitions and state
Panel
Assets/Scripts/Tools/Panel.cs
Base class for all UI panels in the system


Lobby Management Components

Component
File
Purpose
LobbyMenu
Assets/Scripts/Menu/LobbyMenu.cs
Main lobby interface and player management
LobbySearchMenu
Assets/Scripts/Menu/LobbySearchMenu.cs
Lobby discovery and joining functionality
LobbySettingsMenu
Assets/Scripts/Menu/LobbySettingsMenu.cs
Lobby creation and configuration
LobbyListItem
Assets/Scripts/Menu/LobbyListItem.cs
Individual lobby display component
LobbyPlayerItem
Assets/Scripts/Menu/LobbyPlayerItem.cs
Individual player display component


Session Components

Component
File
Purpose
AuthenticationMenu
Assets/Scripts/Menu/AuthenticationMenu.cs
User sign-in and registration interface
StartingSessionMenu
Assets/Scripts/Menu/StartingSessionMenu.cs
Game session initialization and scene transition
LoadingMenu
Assets/Scripts/Menu/LoadingMenu.cs
Async operation progress display
ErrorMenu
Assets/Scripts/Menu/ErrorMenu.cs
Error message display and handling
MainMenu
Assets/Scripts/Menu/MainMenu.cs
Primary navigation interface



Authentication Flow
This diagram illustrates the user flow and state transitions between key Authentication components:

Figure 1: Authentication Service Initialization

The initialization sequence ensures all services are ready before attempting authentication operations. The MenuManager.StartClientService() method coordinates this process and handles initialization failures.

The system integrates with Unity Authentication Service to handle user identity and session management. The MenuManager class provides methods for anonymous sign-in, username/password authentication, and session handling.

Operation
Method
Parameters
Error Handling
Anonymous Sign-in
SignInAnonymouslyAsync()
None
AuthenticationException, RequestFailedException
Username Sign-in
SignInWithUsernamePasswordAsync()
username, password
Error code 0 (invalid credentials)
Username Sign-up
SignUpWithUsernamePasswordAsync()
username, password
Error code 10003 (duplicate username)
Sign Out
SignOut()
None
None
Update Name
UpdatePlayerNameAsync()
playerName
Exception handling


Figure 2: Authentication Flow of Events


Unity Services Integration
The system integrates with Unity's cloud services through a centralized initialization and event management pattern. The MenuManager serves as the primary orchestrator for service initialization and authentication flow.

Figure 3: Service Initialization


Panel Management System
The UI architecture uses a panel-based system where PanelManager controls the visibility and lifecycle of different UI screens. Each panel extends a base Panel class and implements specific functionality.


Figure 4: Panel Class


Lobby System 
The lobby system manages multiplayer sessions through Unity Lobbies Service with real-time event subscriptions and player state management.

Figure 5: Lobby Sessions

The LobbyMenu class provides comprehensive integration with Unity Lobbies Service, handling lobby creation, joining, management, and real-time event subscriptions.

The lobby lifecycle is managed through the following key operations:

Lobby Creation 
The LobbyMenu.CreateLobby method handles lobby creation with configurable parameters:

Lobby Name: User-defined lobby identifier
Max Players: Maximum lobby capacity
Privacy Settings: Public or private lobby visibility
Player Data: Initial player information including name and ready state

Heartbeat System
The lobby system implements heartbeat functionality to maintain lobby state:
Update Frequency: 15-second intervals (reduced to 5 seconds during game start)
Host Validation: Only lobby host sends heartbeat pings
Heartbeat Method: LobbyService.Instance.SendHeartbeatPingAsync


Player Ready State Management
Players can toggle their ready state through LobbyMenu.SwitchReady:
Updates player data with ready status ("0" or "1")
Triggers lobby state refresh via LoadPlayers
Host can only start game when all players are ready


Relay Architecture
The transition from lobby to game session involves creating a Unity Relay allocation and configuring the network transport layer.


The system uses UnityTransport as the transport layer, configured with Unity Relay for NAT traversal and peer-to-peer connectivity:
Component
Purpose
Configuration Location
UnityTransport
Low-level network transport
Retrieved via NetworkManager.Singleton.GetComponent<UnityTransport>()
RelayServerData
Relay connection parameters
Generated from AllocationUtils.ToRelayServerData()
NetworkManager
High-level networking coordination
Singleton pattern access


Relay Allocation Process
The host creates a relay allocation that other players can join using a join code:
Allocation Creation: RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers) creates server allocation
Transport Configuration: AllocationUtils.ToRelayServerData(allocation, "dtls") converts allocation to transport data
Join Code Generation: RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId) creates shareable join code
Transport Setup: transport.SetRelayServerData(data) configures Unity Transport with relay parameters

Scene Flow
AuthenticationMenu
→ User signs in → transitions to LobbyMenu

LobbyMenu
→ Create/join a lobby → Ready up → Start game (host only)

SessionMap01
→ Multiplayer game scene
→ Each player is spawned at a designated point
→ Movement and appearance synced across clients
