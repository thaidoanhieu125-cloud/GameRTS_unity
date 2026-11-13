# Cấu trúc dự án GameRTS

Cập nhật ngày 13/11/2026. Tài liệu giải thích các thư mục, file hiện có và cách chúng kết nối với nhau, giúp người đang học Unity tìm đúng nơi để đọc hoặc chỉnh sửa. Mọi đường dẫn trong tài liệu được tính từ thư mục gốc `GameRTS`.

Bản Claude nguyên văn nằm trong `TAI_LIEU_GOC_CLAUDE.txt`, chỉ đọc và đối chiếu. Kế hoạch thiết kế và tiến độ được cập nhật trong `Game RTS Unity Doc.md`. Luật làm việc nằm trong `role/LUAT_LAM_VIEC.md`; `AGENTS.md` ở gốc dẫn tới các tài liệu này.

## 1. Sơ đồ thư mục

```text
GameRTS/
├── AGENTS.md                       Điểm vào dẫn tới luật và tài liệu
├── Game RTS Unity Doc.md            Kế hoạch triển khai đang cập nhật
├── TAI_LIEU_GOC_CLAUDE.txt          Bản Claude nguyên văn, chỉ đọc
├── role/
│   ├── LUAT_LAM_VIEC.md             Luật bắt buộc mỗi lần làm việc
│   └── CAU_TRUC_DU_AN.md            Danh mục cấu trúc và chức năng file
├── .gitattributes                  Giữ nguyên byte của bản Claude qua Git
├── .gitignore                      Quy tắc bỏ qua file khi quản lý bằng Git
├── .vsconfig                       Cấu hình thành phần Visual Studio
├── Assets/
│   ├── Game/                       Mã nguồn và tài nguyên riêng của game
│   │   ├── Art/
│   │   │   ├── Terrain/            Sprite và Tile asset cho nền map
│   │   │   ├── Units/              Sprite quân và vòng chọn
│   │   │   └── UI/                 4 icon tài nguyên mẫu, nền trong suốt
│   │   ├── Commands/               Hợp đồng lệnh, di chuyển và thử ví bằng C# thuần
│   │   ├── Data/
│   │   │   └── Units/              Asset dữ liệu từng loại quân
│   │   ├── Editor/                 Công cụ thiết lập scene, kiểm tra và build
│   │   ├── Input/                  Đọc chuột và bàn phím
│   │   ├── Scenes/                 Scene gameplay
│   │   ├── Simulation/             Lưới, A*, quân, ví từng người chơi và tick
│   │   ├── Tests/Editor/
│   │   │   ├── Simulation/         Kiểm thử A*, lệnh, va chạm và replay
│   │   │   └── Units/              Kiểm thử dữ liệu, Prefab và spawn
│   │   ├── Tests/PlayMode/          Kiểm thử chọn và điều khiển quân trong scene
│   │   ├── UI/                     HUD tài nguyên bằng uGUI, chỉ đọc Simulation
│   │   │   └── Prefabs/            EconomyHud: Canvas, top bar, bảng thử ví
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

## 2. Tài liệu, luật và cấu hình

| File | Chức năng và cách sử dụng |
| --- | --- |
| `AGENTS.md` | Điểm vào ở gốc project, dẫn tới luật trong role, bản Claude chỉ đọc, kế hoạch Markdown và danh mục cấu trúc. |
| `role/CAU_TRUC_DU_AN.md` | Danh mục các file và thư mục, chức năng, quan hệ và cách tìm phần cần chỉnh. Cập nhật mỗi khi thêm file được giữ lại hoặc thay đổi cấu trúc. |
| `Game RTS Unity Doc.md` | Kế hoạch triển khai đang cập nhật, chuyển đủ 187 đoạn và 4 bảng từ bản DOCX hiện tại. Ghi quyết định đã chốt, thiết kế, tiến độ và kết quả kiểm tra. |
| `TAI_LIEU_GOC_CLAUDE.txt` | Nguyên văn nội dung Claude do người dùng cung cấp, chỉ đọc/đối chiếu, không chỉnh sửa hoặc xóa. Đặt Read-only trên Windows; SHA-256 được lưu trong file luật. |
| `role/LUAT_LAM_VIEC.md` | Luật đọc tài liệu trước khi làm, bảo vệ bản Claude, giải thích chi tiết khi hoàn thành và cập nhật danh mục file. |
| `.gitattributes` | Ngăn Git tự đổi kiểu xuống dòng của bản Claude để nội dung byte và SHA-256 ổn định qua commit/checkout. |
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
| `Assets/Game/Simulation/SimulationWorld.cs` | Mô phỏng 20 tick/giây, giữ ví riêng theo ID người chơi, tính dân số khi spawn và xử lý các loại ISimulationCommand theo player/sequence chung. Giữ cơ chế đường đi 8 hướng, tránh góc, giữ ô đích và nhường đường của Mốc 1. Lệnh thử ví chỉ nhận khi bật cờ riêng; checksum gồm ví, dân số, kết quả lệnh thử, đường đi và lệnh đang chờ. |
| `Assets/Game/Simulation/PlayerEconomyState.cs` | Ví số nguyên gồm Wood/Food/Gold/Population/MaxPopulation. Nạp, thanh toán nhiều loại cùng lúc, giữ/giải phóng dân số và tăng sức chứa; từ chối số âm, thiếu tiền, vượt sức chứa hoặc tràn int mà không thay đổi dở dang. |
| `Assets/Game/Simulation/DeterminismScenario.cs` | Kịch bản chung 100 quân, map tường/khe, 3 lệnh di chuyển cùng nạp/trừ/thanh toán thất bại/tăng sức chứa trong 600 tick. Replay phiên bản 3 ghi Directions=8;Economy=1, đối chiếu hai Simulation và Editor/Windows. Sức chứa riêng của bài stress là 200. |
| `Assets/Game/Commands/GameRTS.Commands.asmdef` | Định nghĩa assembly dành cho dữ liệu lệnh, không tham chiếu UnityEngine. |
| `Assets/Game/Commands/MoveCommand.cs` | Lệnh di chuyển bất biến chứa tick thực thi, mã người chơi, số thứ tự, danh sách ID đã sắp xếp và tọa độ đích số nguyên. Input tạo lệnh, SimulationWorld kiểm tra và thực thi. |
| `Assets/Game/Commands/ISimulationCommand.cs` | Hợp đồng C# thuần Tick/PlayerId/Sequence dùng chung cho các lệnh; SimulationWorld dùng chung hàng đợi và bộ lọc lệnh trùng. |
| `Assets/Game/Commands/EconomyDebugCommand.cs` | Lệnh bất biến để thử nạp, chi 3 loại tài nguyên hoặc tăng giới hạn dân số. Chứa enum thao tác và lượng nguyên dương; bị từ chối mặc định khi SimulationWorld không bật cho phép thử ví. |

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
| `Assets/Game/Input/GameRTS.Input.asmdef` | Assembly điều khiển, tham chiếu Commands, Simulation, View, UI và Unity.InputSystem; UI không tham chiếu ngược Input. |
| `Assets/Game/Input/RtsCameraInput.cs` | Gắn Main Camera, đọc WASD, mũi tên, con lăn, edge scrolling rồi gọi RtsCameraView.ApplyInput. Kiểm tra focus/Game View; không zoom hoặc cuộn mép khi con trỏ nằm trên top bar hoặc bảng thử ví. |
| `Assets/Game/Input/UnitSelectionInput.cs` | Input Actions giữ đúng điểm bắt đầu kéo nhanh; Shift chọn thêm, chuột phải tạo MoveCommand dùng NextLocalSequence của game. ProcessPointer dùng chung với PlayMode test; bỏ qua vùng HUD, hủy kéo khi mất focus và giải phóng Input Actions khi tắt. |
| `Assets/Game/Input/EconomyTestInput.cs` | F9 mở/đóng bảng thử ví; nhận nút bấm từ HUD, tạo EconomyDebugCommand sau 2 tick với lượng 50 mỗi loại hoặc tăng sức chứa 5. Dùng NextLocalSequence chung với di chuyển; không trực tiếp sửa ví. |

### View và Prefab

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/View/GameRTS.View.asmdef` | Định nghĩa assembly hiển thị dùng UnityEngine, tham chiếu Simulation, Commands và Data. |
| `Assets/Game/View/RtsCameraView.cs` | Gắn trên Main Camera, pan theo thời gian, zoom Orthographic và clamp toàn bộ vùng nhìn vào biên Tilemap Ground. Nhận điều khiển từ RtsCameraInput. |
| `Assets/Game/View/UnitView.cs` | Gắn trên gốc Prefab quân, giữ liên kết dữ liệu loại và ID; SetSelected đổi vòng chọn, Render nội suy vị trí giữa hai trạng thái tick của Simulation. |
| `Assets/Game/View/UnitPreviewSpawner.cs` | Gắn trên Unit Preview. Trong GameScene, chuyển dữ liệu và Spawn Positions cho RtsGameView tạo quân trong Simulation và tạo hình hiển thị. Khi không gán game, vẫn hỗ trợ spawn Prefab độc lập để xem thử. |
| `Assets/Game/View/RtsMapView.cs` | Gắn trên Map Grid, chuyển ô Ground có nền và không có tile ở Obstacles thành ô đi được trong GridMap lúc khởi tạo. Kiểm tra map dùng ô XY 1 × 1. |
| `Assets/Game/View/RtsGameView.cs` | Gắn RTS Session: cấu hình ví ban đầu trong Inspector, khởi tạo World và player, tạo quân, chuyển dữ liệu sang số nguyên, chạy tick và cập nhật UnitView. Cấp số thứ tự lệnh cục bộ dùng chung. Hướng dẫn và số quân chọn được chuyển sang HUD. |
| `Assets/Game/View/UnitSelectionView.cs` | Giữ danh sách ID được chọn tại máy người chơi. Xử lý click, kéo khung mọi hướng, Shift chọn thêm/bỏ chọn và vẽ khung quét trên màn hình. Cập nhật vòng chọn qua UnitView. |
| `Assets/Game/View/ReplayVerification.cs` | Chỉ chạy khi bản game có đối số -rtsReplayCheck kèm đường dẫn đầu ra; ghi kịch bản checksum dùng chung rồi thoát. Phục vụ đối chiếu Editor/Windows, không đổi luồng chơi thông thường. |
| `Assets/Game/View/Prefabs/SampleSoldier.prefab` | Khuôn quân mẫu gồm SpriteRenderer và UnitView ở gốc, con SelectionCircle có SpriteRenderer và mặc định tắt. Tham chiếu hai sprite trong Art/Units; UnitDataSO tham chiếu Prefab này. |

`UnitId` trong dữ liệu xác định loại quân; `InstanceId` của UnitView trong GameScene nhận từ ID do SimulationWorld cấp. Chọn quân và góc nhìn là trạng thái cục bộ. Vị trí thật và quyết định di chuyển do Simulation quản lý.

### UI — Thanh tài nguyên

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/UI/GameRTS.UI.asmdef` | Assembly giao diện tham chiếu View, Simulation, Commands và UnityEngine.UI (uGUI đã có sẵn trong project). |
| `Assets/Game/UI/EconomyHudView.cs` | Đọc ví player cục bộ và cập nhật 4 số trên top bar trong LateUpdate. Hiện phản hồi sau tick, mở/đóng bảng thử, phát yêu cầu thao tác cho Input và cung cấp vùng chặn chuột. Dùng uGUI Text với font Unity tích hợp, không cần TMP Essentials. |
| `Assets/Game/UI/Prefabs/EconomyHud.prefab` | Canvas Screen Space Overlay, CanvasScaler 1280 × 720, 4 thẻ icon/số, nút F9, bảng nạp/chi/sức chứa và hướng dẫn dưới màn hình. Có thể chỉnh màu, kích thước, icon trong Prefab; scene gán game vào HUD. |

HUD chỉ đọc ví. UnitSelectionInput bỏ qua click/chuột phải trên top bar hoặc bảng thử; RtsCameraInput bỏ qua edge scrolling và zoom khi con trỏ ở các vùng đó. WASD vẫn dùng được. EventSystem trong GameScene dùng InputSystemUIInputModule để các Button nhận được Input System mới.

## 4. Scene và hình ảnh riêng của game

| File | Chức năng và nơi sử dụng |
| --- | --- |
| `Assets/Game/Scenes/GameScene.unity` | Scene gameplay trong build: Main Camera, Map Grid/Ground/Obstacles, Unit Preview, RTS Session với 5 quân và ví, Economy HUD prefab cùng UI EventSystem. |
| `Assets/Game/Art/Terrain/GroundTile.png` | Sprite nền 32 × 32 pixel, Pixels Per Unit 32, dùng chung cho hai Tile asset nền cỏ. |
| `Assets/Game/Art/Terrain/GrassLight.asset` | Tile nền màu cỏ sáng, tham chiếu GroundTile.png và được vẽ lên Ground trong GameScene. |
| `Assets/Game/Art/Terrain/GrassDark.asset` | Tile nền màu cỏ tối, cùng sprite nhưng màu khác, xếp xen kẽ với GrassLight để dễ quan sát map. |
| `Assets/Game/Art/Terrain/RockObstacle.asset` | Tile màu nâu cho lớp Obstacles, dùng GroundTile.png. Tô tile này lên Obstacles bằng Tile Palette để đặt ô cấm đi trước khi Play. |
| `Assets/Game/Art/Units/SampleSoldier.png` | Sprite quân mẫu nhìn từ trên xuống, 64 × 64 pixel, nền trong suốt, Pixels Per Unit 64; dùng bởi SpriteRenderer ở gốc Prefab quân. |
| `Assets/Game/Art/Units/SelectionCircle.png` | Sprite vòng xanh 64 × 64 pixel, nền trong suốt, Pixels Per Unit 64; dùng bởi đối tượng con SelectionCircle. |
| `Assets/Game/Art/UI/FoodIcon.png` | Icon quả đỏ 64 × 64, nền trong suốt, hình mẫu tự tạo bằng công cụ Editor, dùng ô Thực của HUD. |
| `Assets/Game/Art/UI/WoodIcon.png` | Icon hai khúc gỗ 64 × 64, nền trong suốt, dùng ô Gỗ. |
| `Assets/Game/Art/UI/GoldIcon.png` | Icon thỏi vàng 64 × 64, nền trong suốt, dùng ô Vàng. |
| `Assets/Game/Art/UI/PopulationIcon.png` | Icon nhóm người 64 × 64, nền trong suốt, dùng ô Dân số hiện tại/giới hạn. |

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
| `Assets/Game/Tests/Editor/Simulation/PlayerEconomyTests.cs` | Kiểm tra thanh toán nguyên vẹn, từ chối số âm/tràn số, sức chứa, spawn tính dân số, lệnh đúng tick và đúng chủ, chặn lệnh thử mặc định, sequence chung, thứ tự lệnh và checksum ví. |
| `Assets/Game/Tests/PlayMode/GameRTS.PlayMode.Tests.asmdef` | Assembly PlayMode tham chiếu View, Input, Simulation, UI, Commands và UnityEngine.UI; chỉ đưa vào khi chạy test. |
| `Assets/Game/Tests/PlayMode/SelectionMovementTests.cs` | Nạp GameScene thật để kiểm tra click, Shift, kéo khung, ngưỡng/hủy kéo, ra lệnh cho nhóm đi vòng tường và click đích chéo làm thay đổi đồng thời X/Y trong Simulation và View. |
| `Assets/Game/Tests/PlayMode/EconomyHudTests.cs` | Nạp GameScene thật để kiểm tra ví ban đầu, 5/10 dân, nút nạp chạy sau tick, HUD đổi theo ví, chi thiếu bị từ chối, sức chứa và chặn click xuyên HUD. Kiểm tra nạp và di chuyển không trùng sequence. |

Mở Window > General > Test Runner và chọn EditMode hoặc PlayMode để chạy test tương ứng. Menu GameRTS cũng có lệnh chạy hai bộ kiểm tra. Kết quả được ghi trong Logs; tiến độ và kết quả đã xác nhận được cập nhật trong kế hoạch gốc.

### Công cụ Editor

| File | Chức năng và quan hệ |
| --- | --- |
| `Assets/Game/Editor/GameRTS.Editor.asmdef` | Assembly chỉ dùng trong Unity Editor, tham chiếu các lớp game và API Test Runner để thiết lập scene, kiểm tra và build. |
| `Assets/Game/Editor/MilestoneOneTools.cs` | Các menu GameRTS: thiết lập scene Mốc 1, chạy test EditMode/PlayMode, build Windows và ghi baseline replay. Giữ nội dung Obstacles hiện có khi chạy lại setup, từ chối sửa scene đang có thay đổi chưa lưu. Có thể nhận các tác vụ kiểm tra đã định sẵn qua Library/Milestone1.request khi Editor đang mở. |
| `Assets/Game/Editor/MilestoneTwoTools.cs` | Menu GameRTS > Set up Resource HUD; tạo 4 PNG icon mẫu, Prefab HUD và kết nối HUD/Input/EventSystem vào scene. Không ghi đè icon hoặc Prefab đã có, từ chối scene chưa lưu; cũng nhận action economy-setup qua công cụ Mốc 1. |

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

Mốc 2 Bước 1 đã hoàn thành: RTS Session khởi tạo ví 200 Gỗ/200 Thực/100 Vàng/10 sức chứa; 5 quân spawn sử dụng 5 dân. Bấm nút thử → EconomyTestInput → EconomyDebugCommand → hàng đợi tick chung trong SimulationWorld → PlayerEconomyState kiểm tra và thực hiện → EconomyHudView đọc kết quả. Số kiểm thử sau bổ sung là 52 EditMode và 7 PlayMode. Build PC thành công, replay Editor/Windows khớp đủ 600 tick; log theo nhóm ở mục 9, chi tiết nghiệm thu trong kế hoạch. Chức năng thu hoạch, tài nguyên trên map, xây nhà và huấn luyện chưa thuộc bước này.

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

Tài liệu được khởi tạo và duy trì từ các file thực tế trong project. Ngày 13/11/2026 chuyển vào role, bổ sung bản Claude chỉ đọc, kế hoạch Markdown và luật bảo vệ nguyên bản. Mọi đường dẫn trong danh mục vẫn tính từ gốc GameRTS.
