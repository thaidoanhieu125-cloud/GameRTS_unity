# Cấu trúc dự án GameRTS

Cập nhật ngày 13/11/2026. Tài liệu giải thích các thư mục, file hiện có và cách chúng kết nối với nhau, giúp người đang học Unity tìm đúng nơi để đọc hoặc chỉnh sửa. Mọi đường dẫn trong tài liệu được tính từ thư mục gốc `GameRTS`.

Kế hoạch thiết kế và tiến độ chính thức nằm trong `Game RTS Unity Doc.docx`. Quy trình làm việc và yêu cầu cập nhật tài liệu này nằm trong `AGENTS.md`.

## 1. Sơ đồ thư mục

```text
GameRTS/
├── AGENTS.md                       Luật làm việc phải đọc mỗi lần bắt đầu
├── CAU_TRUC_DU_AN.md                Mô tả cấu trúc và chức năng file
├── Game RTS Unity Doc.docx          Kế hoạch gốc
├── .gitignore                      Quy tắc bỏ qua file khi quản lý bằng Git
├── .vsconfig                       Cấu hình thành phần Visual Studio
├── Assets/
│   ├── Game/                       Mã nguồn và tài nguyên riêng của game
│   │   ├── Art/
│   │   │   ├── Terrain/            Sprite và Tile asset cho nền map
│   │   │   └── Units/              Sprite quân và vòng chọn
│   │   ├── Commands/               Lệnh di chuyển bằng dữ liệu C# thuần
│   │   ├── Data/
│   │   │   └── Units/              Asset dữ liệu từng loại quân
│   │   ├── Editor/                 Công cụ thiết lập scene, kiểm tra và build
│   │   ├── Input/                  Đọc chuột và bàn phím
│   │   ├── Scenes/                 Scene gameplay
│   │   ├── Simulation/             Lưới, A*, trạng thái quân và mô phỏng theo tick
│   │   ├── Tests/Editor/
│   │   │   ├── Simulation/         Kiểm thử A*, lệnh, va chạm và replay
│   │   │   └── Units/              Kiểm thử dữ liệu, Prefab và spawn
│   │   ├── Tests/PlayMode/          Kiểm thử chọn và điều khiển quân trong scene
│   │   ├── UI/                     Thư mục dành cho giao diện, hiện chưa có file
│   │   └── View/
│   │       └── Prefabs/            Khuôn GameObject dùng để tạo đối tượng
│   ├── Scenes/                     Scene có sẵn từ template Unity
│   └── Settings/                   Cấu hình và scene template Universal 2D
├── Packages/                       Danh sách package và phiên bản phụ thuộc
├── ProjectSettings/                Cấu hình chung của project Unity
├── Builds/                         Bản game đã build, Git bỏ qua
├── Library/                        Dữ liệu import và cache Unity, Git bỏ qua
├── Logs/                           Log và kết quả kiểm tra, Git bỏ qua
├── Temp/                           File tạm Unity, Git bỏ qua
└── UserSettings/                   Tùy chọn Editor theo máy, Git bỏ qua
```

`.git/` là dữ liệu quản lý phiên bản của repository này. Các file `.meta` nằm cạnh asset và thư mục trong `Assets`; quy tắc sử dụng được giải thích ở mục 9.

## 2. Tài liệu và cấu hình ở gốc

| File | Chức năng và cách sử dụng |
| --- | --- |
| `AGENTS.md` | Luật bắt buộc trước, trong và sau khi làm dự án: đọc kế hoạch gốc, giải thích chi tiết và duy trì tài liệu cấu trúc. |
| `CAU_TRUC_DU_AN.md` | Danh mục các file và thư mục, chức năng, quan hệ và cách tìm phần cần chỉnh. Cập nhật mỗi khi thêm file được giữ lại hoặc thay đổi cấu trúc. |
| `Game RTS Unity Doc.docx` | Nguồn chính thức về thiết kế, luật chơi, kiến trúc, lộ trình và tiến độ. Phải đọc toàn bộ, bao gồm các bảng, trước mỗi lần làm việc. |
| `.gitignore` | Quy định file và thư mục Git bỏ qua, gồm cache, log, bản build, file IDE và tùy chọn theo máy. |
| `.vsconfig` | Ghi thành phần Visual Studio phục vụ phát triển game bằng Unity; IDE có thể dùng file này để đề xuất thành phần cần cài. |

## 3. Mã nguồn gameplay và dữ liệu

Assembly Definition là file `.asmdef` chia mã C# thành các nhóm biên dịch và quy định nhóm nào được tham chiếu nhóm nào. Tham chiếu tới một assembly cho phép dùng mã trong đó; riêng việc có tham chiếu không có nghĩa tính năng gameplay đã được triển khai.

### Simulation và Commands

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Simulation/GameRTS.Simulation.asmdef` | Định nghĩa assembly mô phỏng C# tham chiếu Commands; `noEngineReferences` bảo đảm không tham chiếu UnityEngine. |
| `Assets/Game/Simulation/GridMap.cs` | Lưới ô đi được, gốc map, tâm ô và 8 hướng lân cận. CanTraverse kiểm tra bước đi cùng hai ô sát góc khi đi chéo. Độ dài cạnh là 1000, đường chéo xấp xỉ 1414 đơn vị mô phỏng; xác định vùng liên thông để loại đích không tới được. |
| `Assets/Game/Simulation/GridPathfinder.cs` | A* với heap, 8 hướng, chi phí cạnh/chéo 1000/1414 và ước lượng octile theo khoảng cách ngang, dọc và chéo. Dùng CanTraverse để tránh cắt góc tường và các ô bị giữ chỗ; thứ tự phân xử thống nhất giữa các máy. |
| `Assets/Game/Simulation/UnitState.cs` | Trạng thái thật của từng quân: ID, chủ sở hữu, loại, tọa độ hiện tại/trước đó, tốc độ, bán kính, ô hiện tại, ô đích, đường đi và SegmentProgress biểu diễn quãng đường đã đi trên đoạn hiện tại. SimulationWorld thay đổi trạng thái; View chỉ đọc để hiển thị. |
| `Assets/Game/Simulation/SimulationWorld.cs` | Quản lý lệnh theo tick, phân ô đích cách nhau 2 ô, giữ ô đích riêng và giữ chỗ khi di chuyển. Bước chéo giữ cả vùng 2 × 2 ô; tốc độ tính theo độ dài đoạn đi để tránh cộng tốc độ đầy đủ vào cả hai trục. Khi gặp luồng đối đầu, ưu tiên nhường ô theo ID để phá tình huống hai quân tìm đường đối xứng; quân vẫn giữ đích ban đầu. Chạy 20 tick/giây và tính checksum gồm tiến độ đoạn đi. |
| `Assets/Game/Simulation/DeterminismScenario.cs` | Kịch bản dùng chung gồm 100 quân, map có tường/khe và ba lệnh trong 600 tick. Replay phiên bản 2 ghi Directions=8 để phân biệt với kết quả cũ; dùng đối chiếu checksum hai Simulation và Editor/Windows. |
| `Assets/Game/Commands/GameRTS.Commands.asmdef` | Định nghĩa assembly dành cho dữ liệu lệnh, không tham chiếu UnityEngine. |
| `Assets/Game/Commands/MoveCommand.cs` | Lệnh di chuyển bất biến chứa tick thực thi, mã người chơi, số thứ tự, danh sách ID đã sắp xếp và tọa độ đích số nguyên. Input tạo lệnh, SimulationWorld kiểm tra và thực thi. |

Simulation giữ trạng thái di chuyển và thực thi MoveCommand bằng phép tính số nguyên theo tick. Một đơn vị thế giới bằng 1000 đơn vị mô phỏng. Lưới dùng ô 1 × 1 và bán kính quân nhỏ hơn 0,5 đơn vị thế giới. Quân đứng chiếm một ô; khi di chuyển giữ ô hiện tại, ô tiếp theo và thêm hai ô sát góc nếu đi chéo để ngăn hai đường chéo giao nhau. Lớp kết nối và truyền lệnh lockstep qua mạng vẫn thuộc mốc multiplayer.

### Data

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Data/GameRTS.Data.asmdef` | Định nghĩa assembly dữ liệu cấu hình dùng UnityEngine và ScriptableObject; được View và kiểm thử đơn vị tham chiếu. |
| `Assets/Game/Data/UnitDataSO.cs` | Định nghĩa ScriptableObject chứa ID loại quân, tên, tốc độ, bán kính và Prefab hiển thị. Có menu Create > GameRTS > Unit Data; chuẩn hóa chuỗi và giới hạn giá trị âm trong Inspector. |
| `Assets/Game/Data/Units/SampleSoldierData.asset` | Dữ liệu loại `sample_soldier`, tên Lính mẫu, tốc độ 3, bán kính 0,45; tham chiếu SampleSoldier.prefab và được Unit Preview trong GameScene sử dụng. Các chỉ số là giá trị thử nghiệm. |

ScriptableObject là asset chứa cấu hình dùng chung cho một loại quân. Trạng thái riêng của từng quân nằm trong UnitState của Simulation. Tốc độ và bán kính lưu dạng số thực trong cấu hình; RtsGameView chuyển sang số nguyên theo tỷ lệ 1000 trước khi tạo trạng thái mô phỏng.

### Input

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Input/GameRTS.Input.asmdef` | Định nghĩa assembly đọc điều khiển; tham chiếu Commands, View và Unity.InputSystem. |
| `Assets/Game/Input/RtsCameraInput.cs` | Gắn trên Main Camera, đọc WASD, phím mũi tên, con lăn và vị trí chuột sát mép viewport rồi gọi RtsCameraView.ApplyInput. Có kiểm tra focus và Game View trong Editor. |
| `Assets/Game/Input/UnitSelectionInput.cs` | Gắn trên RTS Session, dùng Input Actions bắt vị trí tại lúc bấm/thả và di chuyển chuột để giữ đúng điểm bắt đầu kéo nhanh; Shift chọn thêm, chuột phải tạo MoveCommand. ProcessPointer dùng chung đường xử lý với kiểm thử PlayMode; hủy kéo khi mất focus và giải phóng Input Actions khi tắt component. |

### View và Prefab

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/View/GameRTS.View.asmdef` | Định nghĩa assembly hiển thị dùng UnityEngine, tham chiếu Simulation, Commands và Data. |
| `Assets/Game/View/RtsCameraView.cs` | Gắn trên Main Camera, pan theo thời gian, zoom Orthographic và clamp toàn bộ vùng nhìn vào biên Tilemap Ground. Nhận điều khiển từ RtsCameraInput. |
| `Assets/Game/View/UnitView.cs` | Gắn trên gốc Prefab quân, giữ liên kết dữ liệu loại và ID; SetSelected đổi vòng chọn, Render nội suy vị trí giữa hai trạng thái tick của Simulation. |
| `Assets/Game/View/UnitPreviewSpawner.cs` | Gắn trên Unit Preview. Trong GameScene, chuyển dữ liệu và Spawn Positions cho RtsGameView tạo quân trong Simulation và tạo hình hiển thị. Khi không gán game, vẫn hỗ trợ spawn Prefab độc lập để xem thử. |
| `Assets/Game/View/RtsMapView.cs` | Gắn trên Map Grid, chuyển ô Ground có nền và không có tile ở Obstacles thành ô đi được trong GridMap lúc khởi tạo. Kiểm tra map dùng ô XY 1 × 1. |
| `Assets/Game/View/RtsGameView.cs` | Gắn trên RTS Session, tạo SimulationWorld, chuẩn hóa dữ liệu ScriptableObject sang số nguyên, tạo quân, tích lũy thời gian chạy tick và cập nhật UnitView. Hiển thị hướng dẫn điều khiển cùng số quân đang chọn. |
| `Assets/Game/View/UnitSelectionView.cs` | Giữ danh sách ID được chọn tại máy người chơi. Xử lý click, kéo khung mọi hướng, Shift chọn thêm/bỏ chọn và vẽ khung quét trên màn hình. Cập nhật vòng chọn qua UnitView. |
| `Assets/Game/View/ReplayVerification.cs` | Chỉ chạy khi bản game có đối số -rtsReplayCheck kèm đường dẫn đầu ra; ghi kịch bản checksum dùng chung rồi thoát. Phục vụ đối chiếu Editor/Windows, không đổi luồng chơi thông thường. |
| `Assets/Game/View/Prefabs/SampleSoldier.prefab` | Khuôn quân mẫu gồm SpriteRenderer và UnitView ở gốc, con SelectionCircle có SpriteRenderer và mặc định tắt. Tham chiếu hai sprite trong Art/Units; UnitDataSO tham chiếu Prefab này. |

`UnitId` trong dữ liệu xác định loại quân; `InstanceId` của UnitView trong GameScene nhận từ ID do SimulationWorld cấp. Chọn quân và góc nhìn là trạng thái cục bộ. Vị trí thật và quyết định di chuyển do Simulation quản lý.

## 4. Scene và hình ảnh riêng của game

| File | Chức năng và nơi sử dụng |
| --- | --- |
| `Assets/Game/Scenes/GameScene.unity` | Scene gameplay được bật trong danh sách build. Chứa Main Camera, Map Grid/Ground, Map Grid/Obstacles, Unit Preview và RTS Session để chọn và điều khiển 5 quân mẫu. |
| `Assets/Game/Art/Terrain/GroundTile.png` | Sprite nền 32 × 32 pixel, Pixels Per Unit 32, dùng chung cho hai Tile asset nền cỏ. |
| `Assets/Game/Art/Terrain/GrassLight.asset` | Tile nền màu cỏ sáng, tham chiếu GroundTile.png và được vẽ lên Ground trong GameScene. |
| `Assets/Game/Art/Terrain/GrassDark.asset` | Tile nền màu cỏ tối, cùng sprite nhưng màu khác, xếp xen kẽ với GrassLight để dễ quan sát map. |
| `Assets/Game/Art/Terrain/RockObstacle.asset` | Tile màu nâu cho lớp Obstacles, dùng GroundTile.png. Tô tile này lên Obstacles bằng Tile Palette để đặt ô cấm đi trước khi Play. |
| `Assets/Game/Art/Units/SampleSoldier.png` | Sprite quân mẫu nhìn từ trên xuống, 64 × 64 pixel, nền trong suốt, Pixels Per Unit 64; dùng bởi SpriteRenderer ở gốc Prefab quân. |
| `Assets/Game/Art/Units/SelectionCircle.png` | Sprite vòng xanh 64 × 64 pixel, nền trong suốt, Pixels Per Unit 64; dùng bởi đối tượng con SelectionCircle. |

Ground là Tilemap 64 × 64 ô, mỗi ô 1 × 1, biên thế giới từ -32 đến 32 trên mỗi trục. Obstacles là Tilemap riêng có cùng hệ tọa độ; RtsMapView đọc hai lớp để tạo lưới di chuyển lúc khởi tạo. Thay đổi Tilemap trong lúc đang chơi chưa tự cập nhật lưới mô phỏng.

## 5. Kiểm thử

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Tests/Editor/GameRTS.Camera.Tests.asmdef` | Assembly kiểm thử chỉ dùng trong Editor, tham chiếu View, Input và Unity.InputSystem, sử dụng Unity Test Framework. |
| `Assets/Game/Tests/Editor/RtsCameraTests.cs` | Kiểm thử phím điều khiển, pan, hướng chéo, giới hạn zoom, clamp viewport và trường hợp kích thước màn hình/map khác nhau. |
| `Assets/Game/Tests/Editor/Units/GameRTS.Units.Tests.asmdef` | Assembly kiểm thử đơn vị chỉ dùng trong Editor, tham chiếu Data và View; tách thư mục để không có hai asmdef cùng một thư mục. |
| `Assets/Game/Tests/Editor/Units/UnitPreviewTests.cs` | Kiểm thử Prefab, bật/tắt và khởi tạo lại vòng chọn, spawn 5 bản sao có ID riêng, dữ liệu dùng chung, đầu vào không hợp lệ và chuẩn hóa cấu hình. |
| `Assets/Game/Tests/Editor/Simulation/GameRTS.Simulation.Tests.asmdef` | Assembly kiểm thử Editor tham chiếu Simulation và Commands để kiểm tra lõi C# mà không cần GameObject gameplay. |
| `Assets/Game/Tests/Editor/Simulation/SimulationTests.cs` | Kiểm thử A*, tốc độ ở cả 8 hướng, cấm cắt góc, ô sát góc bị giữ chỗ, hai đường chéo giao nhau, đổi đích giữa đoạn chéo, tick và lệnh. Kiểm tra bán kính quân không cắt vật cản và không chồng nhau ở các mốc nội suy 0%, 25%, 50%, 75%, 100%; thử 100 quân qua khe và so checksum replay. |
| `Assets/Game/Tests/PlayMode/GameRTS.PlayMode.Tests.asmdef` | Assembly test chạy PlayMode, tham chiếu View, Input và Simulation; chỉ đưa vào khi chạy test. |
| `Assets/Game/Tests/PlayMode/SelectionMovementTests.cs` | Nạp GameScene thật để kiểm tra click, Shift, kéo khung, ngưỡng/hủy kéo, ra lệnh cho nhóm đi vòng tường và click đích chéo làm thay đổi đồng thời X/Y trong Simulation và View. |

Mở Window > General > Test Runner và chọn EditMode hoặc PlayMode để chạy test tương ứng. Menu GameRTS cũng có lệnh chạy hai bộ kiểm tra. Kết quả được ghi trong Logs; tiến độ và kết quả đã xác nhận được cập nhật trong kế hoạch gốc.

### Công cụ Editor

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Editor/GameRTS.Editor.asmdef` | Assembly chỉ dùng trong Unity Editor, tham chiếu các lớp game và API Test Runner để thiết lập scene, kiểm tra và build. |
| `Assets/Game/Editor/MilestoneOneTools.cs` | Các menu GameRTS: thiết lập scene Mốc 1, chạy test EditMode/PlayMode, build Windows và ghi baseline replay. Giữ nội dung Obstacles hiện có khi chạy lại setup, từ chối sửa scene đang có thay đổi chưa lưu. Có thể nhận các tác vụ kiểm tra đã định sẵn qua Library/Milestone1.request khi Editor đang mở. |

## 6. File có sẵn từ template Universal 2D

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Scenes/SampleScene.unity` | Scene mẫu ban đầu của Unity; scene gameplay hiện dùng là Assets/Game/Scenes/GameScene.unity. |
| `Assets/InputSystem_Actions.inputactions` | Asset Input Actions có sẵn, được tham chiếu trong EditorBuildSettings. RtsCameraInput hiện đọc trực tiếp Keyboard/Mouse của Input System. |
| `Assets/Settings/UniversalRP.asset` | Asset cấu hình Universal Render Pipeline, liên kết renderer và các thông số dựng hình. |
| `Assets/Settings/Renderer2D.asset` | Cấu hình renderer 2D của URP, phục vụ hiển thị sprite và Tilemap. |
| `Assets/Settings/Lit2DSceneTemplate.scenetemplate` | Mẫu tạo scene Lit 2D URP, tham chiếu scene template bên dưới và có dữ liệu ảnh xem trước nhúng. |
| `Assets/Settings/Scenes/URP2DSceneTemplate.unity` | Scene nền cho template Lit 2D URP, có camera Orthographic và Global Light 2D. |
| `Assets/DefaultVolumeProfile.asset` | Volume Profile mặc định cho các thiết lập hiệu ứng hình ảnh của template. |
| `Assets/UniversalRenderPipelineGlobalSettings.asset` | Cấu hình URP ở cấp toàn project, dùng cùng GraphicsSettings và các asset dựng hình. |

Các file này là tài nguyên và thiết lập Unity. Trước khi chỉnh hoặc bỏ một asset, kiểm tra nơi tham chiếu để tránh mất cấu hình dựng hình hoặc input.

## 7. Packages và ProjectSettings

| File | Chức năng |
| --- | --- |
| `Packages/manifest.json` | Danh sách package project yêu cầu, gồm Input System, URP, công cụ 2D và Unity Test Framework. |
| `Packages/packages-lock.json` | Ghi phiên bản và phụ thuộc package đã được Unity giải quyết để giữ môi trường nhất quán khi mở project. |
| `ProjectSettings/ProjectVersion.txt` | Ghi phiên bản Editor của project, hiện là Unity 6000.3.13f1. |
| `ProjectSettings/ProjectSettings.asset` | Player Settings: cấu hình ứng dụng, nền tảng, build và các tùy chọn runtime. |
| `ProjectSettings/EditorBuildSettings.asset` | Danh sách scene build, hiện bật GameScene; đồng thời giữ tham chiếu cấu hình Input Actions. |
| `ProjectSettings/EditorSettings.asset` | Thiết lập Editor cho project, gồm cách quản lý và lưu asset. |
| `ProjectSettings/GraphicsSettings.asset` | Cấu hình dựng hình chung và tham chiếu render pipeline. |
| `ProjectSettings/QualitySettings.asset` | Các mức chất lượng dựng hình và thiết lập theo nền tảng. |
| `ProjectSettings/URPProjectSettings.asset` | Thiết lập ở cấp project do gói Universal Render Pipeline quản lý. |
| `ProjectSettings/ShaderGraphSettings.asset` | Thiết lập công cụ Shader Graph. |
| `ProjectSettings/TagManager.asset` | Tags, Layers và Sorting Layers dùng để phân loại đối tượng và sắp xếp hiển thị. |
| `ProjectSettings/TimeManager.asset` | Cấu hình thời gian của Unity; nhịp Simulation hiện là 20 tick/giây do SimulationWorld và RtsGameView quản lý riêng. |
| `ProjectSettings/InputManager.asset` | Cấu hình Input Manager cũ do Unity lưu; mã camera hiện dùng Input System. |
| `ProjectSettings/Physics2DSettings.asset` | Cấu hình Unity Physics 2D; không dùng để quyết định trạng thái Simulation theo kiến trúc đã chốt. |
| `ProjectSettings/DynamicsManager.asset` | Cấu hình Unity Physics 3D. |
| `ProjectSettings/NavMeshAreas.asset` | Các vùng và chi phí NavMesh của Unity; tìm đường RTS dự kiến triển khai trong Simulation. |
| `ProjectSettings/AudioManager.asset` | Cấu hình âm thanh chung của Unity. |
| `ProjectSettings/MemorySettings.asset` | Cấu hình bộ nhớ và các tùy chọn cấp phát của Unity. |
| `ProjectSettings/PackageManagerSettings.asset` | Thiết lập Package Manager cho project. |
| `ProjectSettings/PresetManager.asset` | Quản lý preset mặc định cho component và importer. |
| `ProjectSettings/SceneTemplateSettings.json` | Cấu hình công cụ scene template của Editor. |
| `ProjectSettings/VersionControlSettings.asset` | Chế độ tích hợp quản lý phiên bản trong Unity; repository Git nằm ở gốc GameRTS. |
| `ProjectSettings/UnityConnectSettings.asset` | Cấu hình tích hợp các dịch vụ Unity. |
| `ProjectSettings/MultiplayerManager.asset` | Cấu hình do công cụ multiplayer của Unity lưu; không thể hiện việc mạng lockstep của game đã được triển khai. |
| `ProjectSettings/NetworkManager.asset` | Thiết lập NetworkManager của Unity; giao thức truyền lệnh của game vẫn thuộc mốc multiplayer trong kế hoạch. |
| `ProjectSettings/ClusterInputManager.asset` | Cấu hình Cluster Input của Unity, không phải mã đọc chuột/bàn phím RTS hiện tại. |
| `ProjectSettings/VFXManager.asset` | Thiết lập hệ thống hiệu ứng VFX. |
| `ProjectSettings/XRSettings.asset` | Cấu hình XR do Unity lưu; nền tảng phát hành đã chốt của game là PC. |

## 8. Các phần đang kết nối với nhau

Luồng camera hiện tại:

```text
WASD / phím mũi tên / con lăn / chuột sát mép
    → RtsCameraInput trên Main Camera
    → RtsCameraView.ApplyInput
    → Camera pan và zoom
    → LateUpdate clamp vùng nhìn theo Ground
```

Luồng tạo quân trong GameScene:

```text
GameScene → Unit Preview → UnitPreviewSpawner.Start
    → đọc SampleSoldierData.asset
    → RtsGameView chuẩn hóa chỉ số và gọi SimulationWorld.AddUnit
    → Simulation cấp ID, tìm ô spawn trống gần vị trí cấu hình
    → tạo 5 bản sao SampleSoldier.prefab và gán ID tương ứng
    → UnitView.Render hiển thị trạng thái từ Simulation
```

Luồng chọn và ra lệnh:

```text
Chuột trái / kéo khung → UnitSelectionInput → UnitSelectionView
    → danh sách ID đang chọn → UnitView.SetSelected
Chuột phải → MoveCommand(tick, player, sequence, IDs, đích số nguyên)
    → SimulationWorld → phân ô đích → A* → giữ chỗ và di chuyển theo tick
    → RtsGameView → UnitView.Render → sprite và vòng chọn đi theo quân
```

Để đổi chỉ số loại quân, chọn asset trong Data/Units. Để đổi hình quân, mở Prefab trong View/Prefabs và thay sprite. Để đổi số lượng hoặc vị trí spawn mẫu, chọn Unit Preview trong Hierarchy và chỉnh Spawn Positions trước khi Play. Để chỉnh map, chọn Ground và dùng Tile Palette với các Tile trong Art/Terrain.

Mốc 0 và Mốc 1 đã hoàn thành. Phần di chuyển được sửa từ 4 sang 8 hướng ngày 13/11/2026: A* và bước di chuyển đều hỗ trợ đường chéo, ngăn cắt góc và giữ đúng khoảng cách di chuyển. Bộ kiểm tra mới có 42 test EditMode và 4 test PlayMode, gồm 100 quân qua khe và thay đổi cả X/Y trong scene khi ra lệnh chéo. Xem kết quả build, đối chiếu checksum Editor/Windows và nghiệm thu trong kế hoạch gốc. Phần kết nối, lobby và truyền lệnh lockstep qua mạng vẫn ở mốc multiplayer.

## 9. File Unity sinh tự động và sản phẩm build

| Nhóm file hoặc thư mục | Chức năng và cách quản lý |
| --- | --- |
| `.git/` | Dữ liệu repository Git, lịch sử commit và cấu hình remote. Dùng công cụ Git hoặc GitHub Desktop để quản lý. |
| `Assets/**/*.meta` | Unity sinh file .meta cho asset và thư mục, chứa GUID và thiết lập import. Quản lý cùng asset bằng Git; giữ nguyên cặp asset/.meta khi đổi tên hoặc di chuyển. Tài liệu ghi nhóm này thay vì tạo một dòng riêng cho từng .meta tự sinh. |
| `Library/` | Cache import, assembly đã biên dịch, package tải về và dữ liệu nội bộ Unity. Git bỏ qua; không đặt mã nguồn hoặc tài nguyên tự làm vào đây. |
| `Temp/` | File tạm trong lúc Unity chạy hoặc build, Git bỏ qua. |
| `Logs/` và `*.log` | Log Unity, build và kết quả kiểm tra. Các log mốc 0 hiện còn ở gốc cũng thuộc nhóm này, Git bỏ qua. |
| `Builds/` và `Build/` | Sản phẩm build, Git bỏ qua. Bản PC hiện ở Builds/Windows/GameRTS.exe và phải đi kèm thư mục dữ liệu cùng các thành phần Unity tạo ra. |
| `UserSettings/` | Cấu hình Editor theo người dùng hoặc máy, Git bỏ qua. |
| `.vs/`, `.idea/`, `*.csproj`, `*.sln`, `*.slnx` | Cache và file project/solution IDE tự sinh; Git bỏ qua. Unity có thể tạo lại khi đồng bộ mã với IDE. |
| `Obj/`, `obj/` | File trung gian khi biên dịch, Git bỏ qua nếu xuất hiện. |

## 10. Cách cập nhật khi thêm hoặc thay đổi file

Mỗi khi thêm file được giữ lại trong dự án, bổ sung vào đúng bảng ở trên trong cùng lượt làm việc. Ghi đường dẫn, chức năng, nơi sử dụng hoặc phần liên quan. Nếu tạo nhóm chức năng mới, thêm thư mục vào sơ đồ và giải thích luồng hoạt động. File do Unity tự sinh được ghi theo các nhóm ở mục 9; file mã nguồn hoặc asset mới do dự án quản lý phải có mục riêng.

Khi di chuyển, đổi tên hoặc xóa file, sửa hoặc bỏ dòng cũ để danh mục tiếp tục khớp với project. Khi chức năng file thay đổi, cập nhật phần mô tả tương ứng. Kiểm tra lại cây thư mục, đường dẫn và quan hệ trước khi báo hoàn thành.

Tài liệu này được khởi tạo ngày 13/11/2026 từ các file thực tế trong project, bao gồm phần camera/map và dữ liệu/spawn mẫu đã triển khai.
