# unity-lobby-auth-system


Metaverse Cloud Services
========================

Overview 
---------

This document provides details regarding an implementation of an Unity lobby authentication system. The system implements a complete multiplayer lobby system with user authentication, lobby management, and game session coordination using Unity's cloud services.

Features
--------

The Unity lobby authentication system is a multiplayer game system that provides:

-   User Authentication: Sign-in/sign-up functionality using Unity Authentication service

-   Lobby Management: Create, search, join, and manage multiplayer game lobbies

-   UI Management: Panel-based interface system for different game states

-   Network Integration: Unity Netcode for GameObjects with Relay service support

Components
----------

### Core Management Classes

|

Component

 |

File

 |

Purpose

 |
|

MenuManager

 |

Assets/Scripts/Menu/MenuManager.cs

 |

Central controller for authentication and service initialization

 |
|

PanelManager

 |

Assets/Scripts/Tools/PanelManager.cs

 |

Singleton managing UI panel transitions and state

 |
|

Panel

 |

Assets/Scripts/Tools/Panel.cs

 |

Base class for all UI panels in the system

 |

### Lobby Management Components

|

Component

 |

File

 |

Purpose

 |
|

LobbyMenu

 |

Assets/Scripts/Menu/LobbyMenu.cs

 |

Main lobby interface and player management

 |
|

LobbySearchMenu

 |

Assets/Scripts/Menu/LobbySearchMenu.cs

 |

Lobby discovery and joining functionality

 |
|

LobbySettingsMenu

 |

Assets/Scripts/Menu/LobbySettingsMenu.cs

 |

Lobby creation and configuration

 |
|

LobbyListItem

 |

Assets/Scripts/Menu/LobbyListItem.cs

 |

Individual lobby display component

 |
|

LobbyPlayerItem

 |

Assets/Scripts/Menu/LobbyPlayerItem.cs

 |

Individual player display component

 |

### Session Components

|

Component

 |

File

 |

Purpose

 |
|

AuthenticationMenu

 |

Assets/Scripts/Menu/AuthenticationMenu.cs

 |

User sign-in and registration interface

 |
|

StartingSessionMenu

 |

Assets/Scripts/Menu/StartingSessionMenu.cs

 |

Game session initialization and scene transition

 |
|

LoadingMenu

 |

Assets/Scripts/Menu/LoadingMenu.cs

 |

Async operation progress display

 |
|

ErrorMenu

 |

Assets/Scripts/Menu/ErrorMenu.cs

 |

Error message display and handling

 |
|

MainMenu

 |

Assets/Scripts/Menu/MainMenu.cs

 |

Primary navigation interface

 |

Authentication Flow
-------------------

This diagram illustrates the user flow and state transitions between key Authentication components:

![](https://lh7-rt.googleusercontent.com/docsz/AD_4nXcM7tKAvypc9yjB_CwgVrpsbGubZAibrB5P5--nEwIQ7VS6fZFaPdUUBXuNYuURg9j1q2J4c9xIZzPX6nQfgqlHGnmQTOkaBfiZ6yA1sFp3DJozqpz7s_F6esxKWqOuno3B-8gJ?key=oxpahlrh1QC6LEh4hjoFWw)

Figure 1: Authentication Service Initialization

The initialization sequence ensures all services are ready before attempting authentication operations. The MenuManager.StartClientService() method coordinates this process and handles initialization failures.

The system integrates with Unity Authentication Service to handle user identity and session management. The MenuManager class provides methods for anonymous sign-in, username/password authentication, and session handling.

|

Operation

 |

Method

 |

Parameters

 |

Error Handling

 |
|

Anonymous Sign-in

 |

SignInAnonymouslyAsync()

 |

None

 |

AuthenticationException, RequestFailedException

 |
|

Username Sign-in

 |

SignInWithUsernamePasswordAsync()

 |

username, password

 |

Error code 0 (invalid credentials)

 |
|

Username Sign-up

 |

SignUpWithUsernamePasswordAsync()

 |

username, password

 |

Error code 10003 (duplicate username)

 |
|

Sign Out

 |

SignOut()

 |

None

 |

None

 |
|

Update Name

 |

UpdatePlayerNameAsync()

 |

playerName

 |

Exception handling

 |

![](https://lh7-rt.googleusercontent.com/docsz/AD_4nXcvaBWcHwsbJPBFF_WZl02bpj173vVYfGs5qt1DtsdDkk-oQrkLlZFGQK5NqCE1QKWkmb4Mr3GxrOSscRYlTL-xXO51iMvc1ggkZs8PKPJMYH7Uy6TPRvIf5u7_mQ1FR-xa2sG7?key=oxpahlrh1QC6LEh4hjoFWw)

Figure 2: Authentication Flow of Events

Unity Services Integration
--------------------------

The system integrates with Unity's cloud services through a centralized initialization and event management pattern. The MenuManager serves as the primary orchestrator for service initialization and authentication flow.

![](https://lh7-rt.googleusercontent.com/docsz/AD_4nXeVWtlg0BuqtHBBHcSKDY3GX1UybCm-7715jOFKEl5ITiVWv02GbejrQEaIzq05pzX_XMeRXRLGCJyWG4UmgoUOQKtEHrXlOKYDhJJGuuOroRrOMtpfSDPMnp8skU_ODZ1l5BOV8Q?key=oxpahlrh1QC6LEh4hjoFWw)
-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

Figure 3: Service Initialization

Panel Management System
-----------------------

The UI architecture uses a panel-based system where PanelManager controls the visibility and lifecycle of different UI screens. Each panel extends a base Panel class and implements specific functionality.

![](https://lh7-rt.googleusercontent.com/docsz/AD_4nXf6N70eHdHB7LvDCa4qcbriHAgyotwcrpDuNkN4YnCFDkdeP_aJM0fuI86PCmrnaMkSKf3z1k_ePIDtVHKHzdnloiApfLJZYaPBtUoL7Z8CUUdbQfLDltfcJlO1E9Qzo3_rP-Q69Q?key=oxpahlrh1QC6LEh4hjoFWw)

Figure 4: Panel Class

Lobby System 
-------------

The lobby system manages multiplayer sessions through Unity Lobbies Service with real-time event subscriptions and player state management.

![](https://lh7-rt.googleusercontent.com/docsz/AD_4nXdFV7Bl53mDUBqrnxLtXp7JDsHyeB8NS18SEEBICvdbZRA7ansjWHte4dYPW_l92XiO1gXvpiT_XiAgzlL2uHVeOl89wQ6cK6LiZ7GuhMqksXk_ge2kWMO9H_cNH5s2hlO29m3ijQ?key=oxpahlrh1QC6LEh4hjoFWw)

Figure 5: Lobby Sessions

The LobbyMenu class provides comprehensive integration with Unity Lobbies Service, handling lobby creation, joining, management, and real-time event subscriptions.

The lobby lifecycle is managed through the following key operations:

### Lobby Creation 

The LobbyMenu.CreateLobby method handles lobby creation with configurable parameters:

-   Lobby Name: User-defined lobby identifier

-   Max Players: Maximum lobby capacity

-   Privacy Settings: Public or private lobby visibility

-   Player Data: Initial player information including name and ready state

### Heartbeat System

The lobby system implements heartbeat functionality to maintain lobby state:

-   Update Frequency: 15-second intervals (reduced to 5 seconds during game start)

-   Host Validation: Only lobby host sends heartbeat pings

-   Heartbeat Method: LobbyService.Instance.SendHeartbeatPingAsync

### Player Ready State Management

Players can toggle their ready state through LobbyMenu.SwitchReady:

-   Updates player data with ready status ("0" or "1")

-   Triggers lobby state refresh via LoadPlayers

-   Host can only start game when all players are ready

Relay Architecture
------------------

The transition from lobby to game session involves creating a Unity Relay allocation and configuring the network transport layer.

![](https://lh7-rt.googleusercontent.com/docsz/AD_4nXepWB8gVYwwUAszO6GzE1Y3n6Wsb0YGBdqtYgSLtP7s7p2__-HnTSPMtmAdKMsDMRfV5uy83dyuzEV-_FbIsmlfQlJOxfvNRTMchO14aTGcmDOi2zTbfn1FsJjgTOIkEQJIhpdztA?key=oxpahlrh1QC6LEh4hjoFWw)

The system uses UnityTransport as the transport layer, configured with Unity Relay for NAT traversal and peer-to-peer connectivity:

|

Component

 |

Purpose

 |

Configuration Location

 |
|

UnityTransport

 |

Low-level network transport

 |

Retrieved via NetworkManager.Singleton.GetComponent<UnityTransport>()

 |
|

RelayServerData

 |

Relay connection parameters

 |

Generated from AllocationUtils.ToRelayServerData()

 |
|

NetworkManager

 |

High-level networking coordination

 |

Singleton pattern access

 |

### Relay Allocation Process

The host creates a relay allocation that other players can join using a join code:

1.  Allocation Creation: RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers) creates server allocation

2.  Transport Configuration: AllocationUtils.ToRelayServerData(allocation, "dtls") converts allocation to transport data

3.  Join Code Generation: RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId) creates shareable join code

4.  Transport Setup: transport.SetRelayServerData(data) configures Unity Transport with relay parameters

Scene Flow
----------

1.  AuthenticationMenu

→ User signs in → transitions to LobbyMenu

1.  LobbyMenu

→ Create/join a lobby → Ready up → Start game (host only)

1.  SessionMap01

→ Multiplayer game scene

→ Each player is spawned at a designated point

→ Movement and appearance synced across clients
